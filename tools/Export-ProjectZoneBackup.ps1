#Requires -Version 5.1
<#
.SYNOPSIS
  Zip PROJECT ZONE production code/scenes/docs (not vendor Asset Store packs).

.EXAMPLE
  powershell -File tools/Export-ProjectZoneBackup.ps1
  powershell -File tools/Export-ProjectZoneBackup.ps1 -OutputRoot "D:\PZ-Backups"
#>
[CmdletBinding()]
param(
    [string]$OutputRoot = ""
)

$ErrorActionPreference = "Stop"
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
Set-Location $repoRoot

if ([string]::IsNullOrWhiteSpace($OutputRoot)) {
    $OutputRoot = Join-Path $repoRoot "Backups"
}

New-Item -ItemType Directory -Force -Path $OutputRoot | Out-Null

$stamp = Get-Date -Format "yyyyMMdd-HHmmss"
$branch = ""
try { $branch = (git rev-parse --abbrev-ref HEAD 2>$null).Trim() } catch { $branch = "unknown" }
$sha = ""
try { $sha = (git rev-parse --short HEAD 2>$null).Trim() } catch { $sha = "nosha" }
$safeBranch = ($branch -replace '[\\/:*?"<>|]', '-')
$zipName = "PZ-code-$stamp-$safeBranch-$sha.zip"
$zipPath = Join-Path $OutputRoot $zipName
$stage = Join-Path $env:TEMP ("pz-backup-stage-" + [guid]::NewGuid().ToString("N"))

New-Item -ItemType Directory -Force -Path $stage | Out-Null

function Copy-Tree([string]$rel) {
    $src = Join-Path $repoRoot $rel
    if (-not (Test-Path $src)) {
        Write-Warning "Skip missing: $rel"
        return
    }
    $dst = Join-Path $stage $rel
    New-Item -ItemType Directory -Force -Path (Split-Path $dst -Parent) | Out-Null
    Copy-Item -Path $src -Destination $dst -Recurse -Force
}

# Production game code + scenes + docs + hub source + package/project config.
# Intentionally omits Assets/* vendor packs and Library/.
Copy-Tree "Assets/_ProjectZone"
Copy-Tree "docs"
Copy-Tree "website"
Copy-Tree "Packages"
Copy-Tree "ProjectSettings"
Copy-Tree ".cursor"
Copy-Tree "tools"

# Drop hub node_modules / build output if present in the copy.
@(
    "website\node_modules",
    "website\dist",
    "website\public\data\hub-snapshot.prev.json"
) | ForEach-Object {
    $p = Join-Path $stage $_
    if (Test-Path $p) { Remove-Item -Recurse -Force $p }
}

if (Test-Path $zipPath) { Remove-Item -Force $zipPath }
Compress-Archive -Path (Join-Path $stage "*") -DestinationPath $zipPath -CompressionLevel Optimal
Remove-Item -Recurse -Force $stage

$sizeMb = [math]::Round((Get-Item $zipPath).Length / 1MB, 2)
Write-Host "Backup written: $zipPath ($sizeMb MB)"
Write-Host "Contains: Assets/_ProjectZone, docs, website (no node_modules), Packages, ProjectSettings, .cursor, tools"
Write-Host "Excluded: vendor Asset Store packs, Library/, Temp/, full LFS media tree"
