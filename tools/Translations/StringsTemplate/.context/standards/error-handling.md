---
doc_id: doc-c4b68e21b1de
description: "StringsTemplate (the translation template writer and checker): exit codes 0/1/2, the FAIL, MISSING, STALE, REFUSED, EMPTY and OBSOLETE report lines, and the one FormatException catch"
applies_to:
  - "*.cs"
  - "*.csproj"
---
# StringsTemplate error handling

StringsTemplate is a console checker run by a script, so its error handling is its output and its exit code. A problem is a line on the console and a count that decides the exit code.

## Principles
**Id:** blk-be2fa051ef59

1. **Report every problem, then fail.** `CheckPo` and `CheckOptionKeys` print each finding and keep going; `Main` fails after the last check, so one run shows the whole list.
2. **Name the key.** Every line carries the translation key (`STRINGS.A.B.C`) and the po file it concerns.
3. **Mirror the game where a check stands for it.** The parse `FAIL` says what the game's reader does ("the game's reader throws here and drops the whole file"), and `REFUSED` names the rule the game enforces on the English `{n}` parameters and `<link>` tags. `MISSING`, `STALE`, `EMPTY` and `OBSOLETE` are bookkeeping between the po and the DLL, not game refusals.
4. **Do not swallow.** The one `catch` in the tool turns a malformed po into a reported failure; nothing is caught and ignored.

## Exit codes
**Id:** blk-4943aa3d2f64

| Code | Meaning | Where |
|------|---------|-------|
| 0 | `ALL CHECKS PASSED` | end of `Main` |
| 1 | `N PROBLEM(S)`, or `FAIL` for a DLL with no `ModStrings` class or no `LocString` fields | `Main` |
| 2 | fewer than three arguments; the usage line is printed | start of `Main` |

An exception that nothing catches (a missing DLL path, a game assembly that cannot load) ends the process with the .NET unhandled-exception output and a non-zero code. TODO(owner): decide whether bad paths should print a one-line `FAIL` and return 1, as the `ModStrings` check does.

## Report keywords
**Id:** blk-4e29c6e83e7b

All messages go to standard output through `Console.WriteLine`, each starting with an upper-case keyword:

| Keyword | Meaning | Counts toward exit code 1 |
|---------|---------|---------------------------|
| `FAIL` | structural failure: no `ModStrings`, no strings, an option key not in the tree, a po that does not parse | yes |
| `MISSING` | key is in the DLL, not in the po | yes |
| `STALE` | the po's `msgid` differs from the English now in the DLL (both texts are printed) | yes |
| `REFUSED` | the translation changes the set of `{...}` parameters, or has more `<link=` tags than the English or unmatched `</link>` | yes |
| `EMPTY` | the po has the key with an empty `msgstr` | no |
| `OBSOLETE` | the po has a key the DLL no longer has | no |

Each po ends with a one-line tally (`<file>: n/total translated, ...`).

## Handling Patterns
**Id:** blk-2ced13ba9b17

### Reading a po file
**Id:** blk-f76eaa17fcbe

`ReadPo` and `Quoted` throw `FormatException` for a quoted line the game's reader would choke on. `CheckPo` is the only catcher: it prints `FAIL <message>` and returns 1 for that file, and the other po files are still checked.

```csharp
catch (FormatException e)
{
	Console.WriteLine("FAIL " + e.Message);
	return 1;
}
```

The message names the file and line number.

### Loading the mod
**Id:** blk-d1067c01b2e8

`AssemblyResolve` looks in the game's Managed folder and returns null when the file is not there. Missing `ModStrings` or an empty tree is a `FAIL` with return 1 before any file is written.

### Option keys
**Id:** blk-ccfb13edda27

`CheckOptionKeys` reads PLib `[Option]` and `[ConfigFile]` attributes by name (no PLib reference) and reports a `Title`, `Tooltip` or `Category` starting with `STRINGS.` that is not a key in the tree.

## Anti-Patterns to Avoid
**Id:** blk-8314ecaf5051

- Printing a problem without counting it, or counting without printing: `EMPTY` and `OBSOLETE` are printed and not counted on purpose; keep that split explicit.
- Passing a bad po silently. The game drops the whole file on a bad line, so the tool reports it.
- Relying on the exit code to say more than pass or fail: `translations.py` runs the tool with its output passed through to the console and counts any non-zero exit as one problem, so it does not tell 1 from 2. Keep the output readable for a person and the exit code non-zero for any problem.

## Related
**Id:** blk-e6e4e7300510

- See also: [code style](code-style.md), [entities](../domains/entities.md)
