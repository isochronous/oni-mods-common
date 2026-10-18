---
description: "The TestRunner node tag dictionary: each term that records here may carry, with the one sentence saying what a record must be about to earn it"
---
# Tags

## code-style
**Id:** blk-7ea247828847

A record earns this term when it is about the house style in which a mod's C# source code is written.

## mod-build
**Id:** blk-ecdfee8844d1

A record earns this term when it is about compiling the mods with dotnet against the game's assemblies and deploying them into the game's mods folder.

## harmony-patching
**Id:** blk-1e0a7cf114fe

A record earns this term when it is about writing or applying the Harmony patches through which a mod changes game methods.

## mod-entry-point
**Id:** blk-37e4f2f01adb

A record earns this term when it is about a mod's UserMod2 class and what it does when the game loads the mod.

## logging
**Id:** blk-83e287939eea

A record earns this term when it is about what a mod writes to the game log and at which level.

## exception-handling
**Id:** blk-5c65f0ec885c

A record earns this term when it is about where a mod catches exceptions and what it does once it has caught one.

## reflection
**Id:** blk-f20167a43bad

A record earns this term when it is about reaching non-public members of the game or of another mod through reflection.

## command-line
**Id:** blk-1bf3858de65b

A record earns this term when it is about the arguments and options the tool is started with and how it reads them.

## activation-guard
**Id:** blk-87ae598d960d

A record earns this term when it is about the tool's mod staying inert unless the game is started with its command-line argument.

## game-launch
**Id:** blk-6579c5e93c5c

A record earns this term when it is about starting the game through Steam for a run and waiting for it to exit.

## run-result
**Id:** blk-1093bb5e7865

A record earns this term when it is about the result file a run writes as it ends and how the launcher reads it.

## watchdog
**Id:** blk-d91b4f718e9a

A record earns this term when it is about the real-time limits that end a stalled run or test.

## crash-dialog
**Id:** blk-af1a8512f81e

A record earns this term when it is about the game's crash dialog and keeping it from ending an unattended run.

## mod-loadout
**Id:** blk-49a840a8e864

A record earns this term when it is about which mods the game has enabled and putting the player's own mod list back afterwards.

## player-log
**Id:** blk-5d8154be30b6

A record earns this term when it is about reading the game's Player.log to find out what went wrong in a run.

## save-files
**Id:** blk-c678ff01cd47

A record earns this term when it is about Oxygen Not Included save files and what is loaded from or written to them.

## game-mode
**Id:** blk-0c45a5b9a6cd

A record earns this term when it is about whether the game runs in base-game or Spaced Out mode and making a run match it.

## game-update-compatibility
**Id:** blk-47f9b8cd428f

A record earns this term when it is about keeping mods and their tests working after a game update changes the game's own code.

## generated-worlds
**Id:** blk-d3d668a0774b

A record earns this term when it is about running the game's world generation from a coordinate to produce a fresh world.

## screenshots
**Id:** blk-ae0f965d4f31

A record earns this term when it is about photographing the game from a test run to produce images to look at.

## test-pacing
**Id:** blk-6601540e02e1

A record earns this term when it is about spacing a test's steps in game time so the game's simulation keeps up with them.

## test-cleanup
**Id:** blk-70722fbc7d67

A record earns this term when it is about how a test's area and changes are cleared or restored so tests do not affect each other.

## test-assertions
**Id:** blk-273d688de1e4

A record earns this term when it is about what a test checks and how it states the expected result.

## simulated-input
**Id:** blk-3e358b55e04c

A record earns this term when it is about driving the game's real tools and screens with mouse and key input fed by a test.

## live-session
**Id:** blk-adb355c58f3e

A record earns this term when it is about a game that stays up after a run and takes the next run with the same loadout from its queue.

## test-discovery
**Id:** blk-02aeab615370

A record earns this term when it is about how the harness finds the [OniTest] methods in the test assemblies and selects which of them run.
