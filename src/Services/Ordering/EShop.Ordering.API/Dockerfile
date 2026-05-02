FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Ordering/EShop.Ordering.API/EShop.Ordering.API.csproj", "src/Services/Ordering/EShop.Ordering.API/"]
COPY ["src/Services/Ordering/EShop.Ordering.Application/EShop.Ordering.Application.csproj", "src/Services/Ordering/EShop.Ordering.Application/"]
COPY ["src/Services/Ordering/EShop.Ordering.Domain/EShop.Ordering.Domain.csproj", "src/Services/Ordering/EShop.Ordering.Domain/"]
COPY ["src/Services/Ordering/EShop.Ordering.Infrastructure/EShop.Ordering.Infrastructure.csproj", "src/Services/Ordering/EShop.Ordering.Infrastructure/"]
COPY ["src/BuildingBlocks/EShop.BuildingBlocks.Common/EShop.BuildingBlocks.Common.csproj", "src/BuildingBlocks/EShop.BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/EShop.BuildingBlocks.Messaging/EShop.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/EShop.BuildingBlocks.Messaging/"]
RUN dotnet restore "src/Services/Ordering/EShop.Ordering.API/EShop.Ordering.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Ordering/EShop.Ordering.API"
RUN dotnet build "EShop.Ordering.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EShop.Ordering.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EShop.Ordering.API.dll"]
