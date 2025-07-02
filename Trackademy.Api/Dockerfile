FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Trackademy.Api/Trackademy.Api.csproj", "Trackademy.Api/"]
COPY ["Trackademy.Application/Trackademy.Application.csproj", "Trackademy.Application/"]
COPY ["Trackademy.Domain/Trackademy.Domain.csproj", "Trackademy.Domain/"]
RUN dotnet restore "Trackademy.Api/Trackademy.Api.csproj"
COPY . .
WORKDIR "/src/Trackademy.Api"
RUN dotnet build "Trackademy.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Trackademy.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Trackademy.Api.dll"]
