using System.Runtime.CompilerServices;
using H.Wallet.Api.Exceptions;
using H.Wallet.Api.Models;

namespace H.Wallet.Api.middlewares;

public class ExceptionHandler
{
    private readonly RequestDelegate _next;

    public ExceptionHandler(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch(ServiceException ex)
        {
            string msg = ex.Response.ToString();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)ex.Code;

            await context.Response.WriteAsync(msg);
        }
        catch (Exception e)
        {
            string msg = new ApiResponse
            {
                Success = false, Message = "An error occurred, please try again later."
            }.ToString();
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(msg);

            throw new RuntimeWrappedException(e);
        }
    }
}