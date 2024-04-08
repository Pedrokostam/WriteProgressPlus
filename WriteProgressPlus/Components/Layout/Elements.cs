using System;
using System.Collections.Generic;
using System.Text;

namespace WriteProgressPlus.Components.Layout;

/// <summary>
/// Parts that can be displayed in a progress bar.0
/// </summary>
[Flags]
public enum Elements
{
    None = 0,
    ItemScript = 0b00000001,
    ItemProperties = 0b00000010,
    TimeRemaining = 0b00000100,
    Iteration = 0b00001000,
    TotalCount = 0b00010000,
    Percentage = 0b00100000,
    Counter = Iteration | TotalCount,
    All = ItemScript | ItemProperties | TimeRemaining | Percentage | Counter,
}
