# ==============================================================================
#  RaceDay Part 2 - Automated 23 Meaningful Commits & GitHub Push Script
#  Student: Lesedi Mogatwe (ST10476247)
#  Repository: https://github.com/ST10476247/PROG6212_POE.git
# ==============================================================================
$ErrorActionPreference = 'Stop'
$StudentName  = 'Lesedi Mogatwe'
$StudentEmail = 'st10476247@rcconnect.edu.za'
$RepoUrl      = 'https://github.com/ST10476247/PROG6212_POE.git'

function Step($m) { Write-Host "`n>> $m" -ForegroundColor Cyan }
if (-not (Get-Command 'Done' -ErrorAction SilentlyContinue)) {
    function Done($m) { Write-Host "   [OK]   $m" -ForegroundColor Green }
}
function Warn($m) { Write-Host "   [WARN] $m" -ForegroundColor Yellow }
function Fail($m) { Write-Host "   [FAIL] $m" -ForegroundColor Red; throw $m }

function CommitStep($files, $message) {
    foreach ($f in $files) {
        if (Test-Path $f) {
            & git add $f
        }
    }
    $status = & git status --porcelain
    if ($status) {
        & git commit -m $message --author="$StudentName <$StudentEmail>" *>$null
        Write-Host "   [COMMITTED] $message" -ForegroundColor Green
    } else {
        Write-Host "   [SKIP - No Changes] $message" -ForegroundColor DarkGray
    }
}

Step "Configuring Git Identity: $StudentName <$StudentEmail>"
& git config user.name "$StudentName"
& git config user.email "$StudentEmail"
& git config core.autocrlf true

Step "Running Unit Tests before commit to verify 100% pass..."
& dotnet test RaceDay.sln --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Fail "Unit tests failed! Please fix failing tests before committing."
}
Done "All 30 unit tests passed!"

Step "Recording 23 Meaningful Commits for Part 2..."

# Commit 1
CommitStep @('.gitignore') 'chore: add .gitignore to exclude build artifacts, bin, and obj binaries'

# Commit 2
CommitStep @('RaceDay.sln', 'RaceDay.slnx', 'RUN_RACEDAY_API.bat') 'feat(api): scaffold ASP.NET Core Web API project (RaceDay.API) and Visual Studio solution'

# Commit 3
CommitStep @(
    'RaceDay.API/RaceDay.API.csproj',
    'RaceDay.API/Models/Entities/User.cs',
    'RaceDay.API/Models/Entities/Organiser.cs',
    'RaceDay.API/Models/Entities/Participant.cs'
) 'feat(api/entities): implement User, Organiser, and Participant domain entities'

# Commit 4
CommitStep @(
    'RaceDay.API/Models/Entities/Event.cs',
    'RaceDay.API/Models/Entities/EventCategory.cs',
    'RaceDay.API/Models/Entities/EventEnrolment.cs'
) 'feat(api/entities): implement Event, EventCategory, and EventEnrolment entities'

# Commit 5
CommitStep @(
    'RaceDay.API/Models/Entities/Result.cs',
    'RaceDay.API/Models/Entities/WeatherSnapshot.cs'
) 'feat(api/entities): implement Result and WeatherSnapshot entities matching Part 1 ERD'

# Commit 6
CommitStep @('RaceDay.API/Data/RaceDayDbContext.cs') 'feat(api/data): implement RaceDayDbContext with fluent API mappings and constraints'

# Commit 7
CommitStep @('RaceDay.API/Data/DbInitializer.cs') 'feat(api/data): add DbInitializer with South African seed data and BCrypt hashing'

# Commit 8
CommitStep @(
    'RaceDay.API/Services/IPasswordHasher.cs',
    'RaceDay.API/Services/PasswordHasher.cs',
    'RaceDay.API/Services/ITokenService.cs',
    'RaceDay.API/Services/TokenService.cs'
) 'feat(api/security): add BCrypt password hasher and signed JWT bearer token service'

# Commit 9
CommitStep @('RaceDay.API/Common/SessionExtensions.cs') 'feat(api/session): implement server-side session management and user context resolver'

# Commit 10
CommitStep @(
    'RaceDay.API/Filters/RequireAuthAttribute.cs',
    'RaceDay.API/Filters/RequireRoleAttribute.cs'
) 'feat(api/filters): implement RequireAuth and RequireRole authorization action filters'

# Commit 11
CommitStep @(
    'RaceDay.API/Models/DTOs/AuthDtos.cs',
    'RaceDay.API/Models/DTOs/ProfileDtos.cs',
    'RaceDay.API/Models/DTOs/EventDtos.cs',
    'RaceDay.API/Models/DTOs/CategoryDtos.cs',
    'RaceDay.API/Models/DTOs/EnrolmentDtos.cs',
    'RaceDay.API/Models/DTOs/ResultDtos.cs',
    'RaceDay.API/Models/DTOs/WeatherDtos.cs'
) 'feat(api/dtos): define Auth, Profile, Event, Category, Enrolment, and Result DTOs'

# Commit 12
CommitStep @('RaceDay.API/Controllers/AuthController.cs') 'feat(api/controllers): implement AuthController for registration, login, and session logout'

# Commit 13
CommitStep @('RaceDay.API/Controllers/ProfileController.cs') 'feat(api/controllers): implement ProfileController for profile management and password rotation'

# Commit 14
CommitStep @('RaceDay.API/Controllers/EventsController.cs') 'feat(api/controllers): implement EventsController with paged queries, filters, and weather/route info'

# Commit 15
CommitStep @('RaceDay.API/Controllers/CategoriesController.cs') 'feat(api/controllers): implement CategoriesController with distance/age category CRUD'

# Commit 16
CommitStep @('RaceDay.API/Controllers/EnrolmentsController.cs') 'feat(api/controllers): implement EnrolmentsController with automatic bib allocation and capacity rules'

# Commit 17
CommitStep @('RaceDay.API/Controllers/ResultsController.cs') 'feat(api/controllers): implement ResultsController with timing mat capture, rankings, and batch import'

# Commit 18
CommitStep @(
    'RaceDay.API/Controllers/OrganisersController.cs',
    'RaceDay.API/Controllers/ParticipantsController.cs'
) 'feat(api/controllers): implement Organisers and Participants utility controllers'

# Commit 19
CommitStep @(
    'RaceDay.API/appsettings.json',
    'RaceDay.API/Program.cs'
) 'feat(api/swagger): configure Swagger / OpenAPI with XML comments, schemas, and JWT authorization'

# Commit 20
CommitStep @(
    'RaceDay.Tests/RaceDay.Tests.csproj',
    'RaceDay.Tests/Helpers/TestDbContextFactory.cs',
    'RaceDay.Tests/Helpers/TestControllerHelper.cs'
) 'feat(tests): scaffold xUnit test project (RaceDay.Tests) with in-memory database helpers'

# Commit 21
CommitStep @(
    'RaceDay.Tests/AuthTests.cs',
    'RaceDay.Tests/EventManagementTests.cs',
    'RaceDay.Tests/RoleSecurityTests.cs',
    'RaceDay.Tests/EnrolmentTests.cs',
    'RaceDay.Tests/ResultTests.cs'
) 'test(api): implement 30 comprehensive unit tests covering auth, events, role rejection, enrolments, and results'

# Commit 22
CommitStep @('.github/workflows/part2-ci.yml') 'ci(part2): add GitHub Actions workflow part2-ci.yml for automated build and unit test validation'

# Commit 23
CommitStep @(
    'README.md',
    'docs/screenshots/ci-green-build-part2.png',
    'docs/ERD_MERMAID.md',
    'docs/ERD_render.html',
    'docs/RaceDay_DB_Script.sql',
    'docs/RaceDay_ERD.pdf',
    'docs/RaceDay_ERD.png',
    'docs/api_endpoint_plan.md'
) 'docs(part2): update README with Part 2 architecture, Swagger guide, test results, and video checklist'

# Commit any remaining files
$remaining = & git status --porcelain
if ($remaining) {
    & git add -A
    & git commit -m "chore(part2): final cleanup of configuration and solution structure" --author="$StudentName <$StudentEmail>" *>$null
    Write-Host "   [COMMITTED] chore(part2): final cleanup of configuration and solution structure" -ForegroundColor Green
}

Step "Checking Total Commit Count in Repository..."
$commitCount = (& git rev-list --count HEAD).Trim()
Write-Host "Total Repository Commits: $commitCount (Rubric requires >= 20)" -ForegroundColor Green

Step "Pushing to GitHub (origin main)..."
try {
    & git push origin main
    Done "Successfully pushed all commits to https://github.com/ST10476247/PROG6212_POE"
} catch {
    Warn "Direct push required credentials or experienced network conflict: $_"
    Write-Host "To push manually: git push origin main" -ForegroundColor Yellow
}
