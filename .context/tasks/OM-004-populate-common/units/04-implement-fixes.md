# Unit 04 — implement: fix change-sets for the unit 03 review

## Objective
Draft change-sets that correct every `fix` finding (F1–F14) in `units/03-review-report.md`, and remove the four soft "usually/typically/likely" phrases the review names (the owner forbids hedging). Leave the other nits.

## Decisions already made
- F10: the Spaced Out fixture `testbed-so.sav` is tracked in common; a base-game run needs `testbed-base.sav` copied in. Say exactly that; do not present the base save as present.
- F3: state the rule as the code has it: game internals are reached through `Reflect.cs`, with the two `AccessTools.Field` uses in `Worldgen.cs` named as the exception, or reword the rule so it does not claim exclusivity. Pick whichever matches the code.
- Every correction is checked against the code before it is written.

## Mechanics
- `section` ops (body excludes the heading and its `**Id:**` line; print the shape with `node c:/dev/ai/icon/bin/ks.mjs write schema --row narrative --mode section`). Merge edits to one section into one op.
- One change-set per node: `C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/changesets/fix-<tool-name>.json`, envelope `task: "OM-004"`, a `why`, `node` relative to common (`tools/<tool>`).
- From `C:/dev/ONI-Mods`: `node c:/dev/ai/icon/bin/ks.mjs write plan --changeset <file> --at common` until accepted. Never apply, never commit. Write JSON with a script (unique scratchpad subfolder) or the Write tool.
- Read `units/common-rules.md` for the change-set rules that still apply.

## Report
- File: C:/dev/ONI-Mods/common/.context/tasks/OM-004-populate-common/units/04-implement-fixes-report.md
- Must contain: per finding, the change-set and section that fixes it and the code evidence; plan results; anything left unresolved.
- Model tier: default
