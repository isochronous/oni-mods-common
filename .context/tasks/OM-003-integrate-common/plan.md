## Task: OM-003
## Branch: OM-003-integrate-common (in the oni-mods-common submodule)
## Objective: Run ICON's init process over common/ so its tool projects become nodes of its store, as icon-init would have scaffolded them.
## Folder: .context/tasks/OM-003-integrate-common/

## Phase State
- Phase plan: [implementation, completion]
- implementation: done
- completion: done

## Decisions
- `ks init --at common` refused (a store already exists at common/) and routed to `ks integrate --at common`; integrate is the door for wiring unwired projects into an existing tree.
- Per-clone runtime settings (--clone-local) not taken: one would disable Claude auto memory, which this project relies on.
- Content (populate-context, backfills) is out of this task; init's job ends at structure and TODO(owner) placeholders.

## Progress
- [x] `ks init --at common`: refused, routed to integrate
- [x] `ks integrate --at common` plan: tools aggregator + 6 leaves (ModCheck, SaveRunner, TestRunner, Translations/StringsTemplate, WorkshopUpload, WorldgenDriver); tools/Translations passes through ← reviewed
- [x] Applied: 8 nodes (common, tools, 6 tool leaves); resolver chains tools/<tool> > tools > common
- [x] Engine defect fixed: integrate relabelled common's ModCheck roster row to point at `tools` and left five rows aimed past the new aggregator; repaired projects.md to the one row `tools` (protected repair, kind correction, changesets/roster.json)
- [x] `ks check --store common`: 0 block, 13 advise (12 unlinked scaffold READMEs; META.md declares aggregator while the engine reads common as a root, which matches common being a child of ONI-Mods)
- [x] Commit in the submodule; bump the pointer in the root

## Open Questions / Blockers
- The six new leaves and tools/ carry TODO(owner) placeholders; populate-context and the backfills are the next step.
- The root store still cannot see common/ as a node (resolver splits submodules into their own store; recorded in the root's tooling-friction log).

## Constraints
- Commit messages: one terse line.
