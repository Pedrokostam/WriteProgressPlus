using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace WriteProgressPlus.Components;
public class NoThrottleDynamicParameter
{
    [Parameter]
    public SwitchParameter NoThrottle { get; set; }
}
