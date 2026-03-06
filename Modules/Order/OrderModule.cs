using FluentValidation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Module.Order.Application.Commands;
using Module.Order.Application.Queries;
using Module.Order.Endpoints;
using ModulerMonolith.Core.Mediator;
using ModulerMonolith.Core.Results;
using ModulerMonolith.Infrastructure.Persistence;

namespace Module.Order;

public static class OrderModule
{
    public static IServiceCollection AddOrderModule(this IServiceCollection services, IConfiguration configuration)
    {
        AppDbContext.RegisterModuleAssembly(typeof(OrderModule).Assembly);

        // Commands
        services.AddScoped<ICommandHandler<CreateOrderCommand, Result<Guid>>, CreateOrderCommandHandler>();
        services.AddScoped<ICommandHandler<ConfirmOrderCommand, Result<string>>, ConfirmOrderCommandHandler>();
        services.AddScoped<ICommandHandler<CancelOrderCommand, Result>, CancelOrderCommandHandler>();

        // Queries
        services.AddScoped<IQueryHandler<GetAllOrdersQuery, IEnumerable<Domain.Order>>, GetAllOrdersQueryHandler>();
        services.AddScoped<IQueryHandler<GetOrderByIdQuery, Domain.Order?>, GetOrderByIdQueryHandler>();

        // Validators — assembly scanning (internal tipler dahil)
        services.AddValidatorsFromAssembly(typeof(OrderModule).Assembly, includeInternalTypes: true);

        // Module contract — diğer modüller bu interface üzerinden Order modülüne erişir
        services.AddScoped<IOrderModuleApi, OrderModuleApi>();

        return services;
    }

    public static IEndpointRouteBuilder MapOrderEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapOrderCrudEndpoints();
        return app;
    }
}
