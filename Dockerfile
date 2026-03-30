# Étape 1 : build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copier uniquement les fichiers nécessaires pour restore
COPY SAN-API.csproj ./
COPY SAN-API.sln ./

# Restore avec la solution explicitement
RUN dotnet restore SAN-API.sln

# Copier tout le reste
COPY . .

# Publish Release
RUN dotnet publish SAN-API.sln -c Release -o out

# Étape 2 : runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/out .

ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Port dynamique pour Render
ENV PORT=5000
EXPOSE $PORT

# Commande de démarrage
ENTRYPOINT ["dotnet", "SAN_API.dll"]