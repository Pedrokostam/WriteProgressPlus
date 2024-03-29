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
Write-Host "`t  Running post-build event on $outputdir"

# ------ MANIFEST
Write-Host "`t  Updating module manifest"
$modulePath = (Join-Path $OutputDir WriteProgressPlus.psd1)
$dllPath = (Join-Path $OutputDir WriteProgressPlus.dll)
$dllContent = powershell -NoProfile -Command "Import-Module '$dllPath';Get-Module WriteProgressPlus | Select-Object ExportedCmdlets, ExportedAliases | ConvertTo-Json -Depth 1" | ConvertFrom-Json -AsHashtable
$dllCmdlets = $dllContent.ExportedCmdlets.Values
$dllAliases = $dllContent.ExportedAliases.Values

$updateParams = [ordered]@{
    ModuleVersion   = $Version
    CmdletsToExport = $dllCmdlets
    AliasesToExport = $dllAliases
    RootModule      = Resolve-Path -Path $dllPath -Relative -RelativeBasePath $OutputDir
    Path            = $modulePath
    ErrorAction     = 'Stop'
}
foreach ($line in $updateParams.GetEnumerator()) {
    Write-Host "`t    $($line.key): $($line.Value)"
}
try {
    $null = Update-ModuleManifest @updateParams -ErrorAction Stop 
    $null = Test-ModuleManifest $modulePath -ErrorAction Stop 
    Write-Host "`t  Manifest is valid"
}
catch {
    Write-host "`t  !!!"
    Write-Host "`t  $($_.Exception.Message)"
    Write-host "`t  !!!"
    Write-Host "`t  Manifest is NOT valid"
    exit
}

# ------ FILE FILTER
$importantFiles = Get-ChildItem $OutputDir -Filter "$name*" -File | Where-Object Extension -NE '.zip'
$filesToDelete = Get-ChildItem $outputdir -Exclude "$name*", 'test*'
$foldersToDelete = Get-ChildItem $outputdir -Directory -Filter "$Name*"
$zipsToDelete = Get-ChildItem $outputdir '*.zip' -File
$itemsToDelete = @($filesToDelete) + @($foldersToDelete) + @($zipsToDelete)

# ------ REMOVE TRASH
if ($itemsToDelete) {
    Write-Host "`t  Deleting old files"
    $itemsToDelete | ForEach-Object { Write-Host "`t    Removing $($_.Name)" }
    $itemsToDelete | Remove-Item -Force -Recurse -ea Stop
}

# ------ HELP
Write-Host "`t  Generating help files"
$markdownDir = Join-Path $ProjectDir 'docs'
Get-ChildItem $markdownDir -Directory | ForEach-Object {
    $output = Join-Path $OutputDir $_.Name
    if (Test-Path $output) {
        Remove-Item $output -Force -Recurse
    }
    $helpFile = New-ExternalHelp -Path $_.FullName -OutputPath $output -Force -ea Stop
    $importantFiles += $helpfile.Directory
}

# ------ PACKAGE DIR
$packageFolder = Join-Path $OutputDir $Name 
if (Test-Path $packageFolder) {
    Remove-Item $packageFolder -Force -Recurse -ea stop
}
$null = New-Item -ItemType Directory $packageFolder -ea stop
$packageFolder_rel = Resolve-Path $packageFolder -Relative -RelativeBasePath $OutputDir
Write-Host "`t  Copying files to $packageFolder_rel"
$importantFiles | ForEach-Object { Write-Host "`t    Copying $($_.Name)" ; $_ } | Copy-Item -Force -Recurse -Destination $packageFolder -ea stop

# ------ ZIP
$zipPath = "$packageFolder.$Version.zip"
Compress-Archive -Path $packageFolder -DestinationPath $zipPath
Write-Host "`t  Compressed package folder ($(Resolve-Path $zipPath -Relative -RelativeBasePath $OutputDir))"

