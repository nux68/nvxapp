# Immagine di nvxapp: client Angular + server .NET in un solo container (punto 6 di nvxapp.server/docs/ANALISI_DOCKER.md).
# Contesto di build = cartella della solution (quella di questo file):
#   docker build -t nvxapp:<tag> .
# Configurazione e segreti NON sono nell'immagine: si passano all'avvio come variabili d'ambiente NVXAPP_*
# (sezione 8 di ANALISI_DOCKER.md), es. con docker compose e un file .env.

# --- 1. build del client Angular -> nvxapp.client/www ---
FROM node:22-bookworm-slim AS web
WORKDIR /src/nvxapp.client
# prima solo package*.json: npm ci resta in cache finche' le dipendenze non cambiano
COPY nvxapp.client/package.json nvxapp.client/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY nvxapp.client/ ./
RUN npx ng build --configuration production

# --- 2. publish del server .NET, con il client in wwwroot ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
# prima solo i progetti: il restore resta in cache finche' i pacchetti non cambiano
COPY nvxapp.server/nvxapp.server.csproj nvxapp.server/
COPY nvxapp.server.data/nvxapp.server.data.csproj nvxapp.server.data/
COPY nvxapp.server.service/nvxapp.server.service.csproj nvxapp.server.service/
RUN dotnet restore nvxapp.server/nvxapp.server.csproj
COPY nvxapp.server/ nvxapp.server/
COPY nvxapp.server.data/ nvxapp.server.data/
COPY nvxapp.server.service/ nvxapp.server.service/
COPY --from=web /src/nvxapp.client/www/ nvxapp.server/wwwroot/
# AngularBuilt=true: salta il target BuildAngular del csproj (il client e' gia' compilato dallo stage web)
RUN dotnet publish nvxapp.server/nvxapp.server.csproj -c Release --no-restore -p:AngularBuilt=true -o /app

# --- 3. runtime ---
# immagine completa (non chiseled/alpine): contiene ICU e tzdata, necessari per RunTime:Culture e RunTime:TimeZone
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app .
# libgssapi-krb5-2: Npgsql la cerca alla prima connessione (senza scrive "Cannot load library libgssapi_krb5.so.2")
# cartella dei log su file (volume nel compose); l'app gira con l'utente non root "app" dell'immagine
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/* \
    && mkdir -p /app/Logs && chown -R $APP_UID /app/Logs
USER $APP_UID

# la porta la apre l'app (RunTime:Port); ASPNETCORE_HTTP_PORTS dell'immagine e' svuotata per non avere due configurazioni
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS= \
    NVXAPP_RunTime__Port=8080 \
    NVXAPP_RunTime__UseHttps=false \
    TZ=Europe/Rome
EXPOSE 8080

ENTRYPOINT ["dotnet", "nvxapp.server.dll"]
