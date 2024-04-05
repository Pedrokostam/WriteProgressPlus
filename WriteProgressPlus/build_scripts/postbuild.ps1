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
$templateModulePath = (Join-Path $ProjectDir WriteProgressPlus.psd1)
$modulePath = (Join-Path $OutputDir WriteProgressPlus.psd1)
$dllPath = (Join-Path $OutputDir WriteProgressPlus.dll)
$initScriptPath = (Join-Path $OutputDir WriteProgressPlus.ps1)
$dllContent = powershell -NoProfile -Command "Import-Module '$dllPath';Get-Module WriteProgressPlus | Select-Object ExportedCmdlets, ExportedAliases | ConvertTo-Json -Depth 1" | ConvertFrom-Json -AsHashtable
$dllCmdlets = $dllContent.ExportedCmdlets.Values
$dllAliases = $dllContent.ExportedAliases.Values

$relativeDllPath = (Resolve-Path -Path $dllPath -Relative -RelativeBasePath $OutputDir).Replace('.\', '')
$relativeScriptPath = (Resolve-Path -Path $initScriptPath -Relative -RelativeBasePath $OutputDir).Replace('.\', '')
$allFiles = $relativeDllPath, $relativeScriptPath

$templateModule = Test-ModuleManifest $templateModulePath

$moduleParams = [ordered]@{
    AliasesToExport          = $dllAliases
    CmdletsToExport          = $dllCmdlets
    FileList                 = $allFiles
    ModuleVersion            = $Version
    RootModule               = $relativeDllPath
    ScriptsToProcess         = $relativeScriptPath
    Path                     = $modulePath
    Author                   = $templateModule.Author
    ClrVersion               = $templateModule.ClrVersion
    CompanyName              = $templateModule.CompanyName
    CompatiblePSEditions     = $templateModule.CompatiblePSEditions
    Copyright                = $templateModule.Copyright
    Description              = $templateModule.Description
    Guid                     = $templateModule.Guid
    HelpInfoUri              = $templateModule.HelpInfoUri
    LicenseUri               = $templateModule.LicenseUri
    ModuleList               = $templateModule.ModuleList
    NestedModules            = $templateModule.NestedModules
    PowerShellHostName       = $templateModule.PowerShellHostName
    PowerShellHostVersion    = $templateModule.PowerShellHostVersion
    PowerShellVersion        = $templateModule.PowerShellVersion
    ProcessorArchitecture    = $templateModule.ProcessorArchitecture
    ProjectUri               = $templateModule.ProjectUri
    ReleaseNotes             = $templateModule.ReleaseNotes
    RequiredAssemblies       = $templateModule.RequiredAssemblies
    RequiredModules          = $templateModule.RequiredModules
    Tags                     = $templateModule.Tags
    FormatsToProcess         = $templateModule.ExportedFormatFiles
    VariablesToExport        = $templateModule.ExportedVariables.Keys
    RequireLicenseAcceptance = $false
    Prerelease               = $false
    DefaultCommandPrefix     = $null
    DotNetFrameworkVersion   = $null
    DscResourcesToExport     = $null
    FunctionsToExport        = @()
    #IconUri                  = $templateModule.IconUri
    TypesToProcess           = $templateModule.ExportedTypeFiles
    ErrorAction              = 'Stop'
}
foreach ($line in $moduleParams.GetEnumerator()) {
    if ([string]::IsNullOrWhiteSpace($line.Value)) { continue; }
    Write-Host "`t    $($line.key): $($line.Value)"
}
try {
    $null = New-ModuleManifest @moduleParams
    # $updatedContent = Get-Content $modulePath
    # $header = ($updatedContent | Select-Object -First 8) -join "`n"
    # $rest = ($updatedContent | Select-Object -Skip 8) -join "`n"
    # $updatedContent = $header + [regex]::Replace($rest, '(^\s*#.*\n){2,}', "`n", [System.Text.RegularExpressions.RegexOptions]::Multiline)
    # $updatedContent = [regex]::Replace($updatedContent, '(^\s*\n){2,}', "`n", [System.Text.RegularExpressions.RegexOptions]::Multiline)
    # $updatedContent | Set-Content -Path $modulePath
    $null = Test-ModuleManifest $modulePath -ErrorAction Stop 
    Write-Host "`t  Manifest is valid"
} catch {
    Write-Host "`t  !!!"
    Write-Host "`t  $($_.Exception.Message)"
    Write-Host "`t  !!!"
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

