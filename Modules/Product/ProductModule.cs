using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Product.Application;
using Module.Product.Endpoints;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Product;

public static class ProductModule
{
    public static IServiceCollection AddProductModule(this IServiceCollection services, IConfiguration configuration)
    {
        AppDbContext.RegisterModuleAssembly(typeof(ProductModule).Assembly);
        services.AddScoped<IProductService, ProductService>();
        return services;
    }

    public static IEndpointRouteBuilder MapProductEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapProductCrudEndpoints();
        return app;
    }
}
