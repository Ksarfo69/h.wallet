using H.Wallet.Api.Exceptions;

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
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("An error occurred, please try again later.");

            Console.WriteLine(e);
        }
    }
}