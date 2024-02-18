[CmdletBinding()]
param([string]$OutputDir, [string]$Name, [string]$Version)
Write-Host "Running post-build event on $outputdir"

Write-Host "Setting version in manifest to $version"
Update-ModuleManifest -Path (Join-Path $OutputDir WriteProgressPlus.psd1) -ModuleVersion $Version -ea stop

$packageFolder = Join-Path $OutputDir $Name
Write-Host "Copying files to $packageFolder"
if (Test-Path $packageFolder) {
    Remove-Item $packageFolder -Force -Recurse -ea stop
}
$null = New-item -ItemType Directory $packageFolder -ea stop
Get-ChildItem $OutputDir -File | Copy-Item -Destination $packageFolder -ea stop
#dotnet publish "$PSScriptRoot\WriteProgressPlus\WriteProgressPlus.csproj" --framework:net46 --configuration:Release --output:"$PSScriptRoot\publish\"
