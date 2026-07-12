#!/usr/bin/env python3
"""Fetch thread-aware review data for one or all open GitHub pull requests."""

from __future__ import annotations

import argparse
import json
import subprocess


QUERY = r"""
query($owner:String!,$repo:String!,$number:Int!,$cursor:String){
  repository(owner:$owner,name:$repo){
    pullRequest(number:$number){
      number title url state headRefName baseRefName reviewDecision mergeable
      reviewThreads(first:100,after:$cursor){
        pageInfo{hasNextPage endCursor}
        nodes{
          id isResolved isOutdated path line originalLine resolvedBy{login}
          comments(first:100){nodes{id author{login} body createdAt updatedAt}}
        }
      }
    }
  }
}
"""


def run(*args: str) -> str:
    result = subprocess.run(args, check=False, capture_output=True, text=True)
    if result.returncode:
        raise SystemExit(result.stderr.strip() or "gh command failed")
    return result.stdout


def fetch(owner: str, repo: str, number: int) -> dict:
    cursor: str | None = None
    pull_request = None
    threads: list[dict] = []
    while True:
        command = [
            "gh", "api", "graphql", "-f", f"query={QUERY}",
            "-F", f"owner={owner}", "-F", f"repo={repo}", "-F", f"number={number}",
        ]
        if cursor:
            command.extend(["-F", f"cursor={cursor}"])
        payload = json.loads(run(*command))["data"]["repository"]["pullRequest"]
        if payload is None:
            raise SystemExit(f"pull request #{number} was not found")
        page = payload.pop("reviewThreads")
        pull_request = pull_request or payload
        threads.extend(page["nodes"])
        if not page["pageInfo"]["hasNextPage"]:
            break
        cursor = page["pageInfo"]["endCursor"]
    return {"pull_request": pull_request, "review_threads": threads}


def summarize(result: dict) -> dict:
    threads = result["review_threads"]
    current = [thread for thread in threads if not thread["isResolved"] and not thread["isOutdated"]]
    outdated = [thread for thread in threads if not thread["isResolved"] and thread["isOutdated"]]
    return {
        **result["pull_request"],
        "threads": {
            "unresolved_current": len(current),
            "unresolved_outdated": len(outdated),
            "resolved": sum(thread["isResolved"] for thread in threads),
        },
        "current": [
            {
                "id": thread["id"], "path": thread["path"], "line": thread["line"],
                "authors": [comment["author"]["login"] for comment in thread["comments"]["nodes"]],
                "body": thread["comments"]["nodes"][0]["body"],
            }
            for thread in current
        ],
    }


def main() -> None:
    parser = argparse.ArgumentParser()
    parser.add_argument("pr", type=int, nargs="?")
    parser.add_argument("--all-open", action="store_true")
    parser.add_argument("--summary", action="store_true")
    parser.add_argument("--owner")
    parser.add_argument("--repo")
    args = parser.parse_args()
    if bool(args.pr) == args.all_open:
        parser.error("provide one PR number or --all-open")

    if not args.owner or not args.repo:
        repository = json.loads(run("gh", "repo", "view", "--json", "owner,name"))
        args.owner = args.owner or repository["owner"]["login"]
        args.repo = args.repo or repository["name"]

    numbers = [args.pr] if args.pr else [
        item["number"] for item in json.loads(run("gh", "pr", "list", "--state", "open", "--limit", "100", "--json", "number"))
    ]
    results = [fetch(args.owner, args.repo, number) for number in numbers]
    output = [summarize(result) for result in results] if args.summary else results
    print(json.dumps(output[0] if args.pr else output, ensure_ascii=False, indent=2))


if __name__ == "__main__":
    main()
