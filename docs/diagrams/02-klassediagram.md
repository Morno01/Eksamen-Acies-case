# Klassediagram - PalleOptimering System

Dette diagram viser de vigtigste klasser og deres relationer.

```mermaid
classDiagram
    %% BRUGER HIERARKI
    class Bruger {
        <<abstract>>
        +int Id
        +string Brugernavn
        +string Password
        +string Rolle
        +sePaller()
        +seElementer()
        +genererPakkeplan()
    }

    class NormalBruger {
        +string Rolle = "NormalUser"
        +sePaller()
        +seElementer()
        +genererPakkeplan()
    }

    class SuperBruger {
        +string Rolle = "SuperUser"
        +sePaller()
        +opretPalle()
        +redigerPalle()
        +sletPalle()
        +seElementer()
        +opretElement()
        +redigerElement()
        +sletElement()
        +administrerRegler()
        +genererPakkeplan()
    }

    Bruger <|-- NormalBruger
    Bruger <|-- SuperBruger

    %% MODELS - Domæne klasser
    class Palle {
        +int Id
        +string Beskrivelse
        +int Laengde
        +int Bredde
        +int Hoejde
        +decimal MaksVaegt
        +int MaksHoejde
        +int LuftMellemElementer
        +bool Aktiv
    }

    class Element {
        +int Id
        +string Reference
        +string Type
        +string Serie
        +int Hoejde
        +int Bredde
        +int Dybde
        +decimal Vaegt
        +string RotationsRegel
        +bool ErGeometrielement
    }

    class PalleOptimeringSettings {
        +int Id
        +string Navn
        +int MaksLag
        +decimal TilladVendeOpTilMaksKg
        +bool Aktiv
    }

    class Pakkeplan {
        +int Id
        +string OrdreReference
        +DateTime Oprettet
        +int AntalPaller
        +int AntalElementer
    }

    class PakkeplanPalle {
        +int Id
        +int PalleNummer
        +int SamletHoejde
        +decimal SamletVaegt
        +int AntalLag
    }

    class PakkeplanElement {
        +int Id
        +int Lag
        +int Plads
        +bool ErRoteret
    }

    %% SERVICES - Forretningslogik
    class PalleService {
        +GetAllePaller()
        +GetPalle(id)
        +OpretPalle(palle)
        +OpdaterPalle(palle)
        +SletPalle(id)
    }

    class ElementService {
        +GetAlleElementer()
        +GetElement(id)
        +OpretElement(element)
        +OpdaterElement(element)
        +OpretFlereElementer(elementer)
    }

    class PalleOptimeringService {
        +GenererPakkeplan(request)
        +GetPakkeplan(id)
        +GetAllePakkeplaner()
    }

    %% RELATIONER mellem bruger og domæne
    SuperBruger "1" --> "0..*" Palle : opretter/redigerer
    SuperBruger "1" --> "0..*" Element : opretter/redigerer
    SuperBruger "1" --> "0..*" PalleOptimeringSettings : administrerer
    NormalBruger --> Pakkeplan : kan se
    SuperBruger --> Pakkeplan : kan se

    %% RELATIONER mellem modeller
    Pakkeplan "1" --> "1..*" PakkeplanPalle : indeholder
    PakkeplanPalle "1" --> "1..*" PakkeplanElement : indeholder
    PakkeplanPalle --> Palle : bruger
    PakkeplanElement --> Element : placerer
    Pakkeplan --> PalleOptimeringSettings : bruger

    %% SERVICES bruger modeller
    PalleService ..> Palle : håndterer
    ElementService ..> Element : håndterer
    PalleOptimeringService ..> Pakkeplan : genererer
    PalleOptimeringService ..> PakkeplanPalle : opretter
    PalleOptimeringService ..> PakkeplanElement : placerer
    PalleOptimeringService --> PalleOptimeringSettings : bruger
```

## Forklaring af Klasser

### Bruger Hierarki

**Bruger (Abstract)**
- Basis klasse for alle brugertyper
- Indeholder fælles properties og grundlæggende metoder

**NormalBruger**
- Read-only adgang til systemet
- Kan se paller, elementer og generere pakkeplaner
- Kan IKKE oprette, redigere eller slette

**SuperBruger**
- Fuld adgang til systemet (administrator)
- Kan oprette, redigere og slette paller
- Kan oprette, redigere og slette elementer
- Kan administrere optimeringsregler (PalleOptimeringSettings)
- Kan generere pakkeplaner

### Domæne Modeller

**Palle**
- Definerer en palle-type med dimensioner og begrænsninger
- `LuftMellemElementer`: Regel for afstand mellem elementer (mellemrumsregel)
- SuperBruger kan administrere paller

**Element**
- Døre/vinduer der skal pakkes
- `RotationsRegel`: Om elementet må/skal roteres (Nej, Ja, Skal)
- `ErGeometrielement`: Om der må stables ovenpå (stablingsregel)
- SuperBruger kan administrere elementer

**PalleOptimeringSettings**
- Indstillinger for optimeringsalgoritmen
- `MaksLag`: Maksimalt antal lag på en palle
- `TilladVendeOpTilMaksKg`: Vægtgrænse for rotation
- Kun SuperBruger kan administrere

**Pakkeplan → PakkeplanPalle → PakkeplanElement**
- Hierarkisk struktur for en komplet pakkeplan
- Én pakkeplan kan have flere paller
- Hver palle kan have flere elementer i forskellige lag
- Både NormalBruger og SuperBruger kan se pakkeplaner

### Services (Forretningslogik)

**PalleService**
- CRUD operationer for paller
- Henter aktive paller til optimering
- Bruges af SuperBruger til administration

**ElementService**
- CRUD operationer for elementer
- Kan oprette mange elementer samtidig
- Bruges af SuperBruger til administration

**PalleOptimeringService**
- Kerne-service der genererer pakkeplaner
- Anvender rotations-, mellemrums- og stablingsregler
- Finder den bedste palle for hvert element
- Bruges af både NormalBruger og SuperBruger

## Rettigheder

| Handling | NormalBruger | SuperBruger |
|----------|--------------|-------------|
| Se paller | ✅ | ✅ |
| Opret/rediger/slet paller | ❌ | ✅ |
| Se elementer | ✅ | ✅ |
| Opret/rediger/slet elementer | ❌ | ✅ |
| Generer pakkeplan | ✅ | ✅ |
| Administrer regler | ❌ | ✅ |

## Vigtige Noter

### Integrerede Regler
Reglerne er **integreret** i modellerne i stedet for separate klasser:
- **Rotationsregel**: `Element.RotationsRegel` property
- **Mellemrumsregel**: `Palle.LuftMellemElementer` property
- **Stablingsregel**: `Element.ErGeometrielement` property
- **Optimeringsregler**: `PalleOptimeringSettings` klasse

### Rolle-baseret Adgangskontrol
- I koden implementeres dette via ASP.NET Identity
- `[Authorize(Roles = "SuperUser")]` på admin-endpoints
- `[Authorize(Roles = "SuperUser,NormalUser")]` på read-endpoints
