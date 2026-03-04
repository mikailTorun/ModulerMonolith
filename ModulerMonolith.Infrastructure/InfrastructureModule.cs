using Microsoft.AspNetCore.Diagnostics;
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

        return services;
    }
}
