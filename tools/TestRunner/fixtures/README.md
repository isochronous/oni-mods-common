# Fixture saves

The launcher loads `testbed-so.sav` when the game is in Spaced Out mode and `testbed-base.sav`
otherwise. Any save works as a fixture as long as it loads in the current mode; the harness picks
the test area itself (the 40x10 window of the active world that is fully revealed and buildable
with the most empty cells, caves preferred over space) and clears it before the first test and after
every test, so the save's contents only matter for what sits next to that window.

What makes a good fixture: sandbox enabled, no Duplicants (nothing wanders into the area, nothing
suffocates), a revealed map, and a small file. A fresh sandbox colony whose Duplicants were deleted
with the sandbox tool is ideal; `Clean Map.sav` from the user's `Mod Test` colony (cycle 0, no
Duplicants, Spaced Out with every DLC) is the current `testbed-so.sav`.

The `.sav` files are not committed (see `.gitignore`); copy one in and the launcher finds it. The
harness logs the area it chose as `[TestRunner] fixture area 40x10 at (x, y)` in `Player.log`.
