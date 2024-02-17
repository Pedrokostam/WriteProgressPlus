[CmdletBinding()]
param([string]$OutputDir, [string]$Name)
Write-Host "Running post-build event on $outputdir"
Write-Host 'Moving additional files to test'
$null = New-Item -ItemType Directory (Join-Path $OutputDir test)
$null = Move-Item $OutputDir/*.ps1 -Destination $OutputDir/test
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
    Remove-Item $zippath
}
$null = Compress-Archive ($archiveFiles) -DestinationPath (Join-Path $OutputDir "$Name.zip")

#$par = @{
#    #framework = 'net7.0'
#    #configuration = 'Release'
#    #output = "$PSScriptRoot\publish\"
#    #interactive = $true
#}
#dotnet publish "$PSScriptRoot\WriteProgressPlus\WriteProgressPlus.csproj" --framework:net46 --configuration:Release --output:"$PSScriptRoot\publish\"
