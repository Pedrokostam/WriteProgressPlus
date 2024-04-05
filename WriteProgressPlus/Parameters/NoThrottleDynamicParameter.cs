using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace WriteProgressPlus.Parameters;
public class NoThrottleDynamicParameter
{
    [Parameter]
    public SwitchParameter NoThrottle { get; set; }
}
