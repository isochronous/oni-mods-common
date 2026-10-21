<#
.SYNOPSIS
  Puts back the mods' shared config folders a test run backed up and did not restore.

.DESCRIPTION
  Dot-sourced by Invoke-OniTests.ps1 and Watch-OniGame.ps1. The game backs up mods\config\NAME of
  each mod under test when a run starts and puts it back when the run ends (ModConfig.cs); a game
  that died mid-run leaves the backup in %LOCALAPPDATA%\IsochronousOniTests\config-backup\NAME
  (NAME\files the folder as it was, NAME\absent when there was none). Restore-ModConfigBackup puts
  every such backup back and removes it; NAME.tmp, a backup the game did not finish, is only removed
  (the player's folder was not yet touched). The config-backup folder goes too once it is empty.
#>

$ModConfigBackupRoot = Join-Path $env:LOCALAPPDATA 'IsochronousOniTests\config-backup'

function Restore-ModConfigBackup([string]$ConfigRoot) {
    $restored = @()
    if (-not (Test-Path -LiteralPath $ModConfigBackupRoot)) { return $restored }
    foreach ($kept in @(Get-ChildItem -LiteralPath $ModConfigBackupRoot -Directory)) {
        if ($kept.Name.EndsWith('.tmp')) { Remove-Item -LiteralPath $kept.FullName -Recurse -Force; continue }
        $live = Join-Path $ConfigRoot $kept.Name
        if (Test-Path -LiteralPath $live) { Remove-Item -LiteralPath $live -Recurse -Force }
        $files = Join-Path $kept.FullName 'files'
        if (Test-Path -LiteralPath $files) {
            New-Item -ItemType Directory -Force $ConfigRoot | Out-Null
            Copy-Item -LiteralPath $files -Destination $live -Recurse -Force
        }
        Remove-Item -LiteralPath $kept.FullName -Recurse -Force
        $restored += $kept.Name
    }
    # An empty backup folder is left by every run the game finished; nothing is kept in it then.
    if (-not (Get-ChildItem -LiteralPath $ModConfigBackupRoot -Force | Select-Object -First 1)) {
        Remove-Item -LiteralPath $ModConfigBackupRoot -Force -ErrorAction SilentlyContinue
    }
    return $restored
}
