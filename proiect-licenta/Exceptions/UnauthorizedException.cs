namespace proiect_licenta.Exceptions;

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message){}
}