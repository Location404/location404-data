FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Location404.Data.API/Location404.Data.API.csproj", "src/Location404.Data.API/"]
COPY ["src/Location404.Data.Application/Location404.Data.Application.csproj", "src/Location404.Data.Application/"]
COPY ["src/Location404.Data.Domain/Location404.Data.Domain.csproj", "src/Location404.Data.Domain/"]
COPY ["src/Location404.Data.Infrastructure/Location404.Data.Infrastructure.csproj", "src/Location404.Data.Infrastructure/"]
RUN dotnet restore "src/Location404.Data.API/Location404.Data.API.csproj"
COPY . .
WORKDIR "/src/src/Location404.Data.API"
RUN dotnet build "./Location404.Data.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Location404.Data.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Location404.Data.API.dll"]
