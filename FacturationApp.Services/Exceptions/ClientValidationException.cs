namespace FacturationApp.Services.Exceptions;

public class ClientValidationException : Exception
{
    public ClientValidationException(string message) : base(message) { }
}
