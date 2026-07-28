# syntax=docker/dockerfile:1

# --- Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore y publish en un solo paso: con el .csproj copiado por separado
# (restore aislado + publish --no-restore) el SDK no generaba bien los
# static web assets del framework (blazor.web.js quedaba fuera de
# /app/publish/wwwroot/_framework). Se pierde algo de cacheo de capas
# (cualquier cambio de codigo repite el restore de NuGet), pero es el
# precio de que el build sea confiable.
COPY RodcastInvoiceApp/RodcastInvoiceApp.Web/ RodcastInvoiceApp/RodcastInvoiceApp.Web/
RUN dotnet publish RodcastInvoiceApp/RodcastInvoiceApp.Web/RodcastInvoiceApp.Web.csproj \
    -c Release -o /app/publish

# --- Runtime ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# QuestPDF usa SkiaSharp para generar los PDF; sin fuentes instaladas en el
# sistema, un contenedor Linux limpio tira una excepcion al generar el primer
# PDF (factura/timesheet). fonts-liberation da fuentes basicas tipo Arial/Times.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libfontconfig1 fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Claves de Data Protection (cookies de sesion + contraseñas SMTP encriptadas
# en "Mi correo"): quedan por defecto en /app/keys. VOLUME hace que Docker le
# cree un volumen aunque el "docker run" no pase un -v explicito, para que un
# redeploy no invalide sesiones ni borre las contraseñas guardadas. Igual se
# recomienda nombrar el volumen a mano (-v rodcast-keys:/app/keys) para poder
# identificarlo/respaldarlo despues.
ENV DataProtection__KeysPath=/app/keys
VOLUME /app/keys

ENTRYPOINT ["dotnet", "RodcastInvoiceApp.Web.dll"]
