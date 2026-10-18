---
doc_id: doc-731b834a4b0c
description: "How ModCheck reports failures: exit codes 0, 1 and 2, the OK, FAIL and WARN output lines, per-type isolation and the ECall warning rule"
applies_to:
  - "Program.cs"
  - "ModCheck.csproj"
---
# Error Handling Conventions

## Principles
**Id:** blk-4aa5247ed7a4

1. **Report every break, not the first.** A check run lists every type and method that fails, so one run shows the whole damage after a game update. `Main` counts failures and warnings and keeps going.
2. **Name the member.** Each line names the stage, the type and, for a JIT failure, the method, with the exception type and message.
3. **Tell a real break from a host artifact.** The check host is CoreCLR with no Unity engine; the game is Mono with one. Only the failures that the game would also hit count.
4. **The exit code is the verdict.** Callers (`common/tools/TestRunner/Invoke-OniTests.ps1`) read the exit code, not the text.

## Exit Codes
**Id:** blk-615b71d8cae1

| Code | Meaning | Set in |
|------|---------|--------|
| 0 | every type loaded and every method compiled, warnings allowed | `Main`, `ALL CHECKS PASSED` |
| 1 | one or more failures | `Main`, `N FAILURE(S)` |
| 2 | wrong arguments, usage printed | `Main`, `args.Length < 2` |

The tool prints `usage: ModCheck <game Managed dir> <mod dll>` to standard output for exit code 2.

## Output Lines
**Id:** blk-635cd6eaebb7

Everything goes to standard output, one line per event, with a fixed prefix:

| Prefix | Meaning |
|--------|---------|
| `OK <type>` | the type stage passed (interface maps resolved, static constructor ran) and the method loop finished; `FAIL jit` lines for its methods print before it |
| `FAIL type <type>: ...` | type load, interface map or static constructor failure |
| `FAIL jit <type>.<method>: ...` | a method body could not be compiled, usually a missing method or field |
| `FAIL loader: <type> :: <message>` | one loader exception from a `ReflectionTypeLoadException` |
| `FAIL: <exception>` | any other exception while loading the assembly |
| `WARN <stage> <member>: ...` | a runtime-mismatch artifact, counted separately and not a failure |

## Handling Patterns
**Id:** blk-b80f483cffaf

### Per-type isolation
**Id:** blk-27d2629f8e78

The type stage and each method's JIT call are wrapped in their own `try`/`catch (Exception)` in `Main`. A failure goes to `Report`, the loop continues, and the type is not printed as `OK` when its type stage failed (`continue` after `Report`). The broad catch is the point of the tool: any exception from touching a type or method is a result to report.

### Loader exceptions
**Id:** blk-82f4670e9d62

`ReflectionTypeLoadException` from `asm.GetTypes()` is caught separately and every entry of `LoaderExceptions` becomes a `FAIL loader` line and a failure. Any other exception around the whole load becomes one `FAIL:` line with the full exception text.

### Host artifacts
**Id:** blk-00ca86cc07f8

`Report` treats an exception whose unwrapped message contains `ECall methods must be packaged into a system module` as a warning. That message means the host tried to run Unity native code, which works in the game. `MissingMethodException`, `MissingFieldException` and `TypeLoadException` are never downgraded. `BeginInvoke` and `EndInvoke` of delegates are not compiled at all, because their stubs throw `PlatformNotSupportedException` on CoreCLR and work on Mono. Abstract methods and methods with open generic parameters are skipped.

### Unwrapping
**Id:** blk-76c47a4d4fde

`Unwrap` follows `TypeInitializationException` to its inner exception, so a failing static constructor shows the cause and not the wrapper.

### Assembly resolution
**Id:** blk-534f0b7eade5

The `AssemblyResolve` handler looks for `<name>.dll` in the game's Managed folder and returns `null` when it is not there, which lets the runtime report the missing assembly in its own words.

## Anti-Patterns to Avoid
**Id:** blk-50d5a04f2f9a

- Stopping at the first failure; the count and the list are the output.
- Downgrading a failure to a warning without a runtime reason written next to the rule.
- Writing results to anything but standard output and the exit code; the callers capture both.

## Related
**Id:** blk-7f1a6895f34c

- See also: [code style](code-style.md)
