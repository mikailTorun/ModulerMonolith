FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ModulerMonolith.Api/ModulerMonolith.Api.csproj ModulerMonolith.Api/
COPY ModulerMonolith.Core/ModulerMonolith.Core.csproj ModulerMonolith.Core/
COPY ModulerMonolith.Infrastructure/ModulerMonolith.Infrastructure.csproj ModulerMonolith.Infrastructure/
COPY Modules/Auth/Module.Auth.csproj Modules/Auth/
COPY Modules/Order/Module.Order.csproj Modules/Order/
COPY Modules/Product/Module.Product.csproj Modules/Product/

RUN dotnet restore ModulerMonolith.Api/ModulerMonolith.Api.csproj

COPY . .

RUN dotnet publish ModulerMonolith.Api/ModulerMonolith.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "ModulerMonolith.Api.dll"]
