namespace AccessFlow.Application.Connections.Exceptions;

public class ConnectionConflictException(string message) : Exception(message)
{
}