$ErrorActionPreference = 'Stop'
Push-Location 'c:\Users\lesed\.gemini\antigravity\scratch\RaceDay'

Write-Output '====== ACTUAL SQL seed verification ======'
$sqlPath = Join-Path (Resolve-Path docs).Path 'RaceDay_DB_Script.sql'
$sql = Get-Content $sqlPath -Raw

function CountSeed([string]$tableName) {
  $m = [regex]::Match($sql, "INSERT \[dbo\]\.\[$tableName\][\s\S]*?VALUES([\s\S]*?);", [Text.RegularExpressions.RegexOptions]::IgnoreCase)
  if (-not $m.Success) { return 0 }
  $block = $m.Groups[1].Value
  $count = 0
  foreach ($line in ($block -split "`r?`n")) {
    if ($line -match '^\s*\(\d+,') { $count++ }
  }
  return $count
}

function Check([string]$label, [int]$actual, [int]$min) {
  $mark = if ($actual -ge $min) {'✅'} else {'❌'}
  Write-Output ('  {0,-45} found={1}  min={2}  {3}' -f $label, $actual, $min, $mark)
}

Check 'Organisers seed rows'  (CountSeed 'Organisers')      2
Check 'Participants seed rows' (CountSeed 'Participants')   2
Check 'Events seed rows'       (CountSeed 'Events')         3
Check 'Categories seed rows'   (CountSeed 'EventCategories') 3
Check 'Enrolments seed rows'   (CountSeed 'EventEnrolments') 1
Check 'Results seed rows'      (CountSeed 'Results')        1
Check 'WeatherSnapshots seeds' (CountSeed 'WeatherSnapshots') 1

Write-Output ''
Write-Output 'CREATE TABLE count:   ' ([regex]::Matches($sql, 'CREATE TABLE', 'IgnoreCase').Count)  + '   (min 6 = ✅)'
Write-Output 'PRIMARY KEY count:    ' ([regex]::Matches($sql, 'PRIMARY KEY', 'IgnoreCase').Count)  + '   (min 6 = ✅)'
Write-Output 'FOREIGN KEY count:    ' ([regex]::Matches($sql, 'FOREIGN KEY', 'IgnoreCase').Count)  + '   (min 5 = ✅)'

Write-Output ''
Write-Output '====== ACTUAL API plan verification ======'
$planPath = Join-Path (Resolve-Path docs).Path 'API_Endpoint_Plan.md'
$plan = Get-Content $planPath -Raw

$verbRows = @()
foreach ($line in ($plan -split "`r?`n")) {
  if ($line -match '^\|\s*(GET|POST|PUT|DELETE|PATCH)\s*\|') { $verbRows += $line.Trim() }
}
Write-Output "Endpoint rows (start with | VERB |):  $($verbRows.Count)    (>=20 = ✅)"
$verbRows | ForEach-Object { if ($_ -match '^\|\s*(GET|POST|PUT|DELETE|PATCH)') { $matches[1] } } | Group-Object | Select-Object Name, Count | Format-Table -AutoSize

Write-Output '6-column check:'
$cols = 'HTTP Method','Route','Description','Role required','Request body','Expected response'
foreach ($c in $cols) {
  $ok = if ($plan.IndexOf($c, [StringComparison]::InvariantCultureIgnoreCase) -ge 0) {'✅'} else {'❌'}
  Write-Output "  $c  : $ok"
}

Pop-Location
