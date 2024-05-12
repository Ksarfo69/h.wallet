using System.Text;
using H.Wallet.Api.Data;
using H.Wallet.Api.Enums;
using H.Wallet.Api.middlewares;
using H.Wallet.Api.Repositories;
using H.Wallet.Api.Services;
using H.Wallet.Api.Utils;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace H.Wallet.Api;

public class Startup
{
    private IConfiguration Configuration { get; set; }
    
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // check all required fields present
        ValidateConfiguration(Configuration);
        
        // check all wallet schemes have a validator
        ValidateWalletSchemeValidators();
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "HWallets", Version = "v1" });
            options.EnableAnnotations();
            
            // Define the Bearer Auth scheme that's in use
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            
            options.OperationFilter<SecureEndpointOperationFilter>(); // only place lock icon on authorized endpoints in UI
        });
    
        // add controllers
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                // so enums don't appear as integers
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"]))
                };
            });

        services.AddAuthorization();
    
        // add logging
        services.AddLogging();
    
        // // add services
        services.AddTransient<IHUserService, HUserService>();
        services.AddTransient<IWalletService, WalletService>();
        
        // add repositories
        services.AddTransient<IHUserRepository, HUserRepository>();
        services.AddTransient<IWalletRepository, WalletRepository>();
        
        // add utils
        services.AddTransient<IValidatorFactory, ValidatorFactory>();
    
        // add db context
        services.AddDbContext<DataContext>(provider => provider.UseInMemoryDatabase("hwallets"));
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataContext dataContext)
    {
        dataContext.Database.EnsureCreated();
    
        // add exception handler
        app.UseMiddleware<ExceptionHandler>();
    
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.html", "V1");
        });
    
        app.UseHttpsRedirection();
    
        app.UseRouting();

        app.UseAuthentication();
        
        app.UseAuthorization();
    
        app.UseEndpoints(e => e.MapControllers());
    }
    
    private static void ValidateConfiguration(IConfiguration configuration)
    {
        var jwtKey = configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            throw new ApplicationException("Critical configuration missing: Jwt:Key");
        }

        if (Encoding.UTF8.GetBytes(jwtKey).Length < 32)
        {
            throw new ApplicationException("Critical configuration requirements not met: Jwt:Key should produce at least 32 bytes.");
        }

        var jwtDuration = configuration["Jwt:Duration"];
        if (string.IsNullOrEmpty(jwtDuration))
        {
            throw new ApplicationException("Critical configuration missing: Jwt:Duration");
        }

        int duration;
        if(!int.TryParse(jwtDuration, out duration)) {
            throw new ApplicationException("Critical configuration requirements not met: Jwt:Duration could not be converted to an int.");
        }

        if (duration < 1)
        {
            throw new ApplicationException("Critical configuration requirements not met: Jwt:Duration cannot be less than 1");
        }
    }
    
    private static void ValidateWalletSchemeValidators()
    {
        foreach (var e in Enum.GetValues<WalletScheme>())
        {
            try
            {
                new ValidatorFactory().GetWalletSchemeValidator(e);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                throw new ApplicationException($"Critical application requirement not met: Validator for {e} not found.");
            }
        }
    }
}