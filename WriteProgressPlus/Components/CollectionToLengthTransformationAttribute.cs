using System.Collections;
using System.Globalization;
using System.Management.Automation;

namespace WriteProgressPlus.Components;

internal class CollectionToLengthTransformationAttribute : ArgumentTransformationAttribute
{
    public override object Transform(EngineIntrinsics engineIntrinsics, object inputData)
    {
        try
        {
            return Convert.ToInt32(inputData, CultureInfo.CurrentCulture);
        }
        catch (Exception e) when (e is FormatException || e is InvalidCastException || e is OverflowException)
        {
            if (inputData is not ICollection collection)
            {
                throw new ArgumentException("Argument must be either convertible to int or an ICollection", nameof(inputData));
            }
            return collection.Count;
        }
    }
}
