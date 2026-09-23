# Build from the solution root:  docker build -f src/Bootstrapper/Api/Dockerfile -t api .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props Directory.Packages.props .editorconfig ./
COPY src/ src/
RUN dotnet restore src/Bootstrapper/Api/Api.csproj

# Pre-generate Wolverine's handler code, so the container does not compile code at startup.
# The connection string is only needed to configure Wolverine; nothing connects to it.
RUN ConnectionStrings__appdb="Host=codegen;Database=codegen" \
    dotnet run --project src/Bootstrapper/Api/Api.csproj --configuration Release --no-restore --no-launch-profile -- codegen write

RUN dotnet publish src/Bootstrapper/Api/Api.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish \
    -p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Api.dll"]
