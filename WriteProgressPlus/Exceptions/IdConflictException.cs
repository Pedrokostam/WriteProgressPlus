namespace WriteProgressPlus.Exceptions;

internal class IdConflictException : Exception
{
    public IdConflictException() : base($"{nameof(WriteProgressPlusCommand.ParentId)} cannot be the same as {nameof(WriteProgressPlusCommand.Id)}")
    {

    }
}
