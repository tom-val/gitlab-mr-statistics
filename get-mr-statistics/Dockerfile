FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["get-mr-statistics/get-mr-statistics.csproj", "get-mr-statistics/"]
RUN dotnet restore "get-mr-statistics/get-mr-statistics.csproj"
COPY . .
WORKDIR "/src/get-mr-statistics"
RUN dotnet build "get-mr-statistics.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "get-mr-statistics.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "get-mr-statistics.dll"]
