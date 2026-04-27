# Hotellivarausjarjestelma

Lakeview-hotellin varausjarjestelman MVP, jossa on .NET 8 Web API -backend ja React + TypeScript + Vite -frontend.

## Teknologiat

- Backend: ASP.NET Core Web API (.NET 8), EF Core 8, SQLite, FluentValidation, Swagger
- Frontend: React 18, TypeScript, Vite, TanStack Query, date-fns
- Testit: xUnit (unit + integration)
- Arkkitehtuuri: Clean Architecture (Api/Application/Domain/Infrastructure)

## Projektin rakenne

- src/HotelLakeview.Api: API, Swagger, CORS, app startup
- src/HotelLakeview.Application: palvelulogiikka + DTO:t + validointi
- src/HotelLakeview.Domain: domain-entiteetit ja enumit
- src/HotelLakeview.Infrastructure: EF Core DbContext + seed-data + DI-rekisterointi
- src/HotelLakeview.Frontend: React-sovellus
- tests/HotelLakeview.UnitTests: yksikkotestit
- tests/HotelLakeview.IntegrationTests: integraatiotestit

## Paikallinen kaynnistys

### 1) Backend

Suorita projektin juuresta:

```powershell
dotnet run --project src/HotelLakeview.Api
```

Swagger:

- http://localhost:5000/swagger

### 2) Frontend

Suorita toisessa terminaalissa:

```powershell
Set-Location src/HotelLakeview.Frontend
npm install
npm run dev
```

Frontend:

- http://localhost:5173

## Build ja testit

### Kokonaisbuild

```powershell
dotnet build HotelLakeview.slnx
```

### .NET-testit

```powershell
dotnet test HotelLakeview.slnx
```

### Frontend build

```powershell
Set-Location src/HotelLakeview.Frontend
npm run build
```

## Frontendin API-asetus

Frontend kayttaa oletuksena API-osoitetta:

- http://localhost:5000

Voit yliajaa sen ymparistomuuttujalla:

- VITE_API_BASE_URL

Esimerkki (PowerShell):

```powershell
$env:VITE_API_BASE_URL = "https://oma-api-osoite"
npm run build
```

## Azure-julkaisu (nykyinen tila)

Sovellus on julkaistu seuraaviin resursseihin:

- Frontend static site: https://stlakeviewhotelaapo.z1.web.core.windows.net
- Backend API: https://lakeviewhotelaapo-bzfgfdhhbjbhg7an.swedencentral-01.azurewebsites.net
- Swagger: https://lakeviewhotelaapo-bzfgfdhhbjbhg7an.swedencentral-01.azurewebsites.net/swagger/index.html

Backendin CORS-originit luetaan asetuksesta:

- Cors:AllowedOrigins

Muoto:

- originit erotetaan puolipisteella (;)

Esimerkki App Service -asetuksessa:

- Cors__AllowedOrigins=https://stlakeviewhotelaapo.z1.web.core.windows.net;http://localhost:5173;http://127.0.0.1:5173

## Huomioita

- SQLite-tietokanta luodaan startupissa EnsureCreated-kutsulla.
- Jos saat build-virheen tyyppia "file is locked", varmista ettei API-prosessi ole kaynnissa buildin aikana.
