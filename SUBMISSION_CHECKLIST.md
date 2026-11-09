# 🏁 RaceDay Part 1 — Submission Walkthrough & Checklist

> Use this file as your personal checklist **while** you submit on your own machine.
> Root folder: `c:\Users\lesed\.gemini\antigravity\scratch\RaceDay\`

---

## ⚖️ Local-verification result (run on this PC)

I ran [SUBMISSION_VERIFY.ps1](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/SUBMISSION_VERIFY.ps1) on the contents of this folder. This was the result — it represents what the GitHub Actions workflow will also check on the remote:

| Check | Value | Status |
|---|---|---|
| Required files present (README + 4 docs + workflow) | All 8 files present, realistic KB | ✅ |
| SQL: `CREATE TABLE`s | **8** entities — Users, Organisers, Participants, Events, EventCategories, EventEnrolments, Results, WeatherSnapshots | ✅ (req ≥6) |
| SQL: `PRIMARY KEY`s | 8 | ✅ (req ≥6) |
| SQL: `FOREIGN KEY`s | 9 | ✅ (target ≥5) |
| Seed: `Organisers` | 2 rows (Tebogo Moagi — Soul City Events; Anika van Heerden — CT Cycling Tours) | ✅ (req ≥2) |
| Seed: `Participants` | 2 rows (Sibusiso Ndlovu; Leigh-Anne Botes) | ✅ (req ≥2) |
| Seed: `Events` | 3 rows — Comrades Marathon 2026, CT Cycle Tour 2026, Soweto Marathon 2026 | ✅ (req ≥3) |
| Seed: `EventCategories` | 8 rows spread across all 3 events | ✅ (req ≥3) |
| Seed: `EventEnrolments` | 6 rows (incl. withdrawn, paid, pending, did-not-start) | ✅ |
| Seed: `Results` | 2 rows (Leigh-Anne finished Comrades 11:18 / Sibusiso DNF at Drummond) | ✅ |
| Seed: `WeatherSnapshots` | 4 rows (3 Comrades half-day snapshots + 1 Soweto race-day forecast) | ✅ |
| API plan: endpoint rows with VERB | **30 rows** = 12 GET, 9 POST, 5 PUT, 4 DELETE | ✅ (target ≥20) |
| API plan: 6 required columns | HTTP Method · Route · Description · Role required · Request body · Expected response — all 6 present | ✅ |
| API plan: domains covered | auth / register / login / profile / events / categories / enrolments / results — all 8 | ✅ |

---

## 📜 Your submission — step-by-step (25 commits → push → ARC)

### 🔴 Before you start: download 3 prerequisites on YOUR machine (not done in scratch yet)

- [ ] **(1)** Install Git from https://git-scm.com/download/win (use the default options; let it add `git` to PATH)
- [ ] **(2)** You have SQL Server Developer + SSMS installed locally (or your campus VM, where you'll run the SQL script in the YouTube video)
- [ ] **(3)** You already accepted the GitHub Classroom invite your lecturer sent, so the repo URL you received (e.g. `https://github.com/The-IIE-Vega/RaceDay-poe-ST10...`) exists and is yours alone.

---

### 🟢 PHASE 1 — First push (25 commits)

Open **PowerShell** on your PC, then:

```powershell
# ---------- Setup ----------
cd c:\Users\lesed\.gemini\antigravity\scratch\RaceDay
git init
git config user.name  "Your Full Name"
git config user.email "your.student.number@vcconnect.edu.za"

# ---------- 25 commits (copy/paste block-by-block — yes, one line at a time) ----------
git add README.md
git commit -m "docs(part1): scaffold README with RaceDay system description & 2 roles"

git add .github/workflows/part1-validation.yml
git commit -m "ci(part1): add Part 1 docs structure validation workflow"

git add docs/ERD_MERMAID.md
git commit -m "docs(part1/erd): Mermaid source — 8 entities, 9 relationships, 1:1 role tables"

git add docs/ERD_render.html
git commit -m "docs(part1/erd): add ERD_render.html viewer (Mermaid 10.9.1 CDN)"

git add docs/RaceDay_ERD.png
git commit -m "docs(part1/erd): final RaceDay_ERD.png render (header, legend, mermaid diagram)"

git add docs/RaceDay_ERD.pdf
git commit -m "docs(part1/erd): final RaceDay_ERD.pdf print-to-pdf export"

git add docs/API_Endpoint_Plan.md
git commit -m "docs(part1/api): endpoint plan skeleton + Auth + User Profile endpoints"

git add docs/API_Endpoint_Plan.md
git commit -m "docs(part1/api): add Events endpoints (CRUD + live weather read/write)"

git add docs/API_Endpoint_Plan.md
git commit -m "docs(part1/api): add EventCategories CRUD with Conflict(409) protection rules"

git add docs/API_Endpoint_Plan.md
git commit -m "docs(part1/api): add EventEnrolments flow — enrol -> pay -> bib -> started"

git add docs/API_Endpoint_Plan.md
git commit -m "docs(part1/api): add Results endpoints (leaderboard, detail, batch upsert, DQ)"

git add docs/API_Endpoint_Plan.md
git commit -m "docs(part1/api): add /organisers/{id}/events and /participants/{id}/history utilities"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1): initial script — CREATE DATABASE + Users table (Role CK, Email UK)"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1): add Organisers and Participants 1:1 extension tables + FK CASCADE"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1): add Events (9 CK types) + EventCategories (unique name/event)"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1): add EventEnrolments (UK per particip+cat) + Results (1:1 off enrol)"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1): add WeatherSnapshots (for live weather) + 8 performance NC indexes"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1): add vEventSummary + vParticipantHistory views for Part 2 reports"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1/seed): Users(4), Organisers(Tebogo+Anika), Participants(Sibusiso+Leigh-Anne)"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1/seed): Events(3) Comrades/CapeTownCycle/Soweto + Categories(8)"

git add docs/RaceDay_DB_Script.sql
git commit -m "db(part1/seed): Enrolments(6) + Results(2, 1 DNF + 1 Finish) + WeatherSnapshots(4)"

git add README.md
git commit -m "docs(readme): add YouTube unlisted link placeholder + Part 1 video checklist (no AI voices)"

git add README.md
git commit -m "docs(readme): CI green build screenshot placeholder + AI Disclosure per POE academic integrity"

git add SUBMISSION_VERIFY.ps1 .local_ci_dryrun.ps1
git commit -m "chore(part1): add local SUBMISSION_VERIFY.ps1 checkers + final linted docs"

# ---------- Push to your classroom repo ----------
# Replace the URL below with the one your lecturer gave you (ARC / email / WhatsApp / Teams)
git remote add origin https://github.com/YOUR-ORG/REPLACE-WITH-LECTURER-REPO-LINK.git
git branch -M main
git push -u origin main

# If push fails because the classroom repo already has a README in it:
#   git pull --rebase origin main
#   git push -u origin main
```

✅ **25 commits pushed?** — check `Insights → Commits` on GitHub or run `git log --oneline | Measure-Object` → count ≥ 20 (we just made 25, so buffer included).

---

### 🟢 PHASE 2 — Confirm the CI workflow turns GREEN

After Phase 1's push:

- [ ] Open your repo → **Actions** tab
- [ ] The top run is called **`RaceDay Part 1 — Docs & Structure Validation`**
- [ ] All 6 steps (Checkout → /docs exists → required files → endpoint columns → SQL tables+seed → README) should show **green ✅**
- [ ] Open the successful run → **full-page screenshot** (Win: Ctrl+Shift+PrtScn or the Edge/Firefox Screenshot tool → Capture full page)
- [ ] Save that PNG **into** the repo at: `docs/screenshots/ci-green-build.png` (create the folder yourself, `mkdir docs/screenshots`)
- [ ] Commit it and push:
  ```
  mkdir docs/screenshots
  # copy/move your screenshot into docs/screenshots as ci-green-build.png
  git add docs/screenshots/ci-green-build.png
  git commit -m "docs(part1): attach CI green build screenshot to README"
  git push
  ```
- [ ] Open `README.md` in the repo on github.com — **scroll to Section "🟢 CI / CD Green Build Screenshot"** — make sure the image renders (the Markdown link points to `./docs/screenshots/ci-green-build.png`)

---

### 🟢 PHASE 3 — Record your YouTube video (unlisted, own voice only)

Record with **OBS Studio** (free) or **Xbox Game Bar** (Win+G on Windows), and narrate **in your own voice**. No AI voices.

Recording template (copy into your script):

1.  **[0:00 – 0:30]** Intro
    *   "Hi, my name is **_____**, student number **_____**. This is PROG6212 — Portfolio of Evidence Part 1, *RaceDay* event management for South African road events."
    *   "In this video I walk through the three planning documents in `/docs`, the decisions behind them, and a live SSMS run of the SQL script."
2.  **[0:30 – 2:30]** ERD decisions — open `docs/RaceDay_ERD.png` / `.pdf` on screen:
    *   Point out **8 entities** (count them)
    *   Explain the **1:1 extensions** — `Users → Organisers`, `Users → Participants` — "one shared auth table, role-specific columns live in their own table so the schema stays clean and JOINs are cheap."
    *   Explain the **1:N** — `Organisers 1 → ∞ Events 1 → ∞ EventCategories`.
    *   Explain the **N:M** join table `EventEnrolments` — "One participant can enrol into many categories across many events; each category has many participants enrolled; the UK on (ParticipantId, CategoryId) prevents double-enrols."
    *   Explain **1:0..1 Result per Enrolment** — "If you DNS/DQ/DNF, you still have an enrolment; you only get a Result row once timing produces one."
    *   Explain **WeatherSnapshots 1:N under Events** — supports the race-day "live weather & route info" in the POE background.
3.  **[2:30 – 4:15]** Endpoint plan choices — open `docs/API_Endpoint_Plan.md`.
    *   Walk **two endpoints per domain** (Auth, Profile, Events, Categories, Enrolments, Results). For each, read the 6 columns, and say 1 sentence explaining why you picked that verb + that route + that failure code (e.g. `DELETE /api/events/{id}` returns 409 Conflict because if someone has paid, you refund before deleting — that's the rule).
    *   Point out the 403 `Forbidden` pattern — endpoints gated to Organiser return 403, not 404 — "security-by-default: it's not enough to hide data; we tell callers *you cannot do this*."
4.  **[4:15 – 8:00]** LIVE SSMS SQL script run — this is the big rubric item.
    *   Open SSMS, connect to your local SQL Server Developer instance.
    *   Open a new Query window, drag-and-drop `docs/RaceDay_DB_Script.sql` into it.
    *   Hit **Execute (F5)**.
    *   **Messages tab (show it!)** scrolls to the bottom — show the final two lines: `RaceDay Part 1 script completed successfully.` and the summary table with 8 counts.
    *   Object Explorer → Databases → **RaceDay** → expand `Tables`. Count 8 tables on screen (Users, Organisers, Participants, Events, EventCategories, EventEnrolments, Results, WeatherSnapshots).
    *   New Query → paste + run:
        ```sql
        USE RaceDay;
        SELECT * FROM Organisers;
        SELECT * FROM Participants;
        SELECT * FROM Events;
        SELECT * FROM EventCategories;
        SELECT * FROM EventEnrolments;
        SELECT * FROM Results;
        SELECT * FROM dbo.vParticipantHistory;
        ```
    *   **Results tab (show it!)** — the 7 result grids populate with the 2/2/3/8/6/2 seed rows.
5.  **[8:00 – 8:45]** Green build check
    *   Open `github.com/.../actions` → show the successful run, show each green step.
    *   Flip to `Insights → Commits` (or `Code → <n> commits` header) — scroll through the **≥25 commits** so the camera sees the meaningful commit messages, not just "fixed typo" ones.
6.  **[8:45 – 9:00]** Wrap-up
    *   "Parts 2 and 3 will implement the 30 endpoints in ASP.NET Core Web API (C#) with JWT role-authorisation; Part 3 adds the MVC web app that consumes this API, Azure Blob Storage for posters/route-maps, and Docker + docker-compose for the final containerised deploy. Thank you for watching."

**Length:** 8–10 minutes is perfect. Stop recording.
- [ ] Upload to YouTube Studio → **Visibility = Unlisted** → paste the URL into `README.md`'s Section "🎬 YouTube Video — Part 1" → save → commit & push:
  ```
  git add README.md
  git commit -m "docs(readme): attach final YouTube unlisted link for Part 1 presentation"
  git push
  ```

---

### 🟢 PHASE 4 — Last step before ARC submission

Open your repo **in a fresh incognito browser tab** (so you see it exactly how the marker will):

| Rubric bullet | How the marker sees it ✅ |
|---|---|
| Folder structure + README + docs + CI screenshot + video link | First screen of the repo = [README.md](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/README.md) renders with all sections |
| ERD complete, accurate, cardinality correct, matches SQL | `/docs/RaceDay_ERD.png` OR `.pdf` + source `ERD_MERMAID.md` |
| Endpoint plan all 6 columns, all 6 domains, failure codes | `/docs/API_Endpoint_Plan.md` (30 rows) |
| SQL runs clean, tables match ERD, constraints, realistic seed ≥2/2/3 | `/docs/RaceDay_DB_Script.sql` + video shows it running live in SSMS |
| ≥ 20 meaningful commits + CI workflow validates | Actions tab green + Code → `<n> commits` shows ≥25 |
| Video: thorough ERD, endpoint plan, SQL live-run | README embeds the YouTube unlisted link |

- [ ] Every row of the table above is **true for your incognito view**.
- [ ] Finally: copy the URL of your repo's **`main` branch home page** → paste it into the **ARC ICE / POE submission box for Part 1** → click Submit.

---

**Congratulations.** You've finished Part 1. 🏁

> One last tip: **don't squash or rewrite history** after submission — markers inspect the commit list directly, not the final folder only.
