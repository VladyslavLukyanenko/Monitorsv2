FROM mcr.microsoft.com/dotnet/runtime:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["src/ProjectMonitors.Scrapper/ProjectMonitors.Scrapper.csproj", "ProjectMonitors.Scrapper/"]
RUN dotnet restore "src/ProjectMonitors.Scrapper/ProjectMonitors.Scrapper.csproj"
COPY . .
WORKDIR "/src/ProjectMonitors.Scrapper"
RUN dotnet build "ProjectMonitors.Scrapper.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ProjectMonitors.Scrapper.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ProjectMonitors.Scrapper.dll"]
