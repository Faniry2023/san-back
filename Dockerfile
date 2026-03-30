# Étape 1 : build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copier tout le projet directement
COPY . .

# Restore + build
RUN dotnet restore SAN-API.csproj
RUN dotnet publish SAN-API.csproj -c Release -o out

# Étape 2 : runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

ENV DOTNET_RUNNING_IN_CONTAINER=true

# Port Render
ENV PORT=5000
EXPOSE $PORT

ENTRYPOINT ["dotnet", "SAN_API.dll"]