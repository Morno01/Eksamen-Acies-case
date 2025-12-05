# Palleoptimering System - Dokumentation

Denne mappe indeholder teknisk dokumentation for Acies Palleoptimering systemet.

## 📚 Indhold

### [Klassediagram](./klassediagram.md)
Viser systemets MVC arkitektur med:
- Controllers (API endpoints)
- Services (forretningslogik)
- Models (datastrukturer)
- Dependencies og relationer

**Brug dette diagram til at forstå:**
- Hvordan kode er organiseret
- Hvilke services controllers bruger
- Hvordan data flyder gennem systemet

### [ER-Diagram](./er-diagram.md)
Viser database strukturen med:
- Alle tabeller og deres kolonner
- Foreign key relationer
- Kardinalitet (one-to-many, many-to-many)
- Data constraints

**Brug dette diagram til at forstå:**
- Database schema
- Hvordan data relaterer til hinanden
- Hvilke felter der er påkrævet
- Cascade delete regler

## 🖥️ Sådan Ser Du Diagrammerne

### I GitHub
GitHub renderer automatisk Mermaid diagrammer. Klik blot på filerne ovenfor.

### I Visual Studio Code
1. Installer extension: "Markdown Preview Mermaid Support"
2. Åbn .md filen
3. Tryk `Ctrl+Shift+V` for preview

### Online
Kopiér Mermaid koden og indsæt den på: https://mermaid.live/

## 🏗️ System Arkitektur

```
┌─────────────────────────────────────────┐
│          Browser (UI)                   │
│  - Razor Views (.cshtml)                │
│  - JavaScript (fetch API)               │
│  - Bootstrap 5.3                        │
└──────────────┬──────────────────────────┘
               │ HTTP/HTTPS
┌──────────────▼──────────────────────────┐
│       Controllers (API Layer)           │
│  - HomeController                       │
│  - AccountController                    │
│  - PallerController (REST API)          │
│  - ElementerController (REST API)       │
│  - PalleOptimeringController            │
│  - SettingsController                   │
└──────────────┬──────────────────────────┘
               │ Dependency Injection
┌──────────────▼──────────────────────────┐
│      Services (Business Logic)          │
│  - PalleService                         │
│  - ElementService                       │
│  - PalleOptimeringService (Core)        │
│  - PalleOptimeringSettingsService       │
└──────────────┬──────────────────────────┘
               │ Entity Framework Core
┌──────────────▼──────────────────────────┐
│   Data Access (Repository Pattern)      │
│  - PalleOptimeringContext (DbContext)   │
└──────────────┬──────────────────────────┘
               │ SQL Connection
┌──────────────▼──────────────────────────┐
│         Azure SQL Database              │
│  Server: bmm-server.database.windows.net│
│  Database: bmm                          │
└─────────────────────────────────────────┘
```

## 🔐 Sikkerhed og Roller

### Roller
- **SuperUser**: Fuld adgang (admin@acies.dk / admin)
- **NormalUser**: Kun læseadgang (bruger@acies.dk / bruger)

### Authorization
- Alle sider kræver login (`[Authorize]`)
- GET endpoints: SuperUser + NormalUser
- POST/PUT/DELETE endpoints: Kun SuperUser

## 📊 Database Seed Data

### Paller (3 stk)
- 75'er Træpalle (2400x750mm)
- 80'er Træpalle (2400x800mm)
- 100'er Træpalle (2400x1000mm)

### Elementer (8 stk)
- 3 døre (Serie-A)
- 3 vinduer (Serie-B)
- 2 special elementer (Serie-C)

### Settings (1 profil)
- Standard optimerings profil

### Brugere (2 stk)
- admin@acies.dk (SuperUser)
- bruger@acies.dk (NormalUser)

## 🔄 Workflow

### 1. Opret Master Data
```
SuperUser → Paller → Opret nye palle typer
SuperUser → Elementer → Opret elementer til pakning
SuperUser → Settings → Konfigurer regler
```

### 2. Generer Pakkeplan
```
SuperUser → Optimering → Vælg elementer
           ↓
    Vælg settings profil (optional)
           ↓
    Klik "Generer Pakkeplan"
           ↓
    PalleOptimeringService kører algoritme
           ↓
    Resultat vises med paller og placering
```

### 3. Se Resultat
```
SuperUser/NormalUser → Se genererede pakkeplaner
                    → Se hvilke elementer er på hvilke paller
                    → Se antal lag og vægtfordeling
```

## 🧮 Optimeringsalgoritme

Algoritmen i `PalleOptimeringService` følger disse trin:

1. **Sortering**: Sortér elementer efter settings.SorteringsPrioritering
2. **Palle Valg**: Find mindste palle der kan rumme elementerne
3. **Lag Opdeling**: Opdel i lag baseret på MaksLag setting
4. **Rotation**: Bestem om elementer skal roteres baseret på:
   - RotationsRegel (Nej/Ja/Skal)
   - TilladVendeOpTilMaksKg
   - HoejdeBreddefaktor
5. **Placering**: Placer elementer med:
   - PlacerLaengsteElementerYderst regel
   - MaksBalanceVaerdi overvejelse
6. **Validering**: Tjek kapacitets constraints:
   - MaksHoejde
   - MaksVaegt
   - TilladStablingOpTilMaksHoejdeInklPalle
   - TilladStablingOpTilMaksElementVaegt

## 📝 Naming Conventions

### Database
- Tabeller: Pluralis (Paller, Elementer, Pakkeplaner)
- Kolonner: PascalCase dansk (PalleBeskrivelse, MaksHoejde)

### C# Code
- Classes: PascalCase (PalleService, ElementController)
- Interfaces: I-prefix (IPalleService)
- Methods: PascalCase dansk (GetAllePaller, OpretElement)
- Private fields: _camelCase (_palleService)

### API Routes
- Base: `/api/[controller]`
- Endpoints: lowercase (GET `/api/paller`, POST `/api/elementer`)

## 🛠️ Teknologier

- **Backend**: ASP.NET Core 6.0 MVC
- **Database**: Entity Framework Core 6.0 + Azure SQL
- **Authentication**: ASP.NET Core Identity
- **Frontend**: Razor Pages + JavaScript (Vanilla)
- **UI Framework**: Bootstrap 5.3.0
- **Icons**: Font Awesome 6.4.0
- **Diagrammer**: Mermaid

## 📞 Support

For spørgsmål eller problemer, kontakt Acies development team.
