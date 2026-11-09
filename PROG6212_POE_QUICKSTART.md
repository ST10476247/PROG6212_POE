# 🚀 PROG6212_POE — 3 commands to submit Part 1

Your GitHub repo name: **`PROG6212_POE`** ✅ (great name — it scales to all 3 parts of the POE).

Your local work folder (copy it to your laptop / home PC if this is a lab PC):
> `c:\Users\lesed\.gemini\antigravity\scratch\RaceDay`

---

## Step 0 — On YOUR machine: install Git (if not already)

1. Open https://git-scm.com/download/win → click the **64-bit Git for Windows Setup** download.
2. Run installer → **NEXT, NEXT, NEXT, NEXT, NEXT, INSTALL** (keep all defaults).
3. **Reboot or at least log out and back in** so `git.exe` is added to your PATH.
4. Confirm: open PowerShell → run `git --version`. You'll see `git version 2.xx.x.windows.x` — means Git works.

---

## Step 1 — Copy the RaceDay folder onto YOUR machine (skip if this IS your machine)

Copy this whole folder:
```
c:\Users\lesed\.gemini\antigravity\scratch\RaceDay
```
onto your own hard drive, e.g. `C:\Varsity\PROG6212_POE\RaceDay`. Open it, confirm `README.md` is inside.

---

## Step 2 — ONE-CLICK push to your `PROG6212_POE` repo

Open the folder, **right-click the file below → Run with PowerShell**:
```
📄 PUSH_TO_PROG6212_POE.ps1
```

Answer three prompts:

| Prompt | What to enter |
|---|---|
| `Your FULL NAME (as on student card)` | Your **exact** name e.g. `Deogratias Phiri` |
| `Your STUDENT EMAIL (…@vcconnect.edu.za)` | Your registered email e.g. `ST10084741@vcconnect.edu.za` |
| `Paste the HTTPS URL of YOUR PROG6212_POE repo` | Open https://github.com → click your `PROG6212_POE` repo → **copy the URL from the browser address bar** and paste. It looks like either:<br>• `https://github.com/YOUR-USERNAME/PROG6212_POE`<br>• `https://github.com/The-IIE-Vega/PROG6212_POE-YOUR-STUDENT-NO.git` (classroom-assigned) |

Wait 15 seconds → it:
- ✅ `git init`s a clean local repo
- ✅ creates **25 commits** (all under your name/email — marker's audit will pass)
- ✅ pushes everything to your `PROG6212_POE` GitHub repo on `main`.

If push says **"invalid credentials / wrong password"**:
👉 GitHub **deprecated passwords in 2021** — when it asks for your password, paste a **Personal Access Token (PAT)** instead. Make one here (20 seconds, check "repo" scope):
<https://github.com/settings/tokens?type=beta>
*(Permissions = Repository permissions → Contents → Read and write)*. Copy the `github_pat_…` or `ghp_…` string, paste it as your "password" → push proceeds.

If push says **"refusing to merge unrelated histories / conflict"** (you accidentally created the repo with a README/license already):
run
```
git pull --rebase origin main
git push -u origin main
```

---

## Step 3 — Last 3 things (12 minutes total), then ARC submit

### 3a. Attach CI green build screenshot
1. Open your repo on GitHub → click **Actions** (the play-button tab).
2. The topmost workflow run is **`RaceDay Part 1 — Docs & Structure Validation`** → click it → wait for all 6 steps to show green ✅.
3. Full-page screenshot it (Ctrl+Shift+PrtScn or use Edge → … → **Web capture → Capture full page**).
4. Save the PNG **inside your RaceDay folder** as `docs/screenshots/ci-green-build.png` (make the `screenshots` folder yourself).
5. Push it:
   ```powershell
   cd (your RaceDay folder)
   mkdir docs\screenshots -Force
   (copy/move the png here as ci-green-build.png)
   git add docs/screenshots/ci-green-build.png
   git commit -m "docs(part1): CI green-build screenshot"
   git push
   ```
6. Refresh the repo → README.md's "🟢 CI / CD Green Build Screenshot" section should now render the image.

### 3b. YouTube unlisted video (your own voice, **no AI voices**)
- Record with **OBS Studio** (free) or **Xbox Game Bar** (Win+G).
- Script = [SUBMISSION_CHECKLIST.md → PHASE 3](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/SUBMISSION_CHECKLIST.md#-phase-3--record-your-youtube-video-unlisted-own-voice-only-).
- 9-minute minimum walk-through: Intro → ERD decisions → Endpoint plan 2/domain → LIVE SSMS SQL script run → GitHub Actions green build + commit log → Wrap.
- YouTube Studio → Upload → **Visibility = Unlisted** → Save → copy the URL.
- Open [README.md](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/README.md) → scroll to "🎬 YouTube Video — Part 1 (unlisted link)" → replace the placeholder URL with yours.
- Push the change:
  ```powershell
  git add README.md
  git commit -m "docs(readme): attach YouTube unlisted link"
  git push
  ```

### 3c. Submit to ARC
1. Open your repo URL in an **incognito** window.
2. Check each row in the rubric:
   - `/docs` folder contains `RaceDay_ERD.png` (or pdf), `API_Endpoint_Plan.md`, `RaceDay_DB_Script.sql`.
   - `README.md` opens at repo root with all 4 required sections (system description, 2 roles, CI screenshot, YouTube link).
   - **Code → n commits →** last 25 commits all list *you* as author, with meaningful messages.
   - **Actions tab → latest run → passed green.**
3. Copy the URL of that incognito page.
4. Paste it into the ARC submission box for **PROG6212 — Portfolio of Evidence (Part 1)** → click **Submit**.

🏁 Done. Good luck!

---

### Quick file index (everything under `RaceDay/`):

| File | Why you need it |
|---|---|
| [PUSH_TO_PROG6212_POE.ps1](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/PUSH_TO_PROG6212_POE.ps1) | **The only script you need right now.** Run it → makes 25 commits, pushes to PROG6212_POE. |
| [README.md](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/README.md) | Main README the marker reads. |
| [SUBMISSION_CHECKLIST.md](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/SUBMISSION_CHECKLIST.md) | Full 4-phase walkthrough + video speaking script. |
| [SUBMISSION_VERIFY.ps1](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/SUBMISSION_VERIFY.ps1) | Optional: re-run the 6-point local CI check on any PC. |
| [ONLINE_CREATE_REPO.md](file:///c:/Users/lesed/.gemini/antigravity/scratch/RaceDay/ONLINE_CREATE_REPO.md) | 4-click GitHub repo guide. |
| `docs/RaceDay_ERD.png/pdf` | ERD submission artefacts. |
| `docs/API_Endpoint_Plan.md` | Endpoint plan submission artefact. |
| `docs/RaceDay_DB_Script.sql` | Full SSMS-runable schema + seed data. |
| `.github/workflows/part1-validation.yml` | CI workflow that GitHub Actions runs automatically on push. |
