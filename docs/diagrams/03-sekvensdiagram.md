# Sekvensdiagrammer - PalleOptimering System

Dette dokument viser de vigtigste brugerflows i palleoptimering systemet.

## 1. Login Flow

```mermaid
sequenceDiagram
    actor Bruger
    participant UI as Web Browser
    participant AC as AccountController
    participant SM as SignInManager
    participant UM as UserManager
    participant DB as Database

    Bruger->>UI: Indtast brugernavn & password
    UI->>AC: POST /Account/Login
    AC->>AC: Valider ModelState
    AC->>SM: PasswordSignInAsync(email, password)
    SM->>DB: Hent bruger + password hash
    DB-->>SM: ApplicationUser + hash
    SM->>SM: Verificer password
    SM->>DB: Hent bruger roller
    DB-->>SM: Roller (SuperUser/NormalUser)
    SM-->>AC: SignInResult (Succeeded)
    AC-->>UI: Redirect til Home
    UI-->>Bruger: Vis palle oversigt
```

## 2. Opret Pakkeplan Flow

```mermaid
sequenceDiagram
    actor Bruger
    participant UI as Web Browser
    participant POC as PalleOptimeringController
    participant POS as PalleOptimeringService
    participant PSS as PalleOptimeringSettingsService
    participant PS as PalleService
    participant ES as ElementService
    participant DB as Database

    Bruger->>UI: Vælg elementer + klik "Generer pakkeplan"
    UI->>POC: POST /api/PalleOptimering/generer
    Note over POC: PakkeplanRequest:<br/>- OrdreReference<br/>- ElementIds[]<br/>- SettingsId?

    POC->>POC: Tjek bruger autorisation
    POC->>POS: GenererPakkeplan(request)

    alt Settings ID angivet
        POS->>PSS: GetSettings(settingsId)
    else Brug standard settings
        POS->>PSS: GetAktivSettings()
    end
    PSS->>DB: SELECT * FROM Settings WHERE...
    DB-->>PSS: PalleOptimeringSettings
    PSS-->>POS: Settings

    POS->>DB: SELECT * FROM Elementer WHERE Id IN (...)
    DB-->>POS: List<Element>

    POS->>PS: GetAlleAktivePaller()
    PS->>DB: SELECT * FROM Paller WHERE Aktiv = true
    DB-->>PS: List<Palle>
    PS-->>POS: Aktive paller

    POS->>POS: Valider data (elementer, paller findes)

    alt Validation fejler
        POS-->>POC: PakkeplanResultat (Status: Error)
        POC-->>UI: BadRequest + fejlmeddelelse
        UI-->>Bruger: Vis fejl
    else Validation OK
        POS->>DB: INSERT INTO Pakkeplaner
        DB-->>POS: Pakkeplan.Id

        POS->>POS: Kør optimeringsalgoritme
        Note over POS: - Sorter elementer<br/>- Find bedste paller<br/>- Placer elementer<br/>- Beregn vægt/højde

        loop For hver palle i planen
            POS->>DB: INSERT INTO PakkeplanPaller
            DB-->>POS: PakkeplanPalle.Id

            loop For hvert element på pallen
                POS->>DB: INSERT INTO PakkeplanElementer
                DB-->>POS: PakkeplanElement.Id
            end
        end

        POS->>DB: UPDATE Pakkeplaner SET AntalPaller, AntalElementer
        DB-->>POS: OK

        POS-->>POC: PakkeplanResultat (Status: Success)
        POC-->>UI: OK(pakkeplan)
        UI-->>Bruger: Vis pakkeplan resultat
    end
```

## 3. Administrer Paller Flow

```mermaid
sequenceDiagram
    actor SuperUser as SuperUser (Admin)
    participant UI as Web Browser
    participant PC as PallerController
    participant PS as PalleService
    participant DB as Database

    Note over SuperUser,DB: Hent alle paller
    SuperUser->>UI: Gå til /Paller
    UI->>PC: GET /api/Paller
    PC->>PC: Autoriser (SuperUser eller NormalUser)
    PC->>PS: GetAllePaller()
    PS->>DB: SELECT * FROM Paller ORDER BY Sortering
    DB-->>PS: List<Palle>
    PS-->>PC: Paller
    PC-->>UI: OK(paller)
    UI-->>SuperUser: Vis palle liste

    Note over SuperUser,DB: Opret ny palle
    SuperUser->>UI: Udfyld formular + klik "Gem"
    UI->>PC: POST /api/Paller
    PC->>PC: Autoriser (kun SuperUser)
    PC->>PS: OpretPalle(palle)
    PS->>DB: INSERT INTO Paller
    DB-->>PS: Palle.Id
    PS-->>PC: Palle
    PC-->>UI: Created(palle)
    UI-->>SuperUser: Vis ny palle

    Note over SuperUser,DB: Opdater palle
    SuperUser->>UI: Ret palle + klik "Gem"
    UI->>PC: PUT /api/Paller/{id}
    PC->>PC: Autoriser (kun SuperUser)
    PC->>PS: OpdaterPalle(palle)
    PS->>DB: UPDATE Paller SET ... WHERE Id = {id}
    DB-->>PS: OK
    PS-->>PC: Palle
    PC-->>UI: OK(palle)
    UI-->>SuperUser: Vis opdateret palle
```

## 4. Administrer Elementer Flow

```mermaid
sequenceDiagram
    actor Bruger
    participant UI as Web Browser
    participant EC as ElementerController
    participant ES as ElementService
    participant DB as Database

    Note over Bruger,DB: Hent alle elementer
    Bruger->>UI: Gå til /Elementer
    UI->>EC: GET /api/Elementer
    EC->>EC: Autoriser (SuperUser eller NormalUser)
    EC->>ES: GetAlleElementer()
    ES->>DB: SELECT * FROM Elementer
    DB-->>ES: List<Element>
    ES-->>EC: Elementer
    EC-->>UI: OK(elementer)
    UI-->>Bruger: Vis element liste med filter muligheder

    Note over Bruger,DB: Opret flere elementer samtidig
    Bruger->>UI: Upload CSV eller indtast multiple
    UI->>EC: POST /api/Elementer/bulk
    EC->>EC: Autoriser (SuperUser)
    EC->>ES: OpretFlereElementer(elementer)
    ES->>DB: INSERT INTO Elementer (bulk)
    DB-->>ES: List<Element>
    ES-->>EC: Elementer
    EC-->>UI: Created(elementer)
    UI-->>Bruger: Vis oprettede elementer
```

## 5. Administrer Settings Flow

```mermaid
sequenceDiagram
    actor SuperUser as SuperUser (Admin)
    participant UI as Web Browser
    participant SC as SettingsController
    participant PSS as PalleOptimeringSettingsService
    participant DB as Database

    Note over SuperUser,DB: Hent aktiv settings
    SuperUser->>UI: Gå til /Settings
    UI->>SC: GET /api/Settings/aktiv
    SC->>SC: Autoriser (SuperUser eller NormalUser)
    SC->>PSS: GetAktivSettings()
    PSS->>DB: SELECT TOP 1 * FROM Settings WHERE Aktiv = true
    DB-->>PSS: PalleOptimeringSettings
    PSS-->>SC: Settings
    SC-->>UI: OK(settings)
    UI-->>SuperUser: Vis settings formular

    Note over SuperUser,DB: Opdater settings
    SuperUser->>UI: Ændre værdier + klik "Gem"
    UI->>SC: PUT /api/Settings/{id}
    SC->>SC: Autoriser (kun SuperUser)
    SC->>PSS: OpdaterSettings(settings)
    PSS->>DB: UPDATE Settings SET ... WHERE Id = {id}
    DB-->>PSS: OK
    PSS-->>SC: Settings
    SC-->>UI: OK(settings)
    UI-->>SuperUser: Vis opdateret settings
```

## Nøglepunkter

### Autorisation
- Login flow: SignInManager håndterer authentication
- Alle endpoints kræver `[Authorize]` attribute
- SuperUser har fuld adgang
- NormalUser har read-only adgang til de fleste endpoints

### Service Layer Pattern
- Controllers afhænger af service interfaces (ikke konkrete implementationer)
- Services tilgår DbContext direkte (ingen repository lag)
- Dependency injection bruges til at injicere services

### Optimeringsalgoritme
`PalleOptimeringService.GenererPakkeplan()` indeholder kompleks logik:
1. Hent og valider input data
2. Sorter elementer efter prioritet (settings)
3. Find bedste palle for hvert element
4. Placer elementer med rotation/lag logik
5. Beregn vægt og højde
6. Gem resultat i database

### Fejlhåndtering
- Services returnerer `null` ved "ikke fundet"
- PakkeplanResultat indeholder `Status` og `Meddelelser[]`
- Controllers returnerer korrekte HTTP status codes

## Forskelle fra oprindeligt diagram

1. **ASP.NET Core Controllers**: I stedet for metoder på Bruger-klassen
2. **Service interfaces**: Dependency injection pattern
3. **DTOs**: PakkeplanRequest/PakkeplanResultat for API kommunikation
4. **Ingen RegelService**: Regler håndteres direkte i PalleOptimeringService
5. **Bulk operationer**: OpretFlereElementer() for efficiency
