# Klassediagram - PalleOptimering System (1:1 med koden)

Dette diagram viser systemets faktiske struktur - præcis som implementeret i koden.

```mermaid
classDiagram
    %% BRUGER (Identity)
    class ApplicationUser {
        +string Id
        +string UserName
        +string Email
        +string FullName
        +DateTime CreatedAt
    }

    class NormalUser_Rolle {
        <<rolle>>
        +sePaller()
        +seElementer()
        +genererPakkeplan()
    }

    class SuperUser_Rolle {
        <<rolle>>
        +sePaller()
        +opretPalle()
        +redigerPalle()
        +sletPalle()
        +seElementer()
        +opretElement()
        +redigerElement()
        +sletElement()
        +administrerSettings()
        +genererPakkeplan()
    }

    ApplicationUser --> NormalUser_Rolle : kan have rolle
    ApplicationUser --> SuperUser_Rolle : kan have rolle

    %% PALLE MODEL
    class Palle {
        +int Id
        +string PalleBeskrivelse
        +int Laengde
        +int Bredde
        +int Hoejde
        +string Pallegruppe
        +string Palletype
        +decimal Vaegt
        +int MaksHoejde
        +decimal MaksVaegt
        +int Overmaal
        +int LuftMellemElementer
        +bool Aktiv
        +int Sortering
    }

    %% ELEMENT MODEL
    class Element {
        +int Id
        +string Reference
        +string Type
        +string Serie
        +int Hoejde
        +int Bredde
        +int Dybde
        +decimal Vaegt
        +bool ErSpecialelement
        +bool ErGeometrielement
        +string RotationsRegel
        +string KraeverPalletype
        +int? MaksElementerPrPalle
    }

    %% SETTINGS MODEL
    class PalleOptimeringSettings {
        +int Id
        +string Navn
        +int MaksLag
        +decimal TilladVendeOpTilMaksKg
        +decimal HoejdeBreddefaktor
        +bool HoejdeBreddefaktorKunForEnkeltElementer
        +int? TilladStablingOpTilMaksHoejdeInklPalle
        +decimal? TilladStablingOpTilMaksElementVaegt
        +int TillaegMonteringAfEndeplade
        +bool Aktiv
        +string SorteringsPrioritering
        +bool PlacerLaengsteElementerYderst
        +decimal? MaksBalanceVaerdi
    }

    %% PAKKEPLAN MODELLER
    class Pakkeplan {
        +int Id
        +string OrdreReference
        +DateTime Oprettet
        +int? SettingsId
        +int AntalPaller
        +int AntalElementer
    }

    class PakkeplanPalle {
        +int Id
        +int PakkeplanId
        +int PalleNummer
        +int PalleId
        +int SamletHoejde
        +decimal SamletVaegt
        +int AntalLag
    }

    class PakkeplanElement {
        +int Id
        +int PakkeplanPalleId
        +int ElementId
        +int Lag
        +int Plads
        +bool ErRoteret
        +int Sortering
    }

    %% SERVICES (Interfaces)
    class IPalleService {
        <<interface>>
        +GetAllePaller()
        +GetAlleAktivePaller()
        +GetPalle(id)
        +OpretPalle(palle)
        +OpdaterPalle(palle)
        +SletPalle(id)
    }

    class IElementService {
        <<interface>>
        +GetAlleElementer()
        +GetElement(id)
        +OpretElement(element)
        +OpdaterElement(element)
        +SletElement(id)
        +OpretFlereElementer(elementer)
    }

    class IPalleOptimeringService {
        <<interface>>
        +GenererPakkeplan(request)
        +GetPakkeplan(id)
        +GetAllePakkeplaner()
    }

    class IPalleOptimeringSettingsService {
        <<interface>>
        +GetAlleSettings()
        +GetAktivSettings()
        +GetSettings(id)
        +OpretSettings(settings)
        +OpdaterSettings(settings)
    }

    %% RELATIONER - Pakkeplan struktur
    Pakkeplan "1" --> "0..*" PakkeplanPalle : indeholder
    PakkeplanPalle "1" --> "0..*" PakkeplanElement : indeholder
    PakkeplanPalle --> Palle : bruger
    PakkeplanElement --> Element : placerer
    Pakkeplan --> PalleOptimeringSettings : bruger

    %% RELATIONER - Roller til adgang
    NormalUser_Rolle -.-> IPalleService : read-only
    NormalUser_Rolle -.-> IElementService : read-only
    NormalUser_Rolle -.-> IPalleOptimeringService : kan bruge

    SuperUser_Rolle -.-> IPalleService : fuld CRUD
    SuperUser_Rolle -.-> IElementService : fuld CRUD
    SuperUser_Rolle -.-> IPalleOptimeringService : kan bruge
    SuperUser_Rolle -.-> IPalleOptimeringSettingsService : fuld CRUD

    %% RELATIONER - Services til modeller
    IPalleService ..> Palle : håndterer
    IElementService ..> Element : håndterer
    IPalleOptimeringService ..> Pakkeplan : genererer
    IPalleOptimeringSettingsService ..> PalleOptimeringSettings : håndterer
```

## Model Forklaring

### ApplicationUser (Bruger)
- **Extends**: `IdentityUser` fra ASP.NET Core Identity
- **Roller**: Håndteres via `AspNetRoles` og `AspNetUserRoles` tabeller
- **Ingen metoder**: Logik ligger i Controllers, ikke på bruger-objektet

### Roller (NormalUser_Rolle & SuperUser_Rolle)
**VIGTIGT**: Disse er IKKE separate klasser i koden!
- De vises her kun for at illustrere forskellen i rettigheder
- I koden er det `[Authorize(Roles = "...")]` attributes der styrer adgang

**NormalUser_Rolle** (Read-only):
- Kan se paller (via IPalleService.GetAllePaller)
- Kan se elementer (via IElementService.GetAlleElementer)
- Kan generere pakkeplaner (via IPalleOptimeringService)
- Kan IKKE oprette, redigere eller slette noget

**SuperUser_Rolle** (Fuld adgang):
- Alt NormalUser kan + CRUD operationer
- Kan oprette, redigere, slette paller (via IPalleService)
- Kan oprette, redigere, slette elementer (via IElementService)
- Kan administrere settings (via IPalleOptimeringSettingsService)
- Kan generere pakkeplaner (via IPalleOptimeringService)

### Palle
**Dimensioner:**
- `Laengde`, `Bredde`, `Hoejde` (mm)
- `Vaegt` (kg)

**Begrænsninger:**
- `MaksHoejde`, `MaksVaegt`
- `Overmaal` (hvor meget må elementer rage ud)

**Regler (integreret):**
- `LuftMellemElementer` (mm) - Mellemrumsregel

**Metadata:**
- `Pallegruppe` ("75", "80", "100")
- `Palletype` ("Trae", "Alu", "Glasstel")
- `Aktiv`, `Sortering`

### Element
**Dimensioner:**
- `Hoejde`, `Bredde`, `Dybde` (mm)
- `Vaegt` (kg)

**Metadata:**
- `Reference`, `Type`, `Serie`

**Regler (integreret):**
- `RotationsRegel` ("Nej", "Ja", "Skal") - Rotationsregel
- `ErGeometrielement` (bool) - Stablingsregel (må ikke stables ovenpå)

**Special:**
- `ErSpecialelement` (prioritering)
- `KraeverPalletype` (kræver specifik palle)
- `MaksElementerPrPalle` (f.eks. foldedøre)

### PalleOptimeringSettings
**Globale optimeringsregler:**
- `MaksLag` - Max antal lag på palle
- `TilladVendeOpTilMaksKg` - Vægtgrænse for rotation
- `HoejdeBreddefaktor` - Ratio for tipping-forebyggelse
- `TilladStablingOpTilMaksHoejdeInklPalle` - Max højde for stabling
- `TilladStablingOpTilMaksElementVaegt` - Max vægt for stabling
- `SorteringsPrioritering` - Sorteringskriterier (kommasepareret)
- `PlacerLaengsteElementerYderst` - Placeringslogik
- `MaksBalanceVaerdi` - Balance tærskel

### Pakkeplan → PakkeplanPalle → PakkeplanElement
**Hierarkisk struktur:**
1. **Pakkeplan** - Overordnet plan for en ordre
   - Linker til `PalleOptimeringSettings` der blev brugt
   - Samlet antal paller og elementer

2. **PakkeplanPalle** - Én palle i planen
   - Palle nummer (1, 2, 3...)
   - Linker til `Palle` (palle-typen)
   - Beregnede værdier (SamletHoejde, SamletVaegt, AntalLag)

3. **PakkeplanElement** - Ét element placeret på en palle
   - Linker til `Element`
   - Position: `Lag` (1, 2, 3...), `Plads` (1-5)
   - `ErRoteret` - Om elementet blev roteret
   - `Sortering` - Rækkefølge

## Service Interfaces

Alle services er interface-baseret for testbarhed og dependency injection:

- **IPalleService**: CRUD for paller
- **IElementService**: CRUD for elementer (inkl. bulk oprettelse)
- **IPalleOptimeringService**: Genererer pakkeplaner
- **IPalleOptimeringSettingsService**: CRUD for settings

## Autorisation (Role-Based)

| Handling | NormalUser | SuperUser |
|----------|------------|-----------|
| Se paller/elementer | ✅ | ✅ |
| Oprette paller/elementer | ❌ | ✅ |
| Redigere paller/elementer (inkl. regler) | ❌ | ✅ |
| Slette paller/elementer | ❌ | ✅ |
| Generer pakkeplan | ✅ | ✅ |
| Administrer settings | ❌ | ✅ |

## Integrerede Regler

**Regler er IKKE separate klasser/tabeller:**

| Regeltype | Implementation | Lokation |
|-----------|----------------|----------|
| Rotationsregel | `Element.RotationsRegel` property | Element tabel |
| Mellemrumsregel | `Palle.LuftMellemElementer` property | Paller tabel |
| Stablingsregel | `Element.ErGeometrielement` property | Element tabel |
| Optimeringsregler | `PalleOptimeringSettings` properties | Settings tabel |

## Teknologi Stack

- **Framework**: ASP.NET Core 6.0
- **ORM**: Entity Framework Core 6
- **Database**: SQL Server / Azure SQL
- **Authentication**: ASP.NET Core Identity
- **Authorization**: Role-based (`[Authorize(Roles = "...")]`)
- **Pattern**: Service Layer (Interface-based)
- **DI**: Built-in Dependency Injection

## Database Context

```csharp
public class PalleOptimeringContext : IdentityDbContext<ApplicationUser>
{
    public DbSet<Palle> Paller { get; set; }
    public DbSet<Element> Elementer { get; set; }
    public DbSet<PalleOptimeringSettings> Settings { get; set; }
    public DbSet<Pakkeplan> Pakkeplaner { get; set; }
    public DbSet<PakkeplanPalle> PakkeplanPaller { get; set; }
    public DbSet<PakkeplanElement> PakkeplanElementer { get; set; }
}
```

Dette klassediagram er nu 1:1 med den faktiske kode!
