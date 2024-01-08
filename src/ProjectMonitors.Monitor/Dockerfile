FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /app
COPY . .
ARG NUGETUSER
ARG NUGETPASS
RUN dotnet nuget add source --username $NUGETUSER --password $NUGETPASS --store-password-in-clear-text --name github "https://nuget.pkg.github.com/ProjectIndustries/index.json"
RUN dotnet restore "/app/src/ProjectMonitors.Monitor/ProjectMonitors.Monitor.csproj"
RUN dotnet build "/app/src/ProjectMonitors.Monitor/ProjectMonitors.Monitor.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "/app/src/ProjectMonitors.Monitor/ProjectMonitors.Monitor.csproj" -c Release -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -r linux-x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectMonitors.Monitor.dll"]
