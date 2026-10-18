# Unit 05 report: tag vocabularies for common's six tool nodes

Output: `work/tags/vocab-tools.json`, with all six nodes as keys. It parses. A check script tested every term against the unit's rules:
- every evidence id exists in that node's lexicon
- every term has at least 3 records across at least 2 documents in evidence
- `est_documents` equals the number of distinct documents in its evidence
- every gloss is one sentence starting with the required words
- every term is lower-case and hyphenated, with no commas

Result: 0 problems. I read all 48 documents in full from disk, not a sample, so the evidence lists are drawn from the bodies.

## Term counts per node

| Node | Records (docs + sections) | Terms | Widest term (est. reach) |
|------|---------------------------|-------|--------------------------|
| tools/ModCheck | 51 | 11 | code-style, 9 (18%) |
| tools/SaveRunner | 61 | 16 | code-style, 9 (15%) |
| tools/TestRunner | 76 | 26 | code-style / run-result / live-session, 9 (12%) |
| tools/Translations/StringsTemplate | 68 | 12 | translations, 18 (26%) |
| tools/WorkshopUpload | 66 | 16 | steam-workshop, 20 (30%) |
| tools/WorldgenDriver | 69 | 17 | generated-worlds, 22 (32%) |

No term is anywhere near the 85% bar.

**Shared core (spelled and glossed the same wherever admitted):**
- code-style: all 6
- command-line: all 6
- exception-handling: all 6
- tool-build: ModCheck, StringsTemplate, WorkshopUpload
- exit-codes: ModCheck, StringsTemplate, WorkshopUpload
- console-output: ModCheck, StringsTemplate, WorkshopUpload
- assembly-resolution: ModCheck, StringsTemplate
- mod-build, harmony-patching, mod-entry-point, logging, activation-guard, game-launch, run-result, watchdog, mod-loadout, player-log: SaveRunner, TestRunner, WorldgenDriver
- crash-dialog: SaveRunner, TestRunner
- save-files: SaveRunner, TestRunner
- game-mode: TestRunner, WorldgenDriver
- generated-worlds: TestRunner, WorldgenDriver
- game-update-compatibility: ModCheck, TestRunner

**Each tool's own terms:**
- ModCheck: type-load-check, jit-check, runtime-mismatch
- SaveRunner: probe
- TestRunner: reflection, screenshots, test-pacing, test-cleanup, test-assertions, simulated-input, live-session, test-discovery
- StringsTemplate: translations, mod-strings, translation-template, refused-translation, mod-options
- WorkshopUpload: steam-workshop, release-process, legacy-workshop-item, steam-app-id, steam-cloud, steamworks-api, workshop-tags, item-visibility, item-description, item-download
- WorldgenDriver: world-coordinate, worldgen-progress

## Terms that reuse an ONI-Mods spelling

**Word for word from the root dictionary:**
- in-game-testing family: test-pacing, test-cleanup, test-assertions, simulated-input
- generated-worlds, save-files, screenshots, mod-loadout, player-log
- game-update-compatibility, mod-build, translations, steam-workshop, release-process

**Word for word from the mod dictionaries:**
- harmony-patching, mod-entry-point, logging (in the three tools that are mods)
- reflection, mod-strings, mod-options

**Spelling reused, gloss adapted** ("a mod's" became "the tool's", "a mod" became "the tool"), in ModCheck, StringsTemplate and WorkshopUpload only:
- code-style
- exception-handling

SaveRunner, TestRunner and WorldgenDriver are mods, so they carry these two glosses word for word.

**New spellings** (no ONI-Mods term exists for the concept): tool-build, command-line, exit-codes, console-output, assembly-resolution, activation-guard, game-launch, run-result, watchdog, crash-dialog, game-mode, plus the tool-specific terms listed above apart from the reused ones.

## Closest calls dropped

- **in-game-testing in TestRunner.** Nearly every TestRunner record is about it, and in that node it means "records about this tool", which is a place, not a subject. The eight test-* and test-subject terms do the sorting there.
- **refrigerator in SaveRunner** (reuse from the mod dictionaries). Its records are almost the same set as `probe`'s, because the fridge probe is the only probe. `probe` is the lasting concept.
- **game-time in SaveRunner** (run length in game seconds and speed). It has 5 records, but command-line, watchdog and probe already split them.
- **game-mode in SaveRunner.** It has 3 thin records (the kplayerprefs row, one business rule, the SO abbreviation). It is admitted in TestRunner and WorldgenDriver.
- **exit-codes in TestRunner.** Only `launcher-exit-codes` is really about it; the other records are one clause or an index line.
- **test-kit in TestRunner.** It overlaps test-assertions and simulated-input, and it reads as the `Kit/` folder.
- **fixture in TestRunner.** Its records fall inside save-files and test-cleanup.
- **assembly-resolution in TestRunner.** Three passing mentions of the test-assembly resolver.
- **run-output** (the run folder) in the three tools that are mods. It overlaps run-result and player-log.
- **mod-zip in WorkshopUpload.** Covered by legacy-workshop-item.
- **stories-and-mixing in WorldgenDriver.** Folded into world-coordinate, whose gloss covers the settings the coordinate carries.
- **translation-keys in StringsTemplate.** Inside mod-strings.
- **Below the floor in every node:** mod-compatibility, vanilla-fallback, machine-setup.
- **Kept although broad:** steam-workshop (WorkshopUpload, about 30%) and generated-worlds (WorldgenDriver, about 32%). Both stay well under the bar, and without them a graph-wide `--tag` query for those subjects would miss the node that owns them.

**TestRunner is over the expected 6–15.** Each of its 26 terms meets the floor and sorts a distinct set. If the manager wants it in range, these are the cheapest to cut, weakest first: crash-dialog, screenshots, player-log, game-mode, mod-build, exception-handling.

## Rules I found wrong

1. **The identical-gloss rule and the "adapt where the gloss names a mod" rule contradict each other inside common.** Three tools are mods and three are not, so code-style and exception-handling each end up with two glosses under one spelling. I followed the adaptation rule as written. The alternative is one gloss for all six common nodes ("the tool's") that differs from the ONI-Mods wording. Either way, one of the two rules gives way. The manager should pick.
2. **The 6–15 range ignores node size and the floor.** TestRunner (76 records) has 26 terms that each pass the floor. A range that the floor itself can overrun is not a rule.
3. **The required reuse brings a near-synonym pair into StringsTemplate.** The mods' `mod-strings` gloss ends "and the translations of that text", which overlaps the root's `translations`. That conflicts with "no near-synonyms of an existing term". I admitted both because their populations differ in practice (the ModStrings tree and keys, versus checking po and pot files). The real fix is to drop that trailing clause from `mod-strings` in the ONI-Mods dictionaries.
4. **The handoff's cross-node reuse rule pulls against the tag-backfill skill.** The skill says a member node's proposing call "sees only its own vocabulary, and never a parent's terms". The handoff, and tag-discipline's advice to read the neighbouring `tags.md` files, require the opposite. I followed the handoff.

## Brief feedback (section 2 of the handoff)

- **Pointers that applied:** the listed entities, code-style, error-handling and naming documents were all among the documents to read.
- **Pointers that did not apply:** none.
- **What was missing:** the brief answered the path question only, so the records this work turns on (the ONI-Mods `tags.md` dictionaries and the task's own lexicon files) were not in it. The handoff's scope named them, so nothing was lost.
- **Pointer reasons:** each one-line reason matched its record.
- **The warning about 21 uncovered records was accurate:** the README indexes and glossaries carry no coverage. They are in this vocabulary's scope.

## Friction

No ICON friction met; this unit invoked no `ks` command. One mistake of my own: I guessed two record ids wrong (the Files tables are nested under `key-entities/` in the lexicons). The check script caught both and they are fixed.
