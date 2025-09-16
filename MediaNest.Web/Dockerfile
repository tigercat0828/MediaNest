FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# 複製 csproj（加速還原）
COPY ["MediaNest.Web/MediaNest.Web.csproj", "MediaNest.Web/"]
COPY ["MediaNest.Shared/MediaNest.Shared.csproj", "MediaNest.Shared/"]
COPY ["MediaNest.ServiceDefaults/MediaNest.ServiceDefaults.csproj", "MediaNest.ServiceDefaults/"]


RUN dotnet restore "MediaNest.Web/MediaNest.Web.csproj"

# 複製原始碼
COPY MediaNest.Web/ MediaNest.Web/
COPY MediaNest.Shared/ MediaNest.Shared/
COPY MediaNest.ServiceDefaults/ MediaNest.ServiceDefaults/

WORKDIR "/src/MediaNest.Web"
RUN dotnet publish "MediaNest.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MediaNest.Web.dll"]
