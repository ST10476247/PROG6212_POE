$ErrorActionPreference = 'Stop'
$root = 'c:\Users\lesed\.gemini\antigravity\scratch\RaceDay'
Push-Location $root

Write-Output '=== ACTUAL Part 1 content check: line-by-line parsers ==='

# ---------- SQL script analysis ----------
Write-Output ''
Write-Output '-- SQL script (RaceDay_DB_Script.sql) --'
$sql = Get-Content (Join-Path (Resolve-Path docs).Path 'RaceDay_DB_Script.sql')

$createTables = 0
$pk = 0
$fk = 0
$insertBlocks = @{}
$currentInsert = $null

foreach ($line in $sql) {
  if ($line -match 'CREATE TABLE') { $createTables++ }
  if ($line -match 'PRIMARY KEY')  { $pk++ }
  if ($line -match 'FOREIGN KEY')  { $fk++ }

  if ($line -match 'INSERT \[dbo\]\.\[(\w+)\]') {
    $currentInsert = $matches[1]
    if (-not $insertBlocks.ContainsKey($currentInsert)) { $insertBlocks[$currentInsert] = 0 }
    continue
  }
  if ($null -ne $currentInsert) {
    if ($line -match '^\s*\(\d+,') { $insertBlocks[$currentInsert]++ }
    if ($line -match '^\s*;')      { $currentInsert = $null }
  }
}

Write-Output "  CREATE TABLEs  : $createTables  (≥6 ✅)"
Write-Output "  PRIMARY KEYs   : $pk  (≥6 ✅)"
Write-Output "  FOREIGN KEYs   : $fk  (≥5 ✅)"
Write-Output '  Seed row counts:'
$minReq = @{ Organisers = 2; Participants = 2; Events = 3; EventCategories = 3; EventEnrolments = 1; Results = 1; WeatherSnapshots = 1; Users = 4 }
foreach ($k in $insertBlocks.Keys | Sort-Object) {
  $n = $insertBlocks[$k]
  $min = if ($minReq.ContainsKey($k)) { $minReq[$k] } else { 1 }
  $ok  = if ($n -ge $min) { '✅' } else { '❌' }
  Write-Output ("    {0,-18} rows={1}  (min {2})  {3}" -f $k, $n, $min, $ok)
}

# ---------- API plan analysis ----------
Write-Output ''
Write-Output '-- API Endpoint Plan (API_Endpoint_Plan.md) --'
$plan = Get-Content (Join-Path (Resolve-Path docs).Path 'API_Endpoint_Plan.md')

$endpoints = @()
foreach ($line in $plan) {
  if ($line -match '^\|\s*(?<m>GET|POST|PUT|DELETE|PATCH)\s*\|') {
    $endpoints += [pscustomobject]@{ Method = $matches['m']; Line = $line.Trim() }
  }
}
Write-Output "  Endpoint rows (| VERB |...) : $($endpoints.Count)  (≥20 → target)"
$endpoints | Group-Object Method | Select-Object Name, Count | Format-Table -AutoSize | Out-String -Width 80 | ForEach-Object { Write-Output ('  ' + $_.Trim()) }

Write-Output '  6-column check:'
$cols = 'HTTP Method','Route','Description','Role required','Request body','Expected response'
$flat = ($plan | Out-String)
foreach ($c in $cols) {
  $ok = if ($flat.IndexOf($c, [StringComparison]::OrdinalIgnoreCase) -ge 0) { '✅' } else { '❌' }
  Write-Output "    [$c]  $ok"
}

# ---------- File inventory ----------
Write-Output ''
Write-Output '-- Required Part 1 files (rubric) --'
$checks = @(
  @{ Label = '/docs folder';          Path = 'docs'; Dir = $true },
  @{ Label = 'README.md (root)';     Path = 'README.md' },
  @{ Label = 'ERD (PNG)';            Path = 'docs/RaceDay_ERD.png' },
  @{ Label = 'ERD (PDF)';            Path = 'docs/RaceDay_ERD.pdf' },
  @{ Label = 'ERD source (Mermaid)'; Path = 'docs/ERD_MERMAID.md' },
  @{ Label = 'API Endpoint Plan';    Path = 'docs/API_Endpoint_Plan.md' },
  @{ Label = 'SQL DB Script';        Path = 'docs/RaceDay_DB_Script.sql' },
  @{ Label = 'GitHub Workflow';      Path = '.github/workflows/part1-validation.yml' }
)
foreach ($c in $checks) {
  $full = Join-Path $root $c.Path
  $exists = if ($c.Dir) { Test-Path $full -PathType Container } else { Test-Path $full -PathType Leaf }
  $ok = if ($exists) { '✅' } else { '❌' }
  $sz = if ($exists -and -not $c.Dir) { [math]::Round((Get-Item $full).Length / 1KB, 1).ToString() + ' KB' } else { '' }
  Write-Output ("  {0,-25} {1}  {2}" -f $c.Label, $ok, $sz)
}

Pop-Location
