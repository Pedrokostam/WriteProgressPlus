
$SetIterMax = 100
$SetIterTest = 10
Write-Host @'
P   - switch percent
T   - switch Total
E   - switch ETA
C   - switch current
H   - switch HideObject
S   - switch Style
I   - set iteration to 100
0-9 - set increment
'@

$params = @{
    NoCounter        = $false
    NoETA            = $false
    NoPercentage     = $false
    TotalCount       = $SetIterMax
    HideObject       = $false
    CurrentIteration = -1
    Increment        = 1 
}
while ($true) {

    if ([Console]::KeyAvailable) {
        # read the key, and consume it so it won't
        # be echoed to the console:
        $keyInfo = [Console]::ReadKey($true)
        # exit loop
    }
    if ($keyInfo) {

        if ($keyInfo.KeyChar -eq 'p') {
            $params['NoPercentage'] = -not $params['NoPercentage'] 
        }
        if ($keyInfo.KeyChar -eq 'e') {
            $params['NoETA'] = -not $params['NoETA'] 
        }
        if ($keyInfo.KeyChar -eq 'c') {
            $params['NoCounter'] = -not $params['NoCounter'] 
        }
        if ($keyInfo.KeyChar -eq 't') {
            $params['TotalCount'] = if ($params['TotalCount'] -eq -1) { $SetIterMax } else { -1 }
        }
        if ($keyInfo.KeyChar -eq 'h') {
            $params['HideObject'] = -not $params['HideObject'] 
        }
        if ($keyInfo.KeyChar -eq 's') {
            $PSStyle.Progress.View = if ($PSStyle.Progress.View -eq 'Minimal') { 'Classic' }else { 'Minimal' }
            Clear-Host
        }
        if ($keyInfo.KeyChar -eq 'i') {
            $params['CurrentIteration'] = $SetIterTest
        }
        if ($keyInfo.KeyChar -in @('0', '1', '2', '3', '4', '5', '6', '7', '8', '9')) {
            $params['Increment'] = [int]$keyInfo.KeyChar.tostring()
        }
        $keyInfo = $false
    }
    Wripro @params -inputObject (Get-Date)
    Start-Sleep -Milliseconds 200
    $params['CurrentIteration'] = -1
}