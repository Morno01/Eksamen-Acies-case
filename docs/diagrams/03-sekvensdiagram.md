# Sekvensdiagram - PalleOptimering System

Dette diagram viser de vigtigste brugerflows gennem systemet, baseret på det oprindelige design.

```mermaid
sequenceDiagram
    actor Bruger
    participant UI
    participant BrugerService as AccountController<br/>(BrugerService)
    participant PalleService
    participant RegelService as PalleOptimeringService<br/>(RegelService)
    participant PlaceringService as PalleOptimeringService<br/>(PlaceringService)
    participant Database

    %% ===== LOGIN FLOW =====
    Note over Bruger,Database: LOGIN
    Bruger->>UI: Indtast brugernavn & password
    UI->>BrugerService: login(brugernavn, password)
    BrugerService->>Database: SELECT * FROM Bruger WHERE...
    Database-->>BrugerService: Bruger + rolle
    BrugerService-->>UI: Login OK + rolle
    UI-->>Bruger: Anmod om palleoversigt

    %% ===== VIS PALLER =====
    Note over Bruger,Database: VIS PALLER
    Bruger->>UI: Vis paller
    UI->>PalleService: hentAllePaller()
    PalleService->>Database: SELECT * FROM Palle
    Database-->>PalleService: Palleliste
    PalleService-->>UI: Palleliste
    UI-->>Bruger: Vis paller

    %% ===== OPRET NY PALLE (kun SuperBruger) =====
    Note over Bruger,Database: OPRET NY PALLE
    Bruger->>UI: Opret ny palle
    UI->>BrugerService: checkRolle()
    BrugerService-->>UI: rolle = Superbruger
    UI->>Bruger: Vis palle formular
    Bruger->>UI: Udfyld data<br/>(beskrivelse, dimensioner, regler)
    UI->>PalleService: opretPalle(palleId)
    PalleService->>Database: INSERT INTO Palle
    Database-->>PalleService: palleId
    PalleService-->>UI: Palle oprettet
    UI-->>Bruger: Palle oprettet

    %% ===== TILFØJ REGLER TIL PALLE =====
    Note over Bruger,Database: TILFØJ REGLER
    Bruger->>UI: Tilføj regler til palle
    UI->>RegelService: opretRotationsRegel(palleId, ...)
    RegelService->>Database: INSERT INTO Rotations_regel
    Database-->>RegelService: OK

    UI->>RegelService: opretMellemrumsRegel(palleId, ...)
    RegelService->>Database: INSERT INTO Mellemrums_regel
    Database-->>RegelService: OK

    UI->>RegelService: opretStablingsRegel(palleId, ...)
    RegelService->>Database: INSERT INTO Stablings_regel
    Database-->>RegelService: OK

    RegelService-->>UI: Regler oprettet
    UI-->>Bruger: Regler tilføjet

    %% ===== REDIGER PALLE (kun SuperBruger) =====
    Note over Bruger,Database: REDIGER PALLE
    Bruger->>UI: Rediger palle
    UI->>BrugerService: checkRolle()
    BrugerService-->>UI: rolle = Superbruger
    UI->>PalleService: hentPalle(palleId)
    PalleService->>Database: SELECT * FROM Palle WHERE id = ?
    Database-->>PalleService: Palle-data
    PalleService-->>UI: Palle-data
    UI-->>Bruger: Gem ændringer

    Bruger->>UI: Ændre palle-data
    UI->>PalleService: opdaterPalle(palleId, data)
    PalleService->>Database: UPDATE Palle SET ...
    Database-->>PalleService: OK
    PalleService-->>UI: Palle opdateret
    UI-->>Bruger: Palle gemt

    %% ===== TILFØJ ELEMENT TIL PALLE =====
    Note over Bruger,Database: GENERER PAKKEPLAN
    Bruger->>UI: Tilføj element til palle
    UI->>PlaceringService: tilføjPlacering(palleId, elementId, lag, roteret)
    PlaceringService->>Database: INSERT INTO Placering ...
    Database-->>PlaceringService: OK
    PlaceringService-->>UI: Placering oprettet
    UI-->>Bruger: Element placeret på palle<br/>Fuldt palle-setup færdigt
```

## Flow Beskrivelser

### 1. Login Flow
- Bruger indtaster brugernavn og password
- `AccountController` (BrugerService) validerer mod database
- Returnerer bruger med rolle (NormalBruger eller SuperBruger)
- UI viser palleoversigt baseret på rolle

### 2. Vis Paller
- Både NormalBruger og SuperBruger kan se paller
- `PalleService` henter alle paller fra database
- UI viser palleliste

### 3. Opret Ny Palle (kun SuperBruger)
- Systemet checker om brugeren er SuperBruger
- Viser formular til at indtaste palle data
- `PalleService` opretter palle i database
- Returnerer palle ID

### 4. Tilføj Regler til Palle
- SuperBruger kan tilføje regler til en palle
- `PalleOptimeringService` (RegelService) håndterer:
  - Rotationsregler (må element roteres?)
  - Mellemrumsregler (afstand mellem elementer)
  - Stablingsregler (må der stables ovenpå?)
- Gemmes i database

**Note**: I den faktiske implementation er regler integreret i Palle og Element modellerne

### 5. Rediger Palle (kun SuperBruger)
- Systemet checker om brugeren er SuperBruger
- `PalleService` henter eksisterende palle data
- Bruger ændrer data
- `PalleService` opdaterer palle i database

### 6. Generer Pakkeplan (Tilføj Element til Palle)
- Bruger vælger elementer og palle
- `PalleOptimeringService` (PlaceringService) opretter placering
- Beregner lag, position og rotation
- Gemmer i database (Pakkeplan → PakkeplanPalle → PakkeplanElement)

## Service Mapping

| Oprindeligt Diagram | Faktisk Implementation |
|---------------------|------------------------|
| BrugerService | `AccountController` + `SignInManager` |
| PalleService | `PalleService` ✅ (match!) |
| RegelService | `PalleOptimeringService` |
| PlaceringService | `PalleOptimeringService` |

## Nøglepunkter

### Autorisation
- **NormalBruger**: Read-only adgang (kan kun se paller og elementer)
- **SuperBruger**: Fuld adgang (kan oprette, redigere, slette)
- Tjekkes via `[Authorize(Roles = "...")]` attributes

### Database Operationer
- Alle services kommunikerer med database via Entity Framework Core
- `PalleOptimeringContext` håndterer alle database operationer
- Transactions håndteres automatisk af DbContext

### Regler i Praksis
I den faktiske kode er reglerne integreret:
- **Rotationsregel**: `Element.RotationsRegel` (Nej, Ja, Skal)
- **Mellemrumsregel**: `Palle.LuftMellemElementer` (int i mm)
- **Stablingsregel**: `Element.ErGeometrielement` (bool)

Men flowet i sekvensdiagrammet viser konceptuelt hvordan regler håndteres.

### Placering/Pakkeplan
"Tilføj element til palle" svarer til at generere en pakkeplan:
1. Vælg elementer
2. Kør optimeringsalgoritme
3. Placer elementer på paller (lag, position, rotation)
4. Gem i `Pakkeplan` → `PakkeplanPalle` → `PakkeplanElement`
