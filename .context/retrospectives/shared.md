### OM-004: populate-context over common's six tool nodes
**Id:** blk-a823ce0beb55

- **Avoid**: Drafting units writing claims from READMEs and from one example instead of the code: the review found 14 statements false against the code across 30 documents, among them usage errors said to skip SteamAPI.Init, a Reflect.cs-only rule that Worldgen.cs breaks, and testbed-base.sav presented as a tracked fixture. That last one is the same mistake OM-001's review caught in the ONI-Mods testing documents. A spec for documents about a test harness names the tracked fixtures (only testbed-so.sav is in the repository) up front.
- **Repeat**: Planning and applying every change-set with --at common, since the engine treats the submodule as its own store: 6 creates and the fix change-sets landed with no refusal. A review pass that checks every concrete claim against the code before anything is called done.
- **Updated**: Nothing promoted: the fixture fact is now stated in the TestRunner documents themselves and in the ONI-Mods testing documents.
