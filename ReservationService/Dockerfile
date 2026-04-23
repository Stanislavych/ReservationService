FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ReservationService/ReservationService.csproj", "ReservationService/"]
COPY ["ReservationService.Application/ReservationService.Application.csproj", "ReservationService.Application/"]
COPY ["ReservationService.Domain/ReservationService.Domain.csproj", "ReservationService.Domain/"]
COPY ["ReservationService.Infrastructure/ReservationService.Infrastructure.csproj", "ReservationService.Infrastructure/"]
RUN dotnet restore "./ReservationService/ReservationService.csproj"
COPY . .
WORKDIR "/src/ReservationService"
RUN dotnet build "./ReservationService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ReservationService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReservationService.dll"]