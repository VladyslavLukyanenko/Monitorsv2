FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build-env
WORKDIR /monitors-panel
COPY ./src ./
WORKDIR MonitorsPanel.Web.ManagerApi
RUN dotnet restore && dotnet publish -c Release -p:PublishReadyToRun=true -p:PublishReadyToRunShowWarnings=true -r linux-x64 -o ../dist

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build-env /monitors-panel/dist .
ENTRYPOINT ["dotnet", "MonitorsPanel.Web.ManagerApi.dll"]
EXPOSE 443
EXPOSE 80
