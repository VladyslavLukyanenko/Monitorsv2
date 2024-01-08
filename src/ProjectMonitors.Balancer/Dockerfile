FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY . .
RUN dotnet restore "/app/src/ProjectMonitors.Balancer/ProjectMonitors.Balancer.csproj"
RUN dotnet build "/app/src/ProjectMonitors.Balancer/ProjectMonitors.Balancer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/app/src/ProjectMonitors.Balancer/ProjectMonitors.Balancer.csproj" -c Release -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -r linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectMonitors.Balancer.dll"]