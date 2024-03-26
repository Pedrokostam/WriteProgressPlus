using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace WriteProgressPlus.Components;
internal record SessionRuntime
{
    public SessionRuntime(SessionState sessionState, ICommandRuntime commandRuntime)
    {
        SessionState = sessionState;
        CommandRuntime = commandRuntime;
    }

    public SessionState SessionState { get; }
    public ICommandRuntime CommandRuntime { get; }
}
