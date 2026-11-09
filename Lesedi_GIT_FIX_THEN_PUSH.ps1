# =========================================================
#  BOOTSTRAPPER: Fix Git PATH + run Lesedi_PUSH_NOW.ps1.
#  Student: Lesedi Mogatwe
#  Use this script instead of the others if git.exe "not in PATH".
# =========================================================
$ErrorActionPreference = 'Continue'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir

function Say($m, $c='Cyan')    { Write-Host ("`n>> " + $m) -ForegroundColor $c }
function Good($m)              { Write-Host ("   [OK]    " + $m) -ForegroundColor Green }
function Bad($m)               { Write-Host ("   [MISS]  " + $m) -ForegroundColor Yellow }
function Fail($m)              { Write-Host ("   [FAIL]  " + $m) -ForegroundColor Red; throw $m }

$GitCandidates = @(
  "$env:ProgramFiles\Git\cmd\git.exe",
  "${env:ProgramFiles(x86)}\Git\cmd\git.exe",
  "$env:LocalAppData\Programs\Git\cmd\git.exe",
  "$env:AppData\Local\Programs\Git\cmd\git.exe",
  "$env:UserProfile\scoop\shims\git.exe",
  "$env:ChocolateyInstall\bin\git.exe",
  "C:\Program Files\Git\cmd\git.exe",
  "D:\Program Files\Git\cmd\git.exe",
  "C:\Git\cmd\git.exe"
)

Say 'Checking for git.exe in PATH...'
$gitInPath = Get-Command git -ErrorAction SilentlyContinue
if ($gitInPath) { Good ("Found git.exe in PATH at: " + $gitInPath.Source) }
else {
  Bad 'git.exe not in current session PATH.'
  Say 'Scanning 9 common install folders on disk...'
  $foundOnDisk = $GitCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
  if ($foundOnDisk) {
    Good ("Found Git on disk at: " + $foundOnDisk)
    $gitDir = Split-Path -Parent $foundOnDisk                # e.g. C:\Program Files\Git\cmd
    $gitUsrBin = (Join-Path (Split-Path -Parent $gitDir) 'usr\bin')
    $env:PATH = "$gitDir;$gitUsrBin;" + $env:PATH
    Say 'Added Git folders to the CURRENT PowerShell session PATH (no reboot needed).'
    $gitInPath = Get-Command git -ErrorAction SilentlyContinue
  } else {
    Bad 'Git not installed on this machine yet. Downloading + installing silently (64-bit)...'
    $installer = Join-Path $env:TEMP 'Git-for-Windows-Installer-x64.exe'
    $url = 'https://github.com/git-for-windows/git/releases/latest/download/Git-2.47.0-64-bit.exe'
    Say 'Downloading Git installer (this can take 30-90 seconds on slow internet)...'
    $ProgressPreference = 'SilentlyContinue'
    try {
      Invoke-WebRequest -Uri $url -OutFile $installer -UseBasicParsing | Out-Null
    } catch {
      Fail ('Download failed. Please manually open https://git-scm.com/download/win in your browser, install with all defaults (NEXT x 6), then re-run this script.')
    }
    Say 'Running installer /VERYSILENT (will take 30-120 seconds, no window shown)...'
    $proc = Start-Process -FilePath $installer `
      -ArgumentList '/VERYSILENT','/NORESTART','/NOCANCEL','/COMPONENTS=icons,ext\reg\shellhere,assoc,assoc_sh,gitlfs,windowsterminal','/PATH=C:\Program Files\Git' `
      -Wait -PassThru
    if ($proc.ExitCode -ne 0) {
      Fail ('Installer exited with code ' + $proc.ExitCode + '. Please run the installer manually (double-click the .exe in Downloads).')
    }
    Remove-Item $installer -Force -ErrorAction SilentlyContinue
    Good 'Git installer finished.'
    Say 'Re-scanning Program Files\Git\cmd...'
    $freshGit = 'C:\Program Files\Git\cmd\git.exe'
    if (-not (Test-Path $freshGit)) {
      $foundOnDisk = $GitCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
      if (-not $foundOnDisk) { Fail 'Installer said OK but git.exe still missing after 30s. Reboot Windows then retry.' }
      $freshGit = $foundOnDisk
    }
    $gitDir = Split-Path -Parent $freshGit
    $gitUsrBin = (Join-Path (Split-Path -Parent $gitDir) 'usr\bin')
    $env:PATH = "$gitDir;$gitUsrBin;" + $env:PATH
    Say 'Added freshly installed Git to session PATH.'
    $gitInPath = Get-Command git -ErrorAction SilentlyContinue
  }
}

if (-not $gitInPath) { Fail 'git.exe still not resolved. Reboot Windows, then re-run this file.' }
Good ("Final git.exe resolved: " + $gitInPath.Source + " -> " + (& git --version))

Say 'Launching Lesedi_PUSH_NOW.ps1 (makes 25 commits, pushes to PROG6212_POE)...'
$runner = Join-Path $ScriptDir 'Lesedi_PUSH_NOW.ps1'
if (-not (Test-Path $runner)) { Fail ("Missing file: " + $runner) }

& powershell -NoProfile -ExecutionPolicy Bypass -File $runner
$exit = $LASTEXITCODE
if ($exit -ne 0) {
  Say ('Lesedi_PUSH_NOW exited with code ' + $exit + '. See diagnostic lines printed above.') Magenta
  Say 'If auth prompted and failed, FIX-B: regenerate a PAT with Contents=Read+Write at https://github.com/settings/tokens?type=beta' Magenta
  Say 'If push said "unrelated histories", FIX-A: run these 2 lines in the RaceDay folder then push again:' Magenta
  Write-Host '    git pull --rebase origin main --allow-unrelated-histories' -ForegroundColor Cyan
  Write-Host '    git push -u origin main' -ForegroundColor Cyan
  exit $exit
}
Good 'Bootstrapper completed without errors.'
exit 0
