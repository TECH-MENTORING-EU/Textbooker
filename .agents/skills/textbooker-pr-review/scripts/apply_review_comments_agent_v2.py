#!/usr/bin/env python3
"""
Version 6:
- checks required components before doing anything useful
- works on the currently checked-out branch
- finds the open PR for that branch
- fetches unresolved current review threads
- writes the prompt to a file
- launches standalone `copilot` CLI in autopilot mode
- avoids passing a huge prompt on the command line
- does not create a commit
- does not ask the user questions

Requires:
- git
- gh
- copilot 1.0.70+

Windows:
  python .\apply_review_comments_agent_v6.py
"""

from __future__ import annotations

import argparse
import json
import os
import shutil
import subprocess
import tempfile
from pathlib import Path
from typing import Any


# GraphQL query used to page through all review threads (and their comments) for a
# given pull request. `cursor` drives pagination via reviewThreads.pageInfo.
GRAPHQL_QUERY = r"""
query($owner:String!,$repo:String!,$number:Int!,$cursor:String){
  repository(owner:$owner,name:$repo){
    pullRequest(number:$number){
      number
      title
      url
      state
      headRefName
      baseRefName
      reviewThreads(first:100,after:$cursor){
        pageInfo{hasNextPage endCursor}
        nodes{
          id
          isResolved
          isOutdated
          path
          line
          originalLine
          comments(first:100){
            nodes{
              id
              author{login}
              body
              createdAt
              updatedAt
            }
          }
        }
      }
    }
  }
}
"""


# --- Small subprocess helpers -------------------------------------------------
# These wrap subprocess.run so callers get plain text/JSON output and consistent
# error handling instead of repeating boilerplate everywhere.


def run(args: list[str], *, cwd: Path | None = None) -> subprocess.CompletedProcess[str]:
    # Run a command, capturing stdout/stderr as text instead of raising on failure.
    # Callers inspect the returncode themselves (see run_text below).
    return subprocess.run(
        args,
        cwd=str(cwd) if cwd else None,
        check=False,
        capture_output=True,
        text=True,
    )


def run_text(args: list[str], *, cwd: Path | None = None) -> str:
    # Run a command and return its stripped stdout, or abort the script with the
    # command's stderr (or a generic message) if it exited non-zero.
    result = run(args, cwd=cwd)
    if result.returncode != 0:
        raise SystemExit(result.stderr.strip() or f"Command failed: {' '.join(args)}")
    return result.stdout.strip()


def run_json(args: list[str], *, cwd: Path | None = None) -> Any:
    # Convenience wrapper for commands (e.g. `gh api graphql`) that emit JSON.
    return json.loads(run_text(args, cwd=cwd))


# --- Repository / branch / remote discovery -----------------------------------


def repo_root() -> Path:
    # Resolve the top-level directory of the current git repository.
    return Path(run_text(["git", "rev-parse", "--show-toplevel"]))


def current_branch() -> str:
    # The script always operates on whatever branch is currently checked out.
    branch = run_text(["git", "branch", "--show-current"])
    if not branch:
        raise SystemExit("No current branch detected. Checkout a branch first.")
    return branch


def infer_owner_repo() -> tuple[str, str]:
    # Prefer asking `gh` for the repo's owner/name (works for any remote naming).
    # Fall back to parsing the `origin` remote URL if `gh` is unavailable/fails,
    # handling both HTTPS (github.com/owner/repo) and SSH (git@github.com:owner/repo) forms.
    try:
        data = run_json(["gh", "repo", "view", "--json", "nameWithOwner"])
        owner, repo = data["nameWithOwner"].split("/", 1)
        return owner, repo
    except Exception:
        origin = run_text(["git", "remote", "get-url", "origin"])
        if origin.endswith(".git"):
            origin = origin[:-4]
        if "github.com/" in origin:
            tail = origin.split("github.com/", 1)[1]
            tail = tail.replace(":", "/")
            parts = tail.strip("/").split("/")
            if len(parts) >= 2:
                return parts[-2], parts[-1]
    raise SystemExit("Could not infer repository owner/repo. Pass --repo OWNER/REPO.")


# --- Environment / prerequisite checks -----------------------------------------


def check_required_components() -> None:
    missing: list[str] = []

    if shutil.which("git") is None:
        missing.append("git")
    if shutil.which("gh") is None:
        missing.append("gh")
    if shutil.which("copilot") is None:
        missing.append("copilot")

    if missing:
        print("Missing required components:")
        for item in missing:
            print(f" - {item}")
        raise SystemExit(1)

    try:
        run_text(["gh", "auth", "status"])
    except Exception:
        print("GitHub CLI is installed, but authentication could not be verified.")
        raise SystemExit(1)

    help_text = run_text(["copilot", "--help"])
    if "--mode" not in help_text or "--prompt" not in help_text or "--no-ask-user" not in help_text:
        print("Your copilot CLI does not expose the required flags for this script.")
        print("Expected: --mode, --prompt, --no-ask-user")
        raise SystemExit(1)

    print("Required components found:")
    print(" - git")
    print(" - gh")
    print(" - copilot")
    print(" - GitHub auth OK")
    print(" - copilot flags OK")


def find_open_pr(owner: str, repo: str, branch: str) -> dict[str, Any]:
    # Look up the open PR whose head branch matches the currently checked-out
    # branch. This is how the script figures out "which PR am I working on".
    query = """
    query($owner:String!, $repo:String!, $headRefName:String!) {
      repository(owner:$owner, name:$repo) {
        pullRequests(first: 20, states: OPEN, headRefName: $headRefName) {
          nodes {
            number
            title
            url
            headRefName
            baseRefName
          }
        }
      }
    }
    """
    result = run_json(
        [
            "gh",
            "api",
            "graphql",
            "-f",
            f"query={query}",
            "-F",
            f"owner={owner}",
            "-F",
            f"repo={repo}",
            "-F",
            f"headRefName={branch}",
        ]
    )
    nodes = result["data"]["repository"]["pullRequests"]["nodes"]
    if not nodes:
        raise SystemExit(f"No open PR found for branch '{branch}'.")
    return nodes[0]


def fetch_threads(owner: str, repo: str, pr_number: int) -> dict[str, Any]:
    # Page through GRAPHQL_QUERY's reviewThreads connection until all pages are
    # collected, accumulating every thread (resolved or not) along with the PR
    # metadata returned on the first page.
    cursor: str | None = None
    pull_request: dict[str, Any] | None = None
    threads: list[dict[str, Any]] = []

    while True:
        command = [
            "gh",
            "api",
            "graphql",
            "-f",
            f"query={GRAPHQL_QUERY}",
            "-F",
            f"owner={owner}",
            "-F",
            f"repo={repo}",
            "-F",
            f"number={pr_number}",
        ]
        if cursor:
            command.extend(["-F", f"cursor={cursor}"])

        payload = run_json(command)["data"]["repository"]["pullRequest"]
        if payload is None:
            raise SystemExit(f"Pull request #{pr_number} not found.")

        # Pop reviewThreads off so `payload` becomes just the PR metadata, which
        # only needs to be captured once (it's identical on every page).
        page = payload.pop("reviewThreads")
        pull_request = pull_request or payload
        threads.extend(page["nodes"])

        if not page["pageInfo"]["hasNextPage"]:
            break
        cursor = page["pageInfo"]["endCursor"]

    return {"pull_request": pull_request, "review_threads": threads}


def unresolved_current_threads(data: dict[str, Any]) -> list[dict[str, Any]]:
    # Filter down to threads that still need action: not resolved and not
    # outdated (i.e. still pointing at current code, not stale diff context).
    return [
        thread
        for thread in data["review_threads"]
        if not thread["isResolved"] and not thread["isOutdated"]
    ]


def build_prompt(pr: dict[str, Any], threads: list[dict[str, Any]]) -> str:
    # Assemble the natural-language prompt handed to the `copilot` CLI: PR
    # context, ground rules for autonomous operation, and a numbered list of
    # unresolved review threads (using each thread's first/original comment).
    lines = [
        f"You are working in the repository on PR #{pr['number']}: {pr['title']}.",
        f"PR URL: {pr['url']}",
        "",
        "Goal:",
        "Implement fixes for the unresolved review comments below.",
        "",
        "Rules:",
        "- Do not ask the user any questions.",
        "- Make reasonable assumptions and proceed independently.",
        "- Do not create a commit.",
        "- Keep changes targeted and minimal.",
        "- If something is ambiguous, choose the safest reasonable implementation.",
        "- Leave the repository with uncommitted changes only.",
        "",
        "Unresolved review threads:",
        "",
    ]

    for i, thread in enumerate(threads, start=1):
        first = thread["comments"]["nodes"][0] if thread["comments"]["nodes"] else {}
        author = first.get("author", {}).get("login", "unknown")
        body = (first.get("body") or "").strip()
        lines.append(f"{i}. {thread.get('path')}:{thread.get('line')} ({author})")
        lines.append(body or "(empty)")
        lines.append("")

    return "\n".join(lines).rstrip() + "\n"


def write_prompt_file(root: Path, prompt: str, filename: str) -> Path:
    # Persist the generated prompt to disk (at the repo root) so it can be
    # inspected/reused, and so launch_copilot doesn't need a huge CLI argument.
    path = root / filename
    path.write_text(prompt, encoding="utf-8")
    return path


def launch_copilot(root: Path, prompt: str) -> int:
    # Write the prompt to a temp file first (avoids OS command-line length
    # limits / escaping issues), then read it back and invoke the standalone
    # `copilot` CLI in unattended autopilot mode.
    with tempfile.NamedTemporaryFile("w", delete=False, suffix=".txt", encoding="utf-8") as tmp:
        tmp.write(prompt)
        tmp_path = Path(tmp.name)

    try:
        # We keep the prompt in a file for traceability and avoid a huge CLI argument.
        # If your copilot version supports reading from a file directly, adapt here.
        cmd = [
            "copilot",
            "--mode",
            "autopilot",
            "--no-ask-user",
            "--allow-all",
            "--prompt",
            tmp_path.read_text(encoding="utf-8"),
        ]
        result = subprocess.run(cmd, cwd=str(root), check=False)
        return result.returncode
    finally:
        # Always clean up the temp file, even if copilot fails to launch.
        try:
            tmp_path.unlink(missing_ok=True)
        except Exception:
            pass


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", help="OWNER/REPO. Defaults to inferred repo.")
    parser.add_argument("--prompt-file", default="copilot-agent-prompt.txt")
    parser.add_argument("--no-copilot", action="store_true")
    args = parser.parse_args()

    # Fail fast if git/gh/copilot aren't installed, gh isn't authenticated, or
    # the installed copilot CLI lacks the flags this script relies on.
    check_required_components()

    root = repo_root()
    branch = current_branch()

    # Determine the target owner/repo either from --repo or by inference.
    if args.repo:
        if "/" not in args.repo:
            raise SystemExit("--repo must be in OWNER/REPO format.")
        owner, repo = args.repo.split("/", 1)
    else:
        owner, repo = infer_owner_repo()

    # Locate the open PR for the current branch and pull all its unresolved,
    # non-outdated review threads.
    pr = find_open_pr(owner, repo, branch)
    data = fetch_threads(owner, repo, pr["number"])
    threads = unresolved_current_threads(data)

    if not threads:
        print("No unresolved current review threads found.")
        return 0

    # Build the prompt describing the unresolved threads and save it to disk.
    prompt = build_prompt(data["pull_request"], threads)
    prompt_path = write_prompt_file(root, prompt, args.prompt_file)

    print(f"Found {len(threads)} unresolved review threads.")
    print(f"Prompt written to: {prompt_path}")

    if args.no_copilot:
        # Caller only wanted the prompt file, not an actual copilot run.
        return 0

    # Hand off to the copilot CLI to actually implement the fixes.
    rc = launch_copilot(root, prompt)
    if rc != 0:
        print("copilot exited with a non-zero code.")
    return rc


if __name__ == "__main__":
    raise SystemExit(main())