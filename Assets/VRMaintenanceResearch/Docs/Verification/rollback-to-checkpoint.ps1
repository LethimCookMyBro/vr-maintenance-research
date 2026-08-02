[CmdletBinding(SupportsShouldProcess=$true)]
param(
    [string]$Checkpoint = 'D:\TMU_VR\XR-Interaction-Toolkit-Examples-checkpoint-20260802-220535'
)

Set-StrictMode -Version Latest
$repo = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..'))
$source = [IO.Path]::GetFullPath((Join-Path $Checkpoint 'Assets\VRMaintenanceResearch'))
$target = [IO.Path]::GetFullPath((Join-Path $repo 'Assets\VRMaintenanceResearch'))
$repoPrefix = $repo.TrimEnd([char[]]('\')) + '\'

if (-not $target.StartsWith($repoPrefix, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Rollback target is outside the repository: $target"
}
if (-not (Test-Path -LiteralPath $source -PathType Container)) {
    throw "Checkpoint source is missing: $source"
}
if (-not (Test-Path -LiteralPath $target -PathType Container)) {
    throw "Current project tree is missing: $target"
}

$quarantine = [IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $repo) ('VRMaintenanceResearch-rollback-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))))
if (Test-Path -LiteralPath $quarantine) {
    throw "Quarantine path already exists: $quarantine"
}

Write-Output "REPO=$repo"
Write-Output "CHECKPOINT_SOURCE=$source"
Write-Output "CURRENT_TARGET=$target"
Write-Output "QUARANTINE_TARGET=$quarantine"

if ($WhatIfPreference) {
    Write-Output "WHATIF_MOVE=$target -> $quarantine"
    Write-Output "WHATIF_COPY=$source -> $target"
    exit 0
}

New-Item -ItemType Directory -Path $quarantine -Force | Out-Null
Move-Item -LiteralPath $target -Destination $quarantine
Copy-Item -LiteralPath $source -Destination $target -Recurse -Force
if (-not (Test-Path -LiteralPath $target -PathType Container)) {
    throw "Rollback verification failed: $target"
}
Write-Output "ROLLBACK_MOVED_CURRENT=True"
Write-Output "ROLLBACK_RESTORED_CHECKPOINT=True"