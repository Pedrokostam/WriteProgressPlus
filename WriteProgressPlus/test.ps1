foreach ($item in 1..10) {
    Test-Test -Activity 'Normal, default'
    foreach ($sub in 1..20) {
        Test-Test -Id 2 -ParentId 1 -Activity 'Sub, Default'
        Start-Sleep -Milliseconds 250
    }
}
Reset-ProgressPlus -id 1
foreach ($item in 1..10) {
    Test-Test -Activity 'Normal, default, total' -TotalCount 10
    foreach ($sub in 1..20) {
        Test-Test -Id 2 -ParentId 1 -Activity 'Sub, Default, total' -TotalCount 20
        Start-Sleep -Milliseconds 250
    }
    Reset-ProgressPlus -id 2
}
Reset-ProgressPlus -id 1