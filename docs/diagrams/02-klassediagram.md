# Klassediagram - PalleOptimering System

Dette diagram viser systemets kernestruktur - simpelt og klart.

```mermaid
classDiagram
    %% BRUGER HIERARKI
    class Bruger {
        <<abstract>>
        +int Id
        +string Brugernavn
        +string Rolle
    }

    class NormalBruger {
        +sePaller()
        +seElementer()
        +genererPakkeplan()
    }

    class SuperBruger {
        +sePaller()
        +opretPalle()
        +redigerPalle()
        +opretElement()
        +administrerRegler()
        +genererPakkeplan()
    }

    Bruger <|-- NormalBruger : arver
    Bruger <|-- SuperBruger : arver

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
    SuperBruger --> Palle : opretter/redigerer
    SuperBruger --> Element : opretter/redigerer
    NormalBruger --> Pakkeplan : kan se
    SuperBruger --> Pakkeplan : kan se

    Pakkeplan --> Palle : bruger
    Pakkeplan --> Element : indeholder

    PalleService ..> Palle
    ElementService ..> Element
    PalleOptimeringService ..> Pakkeplan
```

## Forklaring

### Bruger Hierarki

- **Bruger (abstract)**: Basis klasse med fælles properties
- **NormalBruger**: Read-only adgang - kan se og generere pakkeplaner
- **SuperBruger**: Fuld adgang - kan oprette, redigere og administrere alt

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
- ASP.NET Core med Identity
- Role-based authorization
- Entity Framework Core
