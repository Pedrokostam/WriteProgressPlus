$skryba = 'Jak to jest być skrybą, dobrze? Moim zdaniem to nie ma tak, że dobrze albo że nie dobrze. Gdybym miał powiedzieć, co cenię w życiu najbardziej, powiedziałbym, że ludzi. Ekhm... Ludzi, którzy podali mi pomocną dłoń, kiedy sobie nie radziłem, kiedy byłem sam. I co ciekawe, to właśnie przypadkowe spotkania wpływają na nasze życie. Chodzi o to, że kiedy wyznaje się pewne wartości, nawet pozornie uniwersalne, bywa, że nie znajduje się zrozumienia, które by tak rzec, które pomaga się nam rozwijać. Ja miałem szczęście, by tak rzec, ponieważ je znalazłem. I dziękuję życiu. Dziękuję mu, życie to śpiew, życie to taniec, życie to miłość. Wielu ludzi pyta mnie o to samo, ale jak ty to robisz?, skąd czerpiesz tę radość? A ja odpowiadam, że to proste, to umiłowanie życia, to właśnie ono sprawia, że dzisiaj na przykład buduję maszyny, a jutro... kto wie, dlaczego by nie, oddam się pracy społecznej i będę ot, choćby sadzić... znaczy... marchew.'
function Get-Sequence {
    [CmdletBinding()]
    param (
        [Parameter()]
        [int]
        $Count = 100,
        [Parameter()]
        [int]
        $Milliseconds = 10
    )
    
    $counter = 0
    $words = $skryba -split ' '
    for ($counter=0; $counter -lt $count; $counter++) {
        [PSCustomObject]@{
            Index = $counter
            Time  = [datetime]::Now
            Text  = $words[$counter % $words.count]
        }
        Start-Sleep -Milliseconds $Milliseconds
    }
}
function Get-Hungry {
    [CmdletBinding()]
    param (
        [Parameter(Mandatory, ValueFromPipeline)]
        [object]
        $InputObject
    )
    begin {
        $list = [System.Collections.ArrayList]::new()
    }
    process {
        $null = $list.Add($InputObject)
    }
    end {
        $list
        $list.Count
    }
}

Set-Alias -Name consume -Value Get-Hungry
Set-Alias -Name generate -Value Get-Sequence