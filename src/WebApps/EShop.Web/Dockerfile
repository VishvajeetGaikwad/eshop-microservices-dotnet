FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/WebApps/EShop.Web/EShop.Web.csproj", "src/WebApps/EShop.Web/"]
RUN dotnet restore "src/WebApps/EShop.Web/EShop.Web.csproj"
COPY . .
WORKDIR "/src/src/WebApps/EShop.Web"
RUN dotnet publish "EShop.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/WebApps/EShop.Web/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
