# PalleOptimering System - Opdaterede Diagrammer

Dette bibliotek indeholder opdaterede diagrammer der **matcher den faktiske implementering** af PalleOptimering systemet.

## 📋 Oversigt

Denne mappe indeholder fire komplette diagrammer:

1. **Database Diagram** - Entity Relationship diagram der viser databasestrukturen
2. **Klassediagram** - Viser models, services og deres relationer
3. **Sekvensdiagrammer** - Viser brugerflows gennem systemet
4. **Arkitekturdiagram** - Viser systemets overordnede arkitektur

## 📁 Filstruktur

```
docs/diagrams/
├── README.md                      # Denne fil
├── 01-database-diagram.md         # ER diagram for database
├── 02-klassediagram.md            # Klasse struktur
├── 03-sekvensdiagram.md           # User flows
└── 04-arkitekturdiagram.md        # System arkitektur
```

## 🎯 Formål

Disse diagrammer er opdateret for at matche den **faktiske kodebase** på `claude/create-new-program` branchen.

De oprindelige diagrammer (de 4 billeder der blev vist) var et **tidligt design-forslag**, men implementeringen har udviklet sig anderledes.

## 🔍 Vigtigste Forskelle fra Oprindelige Diagrammer

### 1. Database Struktur

**Oprindeligt diagram:**
- Separate tabeller: `Rotations_regel`, `Mellemrums_regel`, `Stablings_regel`
- Simpel `Placering` tabel

**Faktisk implementation:**
- ✅ Regler integreret i `Paller` og `Elementer` tabeller som properties
- ✅ Kompleks pakkeplan struktur: `Pakkeplaner` → `PakkeplanPaller` → `PakkeplanElementer`
- ✅ `PalleOptimeringSettings` tabel for algoritme-parametre
- ✅ ASP.NET Identity tabeller for brugerstyring

### 2. Arkitektur

**Oprindeligt diagram:**
- Repository pattern mellem services og database
- `BrugerService`, `PalleService`, `RegelService`, `PlaceringService`

**Faktisk implementation:**
- ✅ Services tilgår DbContext direkte (ingen repository lag)
- ✅ `PalleService`, `ElementService`, `PalleOptimeringService`, `PalleOptimeringSettingsService`
- ✅ Ingen separat `RegelService` - regler håndteres i `PalleOptimeringService`
- ✅ Helper klasser: `ElementPlaceringHelper`, `ElementSorteringHelper`

### 3. Services

**Oprindeligt diagram:**
- Metoder på Bruger-klassen (`sePaller()`, `opretPalle()`, etc.)

**Faktisk implementation:**
- ✅ ASP.NET Core Controllers (`PallerController`, `ElementerController`, etc.)
- ✅ Interface-baseret design (`IPalleService`, `IElementService`, etc.)
- ✅ Dependency Injection pattern
- ✅ DTOs for API kommunikation (`PakkeplanRequest`, `PakkeplanResultat`)

### 4. Bruger System

**Oprindeligt diagram:**
- Simpel `Bruger` tabel med rolle property

**Faktisk implementation:**
- ✅ ASP.NET Core Identity framework
- ✅ `ApplicationUser` extends `IdentityUser`
- ✅ `UserManager`, `SignInManager`, `RoleManager`
- ✅ Rolle-baseret authorization med `[Authorize(Roles = "...")]`

## 📊 Diagram Guides

### 1. Database Diagram
Se: [01-database-diagram.md](./01-database-diagram.md)

**Hvad du finder:**
- Alle database tabeller med felter
- Relationer mellem tabeller (foreign keys)
- Forklaring af hver tabel
- Sammenligning med oprindeligt design

**Brug dette til:**
- Forstå datamodellen
- Database migrationer
- SQL queries og optimering

### 2. Klassediagram
Se: [02-klassediagram.md](./02-klassediagram.md)

**Hvad du finder:**
- Model klasser med alle properties
- Service interfaces og implementeringer
- Dependencies mellem klasser
- DTOs

**Brug dette til:**
- Forstå kode struktur
- Dependency injection setup
- Nye features implementering

### 3. Sekvensdiagrammer
Se: [03-sekvensdiagram.md](./03-sekvensdiagram.md)

**Hvad du finder:**
- Login flow
- Generer pakkeplan flow
- Administrer paller/elementer/settings flows
- Interaktion mellem komponenter

**Brug dette til:**
- Forstå brugerflows
- Debugging
- Testing
- Nye features

### 4. Arkitekturdiagram
Se: [04-arkitekturdiagram.md](./04-arkitekturdiagram.md)

**Hvad du finder:**
- 3-lags arkitektur (Præsentation, Business Logic, Data)
- Teknologi stack
- Deployment arkitektur
- Design patterns
- Sikkerhed

**Brug dette til:**
- Overordnet system forståelse
- Onboarding af nye udviklere
- Arkitektur beslutninger
- Deployment planning

## 🚀 Kom i Gang

### Læs Diagrammerne i Rækkefølge

For bedst forståelse, læs dem i denne rækkefølge:

1. **Start med Arkitekturdiagrammet** - Få overordnet forståelse
2. **Derefter Database Diagrammet** - Forstå datamodellen
3. **Så Klassediagrammet** - Forstå kode strukturen
4. **Til sidst Sekvensdiagrammerne** - Forstå flows

### Viewing Mermaid Diagrammer

Diagrammerne bruger Mermaid syntax. Du kan se dem på følgende måder:

1. **GitHub**: GitHub renderer Mermaid automatisk
2. **VS Code**: Installer "Markdown Preview Mermaid Support" extension
3. **Online**: [Mermaid Live Editor](https://mermaid.live/)
4. **IntelliJ/Rider**: Built-in Mermaid support i markdown preview

## 📚 Reference Dokumentation

### Model Klasser
Placering: `/MyProject/Models/`

- `ApplicationUser.cs` - Bruger model
- `Palle.cs` - Palle model
- `Element.cs` - Element model
- `PalleOptimeringSettings.cs` - Settings model
- `Pakkeplan.cs` - Pakkeplan models (3 klasser)

### Services
Placering: `/MyProject/Services/`

- `PalleService.cs` - Palle CRUD
- `ElementService.cs` - Element CRUD
- `PalleOptimeringService.cs` - Pakkeplan generering
- `PalleOptimeringSettingsService.cs` - Settings CRUD

### Controllers
Placering: `/MyProject/Controllers/`

- `AccountController.cs` - Authentication
- `PalleOptimeringController.cs` - Pakkeplan API
- `PallerController.cs` - Palle API
- `ElementerController.cs` - Element API
- `SettingsController.cs` - Settings API
- `HomeController.cs` - Dashboard

### Database
Placering: `/MyProject/Data/`

- `PalleOptimeringContext.cs` - DbContext
- `/Migrations/` - Database migrations

## 🔄 Opdatering af Diagrammer

Når koden ændres, skal diagrammerne også opdateres. Her er retningslinjer:

### Hvornår skal diagrammer opdateres?

- ✅ Nye modeller tilføjes
- ✅ Database struktur ændres (nye tabeller/kolonner)
- ✅ Nye services eller controllers tilføjes
- ✅ Arkitekturen ændres (nye lag, patterns)
- ✅ Major brugerflows ændres

### Hvordan opdaterer du?

1. Rediger den relevante `.md` fil
2. Opdater Mermaid diagram syntaxen
3. Opdater beskrivelserne under diagrammet
4. Test at diagrammet renderer korrekt
5. Commit ændringerne

## ❓ FAQ

### Q: Hvorfor passer diagrammerne ikke med de oprindelige billeder?
**A:** De oprindelige billeder var et tidligt design-forslag. Under udviklingen blev der taget andre arkitektur-beslutninger der gjorde implementeringen mere pragmatisk og moderne.

### Q: Hvilke diagrammer skal jeg bruge til at forstå koden?
**A:** Start med arkitekturdiagrammet for overordnet forståelse, derefter klassediagrammet for kodestruktur, og sekvensdiagrammerne for at forstå flows.

### Q: Kan jeg bruge disse diagrammer i dokumentation?
**A:** Ja! De er skabt netop til det formål. De kan bruges i teknisk dokumentation, onboarding guides, osv.

### Q: Hvad hvis jeg finder fejl i diagrammerne?
**A:** Opdater den relevante markdown fil og lav en commit med ændringerne.

### Q: Hvor er Repository Pattern?
**A:** Implementeringen bruger ikke Repository Pattern. Services tilgår DbContext direkte, hvilket er en accepteret praksis i moderne ASP.NET Core applikationer.

## 📝 Changelog

### 2024-12-08
- ✅ Initial oprettelse af alle 4 opdaterede diagrammer
- ✅ Baseret på faktisk kode fra `claude/create-new-program` branch
- ✅ Inkluderer database, klasse, sekvens og arkitektur diagrammer
- ✅ Dokumentation af forskelle fra oprindeligt design

## 🤝 Bidrag

Hvis du opdager fejl eller mangler i diagrammerne:

1. Tjek den faktiske kode først
2. Opdater den relevante `.md` fil
3. Test at diagrammet renderer korrekt
4. Commit med beskrivende commit message

## 📞 Kontakt

Ved spørgsmål om diagrammerne eller systemarkitekturen, kontakt projektteamet.

---

**Note:** Disse diagrammer er levende dokumenter der skal opdateres når koden ændres. Hold dem synkroniserede med den faktiske implementering!
