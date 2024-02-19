---
external help file: WriteProgressPlus.dll-Help.xml
Module Name: WriteProgressPlus
online version:
schema: 2.0.0
---

# Reset-ProgressPlus

## SYNOPSIS
Reset progress bar status for the specified id, or for all of them.

## SYNTAX

```
Reset-ProgressPlus [-ID <Int32[]>] [-All] [-ProgressAction <ActionPreference>] [<CommonParameters>]
```

## DESCRIPTION
Reset progress bar status for the specified id, or for all of them. 

Only needed when using Write-ProgressPlus in non-pipeline mode.

## EXAMPLES

### Example 1
```powershell
PS C:\> Reset-ProgressPlus -All
```

Resets progress status of all progress bars.

### Example 2
```powershell
PS C:\> Reset-ProgressPlus -ID 1, 2, 4
PS C:\> Reset-ProgressPlus 1, 2, 3
PS C:\> 1, 2, 3 | Reset-ProgressPlus
```

Resets progress status of progress bars with IDs 1, 2, 3

## PARAMETERS

### -All
If specified, the command will remove status for all progress bars started by Write-ProgressPlus.

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
IDs of the progress status to be removed.

```yaml
Type: Int32[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: True (ByPropertyName, ByValue)
Accept wildcard characters: False
```


### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### Int32[]
IDs of progress bars to be reset.
## OUTPUTS

### None
## NOTES

## RELATED LINKS