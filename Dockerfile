# syntax=docker/dockerfile:1

# --- Build ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar solo el .csproj primero: Docker cachea esta capa y no vuelve a
# restaurar paquetes NuGet a menos que el .csproj cambie.
COPY RodcastInvoiceApp/RodcastInvoiceApp.Web/RodcastInvoiceApp.Web.csproj RodcastInvoiceApp/RodcastInvoiceApp.Web/
RUN dotnet restore RodcastInvoiceApp/RodcastInvoiceApp.Web/RodcastInvoiceApp.Web.csproj

COPY RodcastInvoiceApp/RodcastInvoiceApp.Web/ RodcastInvoiceApp/RodcastInvoiceApp.Web/
RUN dotnet publish RodcastInvoiceApp/RodcastInvoiceApp.Web/RodcastInvoiceApp.Web.csproj \
    -c Release -o /app/publish --no-restore

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

ENTRYPOINT ["dotnet", "RodcastInvoiceApp.Web.dll"]
