#Requires -Modules PlatyPS
#Requires -Version 7
[CmdletBinding()]
param(
    [Parameter(Mandatory)]
    [ValidateNotNullOrWhiteSpace()]
    [string]
    $OutputDir, 
    [Parameter(Mandatory)]
    [ValidateNotNullOrWhiteSpace()]
    [string]
    $Name, 
    [Parameter(Mandatory)]
    [ValidateNotNullOrWhiteSpace()]
    [string]
    $Version, 
    [Parameter(Mandatory)]
    [ValidateNotNullOrWhiteSpace()]
    [string]
    $ProjectDir
)
Write-Host "Running post-build event on $outputdir"

Write-Host "Setting version in manifest to $version"
Update-ModuleManifest -Path (Join-Path $OutputDir WriteProgressPlus.psd1) -ModuleVersion $Version -ea stop

$packageFolder = Join-Path $OutputDir $Name
Write-Host "Copying files to $packageFolder"
if (Test-Path $packageFolder) {
    Remove-Item $packageFolder -Force -Recurse -ea stop
}
$null = New-Item -ItemType Directory $packageFolder -ea stop

Get-ChildItem $OutputDir -File | Copy-Item -Destination $packageFolder -ea stop

$markdownDir = Join-Path $ProjectDir 'docs'

Write-host "Generating help files"
Get-ChildItem $markdownDir -Directory | ForEach-Object {
    $output = Join-Path $OutputDir $_.Name
    $null =New-ExternalHelp -Path $_.FullName -OutputPath $output -Force
    $outputFar = Join-Path $packageFolder $_.Name
    $null = New-ExternalHelp -Path $_.FullName -OutputPath $outputFar -Force
}

#dotnet publish "$PSScriptRoot\WriteProgressPlus\WriteProgressPlus.csproj" --framework:net46 --configuration:Release --output:"$PSScriptRoot\publish\"
