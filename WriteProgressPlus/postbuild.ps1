[CmdletBinding()]
param([string]$OutputDir, [string]$Name, [string]$Version)
Write-Host "Running post-build event on $outputdir"

write-host "Setting version in manifest to $version"
Update-ModuleManifest -path (Join-Path $OutputDir WriteProgressPlus.psd1) -moduleversion $Version -ea stop

Write-Host 'Moving additional files to test'
$testDir = (Join-Path $OutputDir test)
if (Test-Path $testDir) {
    remove-item $testDir -Recurse -Force
}
$null = New-Item -ItemType Directory $testDir
$null = Move-Item $OutputDir/*.ps1 -Destination $testDir
$archiveFiles = @(
    '*.dll',
    '*.pdb',
    '*.psd1'
    '*.psm1'
    '*.deps*'
)
$archiveFiles = $archiveFiles | ForEach-Object {
    Join-Path $OutputDir $_
}
Write-Host 'Compressing module files'
$zipPath = (Join-Path $OutputDir "$Name.zip")
if (Test-Path $zipPath) {
    Remove-Item $zippath -Recurse -Force
}
$null = Compress-Archive ($archiveFiles) -DestinationPath (Join-Path $OutputDir "$Name.zip")



#$par = @{
#    #framework = 'net7.0'
#    #configuration = 'Release'
#    #output = "$PSScriptRoot\publish\"
#    #interactive = $true
#}
#dotnet publish "$PSScriptRoot\WriteProgressPlus\WriteProgressPlus.csproj" --framework:net46 --configuration:Release --output:"$PSScriptRoot\publish\"
