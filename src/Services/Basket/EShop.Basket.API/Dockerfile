FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Basket/EShop.Basket.API/EShop.Basket.API.csproj", "src/Services/Basket/EShop.Basket.API/"]
COPY ["src/BuildingBlocks/EShop.BuildingBlocks.Common/EShop.BuildingBlocks.Common.csproj", "src/BuildingBlocks/EShop.BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/EShop.BuildingBlocks.Messaging/EShop.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/EShop.BuildingBlocks.Messaging/"]
RUN dotnet restore "src/Services/Basket/EShop.Basket.API/EShop.Basket.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Basket/EShop.Basket.API"
RUN dotnet build "EShop.Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EShop.Basket.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EShop.Basket.API.dll"]
