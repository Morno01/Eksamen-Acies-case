# Klassediagram - PalleOptimering System

Dette diagram viser systemets faktiske struktur.

```mermaid
classDiagram
    %% BRUGER OG ROLLER
    class ApplicationUser {
        +string Id
        +string UserName
        +string Email
        +string FullName
        +DateTime CreatedAt
    }

    class NormalUser {
        <<rolle>>
        Kan se paller
        Kan se elementer
        Kan generere pakkeplan
    }

    class SuperUser {
        <<rolle>>
        Kan se alt
        Kan oprette paller
        Kan redigere paller
        Kan oprette elementer
        Kan redigere elementer
        Kan administrere settings
    }

    ApplicationUser ..> NormalUser : kan have rolle
    ApplicationUser ..> SuperUser : kan have rolle

    %% PALLE
    class Palle {
        +int Id
        +string PalleBeskrivelse
        +int Laengde
        +int Bredde
        +int Hoejde
        +decimal MaksVaegt
        +int MaksHoejde
        +int LuftMellemElementer
        +bool Aktiv
    }

    %% ELEMENT
    class Element {
        +int Id
        +string Reference
        +string Type
        +int Hoejde
        +int Bredde
        +int Dybde
        +decimal Vaegt
        +string RotationsRegel
        +bool ErGeometrielement
    }

    %% SETTINGS
    class PalleOptimeringSettings {
        +int Id
        +string Navn
        +int MaksLag
        +decimal TilladVendeOpTilMaksKg
        +bool Aktiv
    }

    %% PAKKEPLAN
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

    %% SERVICES
    class PalleService {
        +GetAllePaller()
        +OpretPalle()
        +OpdaterPalle()
    }

    class ElementService {
        +GetAlleElementer()
        +OpretElement()
        +OpdaterElement()
    }

    class PalleOptimeringService {
        +GenererPakkeplan()
    }

    %% RELATIONER
    Pakkeplan "1" --> "0..*" PakkeplanPalle
    PakkeplanPalle "1" --> "0..*" PakkeplanElement
    PakkeplanPalle --> Palle
    PakkeplanElement --> Element
    Pakkeplan --> PalleOptimeringSettings

    PalleService ..> Palle
    ElementService ..> Element
    PalleOptimeringService ..> Pakkeplan
```

## Bruger og Roller

### ApplicationUser
- **Én klasse for alle brugere** (extends IdentityUser)
- Roller gemt i AspNetUserRoles tabel
- INGEN nedarvning!

### NormalUser (rolle)
- ✅ Se paller
- ✅ Se elementer
- ✅ Generere pakkeplan
- ❌ Oprette/redigere NOGET

### SuperUser (rolle)
- ✅ Alt NormalUser kan
- ✅ Oprette paller/elementer
- ✅ Redigere paller/elementer (inkl. regler)
- ✅ Slette paller/elementer
- ✅ Administrere settings

## Modeller

### Palle
- Dimensioner: `Laengde`, `Bredde`, `Hoejde`
- Begrænsninger: `MaksVaegt`, `MaksHoejde`
- **Regel integreret**: `LuftMellemElementer` (mellemrumsregel)

### Element
- Dimensioner: `Hoejde`, `Bredde`, `Dybde`, `Vaegt`
- **Regler integreret**:
  - `RotationsRegel` ("Nej", "Ja", "Skal")
  - `ErGeometrielement` (stablingsregel)

### PalleOptimeringSettings
- Globale optimeringsregler
- `MaksLag`, `TilladVendeOpTilMaksKg`, osv.

### Pakkeplan Struktur
```
Pakkeplan (ordre)
  └─ PakkeplanPalle (palle 1, 2, 3...)
      └─ PakkeplanElement (element placering med lag, plads, rotation)
```

## Services

- **PalleService**: CRUD for paller
- **ElementService**: CRUD for elementer
- **PalleOptimeringService**: Genererer pakkeplaner

## Autorisation

| Handling | NormalUser | SuperUser |
|----------|------------|-----------|
| Se paller/elementer | ✅ | ✅ |
| Oprette | ❌ | ✅ |
| Redigere (inkl. regler) | ❌ | ✅ |
| Slette | ❌ | ✅ |
| Generer pakkeplan | ✅ | ✅ |
| Administrer settings | ❌ | ✅ |

## Vigtige Noter

### Regler er integreret (IKKE separate tabeller):
- **Rotationsregel**: `Element.RotationsRegel` property
- **Mellemrumsregel**: `Palle.LuftMellemElementer` property
- **Stablingsregel**: `Element.ErGeometrielement` property

### Roller er IKKE separate klasser:
- De vises kun for klarhed i diagrammet
- I koden: `[Authorize(Roles = "SuperUser")]` attributes

### Teknologi:
- ASP.NET Core 6.0 + Identity
- Entity Framework Core 6
- SQL Server Database
