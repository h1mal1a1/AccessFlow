namespace AccessFlow.Application.Clients.Exceptions;

public class ClientNotFoundException(long id) : Exception($"Client with id = {id} was not found")
{

}