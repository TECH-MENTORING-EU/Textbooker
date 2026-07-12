#!/usr/bin/env python3
"""
Version 2:
- checks required components before doing anything useful
- works on the currently checked-out branch
- finds the open PR for that branch
- fetches unresolved current review threads
- writes the prompt to a file
- launches standalone `copilot` CLI in autopilot mode
- avoids passing a huge prompt on the command line
- does not create a commit
- does not ask the user questions

Requires (minimum supported versions):
- Python 3.13.7
- git 2.53.0.windows3
- gh 2.96.0 (2026-07-02)
- GitHub Copilot CLI 1.0.70

Windows:
  python .\apply_review_comments_agent_v2.py
"""

from __future__ import annotations

import argparse
import json
import shutil
import subprocess
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
    # Decode as UTF-8 explicitly so GitHub CLI JSON/text output does not go
    # through the Windows ANSI code page, which can fail on characters such as
    # smart quotes.
    # Callers inspect the returncode themselves (see run_text below).
    return subprocess.run(
        args,
        cwd=str(cwd) if cwd else None,
        check=False,
        capture_output=True,
        text=True,
        encoding="utf-8",
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


def check_required_components() -> bool:
    # Returns whether the installed copilot CLI supports --prompt-file, so the
    # caller can decide how to hand off the prompt in launch_copilot.
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
    required_flags = ["--mode", "--no-ask-user", "--allow-all"]
    missing_flags = [flag for flag in required_flags if flag not in help_text]
    supports_prompt_file = "--prompt-file" in help_text
    # Either --prompt or --prompt-file must be supported to hand off the prompt.
    if not supports_prompt_file and "--prompt" not in help_text:
        missing_flags.append("--prompt/--prompt-file")
    if missing_flags:
        print("Your copilot CLI does not expose the required flags for this script.")
        print(f"Expected: {', '.join(required_flags)} and --prompt or --prompt-file")
        raise SystemExit(1)

    print("Required components found:")
    print(" - git")
    print(" - gh")
    print(" - copilot")
    print(" - GitHub auth OK")
    print(" - copilot flags OK")

    return supports_prompt_file


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


def launch_copilot(root: Path, prompt: str, prompt_path: Path, supports_prompt_file: bool) -> int:
    # Reuse the prompt file already written to disk (see write_prompt_file) so
    # we don't create a second temp file. Prefer --prompt-file when the
    # installed copilot CLI supports it (avoids huge CLI arguments); otherwise
    # fall back to passing the prompt text via --prompt.
    cmd = [
        "copilot",
        "--mode",
        "autopilot",
        "--no-ask-user",
        "--allow-all",
    ]
    if supports_prompt_file:
        cmd.extend(["--prompt-file", str(prompt_path)])
    else:
        cmd.extend(["--prompt", prompt])

    result = subprocess.run(cmd, cwd=str(root), check=False)
    return result.returncode


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--repo", help="OWNER/REPO. Defaults to inferred repo.")
    parser.add_argument("--prompt-file", default="copilot-agent-prompt.txt")
    parser.add_argument("--no-copilot", action="store_true")
    args = parser.parse_args()

    # Fail fast if git/gh/copilot aren't installed, gh isn't authenticated, or
    # the installed copilot CLI lacks the flags this script relies on.
    supports_prompt_file = check_required_components()

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
    rc = launch_copilot(root, prompt, prompt_path, supports_prompt_file)
    if rc != 0:
        print("copilot exited with a non-zero code.")
    return rc


if __name__ == "__main__":
    raise SystemExit(main())