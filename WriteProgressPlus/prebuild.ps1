# [CmdletBinding()]
# param([string]$ProjectDir, [string]$Version)
# write-host "Setting version in manifest to $version"
# Update-ModuleManifest -path (Join-Path $ProjectDir WriteProgressPlus.psd1) -version $Version -ea stop