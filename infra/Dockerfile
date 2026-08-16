FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY api/TechStoreCloud.Api/TechStoreCloud.Api.csproj ./TechStoreCloud.Api/
RUN dotnet restore TechStoreCloud.Api/TechStoreCloud.Api.csproj

COPY api/TechStoreCloud.Api/ ./TechStoreCloud.Api/
RUN dotnet publish TechStoreCloud.Api/TechStoreCloud.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos '' appuser
USER appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 5000

ENTRYPOINT ["dotnet", "TechStoreCloud.Api.dll"]
