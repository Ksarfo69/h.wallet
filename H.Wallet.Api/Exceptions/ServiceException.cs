using System.Net;
using H.Wallet.Api.Models;

namespace H.Wallet.Api.Exceptions;

public class ServiceException : Exception
{
    private readonly string _message;
    private readonly HttpStatusCode _code;

    protected ServiceException(string message, HttpStatusCode code)
    {
        _message = message;
        _code = code;
    }

    public HttpStatusCode Code => _code;
    public ApiResponse Response => new ApiResponse {Success = false, Message = _message};
}

public class NotFoundException : ServiceException
{
    public NotFoundException(string message) :  base(message, HttpStatusCode.NotFound)
    {
        
    }
}


public class AlreadyExistsException : ServiceException
{
    public AlreadyExistsException(string message) :  base(message, HttpStatusCode.Conflict)
    {
        
    }
}


public class BadRequestException : ServiceException
{
    public BadRequestException(string message) :  base(message, HttpStatusCode.BadRequest)
    {
        
    }
}


public class UnauthorizedException : ServiceException
{
    public UnauthorizedException(string message) :  base(message, HttpStatusCode.Unauthorized)
    {
        
    }
}


public class ForbiddenException : ServiceException
{
    public ForbiddenException(string message) :  base(message, HttpStatusCode.Forbidden)
    {
        
    }
}