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
 [-Increment <Int32>] [-CurrentIteration <Int32>] [-InputObject <Object>] [-NoETA]
 [-DisplayScript <ScriptBlock>] [-DisplayProperties <String[]>] [-DisplayPropertiesSeparator <String>]
 [-HideObject] [-NoCounter] [-NoPercentage] [-PassThru] [-ProgressAction <ActionPreference>]
 [<CommonParameters>]
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
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Activity
Activity description. Will be showed before progress bar.

Equivalent of Activity of Write-Progress

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CurrentIteration
Overrides the calculated iteration.

Works similar to its analogue in WriteProgress

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

### -DisplayProperties
List of property names of the input object to format into status.

You can use wildcard, for example if the InputObject is a DateTime, specifying *seconds will give both Seconds and Milliseconds.

Overriden by DisplayScript.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayPropertiesSeparator
If DisplayProperties are specified, this string will be used to join them.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: ", "
Accept pipeline input: False
Accept wildcard characters: False
```

### -DisplayScript
Scriptblock used for formatting status string.

The script receives 4 parameters: InputObject, CurrentIteration, PercentDone, TotalCount.
Use $Args[0] to $Args[3] to access them. Alternatively, you can use their aliases: $_, $c, $p, $t, respectively.

Will override DisplayProperties.

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
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
if specified, hides ETA from status.

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

### -TotalCount
Total count of expected iterations.

If positive, will enable showing percent done (and accurate progress length) and time remaining.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### System.Object
	Any object can be inputted.
## OUTPUTS

### System.Object
	If -PassThru is true, the input object is outputted, otherwise nothing is.
## NOTES

## RELATED LINKS
