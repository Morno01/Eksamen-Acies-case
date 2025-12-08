# Database Diagram - PalleOptimering System

Dette diagram viser databasestrukturen for palleoptimering systemet.

```mermaid
erDiagram
    AspNetUsers ||--o{ AspNetUserRoles : has
    AspNetRoles ||--o{ AspNetUserRoles : has

    AspNetUsers {
        string Id PK
        string UserName
        string Email
        string PasswordHash
        string FullName
        datetime CreatedAt
    }

    AspNetRoles {
        string Id PK
        string Name
    }

    AspNetUserRoles {
        string UserId FK
        string RoleId FK
    }

    Paller {
        int Id PK
        string PalleBeskrivelse
        int Laengde
        int Bredde
        int Hoejde
        string Pallegruppe
        string Palletype
        decimal Vaegt
        int MaksHoejde
        decimal MaksVaegt
        int Overmaal
        int LuftMellemElementer
        bool Aktiv
        int Sortering
    }

    Elementer {
        int Id PK
        string Reference
        string Type
        string Serie
        int Hoejde
        int Bredde
        int Dybde
        decimal Vaegt
        bool ErSpecialelement
        bool ErGeometrielement
        string RotationsRegel
        string KraeverPalletype
        int MaksElementerPrPalle
    }

    PalleOptimeringSettings {
        int Id PK
        string Navn
        int MaksLag
        decimal TilladVendeOpTilMaksKg
        decimal HoejdeBreddefaktor
        bool HoejdeBreddefaktorKunForEnkeltElementer
        int TilladStablingOpTilMaksHoejdeInklPalle
        decimal TilladStablingOpTilMaksElementVaegt
        int TillaegMonteringAfEndeplade
        bool Aktiv
        string SorteringsPrioritering
        bool PlacerLaengsteElementerYderst
        decimal MaksBalanceVaerdi
    }

    Pakkeplaner {
        int Id PK
        string OrdreReference
        datetime Oprettet
        int SettingsId FK
        int AntalPaller
        int AntalElementer
    }

    PakkeplanPaller {
        int Id PK
        int PakkeplanId FK
        int PalleNummer
        int PalleId FK
        int SamletHoejde
        decimal SamletVaegt
        int AntalLag
    }

    PakkeplanElementer {
        int Id PK
        int PakkeplanPalleId FK
        int ElementId FK
        int Lag
        int Plads
        bool ErRoteret
        int Sortering
    }

    PalleOptimeringSettings ||--o{ Pakkeplaner : "bruges i"
    Pakkeplaner ||--o{ PakkeplanPaller : "indeholder"
    Paller ||--o{ PakkeplanPaller : "bruges i"
    PakkeplanPaller ||--o{ PakkeplanElementer : "indeholder"
    Elementer ||--o{ PakkeplanElementer : "placeret som"
```

## Tabelbeskrivelser

### AspNetUsers (Identity Framework)
Brugere i systemet med ASP.NET Identity integration.
- Roller: SuperUser (admin) eller NormalUser

### Paller
Definerer de forskellige palletyper der kan bruges til pakning.
- Indeholder dimensioner (længde, bredde, højde)
- Maks begrænsninger (vægt, højde)
- Regler (overmål, luft mellem elementer)

### Elementer
Døre og vinduer der skal pakkes på paller.
- Indeholder dimensioner og vægt
- Rotationsregler integreret direkte (ikke separat tabel)
- Special flags (geometrielement, specialelement)

### PalleOptimeringSettings
Globale indstillinger for optimeringsalgoritmen.
- Maks lag, vægtgrænser, sorteringskriterier
- Bruges ved generering af pakkeplaner

### Pakkeplaner
Genererede pakkeplaner for specifikke ordrer.
- Linker til settings der blev brugt
- Indeholder summary (antal paller, elementer)

### PakkeplanPaller
Repræsenterer én palle i en pakkeplan.
- Linker til palle-typen
- Indeholder beregnet vægt og højde

### PakkeplanElementer
Specifikke element-placeringer på paller.
- Lag og plads position
- Om elementet blev roteret
- Sorteringsorden

## Forskelle fra oprindeligt diagram

1. **Integrerede regler**: Rotations-, mellemrums- og stablingsregler er integreret i Paller og Elementer tabellerne i stedet for separate tabeller
2. **Kompleks pakkeplan**: I stedet for én simpel Placering-tabel, er der nu 3 relaterede tabeller (Pakkeplaner → PakkeplanPaller → PakkeplanElementer)
3. **Identity framework**: ASP.NET Identity bruges til brugerstyring (AspNetUsers, AspNetRoles, etc.)
4. **Settings tabel**: PalleOptimeringSettings styrer algoritme-parametre
