namespace WriteProgressPlus.Exceptions;

internal class IdConflictException : Exception
{
    public IdConflictException() : base($"{nameof(WriteProgressPlusCommand.ParentID)} cannot be the same as {nameof(WriteProgressPlusCommand.ID)}")
    {

    }
}
