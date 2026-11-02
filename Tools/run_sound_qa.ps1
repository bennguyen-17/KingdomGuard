param([string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe')
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (!(Test-Path -LiteralPath $Unity)) { throw "Unity not found: $Unity" }
New-Item -ItemType Directory -Force (Join-Path $projectRoot 'Logs') | Out-Null
$arguments = @('-batchmode', '-projectPath', ('"' + $projectRoot + '"'), '-executeMethod', 'SoundQaVerification.Run', '-logFile', ('"' + (Join-Path $projectRoot 'Logs/SoundQaVerification.log') + '"'))
$editorRun = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$editorRun.WaitForExit(900000)) { $editorRun.Kill(); throw 'Unity QA exceeded 15 minutes.' }
if ($editorRun.ExitCode -ne 0) { throw "Unity QA failed: $($editorRun.ExitCode)" }
$player = Join-Path $projectRoot 'Builds/QA/KingdomGuard.exe'
function Invoke-AudioPlayer([string]$Flag, [string]$LogName) {
    $logPath = Join-Path $projectRoot ('Logs/' + $LogName)
    $run = Start-Process -FilePath $player -ArgumentList @('-batchmode', $Flag, '-logFile', ('"' + $logPath + '"')) -WindowStyle Hidden -PassThru
    if (!$run.WaitForExit(120000)) { $run.Kill(); throw ('Player timed out: ' + $Flag) }
    if ($run.ExitCode -ne 0) { throw ('Player failed: ' + $Flag + '; see ' + $logPath) }
    return @(Get-Content -LiteralPath $logPath)
}
$playerLog = Invoke-AudioPlayer '--kg-audio-smoke-test' 'AudioPlayerSmoke.log'
$sceneMarkers = @($playerLog | Select-String -Pattern '^KG_AUDIO_SMOKE_PASS scene=')
$completed = [bool]($playerLog | Select-String -Pattern '^KG_AUDIO_SMOKE_COMPLETE')
$verification = Get-Content -LiteralPath (Join-Path $projectRoot 'Docs/QA/unity_verification.json') -Raw | ConvertFrom-Json
if ($sceneMarkers.Count -ne $verification.sceneCount -or !$completed) { throw 'Player did not complete all configured scenes.' }
$reopenPassed = $false
try {
    $null = Invoke-AudioPlayer '--kg-audio-restore-settings' 'AudioPrefsCleanup.log'
    $saveLog = Invoke-AudioPlayer '--kg-audio-save-settings' 'AudioPrefsSave.log'
    if (!($saveLog | Select-String '^KG_AUDIO_SETTINGS_SAVED')) { throw 'Missing settings save marker' }
    $checkLog = Invoke-AudioPlayer '--kg-audio-check-settings' 'AudioPrefsReopen.log'
    $reopenPassed = [bool]($checkLog | Select-String '^KG_AUDIO_SETTINGS_REOPEN_PASS')
    if (!$reopenPassed) { throw 'Fresh process did not reload audio settings' }
} finally {
    $null = Invoke-AudioPlayer '--kg-audio-restore-settings' 'AudioPrefsRestore.log'
}
$buildFiles = @('Builds/QA/KingdomGuard.exe', 'Builds/QA/KingdomGuard_Data/resources.assets', 'Builds/QA/KingdomGuard_Data/Managed/Assembly-CSharp.dll')
$results = [ordered]@{
    exit_code = 0; scene_passes = $sceneMarkers.Count; complete = $completed; fresh_process_volume_reload = $reopenPassed
    build_files = @($buildFiles | ForEach-Object { [ordered]@{ path = $_; sha256 = (Get-FileHash -LiteralPath (Join-Path $projectRoot $_) -Algorithm SHA256).Hash } })
    markers = @($playerLog | Select-String '^KG_AUDIO' | ForEach-Object { $_.Line })
}
$results | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $projectRoot 'Docs/QA/player_smoke.json') -Encoding UTF8
Write-Output 'PASS: Unity Play Mode, Windows build, all scene smoke checks and settings reload across player processes.'
