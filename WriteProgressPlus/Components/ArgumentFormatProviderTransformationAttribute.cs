using System;
using System.Collections.Generic;
using System.Globalization;
using System.Management.Automation;
using System.Text;

namespace WriteProgressPlus.Components;
internal class ArgumentFormatProviderTransformationAttribute : ArgumentTransformationAttribute
{
    public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
    {
        IFormatProvider provider = inputData switch
        {
            string name => new CultureInfo(name),
            int num => new CultureInfo(num),
            IFormatProvider formatProvider => formatProvider,
            _ => CultureInfo.CurrentCulture,
        };
        return provider;
    }
}
