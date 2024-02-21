---
external help file: WriteProgressPlus.dll-Help.xml
Module Name: WriteProgressPlus
online version:
schema: 2.0.0
---

# Write-ProgressPlus

## SYNOPSIS
Show a progress bar which can calculate estimated time and percentage.

## SYNTAX

```
Write-ProgressPlus [-ID <Int32>] [-ParentID <Int32>] [-Activity <String>] [-TotalCount <Int32>]
 [-Increment <Int32>] [-CurrentIteration <Int32>] [-InputObject <Object>] [-DisplayScript <ScriptBlock>]
 [-DisplayProperties <String[]>] [-DisplayPropertiesSeparator <String>] [-HideObject] [-NoCounter]
 [-NoPercentage] [-NoETA] [-PassThru] [<CommonParameters>]
```

## DESCRIPTION
Show a progress bar which can calculate estimated time and percentage.

Works similarly to Write-Progress, but automates many things, including:

- iteration count can incremented automatically;
- percent complete can be calculated automatically, if given -TotalCount;
- estimated time to completion can be calculated automatically, if given -TotalCount;
- can be used as a step in pipeline, passing through all input objects;
- automatic create of bar status, which allows custom formatting and removing parts of it.

## EXAMPLES

### Example 1
```powershell
PS C:\>1..100 | Write-ProgressPlus | % {Start-Sleep -seconds 1}
```

Will display a progress which will update 100 times, displaying number of iterations passed.

### Example 2
```powershell
PS C:\>1..100 | Write-ProgressPlus -TotalCount 100 | % {Start-Sleep -seconds 1}
```

Will display a progress which will update 100 times, displaying number of iterations passed.

Percentage of completion will be displayed, as well as estimated time to completion.

### Example 3
```powershell
PS C:\>1..100 | % {get-date} | Write-ProgressPlus -TotalCount 100 -DisplayProperties Hour, Minute, Second -DisplayPropertiesSeparator ":" | % {Start-Sleep -seconds 1}
```

Will display a progress which will update 100 times, displaying number of iterations passed.

The status will contain current time in format HH:MM:SS.

Percentage of completion will be displayed, as well as estimated time to completion.

### Example 4
```powershell
PS C:\>1..100 | % {get-date} | Write-ProgressPlus -TotalCount 100 -DisplayProperties *second | % {Start-Sleep -seconds 1}
```

Will display a progress which will update 100 times, displaying number of iterations passed.

The status will contain Millisecond, Microsecond, Nanosecond, Second - separated by ", "

Percentage of completion will be displayed, as well as estimated time to completion.

### Example 5
```powershell
PS C:\>100..300 | Write-ProgressPlus -TotalCount 200 -DisplayScript {[math]::sqrt($_)} | % {Start-Sleep -seconds 1}
```

Will display a progress bar which will update 200 times, displaying number of iterations passed.

The status will contain the square root of current object (10, 10.0498756211, 10.0995049384, ...)

Percentage of completion will be displayed, as well as estimated time to completion.

### Example 6
```powershell
PS D:\>ls -Directory | WriPro -ID 1 -Properties Name | % {$_ | ls -File | WriPro -ParentID 1 -ID 2 -Properties Name | %{Start-Sleep -Milliseconds 250; $_}}
```

Will display 2 nested progress bars.

The main one will updated for each folder showing folder name in its status.

The nested one will updated for every file in currently processed folder.

### Example 6
```powershell
PS D:\>foreach($i in 1..100){
	Start-Sleep -Milliseconds 500
	Write-ProgressPlus -ID 1 -InputObject $i -TotalCount 100
}
Reset-ProgressPlus -ID 1
```

Same progress bar as in Example 2, but not in pipeline.

If Reset-ProgressPlus is not called, subsequent calls to this bar ID will continue from 100.

## PARAMETERS

### -ID
Unique ID of progress bar. Used for nesting progress bars.

While IDs is shared with ordinary Write-Progress, 
this module offsets all IDs, so there should not be any conflict.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ParentID
ID of parent progress bar. Used to create sub-bars.

To make parent independent, set to a negative value.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: -1
Accept pipeline input: False
Accept wildcard characters: False
```

### -Activity
Activity description. Will be showed before progress bar.

Equivalent of Activity of Write-Progress

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: "Processing..."
Accept pipeline input: False
Accept wildcard characters: False
```

### -TotalCount
Total count of expected iterations.

If positive, will enable showing percent done (and accurate progress length) and time remaining.

If at any time iteration exceeds TotalCount, command will continue wokring, but a warning will be displayed in status.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: Count

Required: False
Position: Named
Default value: -1
Accept pipeline input: False
Accept wildcard characters: False
```

### -Increment
How much to increase the CurrentIteration if it was not specified.

If CurrentIteration is specified, Increment is ignored. Set to zero to freeze the progress bar.

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: 1
Accept pipeline input: False
Accept wildcard characters: False
```

### -CurrentIteration
Overrides the calculated iteration.

Works similar to its analogue in WriteProgress

```yaml
Type: Int32
Parameter Sets: (All)
Aliases: Iteration

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -InputObject
Current object. If specified, can be used for formatting status.

```yaml
Type: Object
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByValue)
Accept wildcard characters: False
```

### -DisplayScript
Scriptblock used for formatting status string.

The script receives 4 parameters: InputObject, CurrentIteration, PercentDone, TotalCount.

Use $Args[0] to $Args[3] to access them. Alternatively, you can use their aliases: $_, $c, $p, $t, respectively.

Example - for CurrentIteration = 12; TotalCount = 200, InputObject "Test"

   {$_}        gives "Test"
   
   {$_.Length} gives "4"
   
   {"$c / $t"} gives "12 / 200"
   
   {$p}        gives "6" (notice lack of percent sign)

Will override DisplayProperties.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases: Script

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayProperties
List of property names of the input object to format into status.

You can use wildcard, for example if the InputObject is a DateTime, specifying *second will give both Seconds and Milliseconds.

Overriden by DisplayScript.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: Properties

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: True
```

### -DisplayPropertiesSeparator
If DisplayProperties are specified, this string will be used to join them.

```yaml
Type: String
Parameter Sets: (All)
Aliases: Separator

Required: False
Position: Named
Default value: ", "
Accept pipeline input: False
Accept wildcard characters: False
```

### -HideObject
If specified, hides object (and its formatting) from status

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoCounter
If specified, hides counter from status.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoETA
If specified, hides ETA from status.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -NoPercentage
If specified, hides percentage from status.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: false
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeepState
If specified, progress bar status will be preserved across different commands (the ones listed in Get-History).

By default, the state is removed when the calling instance is from a different command - this parameter can prevent that.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases: Persist

Required: False
Position: Named
Default value: false
Accept pipeline input: False
Accept wildcard characters: False
```

### -PassThru
if specified, will emit the input object.

If this command is in the middle of a pipeline, this parameter if set to true and cannot be changed

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: true for pipeline mode, false otherwise
Accept pipeline input: False
Accept wildcard characters: False
```


### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Object
	Any object can be inputted.

## OUTPUTS

### System.Object
	If -PassThru is true (or used in the middle of a pipeline), the input object is outputted, otherwise nothing is.

## NOTES

## RELATED LINKS
