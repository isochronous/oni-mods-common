# Unit 08 — review: confirm the unit 06 findings are fixed

## Objective
Confirm that every finding of `units/06-review-report.md` taken into scope is fixed in the common store, and that the fixes broke nothing.

## Scope (read only)
- `C:/dev/ONI-Mods/common`: `git diff f1cb73e..HEAD -- . ':!.context/tasks'`, plus `units/06-review-report.md`, `units/07-implement-review-fixes-report.md` and `plan.md` (its Decisions record what was left on purpose: 3.2, 3.3, 4.2, 4.11, and the whole-tool term rule).
- For each of 1.1, 2.1, 3.1, 4.1, 4.3, 4.4, 4.5, 4.6, 4.7 and nits 1.2–1.8, 2.2–2.4, 4.8–4.10: resolved, partly resolved, or not, with evidence. Check every new fact in a changed overview, README or description against the code.
- Check the fixes broke nothing: every link in the common store resolves; no section repeats a tag its document or an enclosing heading carries; every term in each `tags.md` has at least one carrier (count `**Tags:**` lines and frontmatter `tags:` yourself; `ks tags` counts records matched through a shared word); `ks check --all --strict --store C:/dev/ONI-Mods/common` is 0 block; `ks brief` over the eight nodes no longer lists `tools:overview.md` as unreached.
- Manager change after unit 07: `mod-build` taken off WorldgenDriver's code-style "Language and Toolchain" and retired from its tags.md (one carrier left after 4.3).
- OUT: fixing anything; writing the store.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-005-common-backfills/units/08-review-fixes-report.md
- Must contain: a line per finding with its state and evidence; any new finding with severity (block / fix / nit), location and expected fix; friction met.
- Model tier: default
