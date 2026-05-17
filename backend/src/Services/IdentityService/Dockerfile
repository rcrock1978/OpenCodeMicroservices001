# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/Shared/SaaSCommon/ src/Shared/SaaSCommon/
COPY src/Services/IdentityService/ src/Services/IdentityService/

RUN dotnet restore src/Services/IdentityService/IdentityService.csproj
RUN dotnet publish src/Services/IdentityService/IdentityService.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "IdentityService.dll"]
