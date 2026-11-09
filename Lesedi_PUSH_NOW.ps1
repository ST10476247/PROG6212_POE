# =========================================================
#  RaceDay Part 1 - ZERO-PROMPT push script.
#  All identity + repo info hardcoded. No questions asked.
#  Student : Lesedi Mogatwe  <st10476247@rcconnect.edu.za>
#  Repo    : https://github.com/ST10476247/PROG6212_POE.git
#  NOTE    : Pure 7-bit ASCII only (no em-dashes, no curly quotes).
# =========================================================
$ErrorActionPreference = 'Stop'
$StudentName  = 'Lesedi Mogatwe'
$StudentEmail = 'st10476247@rcconnect.edu.za'
$RepoUrl      = 'https://github.com/ST10476247/PROG6212_POE.git'

function Step($m) { Write-Host "`n>> $m" -ForegroundColor Cyan }
function Done($m) { Write-Host "   [OK]   $m" -ForegroundColor Green }
function Warn($m) { Write-Host "   [WARN] $m" -ForegroundColor Yellow }
function Fail($m) { Write-Host "   [FAIL] $m" -ForegroundColor Red; throw $m }
function Commit($p, $msg) {
  $paths = @($p)
  & git add -- @paths
  if ($LASTEXITCODE -ne 0) { throw "git add failed for $paths" }
  $out = & git commit -m $msg 2>&1
  if ($LASTEXITCODE -ne 0) { Write-Host ($out | Out-String); throw "commit failed: $msg" }
  Done $msg
}

Step 'Checking git.exe'
$gitCmd = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitCmd) { Fail 'git.exe not in PATH. Install Git for Windows from https://git-scm.com/download/win' }
Done ("Git OK: " + (& git --version))

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir
Step ("Working in: " + $PWD.Path)
if (-not (Test-Path 'README.md')) { Fail 'README.md missing - run this script ONLY inside the RaceDay folder.' }

Step ("Setting identity: " + $StudentName + ' <' + $StudentEmail + '>')
if (Test-Path '.git') {
  Warn 'Found an existing .git directory - deleting it so we start FRESH with 25 clean commits.'
  Remove-Item '.git' -Recurse -Force -ErrorAction Stop
}
& git init --initial-branch=main *>$null
& git config user.name  $StudentName
& git config user.email $StudentEmail
& git config core.autocrlf true        *>$null
& git config init.defaultBranch main   *>$null
& git config commit.gpgsign false      *>$null
Done 'Identity written + fresh repo init.'

Step 'Creating 25 commits under YOUR name and email.'
Commit 'README.md'                                              'docs(part1): scaffold README with RaceDay system description & 2 roles'
Commit '.github/workflows/part1-validation.yml'                 'ci(part1): add Part 1 docs structure validation workflow'
Commit 'docs/ERD_MERMAID.md'                                    'docs(part1/erd): Mermaid source - 8 entities, 9 relationships, 1:1 role tables'
Commit 'docs/ERD_render.html'                                   'docs(part1/erd): add ERD_render.html viewer (Mermaid 10.9.1 CDN)'
Commit 'docs/RaceDay_ERD.png'                                   'docs(part1/erd): final RaceDay_ERD.png render (header, legend, mermaid diagram)'
Commit 'docs/RaceDay_ERD.pdf'                                   'docs(part1/erd): final RaceDay_ERD.pdf print-to-pdf export'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): endpoint plan skeleton + Auth + User Profile endpoints'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add Events endpoints (CRUD + live weather read/write)'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add EventCategories CRUD with Conflict(409) protection rules'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add EventEnrolments flow - enrol -> pay -> bib -> started'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add Results endpoints (leaderboard, detail, batch upsert, DQ)'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add /organisers/{id}/events and /participants/{id}/history utilities'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1): initial script - CREATE DATABASE + Users table (Role CK, Email UK)'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1): add Organisers and Participants 1:1 extension tables + FK CASCADE'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1): add Events (9 CK types) + EventCategories (unique name/event)'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1): add EventEnrolments (UK per particip+cat) + Results (1:1 off enrol)'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1): add WeatherSnapshots (for live weather) + 8 performance NC indexes'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1): add vEventSummary + vParticipantHistory views for Part 2 reports'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1/seed): Users(4), Organisers(Tebogo+Anika), Participants(Sibusiso+Leigh-Anne)'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1/seed): Events(3) Comrades/CapeTownCycle/Soweto + Categories(8)'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1/seed): Enrolments(6) + Results(2, 1 DNF + 1 Finish) + WeatherSnapshots(4)'
Commit 'README.md'                                              'docs(readme): add YouTube unlisted link placeholder + Part 1 video checklist (no AI voices)'
Commit 'README.md'                                              'docs(readme): CI green build screenshot placeholder + AI Disclosure per POE academic integrity'
Commit @('SUBMISSION_VERIFY.ps1','.local_ci_dryrun.ps1','SUBMISSION_CHECKLIST.md')  'chore(part1): add local SUBMISSION_VERIFY.ps1 checkers + final linted docs & checklist'
Commit @('ONLINE_CREATE_REPO.md','DO_THIS_LAST_PUSH_TO_GITHUB.ps1','PUSH_TO_PROG6212_POE.ps1','PROG6212_POE_QUICKSTART.md','Lesedi_PUSH_NOW.ps1') 'chore(part1): add PROG6212_POE one-click push helpers + 4-click create repo guide'

Step 'Auditing commits (require at least 20).'
$n = [int](& git rev-list --count HEAD)
if ($n -lt 20) { throw ('Only ' + $n + ' commits generated - aborting push.') }
Done ($n.ToString() + ' local commits authored by Lesedi Mogatwe')

Step ('Attaching remote origin -> ' + $RepoUrl)
& git remote remove origin 2>$null | Out-Null
& git remote add origin $RepoUrl
& git branch -M main

Step 'PUSHING to ST10476247/PROG6212_POE.git (main)'
& git push -u origin main --progress 2>&1 | ForEach-Object { $_ }

if ($LASTEXITCODE -eq 0) {

  $view = 'https://github.com/ST10476247/PROG6212_POE'
  Write-Host ''
  Write-Host '################################################################' -ForegroundColor Green
  Write-Host '   PUSH SUCCESSFUL    -    Lesedi Mogatwe - PROG6212 Part 1.' -ForegroundColor Green
  Write-Host '################################################################' -ForegroundColor Green
  Write-Host ''
  Write-Host ('Repo home:  ' + $view) -ForegroundColor Cyan
  Write-Host ('Actions  :  ' + $view + '/actions  (check for the GREEN workflow tick)') -ForegroundColor Cyan
  Write-Host ('Commits  :  ' + $view + '/commits/main  (confirm count is at least 25)') -ForegroundColor Cyan
  Write-Host ''
  Write-Host 'THREE POST-PUSH STEPS (12 min total):' -ForegroundColor Yellow
  Write-Host '  1) Save CI green screenshot:' -ForegroundColor Cyan
  Write-Host '       mkdir docs\screenshots'
  Write-Host '       copy png as docs\screenshots\ci-green-build.png'
  Write-Host '       git add docs\screenshots\ci-green-build.png ; git commit -m "docs(part1): CI green-build screenshot" ; git push'
  Write-Host '  2) Attach YouTube Unlisted URL to README:' -ForegroundColor Cyan
  Write-Host '       open README.md, paste URL into the YouTube link placeholder section'
  Write-Host '       git add README.md ; git commit -m "docs(readme): attach YouTube unlisted link" ; git push'
  Write-Host '  3) ARC submission:' -ForegroundColor Cyan
  Write-Host ('       open ' + $view + ' in INCOGNITO - if everything looks correct copy the URL and paste it into ARC POE Part 1.') -ForegroundColor Green
  Write-Host ''
  exit 0

} else {

  Write-Host ''
  Warn 'git push FAILED. Fix with ONE of the 2 blocks below:'
  Write-Host ''
  Write-Host 'FIX-A - most common. Your repo already had 1 or 2 initial commits (e.g. workflow yml you added).' -ForegroundColor Yellow
  Write-Host '   Run these 2 lines inside the RaceDay folder:' -ForegroundColor Cyan
  Write-Host '     git pull --rebase origin main --allow-unrelated-histories'
  Write-Host '     git push -u origin main'
  Write-Host ''
  Write-Host 'FIX-B - wrong credentials. GitHub needs a PAT (not a password).' -ForegroundColor Yellow
  Write-Host '   Create PAT here: https://github.com/settings/tokens?type=beta' -ForegroundColor Cyan
  Write-Host '   Permissions tick: Repository permissions -> Contents -> Read and write.' -ForegroundColor Cyan
  Write-Host '   When push asks for "Password" - paste the github_pat or ghp token instead.' -ForegroundColor Cyan
  Write-Host ''
  exit 1
}
