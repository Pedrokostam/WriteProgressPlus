# WriteProgressPlus &ndash; automated Write-Progress extension

[project]:https://github.com/Pedrokostam/WriteProgressPlus
[license]:https://github.com/Pedrokostam/WriteProgressPlus/blob/main/LICENSE.txt
[ps]:https://www.powershellgallery.com/packages/WriteProgressPlus

[![PowerShell Gallery](https://img.shields.io/powershellgallery/dt/WriteProgressPlus.svg)][ps]

[WriteProgress-Plus][project] is a cmdlet that automates and extends the functionalities of the built-in Write-Progress cmdlet.

The defining features of this cmdlet are:

* ability to use in a pipeline directly, without ForEach-Object block;
* automatic calculation of estimated time to completion;
* automatic incrementing of current iteration and percentage calculation;
* automatic creation of status message:
  * the cmdlet accepts an input item and can access all of its properties when creating a status message;
  * user can provide a scriptblock which returns a string or can specify properties of the input objects;
  * the status message may include additional element, all optional:
    * iteration counter;
    * total count of iterations
    * percentage;
* automatic completion and clean-up of progress bar upon exiting command or pipeline;
* ability to keep the state of progress bar between different commands or pipelines;
* built-in throtlling to increase performance when iteration time is very short;

## Examples

#### Simple pipeline
```powershell
100..200 | Write-ProgressPlus | % {Start-Sleep -seconds 1}
```
Will display a progress which will update 100 times, displaying number of iterations passed and current object.

#### Simple pipeline with known count
```powershell
100..200 | Write-ProgressPlus -TotalCount 100 | % {Start-Sleep -seconds 1}
```
As before, but it will also calculate percent done and estimated time to completion.

#### Complex object with specific DisplayProperties
```powershell
1..100 | % {get-date} | Write-ProgressPlus -TotalCount 100 -DisplayProperties Hour, Minute, Second -DisplayPropertiesSeparator ":" | % {Start-Sleep -seconds 1}
```
Similar to previous ones, but the status message will feature current time (HH:MM:SS).

#### Complex object with wildcard DisplayProperties
```powershell
1..100 | % {get-date} | Write-ProgressPlus -TotalCount 100 -DisplayProperties *second -DisplayPropertiesSeparator '; ' | % {Start-Sleep -seconds 1}
```

The status message will feature Millisecond, Microsecond, Nanosecond, and Second - separated by "; ".

#### Triple nested bar
```powershell
foreach($i in 1..10){
     Write-ProgressPlus -Activity 'Outer' -Count 10
     foreach($j in 1..10){
          Write-ProgressPlus -ParentId 1 -Id 2 -Activity 'Middle' -Count 10
          foreach($k in 1..10){
              Write-ProgressPlus -ParentId 2 -Id 3 -Activity 'Inner' -Count 10
              Sleep -Milliseconds 250
          }
          Reset-ProgressPlus -Id 3
     }
     Reset-ProgressPlus -Id 2
}
```

## Installation

Automatically install WriteProgressPlus module from the [PowerShell Gallery][ps]:

```powershell
Install-Module -Name WriteProgressPlus
Import-Module Write-ProgressPlus
```

## Changelog

* [CHANGELOG.md](CHANGELOG.md)
* <https://github.com/Pedrokostam/WriteProgressPlus/releases>.

## License

This project is licensed under the [MIT License][license].
