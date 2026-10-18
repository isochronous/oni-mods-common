### OM-005: overviews and the description, coverage and tag backfills over common
**Id:** blk-2fd7612e80b0

- **Avoid**: Writing an aggregator overview's paths relative to its own folder (`Release/release.py`): source-mention matches store-relative paths, so `tools/.context/overview.md` was unreached by every brief until review 1.1 rewrote them. Also deciding a whole-tool term per node during vocabulary checking: in-game-testing was dropped from TestRunner as a place while steam-workshop, translations and generated-worlds stayed elsewhere, and the review had to impose one rule afterwards.
- **Repeat**: Driving the six nodes through OM-002's tagwork driver retargeted at the common store (survey, chunks, begin/complete passes, check, write): 10 chunks validated and written in 10-17 rounds per node with no refusal. A review pass, a fix unit, and a confirmation review before closing.
- **Updated**: Decisions in OM-005's plan.md: whole-tool terms sit at document level on overview, domains/README, entities and glossary of each tool node; a term with one carrier is retired. Six friction entries in the ONI-Mods log.


### OM-004: populate-context over common's six tool nodes
**Id:** blk-a823ce0beb55

- **Avoid**: Drafting units writing claims from READMEs and from one example instead of the code: the review found 14 statements false against the code across 30 documents, among them usage errors said to skip SteamAPI.Init, a Reflect.cs-only rule that Worldgen.cs breaks, and testbed-base.sav presented as a tracked fixture. That last one is the same mistake OM-001's review caught in the ONI-Mods testing documents. A spec for documents about a test harness names the tracked fixtures (only testbed-so.sav is in the repository) up front.
- **Repeat**: Planning and applying every change-set with --at common, since the engine treats the submodule as its own store: 6 creates and the fix change-sets landed with no refusal. A review pass that checks every concrete claim against the code before anything is called done.
- **Updated**: Nothing promoted: the fixture fact is now stated in the TestRunner documents themselves and in the ONI-Mods testing documents.
