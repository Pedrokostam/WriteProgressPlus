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

    public object GetVariable(string name, object defaultValue)
    {
        return SessionState.PSVariable.GetValue(name, defaultValue);
    }
    public object GetVariable(string name)
    {
        return SessionState.PSVariable.GetValue(name);
    }
    public T? GetVariable<T>(string name)
    {
        return (T)GetVariable(name);
    }

    public T GetVariable<T>(string name, T defaultValue)
    {
        return (T)GetVariable(name, defaultValue);
    }
}
