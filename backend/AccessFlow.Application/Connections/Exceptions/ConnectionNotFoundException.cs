namespace AccessFlow.Application.Connections.Exceptions;

public class ConnectionNotFoundException(long id) : Exception($"Connection with id = {id} was not found")
{

}