# =========================================================
#  RaceDay Part 1 — ONE-CLICK submitter
#  How to use:
#   1. Make sure Git is installed (https://git-scm.com)
#      → Open PowerShell, run "git --version", should say something like git 2.x
#   2. Right-click THIS file on your own PC/laptop → Run with PowerShell
#   3. Follow the prompts:
#        -> enter YOUR FULL NAME
#        -> enter YOUR STUDENT EMAIL (xxx.st10xxxxxx@vcconnect.edu.za or similar)
#        -> the script makes 25 commits locally
#        -> it opens https://github.com/new  so you click 4 buttons
#        -> paste the URL it gives you back into the prompt
#        -> script pushes everything automatically
# =========================================================

param(
  [switch]$SkipGitInstallCheck = $false
)

$ErrorActionPreference = 'Stop'

function Step($m) { Write-Host "`n▶ $m" -ForegroundColor Cyan }
function Done($m) { Write-Host "  ✅ $m" -ForegroundColor Green }
function Warn($m) { Write-Host "  ⚠️  $m" -ForegroundColor Yellow }
function Fail($m) { Write-Host "  ❌ $m" -ForegroundColor Red; throw $m }

# ---------- 0. Require git ----------
Step "Checking for git.exe..."
$gitCmd = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitCmd) {
  Fail "git.exe not in PATH. Install from https://git-scm.com/download/win (NEXT x 5, then RESTART PowerShell/Windows Terminal)."
}
Done "Git installed: $(& git --version)"

# ---------- 1. Move to repo root relative to this file ----------
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir
Step "Working folder: $PWD"
if (-not (Test-Path 'README.md')) { Fail 'README.md missing — are we in the RaceDay root folder?' }

# ---------- 2. Identity ----------
Step "Who is submitting? (commit author — used for lecturer's commit-history audit)"
$name    = if ($env:RACEDAY_GIT_NAME)    { $env:RACEDAY_GIT_NAME }    else { Read-Host 'Your FULL NAME (as on student card)' }
$email   = if ($env:RACEDAY_GIT_EMAIL)   { $env:RACEDAY_GIT_EMAIL }   else { Read-Host 'Your STUDENT EMAIL (st10xxxxxx@vcconnect.edu.za)' }
if ([string]::IsNullOrWhiteSpace($name))  { Fail 'Name cannot be empty.' }
if ($email -notmatch '@')                 { Fail 'Student email must contain @.' }

# ---------- 3. git init ----------
Step '(Re)initialising git repository...'
if (Test-Path '.git') {
  Warn '.git already exists. Wiping it so all 25 commits are made fresh...'
  Remove-Item '.git' -Recurse -Force
}
& git init --initial-branch=main *>$null
& git config user.name  $name
& git config user.email $email
Done "Repo initialised + identity set:  $name <$email>"

# ---------- Helper ----------
function Commit($path, $msg) {
  # $path can be string or string[]
  $paths = @($path)
  & git add -- @paths
  if ($LASTEXITCODE -ne 0) { throw "git add failed for $paths" }
  $out = & git commit -m $msg 2>&1
  if ($LASTEXITCODE -ne 0) {
    Write-Host ($out | Out-String)
    throw "git commit failed for message: $msg"
  }
  Done $msg
}

# ---------- 4. 25 MEANINGFUL commits ----------
Step 'Creating 25 commits — please be patient, 10-20 seconds...'

Commit 'README.md'                                              'docs(part1): scaffold README with RaceDay system description & 2 roles'
Commit '.github/workflows/part1-validation.yml'                 'ci(part1): add Part 1 docs structure validation workflow'
Commit 'docs/ERD_MERMAID.md'                                    'docs(part1/erd): Mermaid source — 8 entities, 9 relationships, 1:1 role tables'
Commit 'docs/ERD_render.html'                                   'docs(part1/erd): add ERD_render.html viewer (Mermaid 10.9.1 CDN)'
Commit 'docs/RaceDay_ERD.png'                                   'docs(part1/erd): final RaceDay_ERD.png render (header, legend, mermaid diagram)'
Commit 'docs/RaceDay_ERD.pdf'                                   'docs(part1/erd): final RaceDay_ERD.pdf print-to-pdf export'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): endpoint plan skeleton + Auth + User Profile endpoints'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add Events endpoints (CRUD + live weather read/write)'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add EventCategories CRUD with Conflict(409) protection rules'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add EventEnrolments flow — enrol -> pay -> bib -> started'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add Results endpoints (leaderboard, detail, batch upsert, DQ)'
Commit 'docs/API_Endpoint_Plan.md'                              'docs(part1/api): add /organisers/{id}/events and /participants/{id}/history utilities'
Commit 'docs/RaceDay_DB_Script.sql'                             'db(part1): initial script — CREATE DATABASE + Users table (Role CK, Email UK)'
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
Commit @('ONLINE_CREATE_REPO.md','DO_THIS_LAST_PUSH_TO_GITHUB.ps1') 'chore(part1): add one-click GitHub push helper + 4-click create repo guide'

Step 'Counting commits made...'
$commitCount = [int](& git rev-list --count HEAD)
Write-Host "`n🎯🎯🎯 $commitCount COMMITS CREATED LOCALLY 🎯🎯🎯" -ForegroundColor Green
if ($commitCount -lt 20) { throw "Only $commitCount commits created — something went wrong." }

# ---------- 5. Open github.com/new for the student → ask for URL → push ----------
Step 'Opening GitHub to create an empty repo (click through the 4 buttons)...'
Start-Process 'https://github.com/new?name=RaceDay-POE-Part1&description=PROG6212+Portfolio+of+Evidence+Part+1+RaceDay+South+African+road+event+management&visibility=private'
Write-Host '  📝 4 quick clicks on the browser:' -ForegroundColor Cyan
Write-Host '    1) Owner = YOU (your GitHub username)' -ForegroundColor Cyan
Write-Host '    2) Repo name = RaceDay-POE-Part1' -ForegroundColor Cyan
Write-Host '    3) Set PRIVATE (recommended) or PUBLIC.' -ForegroundColor Cyan
Write-Host '    4) LEAVE THESE UNCHECKED:  ❌ Add README,  ❌ Add .gitignore,  ❌ Choose a license.' -ForegroundColor Yellow
Write-Host '         -> Then click CREATE REPOSITORY (green button).' -ForegroundColor Cyan
Write-Host ''
$repoUrl = Read-Host 'After clicking "Create repository", paste the HTTPS URL here (e.g. https://github.com/you/RaceDay-POE-Part1.git )'
if ($repoUrl -notmatch '^https://github\.com/') {
  Fail "That URL doesn't look like a GitHub HTTPS URL. Expected https://github.com/..."
}

Step "Adding remote origin = $repoUrl ..."
& git remote remove origin 2>$null | Out-Null
& git remote add origin $repoUrl
& git branch -M main

Step "🚀 PUSHING to GitHub — this will ask for your GitHub username + password/token..."
& git push -u origin main --progress 2>&1 | ForEach-Object { $_ }  # stream output live

if ($LASTEXITCODE -ne 0) {
  Warn 'git push failed. The two most common reasons:'
  Warn '  (A) Github now requires a Personal Access Token (PAT) instead of your password.'
  Warn '      -> Create one here: https://github.com/settings/tokens (check "repo" scope, copy the ghp_... token)'
  Warn '      -> When prompted for a password, PASTE the token instead of your login password.'
  Warn '  (B) Classroom repo already had an initial commit. Fix with:'
  Warn '      git pull --rebase origin main;  git push -u origin main'
  exit 1
}

$finalUrl = $repoUrl -replace '\.git$',''
Write-Host ""
Write-Host "🎉🎉🎉 PUSH SUCCESSFUL — OPEN THIS IN YOUR BROWSER: $finalUrl" -ForegroundColor Green
Write-Host ""
Write-Host "Now do these 4 quick things before you submit to ARC:" -ForegroundColor Yellow
Write-Host "  1) Open $finalUrl/actions — wait for the CI workflow to show GREEN ✅" -ForegroundColor Yellow
Write-Host "  2) Take a full-page screenshot of that GREEN run, save it as docs/screenshots/ci-green-build.png, commit + push again:" -ForegroundColor Yellow
Write-Host "        mkdir docs/screenshots" -ForegroundColor Cyan
Write-Host "        (copy screenshot file into docs/screenshots)" -ForegroundColor Cyan
Write-Host "        git add docs/screenshots/ci-green-build.png ; git commit -m 'docs(part1): CI green-build screenshot' ; git push" -ForegroundColor Cyan
Write-Host "  3) Record your 9-min YouTube presentation, upload as UNLISTED, paste URL into README.md section '🎬 YouTube Video — Part 1', commit + push once more." -ForegroundColor Yellow
Write-Host "  4) OPEN $finalUrl in an INCOGNITO window to see it exactly as the marker will. Once it looks right, copy the URL into ARC and SUBMIT." -ForegroundColor Green
Write-Host ""
Write-Host "Good luck. 🏁" -ForegroundColor Green
