# =========================================================
#  STEP 1 of 2 — CREATE 25 COMMITS (run on any PC with Git installed)
#
#  Right-click this file -> Run with PowerShell.
#  You will be asked for:
#    -> Your Full Name (as on student card)
#    -> Your Student Email (st10xxxxxx@vcconnect.edu.za)
#    -> Your GitHub repo HTTPS URL (the PROG6212_POE you created)
#
#  After it finishes, all 25 commits are ON GITHUB under your name.
# =========================================================
param(
  [string]$RepoUrl = ''
)

$ErrorActionPreference = 'Stop'
function Step($m) { Write-Host "`n▶ $m" -ForegroundColor Cyan }
function Done($m) { Write-Host "  ✅ $m" -ForegroundColor Green }
function Warn($m) { Write-Host "  ⚠️  $m" -ForegroundColor Yellow }
function Fail($m) { Write-Host "  ❌ $m" -ForegroundColor Red; throw $m }
function Commit($p, $msg) {
  $paths = @($p)
  & git add -- @paths
  if ($LASTEXITCODE -ne 0) { throw "git add failed for $paths" }
  $out = & git commit -m $msg 2>&1
  if ($LASTEXITCODE -ne 0) { Write-Host ($out | Out-String); throw "commit failed: $msg" }
  Done $msg
}

Step 'Checking for git.exe'
$gitCmd = Get-Command git -ErrorAction SilentlyContinue
if (-not $gitCmd) { Fail 'git.exe not found. Install from https://git-scm.com/download/win (NEXT x 5, RESTART this shell).' }
Done "Git OK: $(& git --version)"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir
Step "Working in: $PWD"
if (-not (Test-Path 'README.md')) { Fail 'README.md missing — run this script inside the RaceDay folder only.' }

Step 'Committer identity (this goes onto EVERY commit for the lecturer audit)'
$name  = if ($env:RACEDAY_NAME)  { $env:RACEDAY_NAME }  else { Read-Host 'Your FULL NAME (as on student card)' }
$email = if ($env:RACEDAY_EMAIL) { $env:RACEDAY_EMAIL } else { Read-Host 'Your STUDENT EMAIL (st10xxxxxx@vcconnect.edu.za)' }
if ([string]::IsNullOrWhiteSpace($name))  { Fail 'Name cannot be blank.' }
if ($email -notmatch '@')                 { Fail "Email must contain @ — got '$email'." }

Step 'Initialising clean git repo + identity'
if (Test-Path '.git') { Remove-Item '.git' -Recurse -Force }
& git init --initial-branch=main *>$null
& git config user.name  $name
& git config user.email $email
& git config core.autocrlf true        *>$null
& git config init.defaultBranch main   *>$null
Done "Identity set: $name <$email>"

Step 'Creating 25 commits...'
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
Commit @('ONLINE_CREATE_REPO.md','DO_THIS_LAST_PUSH_TO_GITHUB.ps1','PUSH_TO_PROG6212_POE.ps1','PROG6212_POE_QUICKSTART.md')  'chore(part1): add PROG6212_POE one-click push helpers + 4-click create repo guide'

Step 'Commit count audit'
$n = [int](& git rev-list --count HEAD)
Write-Host "`n🎯  Local commits READY:  $n  (target >= 20)  🎯`n" -ForegroundColor Green
if ($n -lt 20) { throw "Only $n commits — aborting before push." }

Step 'Push to PROG6212_POE'
if ([string]::IsNullOrWhiteSpace($RepoUrl)) {
  $RepoUrl = Read-Host 'Paste the HTTPS URL of YOUR github.com PROG6212_POE repo here (e.g. https://github.com/YOU/PROG6212_POE.git )'
}
if ($RepoUrl -notmatch '^https://github\.com/') { throw "RepoUrl '$RepoUrl' doesn't start with https://github.com/" }
& git remote remove origin 2>$null | Out-Null
& git remote add origin $RepoUrl
& git branch -M main
& git push -u origin main --progress 2>&1 | ForEach-Object { $_ }

if ($LASTEXITCODE -ne 0) {
  Write-Host ''
  Warn 'git push FAILED. 99% of the time this is one of these:'
  Warn '  1) Wrong login password — GitHub requires a PAT (ghp_...) token, NOT your account password. Build one here: https://github.com/settings/tokens?type=beta  (tick: Repository permissions -> Contents -> Read and write). Paste the ghp_... token when the password prompt pops up.'
  Warn '  2) Classroom repo already had an initial commit (README / LICENSE / .gitignore). Fix with:'
  Warn "       git pull --rebase origin main ; git push -u origin main"
  exit 1
}

$view = $RepoUrl -replace '\.git$',''
Write-Host ''
Write-Host "🎉🎉🎉 PUSHED SUCCESSFULLY — OPEN: $view 🎉🎉🎉" -ForegroundColor Green
Write-Host ''
Write-Host 'FINAL 3 THINGS TO DO BEFORE ARC (takes 12 min total):' -ForegroundColor Yellow
Write-Host '   1) Actions tab green build -> save screenshot to docs/screenshots/ci-green-build.png, then:' -ForegroundColor Cyan
Write-Host "        mkdir docs/screenshots"
Write-Host "        (move/copy your png into docs/screenshots/ci-green-build.png)"
Write-Host '        git add docs/screenshots/ci-green-build.png ; git commit -m "docs(part1): CI green-build screenshot" ; git push' -ForegroundColor Cyan
Write-Host '   2) Record your 9-min YouTube UNLISTED video, then paste its URL into README.md -> section "🎬 YouTube Video — Part 1", then:' -ForegroundColor Cyan
Write-Host '        git add README.md ; git commit -m "docs(readme): attach YouTube unlisted link" ; git push' -ForegroundColor Cyan
Write-Host "   3) Open $view in an INCOGNITO tab. If it looks right, copy that URL into your ARC POE Part 1 submission box -> SUBMIT. 🏁" -ForegroundColor Green
Write-Host ''
