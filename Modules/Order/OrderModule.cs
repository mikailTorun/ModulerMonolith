using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Order.Application;
using Module.Order.Endpoints;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        AppDbContext.RegisterModuleAssembly(typeof(OrderModule).Assembly);
        services.AddScoped<IOrderService, OrderService>();
        return services;
    }

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapOrderCrudEndpoints();
        return app;
    }
}
