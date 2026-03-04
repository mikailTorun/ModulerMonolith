using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Infrastructure.Http;
using ModulerMonolith.Infrastructure.Mediator;
using ModulerMonolith.Infrastructure.Outbox;
using ModulerMonolith.Infrastructure.Persistence;

namespace ModulerMonolith.Infrastructure;

public static class InfrastructureModule
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        AppDbContext.RegisterModuleAssembly(typeof(InfrastructureModule).Assembly);

        services.AddScoped<IOutboxService, OutboxService>();
        services.AddHostedService<OutboxProcessor>();
        services.AddHttpClient("n8n");

        services.AddScoped<TransactionBehavior>();
        services.AddScoped<IMediator, Mediator.Mediator>();

        services.AddProblemDetails();
        services.AddExceptionHandler<ValidationExceptionHandler>();
        services.AddExceptionHandler<GeneralExceptionHandler>();

        services.AddHttpLogging(options =>
        {
            options.LoggingFields =
                HttpLoggingFields.RequestMethod |
                HttpLoggingFields.RequestPath |
                HttpLoggingFields.RequestQuery |
                HttpLoggingFields.RequestBody |
                HttpLoggingFields.ResponseStatusCode |
                HttpLoggingFields.ResponseBody |
                HttpLoggingFields.Duration;

            options.MediaTypeOptions.AddText("application/json");
            options.RequestBodyLogLimit  = 4096;
            options.ResponseBodyLogLimit = 4096;
            options.CombineLogs = true;
        });

        return services;
    }
}
