# Unit 03 report: descriptions for 26 documents

## Result

Done. `work/desc-answers-03.json` has one description for each of the 26 listed documents. There are no nulls, and the store was not written.

Each document was read in full from disk. Descriptions are 28 to 34 words, one line each, all 26 distinct. Every tool document names its tool.

## Key count check (output)

```
parsed OK; keys: 26 expected: 26 missing: [] extra: []
distinct values: 26 nulls: 0
words min/max: 28 34 line breaks: 0
```

The expected key list was taken from the backticked `<node>::<path>` lines of `units/03-implement-descriptions.md` and compared against the answers file both ways.

## Nulls

None.

## Code names cited

Every file or identifier named in a description exists inside that document's own node. Files cited: `oni-mod.props`, `oni-mod.targets`, `ilrepack-plib.targets`, `src/Translations.cs`, `README.md`, `Kanim/kanim.py`, `Release/release.py`, `requirements.txt`, `Program.cs`, `ModCheck.csproj`, `Mod.cs`, `Runner.cs`, `Patches.cs`, `RunBehaviour.cs`, `Invoke-SaveRun.ps1`, `SaveRunner.csproj`, `Session.cs`, `TestBehaviour.cs`, `Kit/`, `fixtures/`, `Invoke-OniTests.ps1`, `Watch-OniGame.ps1`, `TestRunner.csproj`, `Reflect.cs`. All were checked with `ls`.

Two kinds of name were left out on purpose:
- Paths in another node, such as TestRunner's `Invoke-OniTests.ps1` in the ModCheck entities line. That line says "the script that calls it" instead.
- Runtime files that are not in the repo: `mods.json`, `probe.csv`, `results.jsonl`. Those lines say "loadout restore" and "result fields" instead.

## Rules found wrong or unclear

1. **The answers file format differs from the skill's.** The unit asks for a flat object keyed `<node>::<path>`. The description-backfill skill (step 2) wants the payload's `survey_id` copied verbatim as `survey`, with the answers under `answers`. That is what the step 3-5 command reads (`answers.answers`, `bindAnswers(survey, answers)`). The manager's merge has to wrap the flat objects and add `"survey": "sha256:a547a12545d99f6788a7942e8f623b9417fe7edfd04db4f5e58880b0784344c9"` (from `work/desc-input.json`). If it doesn't, the run refuses the answers.
2. **"Name the tool" conflicts with "code names inside the node".** This bites the `.` and `tools` overviews, and documents whose content is about a caller in another node. In the ModCheck entities document, the caller is TestRunner's launcher. The rule doesn't say whether a child node's folder counts as "inside" the parent node. I treated child-node paths as outside: the root overview does not name `tools/`, and the tools overview does not name the six child-node folders as paths. Plain tool words such as "ModCheck" in a ModCheck document were used freely.
3. **The rule doesn't say whether runtime file names count as code names.** Examples are output or game files such as `mods.json`, `probe.csv` and `results.jsonl`. A coverage pass reading them would find nothing. I left them out, but the rule should say so.
4. **Index documents can't avoid matching their own text.** The `domains/README.md` and `standards/README.md` documents hold nothing but a link list with per-link summaries. A true "what it contains" line has to overlap that list. The three tools' index lines share an opening, which the skill allows because the files are the same kind of thing.

## Friction met

None with ICON (`ks`, skills, hooks). `ks` wasn't needed: the skill requires reading the whole document, so the documents were read from disk as the unit says.

## Brief feedback (handoff section 2)

- **Pointers that didn't apply.** All the StringsTemplate, WorkshopUpload and WorldgenDriver pointers didn't apply. Each matched only because the `.` and `tools` scope contains their paths ("names ... → ."). None of them governs writing descriptions for these 26 documents. The ModCheck pointers matched four of the documents being described, so they were inputs rather than governing records.
- **What was missing.** Within the 8 KB budget, no SaveRunner or TestRunner record was listed, though 16 of the 26 documents are theirs. The rules that actually govern this work come from the description-backfill skill, which is not a store record, so its absence from the brief is expected.
- **Reason text that misleads.** The ModCheck pointers' reasons read "Program.cs, ModCheck.csproj → .". The `applies_to` globs are relative to `tools/ModCheck`, but the line looks like it names a `Program.cs` at the root of common. `blk-3ca89d57dfde`'s reason (it names `tools/TestRunner/Invoke-OniTests.ps1`) is accurate: it is the cross-node mention described in rule 2 above.
