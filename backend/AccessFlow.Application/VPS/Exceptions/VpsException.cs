namespace AccessFlow.Application.VPS.Exceptions;

public class VpsException(VpsErrorType errorType, string message, Exception? innerException = null) :
    Exception(message, innerException)
{
    public VpsErrorType ErrorType { get; } = errorType;
}