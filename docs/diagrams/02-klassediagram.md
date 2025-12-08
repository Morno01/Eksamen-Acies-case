# Klassediagram - PalleOptimering System

Dette diagram viser de vigtigste klasser og deres relationer.

```mermaid
classDiagram
    %% MODELS - Domæne klasser
    class Bruger {
        +int Id
        +string Brugernavn
        +string Password
        +string Rolle
        +sePaller()
        +opretPalle()
        +redigerPalle()
        +administrerRegler()
    }

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

    %% RELATIONER mellem modeller
    Bruger "1" --> "0..*" Palle : opretter
    Pakkeplan "1" --> "1..*" PakkeplanPalle : indeholder
    PakkeplanPalle "1" --> "1..*" PakkeplanElement : indeholder
    PakkeplanPalle --> Palle : bruger
    PakkeplanElement --> Element : placerer

    %% SERVICES bruger modeller
    PalleService ..> Palle : håndterer
    ElementService ..> Element : håndterer
    PalleOptimeringService ..> Pakkeplan : genererer
    PalleOptimeringService ..> PakkeplanPalle : opretter
    PalleOptimeringService ..> PakkeplanElement : placerer
```

## Forklaring af Klasser

### Domæne Modeller

**Bruger**
- Repræsenterer en bruger (SuperUser eller NormalUser)
- Metoder repræsenterer de handlinger brugeren kan udføre

**Palle**
- Definerer en palle-type med dimensioner og begrænsninger
- `LuftMellemElementer`: Regel for afstand mellem elementer

**Element**
- Døre/vinduer der skal pakkes
- `RotationsRegel`: Om elementet må/skal roteres (Nej, Ja, Skal)
- `ErGeometrielement`: Om der må stables ovenpå (stablingsregel)

**Pakkeplan → PakkeplanPalle → PakkeplanElement**
- Hierarkisk struktur for en komplet pakkeplan
- Én pakkeplan kan have flere paller
- Hver palle kan have flere elementer i forskellige lag

### Services (Forretningslogik)

**PalleService**
- CRUD operationer for paller
- Henter aktive paller til optimering

**ElementService**
- CRUD operationer for elementer
- Kan oprette mange elementer samtidig

**PalleOptimeringService**
- Kerne-service der genererer pakkeplaner
- Anvender rotations-, mellemrums- og stablingsregler
- Finder den bedste palle for hvert element

## Vigtige Noter

### Integrerede Regler
Reglerne er **integreret** i modellerne i stedet for separate klasser:
- **Rotationsregel**: `Element.RotationsRegel` property
- **Mellemrumsregel**: `Palle.LuftMellemElementer` property
- **Stablingsregel**: `Element.ErGeometrielement` property

### Service Pattern
- Services håndterer al forretningslogik
- Modeller er rene data-objekter (minus Bruger der har metoder i diagrammet)
- I koden bruges ASP.NET Identity til brugerstyring
