# Klassediagram - PalleOptimering System

Dette diagram viser systemets kernestruktur - simpelt og klart.

```mermaid
classDiagram
    class Bruger {
        +int Id
        +string Brugernavn
        +string Email
        +string Rolle
        +sePaller()
        +seElementer()
        +genererPakkeplan()
    }

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

    Bruger --> Palle : opretter/redigerer
    Bruger --> Element : opretter/redigerer
    Bruger --> Pakkeplan : kan se

    Pakkeplan --> Palle : bruger
    Pakkeplan --> Element : indeholder

    PalleService ..> Palle : håndterer
    ElementService ..> Element : håndterer
    PalleOptimeringService ..> Pakkeplan : genererer
```

## Bruger Roller

**Bruger klasse** (ApplicationUser i koden)
- Rolle property: `"NormalUser"` eller `"SuperUser"`

| Rolle | Rettigheder |
|-------|-------------|
| **NormalUser** | Read-only: Se paller, se elementer, generer pakkeplan |
| **SuperUser** | Fuld adgang: Oprette, redigere, slette alt + administrere regler |

## Modeller

**Palle**
- Definerer palle-type med dimensioner
- `LuftMellemElementer`: Mellemrumsregel (integreret)

**Element**
- Døre/vinduer der skal pakkes
- `RotationsRegel`: Rotationsregel (Nej/Ja/Skal) - integreret
- `ErGeometrielement`: Stablingsregel - integreret

**Pakkeplan**
- Resultat af pakkeplan-generering
- Indeholder elementer placeret på paller

## Services

- **PalleService**: CRUD for paller (kun SuperUser kan oprette/redigere)
- **ElementService**: CRUD for elementer (kun SuperUser kan oprette/redigere)
- **PalleOptimeringService**: Genererer pakkeplaner (begge roller)

## Ændre Regler

**SuperUser kan ændre regler:**
- På Element: `RotationsRegel`, `ErGeometrielement`
- På Palle: `LuftMellemElementer`
- Ændres via redigering af element/palle (én UPDATE operation)

## Integrerede Regler

Regler er **properties** på modellerne:
- **Rotationsregel**: `Element.RotationsRegel` property
- **Mellemrumsregel**: `Palle.LuftMellemElementer` property
- **Stablingsregel**: `Element.ErGeometrielement` property

## Implementation

- ASP.NET Core Identity: `ApplicationUser extends IdentityUser`
- Roller: `AspNetRoles` og `AspNetUserRoles` tabeller
- Authorization: `[Authorize(Roles = "SuperUser")]`
- ORM: Entity Framework Core
