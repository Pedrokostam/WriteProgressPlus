using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace WriteProgressPlus.Extensions;

internal static class SessionStateExtensions
{
    public static object GetVariable(this SessionState state, string name, object defaultValue)
    {
        return state.PSVariable.GetValue(name, defaultValue);
    }

    public static object? GetVariable(this SessionState state, string name)
    {
        return state.PSVariable.GetValue(name);
    }

    public static T? GetVariable<T>(this SessionState state, string name)
    {
        var value = state.GetVariable(name);
        if (value is null)
        {
            return default;
        }
        return (T)value;
    }

    public static T GetVariable<T>(this SessionState state, string name, T defaultValue)
    {
        return state.GetVariable(name, defaultValue);
    }
}
