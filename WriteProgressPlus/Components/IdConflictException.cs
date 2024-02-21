using System;
using System.Collections.Generic;
using System.Text;

namespace WriteProgressPlus.Components;
internal class IdConflictException:Exception
{
    public IdConflictException():base($"{nameof(WriteProgressPlusCommand.ParentID)} cannot be the same as {nameof(WriteProgressPlusCommand.ID)}")
    {
        
    }
}
