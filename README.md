# Palleoptimering System - Acies Case

Et ASP.NET Core webapplikation til optimering af pallepakning af døre og vinduer for Acies A/S.

## Indhold

- [Oversigt](#oversigt)
- [Teknologier](#teknologier)
- [Funktionalitet](#funktionalitet)
- [Installation](#installation)
- [Database Setup](#database-setup)
- [API Endpoints](#api-endpoints)
- [Palleoptimering Algoritme](#palleoptimering-algoritme)
- [Testing](#testing)
- [Projektstruktur](#projektstruktur)

## Oversigt

Dette system løser Acies' behov for automatisk optimering af pallepakning. Når kunder har færdigproduceret døre og vinduer til en ordre, skal elementerne pakkes optimalt på paller for at minimere forsendelsesomkostninger og sikre sikker transport.

### Hovedfunktioner

- CRUD operationer for paller, elementer og indstillinger
- Intelligent palleoptimering algoritme
- Konfigurérbare regler for pakning
- REST API til integration med eksisterende systemer
- Omfattende unit tests

## Teknologier

- **Framework:** ASP.NET Core 6.0
- **Database:** MS SQL Server med Entity Framework Core 6.0
- **API Dokumentation:** Swagger/OpenAPI
- **Testing:** XUnit med Moq
- **Arkitektur:** Repository pattern med service layer

## Funktionalitet

### 1. Palle Management

Administrer forskellige typer paller med følgende egenskaber:
- Dimensioner (længde, bredde, højde)
- Vægtbegrænsninger
- Palletype (træ, alu, glasstell)
- Overmål tilladt
- Aktiv/inaktiv status

### 2. Element Management

Håndter elementer (døre/vinduer) med:
- Dimensioner og vægt
- Rotationsregler (Nej, Ja, Skal)
- Mærke for gruppering
- Serie (produktions batch)
- Special krav (palletype, maks elementer pr. palle)

### 3. Palleoptimering Settings

Konfigurér optimeringsalgoritmen:
- Maks antal lag
- Vægt grænser for rotation
- Højde/bredde faktor
- Stabling regler
- Sorteringsprioriteringer

### 4. Pakkeplan Generering

Generér optimal pakkeplan baseret på:
- Liste af elementer
- Valgte settings
- Tilgængelige paller

Outputtet inkluderer:
- Antal paller nødvendigt
- Præcis placering af hvert element
- Lag og plads nummer
- Rotation information

## Installation

### Forudsætninger

- .NET 6.0 SDK
- SQL Server (LocalDB eller fuld installation)
- Visual Studio 2022 eller VS Code

### Trin-for-trin

1. **Klon repository**
   ```bash
   git clone <repository-url>
   cd Eksamen-Acies-case
   ```

2. **Gendan NuGet pakker**
   ```bash
   dotnet restore
   ```

3. **Opdater connection string**

   Rediger `MyProject/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PalleOptimeringDb;Trusted_Connection=True;MultipleActiveResultSets=true"
   }
   ```

4. **Opret database og kør migrationer**
   ```bash
   cd MyProject
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

5. **Kør applikationen**
   ```bash
   dotnet run
   ```

6. **Åbn Swagger UI**

   Naviger til: `https://localhost:5001/swagger`

## Database Setup

### Entity Framework Migrations

Projektet bruger Entity Framework Code-First migrations.

**Opret ny migration:**
```bash
dotnet ef migrations add <MigrationName> --project MyProject
```

**Opdater database:**
```bash
dotnet ef database update --project MyProject
```

**Fjern sidste migration:**
```bash
dotnet ef migrations remove --project MyProject
```

### Seed Data

Databasen seedes automatisk med:
- 3 standard paller (75'er, 80'er, 100'er)
- 1 standard settings profil

## API Endpoints

### Paller

| Method | Endpoint | Beskrivelse |
|--------|----------|-------------|
| GET | `/api/paller` | Hent alle paller |
| GET | `/api/paller/aktive` | Hent kun aktive paller |
| GET | `/api/paller/{id}` | Hent specifik palle |
| POST | `/api/paller` | Opret ny palle |
| PUT | `/api/paller/{id}` | Opdater palle |
| DELETE | `/api/paller/{id}` | Slet palle |

### Elementer

| Method | Endpoint | Beskrivelse |
|--------|----------|-------------|
| GET | `/api/elementer` | Hent alle elementer |
| GET | `/api/elementer/{id}` | Hent specifikt element |
| POST | `/api/elementer` | Opret nyt element |
| POST | `/api/elementer/bulk` | Opret flere elementer |
| PUT | `/api/elementer/{id}` | Opdater element |
| DELETE | `/api/elementer/{id}` | Slet element |

### Settings

| Method | Endpoint | Beskrivelse |
|--------|----------|-------------|
| GET | `/api/settings` | Hent alle settings |
| GET | `/api/settings/aktiv` | Hent aktive settings |
| GET | `/api/settings/{id}` | Hent specifikke settings |
| POST | `/api/settings` | Opret nye settings |
| PUT | `/api/settings/{id}` | Opdater settings |
| DELETE | `/api/settings/{id}` | Slet settings |

### Palleoptimering

| Method | Endpoint | Beskrivelse |
|--------|----------|-------------|
| POST | `/api/palleoptimering/generer` | Generer pakkeplan |
| GET | `/api/palleoptimering/pakkeplaner` | Hent alle pakkeplaner |
| GET | `/api/palleoptimering/pakkeplaner/{id}` | Hent specifik pakkeplan |

### Eksempel Request: Generer Pakkeplan

```json
POST /api/palleoptimering/generer
Content-Type: application/json

{
  "elementIds": [1, 2, 3, 4, 5],
  "ordreReference": "ORD-2024-001",
  "settingsId": 1
}
```

### Eksempel Response: Pakkeplan

```json
{
  "pakkeplanId": 1,
  "antalPaller": 2,
  "antalElementer": 5,
  "status": "Success",
  "meddelelser": ["Pakkeplan genereret med 2 paller"],
  "paller": [
    {
      "palleNummer": 1,
      "palleBeskrivelse": "75'er Træpalle",
      "antalLag": 1,
      "samletVaegt": 125.50,
      "samletHoejde": 350,
      "elementer": [
        {
          "elementId": 1,
          "reference": "Pos-001",
          "lag": 1,
          "plads": 1,
          "erRoteret": false,
          "hoejde": 2100,
          "bredde": 800,
          "vaegt": 45.50
        }
      ]
    }
  ]
}
```

## Palleoptimering Algoritme

### Trin 1: Find Mindste Palle

For hvert element findes den mindste palle som elementet kan passe på baseret på:
- Dimensioner (med eventuel rotation)
- Højdebegrænsninger
- Palletype krav

### Trin 2: Sortering af Elementer

Elementer sorteres efter konfigurerbare kriterier:
1. **Mærke** - Gruppering/optimeringsklump
2. **Specialelement** - Specialelementer først
3. **Pallestørrelse** - Mindste palle først
4. **Elementstørrelse** - Største elementer først
5. **Vægt** - Tungeste elementer først
6. **Serie** - Produktions batches sammen

### Trin 3: Placering på Paller

For hvert element (i sorteret rækkefølge):

1. **Forsøg eksisterende paller** - Placer på første palle hvor det passer
2. **Opret ny palle** - Hvis ingen paller passer

#### Rotationsregler

- Elementer placeres som udgangspunkt på korteste side
- Rotation overvejes baseret på:
  - Element rotationsregel (Nej/Ja/Skal)
  - Vægtgrænse for rotation
  - Højde/bredde faktor
  - Pladshensyn

#### Stablings regler

- Maks antal lag respekteres
- Højdebegrænsninger tjekkes
- Vægtbegrænsninger for stabling
- **Der må aldrig stables ovenpå geometri-elementer**

#### Placering i Rækker

Standard rækkefølge når `PlacerLaengsteElementerYderst = true`:
- **1, 5, 2, 4, 3** - Længste elementer yderst (for foliering)

Alternativ rækkefølge:
- **1, 2, 3, 4, 5** - Vægtfordeling prioriteres

## Testing

### Kør Unit Tests

```bash
cd MyProject.Tests
dotnet test
```

### Test Coverage

Projektet inkluderer tests for:
- **PalleService** - CRUD operationer
- **ElementSorteringHelper** - Sorteringslogik
- **ElementPlaceringHelper** - Placerings- og rotationsregler

### Test med In-Memory Database

Tests bruger Entity Framework In-Memory database provider for hurtige, isolerede tests.

## Projektstruktur

```
MyProject/
├── Controllers/          # API Controllers
│   ├── PallerController.cs
│   ├── ElementerController.cs
│   ├── SettingsController.cs
│   └── PalleOptimeringController.cs
├── Data/                 # Database Context
│   └── PalleOptimeringContext.cs
├── Models/               # Domain Models
│   ├── Palle.cs
│   ├── Element.cs
│   ├── PalleOptimeringSettings.cs
│   └── Pakkeplan.cs
├── Services/             # Business Logic
│   ├── IPalleService.cs / PalleService.cs
│   ├── IElementService.cs / ElementService.cs
│   ├── IPalleOptimeringSettingsService.cs / PalleOptimeringSettingsService.cs
│   ├── IPalleOptimeringService.cs / PalleOptimeringService.cs
│   ├── DTOs/             # Data Transfer Objects
│   └── Helpers/          # Helper Classes
│       ├── ElementSorteringHelper.cs
│       └── ElementPlaceringHelper.cs
└── Program.cs            # Application Entry Point

MyProject.Tests/
└── Services/             # Unit Tests
    ├── PalleServiceTests.cs
    ├── ElementSorteringHelperTests.cs
    └── ElementPlaceringHelperTests.cs
```

## Konfiguration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PalleOptimeringDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Videre Udvikling

### Potentielle Udvidelser

1. **Visualisering** - Babylon.js til 3D visning af pakkeplaner
2. **Detaljeret Stablings Regler** - Implementer tabellen fra 5.3.7.2
3. **Balance Beregning** - Udregn vægtfordeling på paller
4. **Automatisk Omplanlægning** - Optimer eksisterende pakkeplaner
5. **Integration** - API til Acies' eksisterende systemer
6. **Rapportering** - PDF/Excel export af pakkeplaner
7. **Bruger Autentifikation** - Tilføj login og roller

## Support og Bidrag

Dette projekt er udviklet som en eksamenscase for EAMV i samarbejde med Acies A/S.

For spørgsmål kontakt projektteamet.

## Licens

Dette projekt er udviklet til uddannelsesformål.
