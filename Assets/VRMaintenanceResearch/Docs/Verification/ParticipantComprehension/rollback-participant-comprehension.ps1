[CmdletBinding()]
param(
    [switch]$Check,
    [string]$Commit
)

$ErrorActionPreference = 'Stop'
$repo = (& git rev-parse --show-toplevel 2>$null).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($repo)) {
    throw 'Run this script from the participant-comprehension repository checkout.'
}
if ([string]::IsNullOrWhiteSpace($Commit)) {
    $Commit = (& git -C $repo log -1 --format=%H -- Assets/VRMaintenanceResearch/Scripts/Interaction/ResearchInteractable.cs).Trim()
}
& git -C $repo cat-file -e "$Commit`^{commit}"
if ($LASTEXITCODE -ne 0) { throw "Unknown commit: $Commit" }
$dirty = @(& git -C $repo status --porcelain)
if ($dirty.Count -ne 0) { throw 'Rollback requires a clean working tree.' }
$marker = @(& git -C $repo diff-tree --no-commit-id --name-only -r $Commit | Where-Object { $_ -eq 'Assets/VRMaintenanceResearch/Scripts/Interaction/ResearchInteractable.cs' })
if ($marker.Count -ne 1) { throw "Commit $Commit is not the participant-comprehension commit." }
if ($Check) {
    Write-Output "ROLLBACK_CHECK=PASS COMMIT=$Commit"
    exit 0
}
& git -C $repo revert --no-edit $Commit
if ($LASTEXITCODE -ne 0) { throw "Rollback failed with git exit code $LASTEXITCODE" }
Write-Output "ROLLBACK_APPLIED=PASS COMMIT=$Commit"
