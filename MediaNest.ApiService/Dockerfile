FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 先只複製 csproj 加速還原
COPY ["MediaNest.ApiService/MediaNest.ApiService.csproj", "MediaNest.ApiService/"]
COPY ["MediaNest.ServiceDefaults/MediaNest.ServiceDefaults.csproj", "MediaNest.ServiceDefaults/"]
COPY ["MediaNest.Shared/MediaNest.Shared.csproj", "MediaNest.Shared/"]

RUN dotnet restore "MediaNest.ApiService/MediaNest.ApiService.csproj"

# 複製剩下原始碼
COPY MediaNest.ApiService/ MediaNest.ApiService/
COPY MediaNest.ServiceDefaults/ MediaNest.ServiceDefaults/
COPY MediaNest.Shared/ MediaNest.Shared/

WORKDIR "/src/MediaNest.ApiService"
RUN dotnet publish "MediaNest.ApiService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MediaNest.ApiService.dll"]
