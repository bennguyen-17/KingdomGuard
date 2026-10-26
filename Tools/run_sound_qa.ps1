param(
    [string]$Unity = 'C:/Program Files/Unity/Hub/Editor/6000.6.0f1/Editor/Unity.exe'
)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $PSScriptRoot
if (!(Test-Path -LiteralPath $Unity)) { throw "Unity not found: $Unity" }
New-Item -ItemType Directory -Force (Join-Path $projectRoot 'Logs') | Out-Null
$arguments = @('-batchmode', '-projectPath', ('"' + $projectRoot + '"'), '-executeMethod', 'SoundQaVerification.Run', '-logFile', ('"' + (Join-Path $projectRoot 'Logs/SoundQaVerification.log') + '"'))
$editorRun = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -PassThru
if (!$editorRun.WaitForExit(900000)) { $editorRun.Kill(); throw 'Unity QA exceeded 15 minutes. See Logs/SoundQaVerification.log.' }
if ($editorRun.ExitCode -ne 0) { throw "Unity QA failed with exit code $($editorRun.ExitCode)" }
$player = Join-Path $projectRoot 'Builds/QA/KingdomGuard.exe'
$playerRun = Start-Process -FilePath $player -ArgumentList @('-batchmode', '--kg-audio-smoke-test', '-logFile', ('"' + (Join-Path $projectRoot 'Logs/AudioPlayerSmoke.log') + '"')) -WindowStyle Hidden -PassThru
if (!$playerRun.WaitForExit(120000)) { $playerRun.Kill(); throw 'Player smoke test timed out.' }
if ($playerRun.ExitCode -ne 0) { throw "Player smoke failed with exit code $($playerRun.ExitCode)" }
$playerLog = Get-Content -LiteralPath (Join-Path $projectRoot 'Logs/AudioPlayerSmoke.log')
$sceneMarkers = @($playerLog | Select-String -Pattern '^KG_AUDIO_SMOKE_PASS scene=')
$completed = [bool]($playerLog | Select-String -Pattern '^KG_AUDIO_SMOKE_COMPLETE')
if ($sceneMarkers.Count -ne 3 -or !$completed) { throw 'Player exited without completing all three scene checks.' }
$buildFiles = @('Builds/QA/KingdomGuard.exe', 'Builds/QA/KingdomGuard_Data/resources.assets', 'Builds/QA/KingdomGuard_Data/Managed/Assembly-CSharp.dll')
$smokeResults = [ordered]@{
    exit_code = $playerRun.ExitCode
    scene_passes = $sceneMarkers.Count
    complete = $completed
    build_files = @($buildFiles | ForEach-Object { [ordered]@{ path = $_; sha256 = (Get-FileHash -LiteralPath (Join-Path $projectRoot $_) -Algorithm SHA256).Hash } })
    markers = @($playerLog | Select-String -Pattern '^KG_AUDIO' | ForEach-Object { $_.Line })
}
$smokeResults | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $projectRoot 'Docs/QA/player_smoke.json') -Encoding UTF8
Write-Output 'Unity Play Mode, Windows build and player smoke checks passed.'
