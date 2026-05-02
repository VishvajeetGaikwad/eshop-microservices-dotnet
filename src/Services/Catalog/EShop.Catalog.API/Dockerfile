FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Install MongoDB for Mongo2Go embedded database
RUN apt-get update && \
    apt-get install -y gnupg curl && \
    curl -fsSL https://www.mongodb.org/static/pgp/server-7.0.asc | gpg --dearmor -o /usr/share/keyrings/mongodb-server-7.0.gpg && \
    echo "deb [ signed-by=/usr/share/keyrings/mongodb-server-7.0.gpg ] http://repo.mongodb.org/apt/debian bookworm/mongodb-org/7.0 main" | tee /etc/apt/sources.list.d/mongodb-org-7.0.list && \
    apt-get update && \
    apt-get install -y mongodb-org-server && \
    apt-get clean && rm -rf /var/lib/apt/lists/*

# Create directory structure that Mongo2Go expects
RUN mkdir -p /app/tools/mongodb-linux-x86_64/bin && \
    ln -s /usr/bin/mongod /app/tools/mongodb-linux-x86_64/bin/mongod && \
    ln -s /usr/bin/mongos /app/tools/mongodb-linux-x86_64/bin/mongos 2>/dev/null; true

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Services/Catalog/EShop.Catalog.API/EShop.Catalog.API.csproj", "src/Services/Catalog/EShop.Catalog.API/"]
COPY ["src/BuildingBlocks/EShop.BuildingBlocks.Common/EShop.BuildingBlocks.Common.csproj", "src/BuildingBlocks/EShop.BuildingBlocks.Common/"]
COPY ["src/BuildingBlocks/EShop.BuildingBlocks.Messaging/EShop.BuildingBlocks.Messaging.csproj", "src/BuildingBlocks/EShop.BuildingBlocks.Messaging/"]
RUN dotnet restore "src/Services/Catalog/EShop.Catalog.API/EShop.Catalog.API.csproj"
COPY . .
WORKDIR "/src/src/Services/Catalog/EShop.Catalog.API"
RUN dotnet build "EShop.Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EShop.Catalog.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EShop.Catalog.API.dll"]
