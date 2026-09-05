---
name: textbooker-pr-review
description: Inspect and address GitHub review feedback for Textbooker, including unresolved review threads, Copilot and human comments, stacked PR dependencies, per-branch worktrees, CI checks, pushes, and merge-readiness reporting. Use when asked to fix PR comments, audit open PRs, or identify which Textbooker PRs are ready to merge.
---

# Textbooker PR Review

Use `gh` for thread-aware GitHub data. Flat review-comment lists do not expose reliable resolution and outdated state.

## Safety

- Confirm `gh auth status` and repository remotes first.
- Read `git status --short --branch` before switching branches or building.
- Preserve user changes and untracked files. Never clean or reset them.
- For multiple PR branches, create separate worktrees outside the main checkout.
- Do not reply, resolve threads, submit reviews, merge, or delete branches unless the user explicitly requests that GitHub write.
- A requested implementation normally authorizes committing and pushing the fixes to the affected PR branches; state what will be pushed.

## Inventory

1. List open PRs with number, head, base, review decision, mergeability, and checks.
2. Fetch `reviewThreads` with `isResolved`, `isOutdated`, path, line, comments, and authors. Use `scripts/fetch_review_threads.py <pr>` for one PR or `--all-open --summary` for a compact inventory.
3. Fetch review bodies and top-level conversation comments when reviewers may have left summary instructions.
4. Group findings by PR, reviewer, file, and behavior.
5. Separate:
   - actionable and current;
   - already fixed but formally unresolved;
   - outdated or superseded;
   - informational/approval comments;
   - ambiguous or potentially regressive suggestions.

Never infer that an unresolved thread means the code is still wrong. Verify the current head.

Treat “nice”, questions already followed by approval, and syntax preferences that reduce correctness as informational. Conversely, an approval does not override a reproducible security or correctness defect.

## Stacked PRs

Textbooker may use PRs whose bases are other feature branches. Build the dependency chain from each PR's `baseRefName` and `headRefName`.

- Compare each PR against its declared base.
- Apply a fix to the branch where the reviewed code belongs.
- Do not assume a fix pushed to a lower stack branch is already contained in descendant heads.
- Validate each changed head independently, then report the required merge order.
- Re-query mergeability after pushing base-branch changes; GitHub may briefly return `UNKNOWN` while recomputing.
- Distinguish required checks from checks that merely ran. Inspect branch protection when that distinction affects readiness.
- “Clean” means GitHub can merge the current head into its current base. It does not mean the whole stack can merge into `main` in arbitrary order.
- Keep PRs reviewable, generally below about 400 changed lines when the work has natural dependency boundaries. Split by behavior, not by arbitrary file count.
- Rewrite branch history or reorder a stack only with explicit permission. After rewriting, verify every PR base/head pair and force-push only the intended branches.

## Isolated implementation

Create one worktree per affected branch:

```powershell
git worktree add F:\projects\Textbooker-worktrees\<safe-name> <branch>
```

Use existing worktrees when present. Keep each commit traceable to one PR or review cluster. Stage explicit project files; do not stage generated databases, test results, plans, local settings, or unrelated changes.

For code-review rules, also use `textbooker-review`.

## Verification and publishing

For every changed branch:

1. Run `git diff --check`.
2. Build or test using commands supported by that branch.
3. Run Playwright when browser behavior or accessibility tests changed.
4. Inspect `git status --short` before committing.
5. Commit with a focused message.
6. Push the exact checked-out branch. From a worktree, use `git push origin HEAD:<branch>` when branch-name/path transformations could cause mistakes.

After pushes, re-fetch PR metadata and checks. Do not report “all checks passed” when most PRs have no checks; say exactly which workflows ran and which PRs have none.

## Merge-readiness report

Classify PRs explicitly:

- **Ready now:** mergeable/clean, required checks successful, required review approved.
- **Technically ready, awaiting review:** clean and verified, but no approval decision.
- **Blocked:** conflicts, failed/pending required checks, requested changes, or unresolved code defect.
- **Informational:** no required checks configured; local validation is evidence, not a substitute for CI.

Include the stack merge order. Distinguish “code addressed” from “thread marked resolved.” List intentionally unmodified comments and why.
