﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace WriteProgressPlus;
[Cmdlet(VerbsCommon.Reset, "ProgressPlus")]
[CmdletBinding(PositionalBinding = false, DefaultParameterSetName = "NORMAL")]
public sealed class ResetProgressPlus : ProgressBase
{
    [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    public int[]? ID { get; set; }
    [Parameter]
    public SwitchParameter All { get; set; }

    protected override void BeginProcessing()
    {
        if (All.IsPresent)
        {
            int count = ProgressDict.Count;
            ClearProgressInners();
            WriteVerbose($"Removed all progress bars - {count}");
        }
    }
    protected override void ProcessRecord()
    {
        if (ID != null)
        {
            foreach (int i in ID)
            {
                if (RemoveProgressInner(i))
                    WriteVerbose($"Removed progress bar - {i}");
            }
        }
        else
        {
            ErrorRecord ec = new(
                new ArgumentException("Neither -All nor any ID were specified."),
                "Parameter error",
                ErrorCategory.InvalidArgument,
                this);
            WriteError(ec);
        }
    }
}
