# Klassediagram - PalleOptimering System

Dette diagram viser systemets kernestruktur - simpelt og klart.

```mermaid
classDiagram
    %% BRUGER
    class Bruger {
        +int Id
        +string Brugernavn
        +string Email
        +string Rolle
        +sePaller()
        +seElementer()
        +genererPakkeplan()
    }

    note for Bruger "Rolle: 'NormalUser' eller 'SuperUser'\nNormalUser = read-only\nSuperUser = fuld adgang"

    %% MODELS
    class Palle {
        +int Id
        +string Beskrivelse
        +int Dimensioner
        +int LuftMellemElementer
        +bool Aktiv
    }

    class Element {
        +int Id
        +string Type
        +int Dimensioner
        +string RotationsRegel
        +bool ErGeometrielement
    }

    class Pakkeplan {
        +int Id
        +string OrdreReference
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
    }

    class PalleOptimeringService {
        +GenererPakkeplan()
    }

    %% RELATIONER
    Bruger --> Palle : SuperUser: opretter/redigerer
    Bruger --> Element : SuperUser: opretter/redigerer
    Bruger --> Pakkeplan : begge roller: kan se

    Pakkeplan --> Palle : bruger
    Pakkeplan --> Element : indeholder

    PalleService ..> Palle
    ElementService ..> Element
    PalleOptimeringService ..> Pakkeplan
```

## Forklaring

### Bruger

**Bruger** (ApplicationUser i koden)
- ÉN klasse for alle brugere - ingen nedarvning
- Rolle gemt som property: `"NormalUser"` eller `"SuperUser"`
- **NormalUser rolle**: Read-only adgang (se paller/elementer, generer pakkeplan)
- **SuperUser rolle**: Fuld adgang (oprette, redigere, administrere alt)

### Modeller

**Palle**
- Definerer palle-type med dimensioner
- `LuftMellemElementer`: Mellemrumsregel integreret

**Element**
- Døre/vinduer der skal pakkes
- `RotationsRegel`: Rotationsregel integreret (Nej/Ja/Skal)
- `ErGeometrielement`: Stablingsregel integreret

**Pakkeplan**
- Resultat af pakkeplan-generering
- Indeholder elementer placeret på paller

### Services

- **PalleService**: Håndterer paller (CRUD)
- **ElementService**: Håndterer elementer (CRUD)
- **PalleOptimeringService**: Genererer pakkeplaner

## Rettigheder

| Handling | NormalBruger | SuperBruger |
|----------|--------------|-------------|
| Se paller/elementer | ✅ | ✅ |
| Oprette/redigere | ❌ | ✅ |
| Generer pakkeplan | ✅ | ✅ |

## Vigtige Noter

**Integrerede Regler:**
- Rotationsregel: `Element.RotationsRegel`
- Mellemrumsregel: `Palle.LuftMellemElementer`
- Stablingsregel: `Element.ErGeometrielement`

**Implementation:**
- ASP.NET Core Identity: `ApplicationUser` extends `IdentityUser`
- Roller håndteres via `AspNetRoles` og `AspNetUserRoles` tabeller
- Role-based authorization: `[Authorize(Roles = "SuperUser")]`
- Entity Framework Core
