# The game's mode lives in kplayerprefs.yaml as "EXPANSION1_ID.ENABLED: 1" (Spaced Out) or 0, or no
# line at all (base game). A run that needs the other mode (Invoke-OniTests.ps1 -Mode) writes the
# player's value to kplayerprefs.yaml.onitest-mode first, switches the line, and the player's value
# goes back once the game has exited: by the warden (Watch-OniGame.ps1), by the launcher when it has
# no warden, or by the next launcher when a run died before either could.

# The file has CRLF line ends, and .NET's multiline $ stops only before \n.
$GameModeLine = '(?m)^([ \t]*EXPANSION1_ID\.ENABLED:[ \t]*)(\d+)(?=[ \t]*\r?$)'

function Get-GameModeValue([string]$Prefs) {
    # '1', '0', or 'none' when the line is missing.
    $text = [IO.File]::ReadAllText($Prefs)
    $m = [regex]::Match($text, $GameModeLine)
    if ($m.Success) { return $m.Groups[2].Value }
    return 'none'
}

function Set-GameModeValue([string]$Prefs, [string]$Value) {
    $text = [IO.File]::ReadAllText($Prefs)
    $newline = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
    if ($Value -eq 'none') {
        $text = [regex]::Replace($text, '(?m)^[ \t]*EXPANSION1_ID\.ENABLED:[^\r\n]*(\r?\n)?', '')
    } elseif ([regex]::IsMatch($text, $GameModeLine)) {
        $text = [regex]::Replace($text, $GameModeLine, '${1}' + $Value)
    } else {
        $m = [regex]::Match($text, '(?m)^ints:[ \t]*\r?\n')
        if (-not $m.Success) { throw "$Prefs has no ints: section to hold EXPANSION1_ID.ENABLED" }
        $text = $text.Insert($m.Index + $m.Length, "  EXPANSION1_ID.ENABLED: $Value$newline")
    }
    [IO.File]::WriteAllText($Prefs, $text, (New-Object System.Text.UTF8Encoding($false)))
}

# The mode the player chose: the kept value while a run has switched it, else the file's.
function Get-PlayerSpacedOut([string]$Prefs) {
    $kept = "$Prefs.onitest-mode"
    $value = if (Test-Path -LiteralPath $kept) { (Get-Content -LiteralPath $kept -Raw).Trim() } else { Get-GameModeValue $Prefs }
    return $value -eq '1'
}

# Switches the game to the mode wanted, keeping the player's value unless one is kept already.
# Returns true when the file was changed.
function Switch-GameMode([string]$Prefs, [bool]$SpacedOut) {
    $want = if ($SpacedOut) { '1' } else { '0' }
    $now = Get-GameModeValue $Prefs
    if (($now -eq '1') -eq $SpacedOut) { return $false }
    $kept = "$Prefs.onitest-mode"
    if (-not (Test-Path -LiteralPath $kept)) { [IO.File]::WriteAllText($kept, $now) }
    Set-GameModeValue $Prefs $want
    return $true
}

# Puts the player's mode back if a run switched it. Returns the value restored, or nothing.
function Restore-GameMode([string]$Prefs) {
    $kept = "$Prefs.onitest-mode"
    if (-not (Test-Path -LiteralPath $kept)) { return }
    $value = (Get-Content -LiteralPath $kept -Raw).Trim()
    Set-GameModeValue $Prefs $value
    if ((Get-GameModeValue $Prefs) -ne $value) { throw "kplayerprefs.yaml does not hold EXPANSION1_ID.ENABLED $value after the restore; the kept value is left at $kept" }
    Remove-Item -LiteralPath $kept -Force
    return $value
}

# --- the last-save pointer ---------------------------------------------------------------------
# The game writes "SaveFilenameKey/: <path>" under strings: in kplayerprefs.yaml every time it saves or
# loads a game, so a run leaves it pointing at the run's last test save. The value the player had is
# kept in kplayerprefs.yaml.onitest-savekey ('absent', or 'present:' and the text after the colon,
# exactly) and put back at the same places as the game's mode.
$SaveKeyLine = '(?m)^([ \t]*SaveFilenameKey/:)([^\r\n]*)'

function Get-SaveKeyRaw([string]$Prefs) {
    # 'absent' when the line is missing, else 'present:' and the text after the colon.
    $m = [regex]::Match([IO.File]::ReadAllText($Prefs), $SaveKeyLine)
    if ($m.Success) { return 'present:' + $m.Groups[2].Value }
    return 'absent'
}

function Set-SaveKeyRaw([string]$Prefs, [string]$Raw) {
    $text = [IO.File]::ReadAllText($Prefs)
    $newline = if ($text.Contains("`r`n")) { "`r`n" } else { "`n" }
    if ($Raw -eq 'absent') {
        $text = [regex]::Replace($text, '(?m)^[ \t]*SaveFilenameKey/:[^\r\n]*(\r?\n)?', '')
    } else {
        $rest = $Raw.Substring('present:'.Length)
        if ([regex]::IsMatch($text, $SaveKeyLine)) {
            $text = [regex]::Replace($text, $SaveKeyLine, [Text.RegularExpressions.MatchEvaluator]{ param($m) $m.Groups[1].Value + $rest })
        } else {
            $m = [regex]::Match($text, '(?m)^strings:[ \t]*\r?\n')
            if (-not $m.Success) { throw "$Prefs has no strings: section to hold SaveFilenameKey/" }
            $text = $text.Insert($m.Index + $m.Length, "  SaveFilenameKey/:$rest$newline")
        }
    }
    [IO.File]::WriteAllText($Prefs, $text, (New-Object System.Text.UTF8Encoding($false)))
}

# Keeps the player's pointer before a game starts, unless one is kept already (an earlier run's).
function Save-SaveKey([string]$Prefs) {
    $kept = "$Prefs.onitest-savekey"
    if (Test-Path -LiteralPath $kept) { return }
    [IO.File]::WriteAllText($kept, (Get-SaveKeyRaw $Prefs))
}

# Puts the player's pointer back if a run kept it. Returns true when it did.
function Restore-SaveKey([string]$Prefs) {
    $kept = "$Prefs.onitest-savekey"
    if (-not (Test-Path -LiteralPath $kept)) { return $false }
    $raw = [IO.File]::ReadAllText($kept)
    Set-SaveKeyRaw $Prefs $raw
    if ((Get-SaveKeyRaw $Prefs) -ne $raw) { throw "kplayerprefs.yaml does not hold the player's SaveFilenameKey/ after the restore; the kept value is left at $kept" }
    Remove-Item -LiteralPath $kept -Force
    return $true
}
