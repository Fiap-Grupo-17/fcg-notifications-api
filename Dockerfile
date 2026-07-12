FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Feed local com FCG.Contracts
COPY packages/ packages/
COPY NuGet.Config .

COPY ["src/FCG.NotificationsAPI/FCG.NotificationsAPI.csproj", "src/FCG.NotificationsAPI/"]
RUN dotnet restore "src/FCG.NotificationsAPI/FCG.NotificationsAPI.csproj" \
    --configfile ./NuGet.Config

COPY . .

WORKDIR "/src/src/FCG.NotificationsAPI"
RUN dotnet publish "FCG.NotificationsAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Docker
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FCG.NotificationsAPI.dll"]
