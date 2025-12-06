# Testplan - Palleoptimering System

## 1. Formål

Formålet med testplanen er at sikre, at systemets funktionalitet, kvalitet og krav bliver verificeret og valideret gennem en struktureret testproces. Testplanen baseres på systemets arkitektur som beskrevet i [klassediagrammet](./klassediagram.md) og [ER-diagrammet](./er-diagram.md).

## 2. Teststrategi

### Overordnet tilgang til test
Projektet følger spiralmodellen, hvilket betyder, at udviklingen foregår iterativt og risikobaseret. Dette gør det muligt at justere løsningen løbende og minimere risici.

Spiralmodellen vil hjælpe med struktur og fleksibilitet. Der er stærkt fokus på risikostyring i hver fase, så da vi arbejder med nye teknologier med krav som kan være svære at forstå, synes vi at spiralmodellen passer godt til vores testplan.

Vi har i afsnittet testtyper opsat rækkefølgen på vores tests, her vil vi også skrive hvornår den enkelte testtype kan påbegyndes.

Vi har valgt at benytte os af disse metoder, da de passer godt sammen med hinanden, de fremhæver begge iterative processer og løbende evaluering af projektets fremdrift.

## 3. Testomfang (Scope)

Dele af systemet der indgår i test:

- **Backend funktioner** - Services og Controllers
- **UI funktionalitet** - Views og frontend komponenter
- **Datavalidering** - Model validering og business rules
- **Integration mellem moduler** - Service lag og database
- **Brugerscenarier** - End-to-end flows
- **Krav opfyldelse** - Funktionelle og ikke-funktionelle krav
- **System helhed** - Hele applikationens funktion

## 4. Testtyper

### 4.1 Unit Test

**Formål**: At isolere og teste mindre enheder i koden (funktioner, klasser eller metoder) for at se om de virker korrekt, uafhængigt af resten af koden.

**Hvorfor denne type giver mening**: Det sikrer kvaliteten, da den tester små dele af koden/isolerede dele af programmet.

**Hvornår den udføres**: Unit testing bliver udført på enkelte valgte dele af vores program, hvor vi gerne ville have hurtigt respons med lav risiko for fejl. Disse tests kan udføres tidligt i udviklingsprocessen da det kun er nødvendigt at have et færdigt kodestykke for at begynde unit tests.

#### Konkret kode der testes (reference til klassediagram):

**Service Layer Tests:**
- **PalleService klassen**:
  - `GetAllePaller()` - Tester at alle paller returneres korrekt
  - `GetAlleAktivePaller()` - Tester filtrering af kun aktive paller
  - `GetPalle(id)` - Tester hentning af enkelt palle med valid/invalid ID
  - `OpretPalle(palle)` - Tester oprettelse med valid/invalid data
  - `OpdaterPalle(palle)` - Tester opdatering af eksisterende palle
  - `SletPalle(id)` - Tester sletning af palle

- **ElementService klassen**:
  - `GetAlleElementer()` - Tester returnering af alle elementer
  - `GetElement(id)` - Tester hentning af enkelt element
  - `OpretElement(element)` - Tester oprettelse med validering
  - `OpretFlereElementer(elementer)` - Tester bulk insert
  - `OpdaterElement(element)` - Tester opdatering
  - `SletElement(id)` - Tester sletning

- **PalleOptimeringSettingsService klassen**:
  - `GetSettings()` - Tester hentning af alle settings
  - `GetAktivSettings()` - Tester hentning af aktive settings
  - `OpdaterSettings(settings)` - Tester opdatering af indstillinger

**Model Validation Tests:**
- **Palle model** (reference til ER-diagram):
  - Validering af påkrævede felter (PalleBeskrivelse, Laengde, Bredde, Hoejde)
  - Validering af numeriske værdier er positive
  - Validering af decimal præcision (Vaegt: DECIMAL(18,2))
  - Validering af string længder (PalleBeskrivelse: max 100 tegn)

- **Element model**:
  - Validering af Reference (max 100 tegn, påkrævet)
  - Validering af Type og Serie (max 50-100 tegn)
  - Validering af dimensioner (Hoejde, Bredde, Dybde > 0)
  - Validering af Vaegt (DECIMAL(18,2), positiv)
  - Validering af RotationsRegel enum værdier (Nej, Ja, Skal)

- **PalleOptimeringSettings model**:
  - Validering af Navn (max 100 tegn, påkrævet)
  - Validering af MaksLag (positiv integer)
  - Validering af decimal faktorer (HoejdeBreddefaktor, TilladVendeOpTilMaksKg)
  - Validering af SorteringsPrioritering (max 500 tegn)

### 4.2 Komponenttest

**Formål**: At teste individuelle komponenter og deres interne logik isoleret fra resten af systemet.

**Hvorfor denne type giver mening**: Sikrer at hver komponent fungerer korrekt før integration.

**Hvornår den udføres**: Efter unit tests og før integrationstests.

#### Konkret kode der testes:

**Controller komponenter** (reference til klassediagram):
- **PallerController**:
  - `GetAllePaller()` - Test HTTP GET returnerer 200 OK med data
  - `GetPalle(id)` - Test 200 OK ved fund, 404 NotFound ved manglende
  - `OpretPalle(palle)` - Test 201 Created ved success, 400 BadRequest ved invalid data
  - `OpdaterPalle(id, palle)` - Test 204 NoContent ved success
  - `SletPalle(id)` - Test 204 NoContent ved success

- **ElementerController**:
  - `GetAlleElementer()` - Test returværdi og status kode
  - `OpretFlereElementer(elementer)` - Test bulk insert returnerer korrekt antal
  - `ForceSeedElementer()` - Test seed data oprettes korrekt

- **PalleOptimeringController**:
  - `GenererPakkeplan(request)` - Test med valid/invalid PakkeplanRequest DTO
  - `GetAllePakkeplaner()` - Test returnering af historik
  - `GetPakkeplan(id)` - Test hentning af specifik pakkeplan

**DTO Validering**:
- **PakkeplanRequest**:
  - Test at ElementIds liste ikke er tom
  - Test at OrdreReference er valid
  - Test optional SettingsId håndteres korrekt

### 4.3 Integrationstest

**Formål**: At sikre de forskellige dele og funktioner af koden kan arbejde sammen og udføre den ønskede handling uden fejl eller mangler.

**Hvorfor giver denne type mening**: Denne type af test sikrer at vi holder en vis standard af kvalitet i koden, samt at vores funktioner, klasser og metoder kan arbejde sammen og udføre det ønskede resultat.

**Hvornår den udføres**: Når vi har nogle stykker kode/dele af programmet som skal arbejde sammen og bruge hinanden. Disse tests kan påbegyndes så snart vi har flere stykker kode som skal arbejde sammen.

#### Konkret kode der testes:

**Controller → Service Integration** (reference til klassediagram):
- **PallerController → IPalleService**:
  - Test at controller korrekt kalder service metoder
  - Test at service exceptions håndteres i controller
  - Test at data transformeres korrekt mellem lag

- **ElementerController → IElementService**:
  - Test CRUD operationer gennem hele stakken
  - Test logger integration og error logging
  - Test at bulk operationer fungerer end-to-end

- **PalleOptimeringController → IPalleOptimeringService**:
  - Test at optimeringsalgoritmen kaldes korrekt
  - Test at alle afhængigheder (IElementService, IPalleService, ISettingsService) fungerer sammen

**Service → Database Integration** (reference til ER-diagram):
- **PalleService → PalleOptimeringContext**:
  - Test at DbSet<Palle> operationer fungerer
  - Test at Entity Framework tracking virker
  - Test at Aktiv filtering fungerer korrekt

- **ElementService → PalleOptimeringContext**:
  - Test at DbSet<Element> CRUD operationer fungerer
  - Test at seed data indsættes korrekt via ForceSeedElementer

- **PalleOptimeringService → Multiple Services**:
  - Test integration mellem PalleOptimeringService og dets tre dependencies:
    - IPalleOptimeringSettingsService (for at hente aktive settings)
    - IElementService (for at hente elementer)
    - IPalleService (for at finde passende paller)
  - Test at GenererPakkeplan korrekt gemmer til database:
    - Pakkeplan tabel (Id, OrdreReference, Oprettet, SettingsId)
    - PakkeplanPaller tabel (med relationer til Paller)
    - PakkeplanElementer tabel (med relationer til Elementer)

**Database Relationer** (reference til ER-diagram):
- Test **Pakkeplaner → Settings** foreign key relation
- Test **Pakkeplaner → PakkeplanPaller** one-to-many relation (CASCADE delete)
- Test **PakkeplanPaller → Paller** foreign key relation
- Test **PakkeplanPaller → PakkeplanElementer** one-to-many relation (CASCADE delete)
- Test **PakkeplanElementer → Elementer** foreign key relation
- Test at RESTRICT delete på master data (Paller, Elementer) forhindrer sletning når i brug

### 4.4 Systemtest

**Formål**: At sikre sammenspil i hele programmet, altså at al koden fungerer som den skal og at alle de nødvendige forbindelser mellem funktionerne arbejder korrekt sammen.

**Hvorfor giver denne type mening**: Systemtesten skal validere at hele systemet fungerer som tiltænkt og at integrationer mellem kodestykker alle virker uden fejl. Den er også med til at lede efter fejl som tidligere ikke er opdaget. Vi vil med denne test sikre at de funktionelle og ikke-funktionelle krav er opnået.

**Hvornår den udføres**: Den udføres når al koden er færdigskrevet og kun har minimale mangler, og de foregående tests alle er gennemført og godkendt.

#### Konkret kode der testes (hele systemet):

**End-to-End Workflows**:

1. **Komplet Pakkeplan Workflow**:
   - Bruger logger ind via AccountController (authentication)
   - Navigerer til Optimering side via HomeController
   - Frontend henter elementer via ElementerController.GetAlleElementer()
   - Frontend henter paller via PallerController.GetAllePaller()
   - Frontend henter settings via SettingsController.GetSettings()
   - Bruger vælger elementer og sender request til PalleOptimeringController.GenererPakkeplan()
   - Service lag:
     - PalleOptimeringService henter aktive settings via IPalleOptimeringSettingsService
     - Henter element detaljer via IElementService
     - Finder passende paller via IPalleService
     - Udfører optimeringsalgoritme
     - Gemmer Pakkeplan, PakkeplanPaller og PakkeplanElementer til database
   - Frontend modtager PakkeplanResultat DTO med komplet plan
   - Bruger kan se historik via PalleOptimeringController.GetAllePakkeplaner()

2. **Element Management Workflow**:
   - Admin bruger logger ind med SuperUser rolle
   - Navigerer til Elementer via HomeController.Elementer()
   - CRUD operationer via ElementerController
   - Verificer rolle-baseret adgangskontrol virker (NormalUser kan kun læse)

3. **Palle Management Workflow**:
   - Admin administrerer palle typer via PallerController
   - Test Aktiv/Inaktiv toggling
   - Test sortering fungerer

**Database Konsistens**:
- Test at alle foreign keys fungerer korrekt
- Test cascade delete på Pakkeplaner
- Test at master data ikke kan slettes ved RESTRICT constraints

**ASP.NET Core Identity Integration** (reference til ER-diagram):
- Test at AspNetUsers, AspNetRoles, AspNetUserRoles tabeller fungerer
- Test login/logout flow gennem AccountController
- Test rolle-baseret autorisation på alle controllers

### 4.5 Sikkerhedstest

**Formål**: At sikre at systemet er beskyttet mod uautoriseret adgang og at kun brugere med korrekte rettigheder kan udføre specifikke handlinger.

**Hvorfor denne type giver mening**: Systemet håndterer virksomhedsdata og skal overholde sikkerhedskrav for adgangskontrol.

**Hvornår den udføres**: Efter integrationstest og som del af systemtest.

#### Konkret kode der testes:

**Authentication Tests** (reference til klassediagram - AccountController):
- **AccountController.Login()**:
  - Test login med valid credentials
  - Test login med invalid credentials returnerer fejl
  - Test at SignInManager håndterer authentication korrekt
  - Test password hashing i AspNetUsers tabel (PasswordHash kolonne)

- **AccountController.Register()**:
  - Test bruger oprettelse via UserManager
  - Test at passwords hashs korrekt
  - Test email validering
  - Test at CreatedAt sættes automatisk

- **AccountController.Logout()**:
  - Test at session invalideres korrekt
  - Test redirect efter logout

**Authorization Tests** (reference til ER-diagram - Rolle struktur):
- Test **SuperUser rolle**:
  - Har adgang til alle CRUD operationer
  - Kan oprette/redigere via PallerController, ElementerController, SettingsController
  - Kan se alle views via HomeController

- Test **NormalUser rolle**:
  - Kan kun udføre GET requests
  - POST/PUT/DELETE returnerer 403 Forbidden
  - Kan se data men ikke ændre

- Test **AspNetUserRoles** junction tabel:
  - Test at rolle-tildeling fungerer
  - Test at brugere kan have multiple roller (many-to-many)

**Input Validation & Injection Prevention**:
- Test SQL injection beskyttelse (Entity Framework parameterisering)
- Test XSS beskyttelse i views
- Test CSRF token validering på forms
- Test model binding validation for alle DTOs

**Session Management**:
- Test session timeout
- Test concurrent login håndtering
- Test LockoutEnabled og AccessFailedCount i AspNetUsers

### 4.6 Performancetest

**Formål**: At sikre at systemet performer tilstrækkeligt under realistisk og høj belastning.

**Hvorfor denne type giver mening**: Optimeringsalgoritmen kan være ressourcekrævende, og database queries skal være effektive.

**Hvornår den udføres**: Efter systemtest og før accepttest.

#### Konkret kode der testes:

**Database Query Performance**:
- **PalleService.GetAllePaller()**:
  - Test query tid med 100+ paller
  - Test at EF Core genererer effektive SQL queries
  - Test index usage på Paller tabel

- **ElementService.GetAlleElementer()**:
  - Test query tid med 1000+ elementer
  - Test at relationer ikke medfører N+1 queries

**Optimeringsalgoritme Performance** (PalleOptimeringService):
- **GenererPakkeplan()**:
  - Test responstid med 10 elementer (< 2 sekunder)
  - Test responstid med 100 elementer (< 10 sekunder)
  - Test responstid med 500 elementer
  - Test memory forbrug under optimering
  - Test at Settings parametre (MaksLag, HoejdeBreddefaktor) ikke skaber performance problemer

**Bulk Operations**:
- **ElementService.OpretFlereElementer()**:
  - Test insert tid for 100+ elementer samtidig
  - Test database connection pooling

**Database Connection Tests**:
- Test connection pooling i PalleOptimeringContext
- Test at connections lukkes korrekt efter brug
- Test max concurrent connections

### 4.7 Navigeringstest

**Formål**: At sikre at brugere kan navigere rundt i systemet uden problemer og at alle links og menuer virker.

**Hvorfor denne type giver mening**: God brugeroplevelse kræver intuitiv navigation.

**Hvornår den udføres**: Løbende under udvikling og i systemtest fasen.

#### Konkret kode der testes:

**HomeController Navigation** (reference til klassediagram):
- **HomeController.Index()**:
  - Test forside loader korrekt
  - Test navigation links til andre sider

- **HomeController.Paller()**:
  - Test navigation til palle administrations view
  - Test at view renderer korrekt med data

- **HomeController.Elementer()**:
  - Test navigation til element administrations view

- **HomeController.Optimering()**:
  - Test navigation til optimerings interface
  - Test at alle nødvendige komponenter loader

- **HomeController.Settings()**:
  - Test navigation til settings side
  - Test autorisation (kun SuperUser)

**Menu & Layout Tests**:
- Test at navigation menu vises på alle sider
- Test at login/logout links virker
- Test breadcrumbs hvis implementeret
- Test back-button funktionalitet

**URL Routing Tests**:
- Test at alle routes i Program.cs fungerer
- Test 404 handling for ugyldige URLs
- Test redirect efter login

### 4.8 Indholdstest

**Formål**: At verificere at data vises korrekt, at beregninger er præcise, og at information præsenteres som forventet.

**Hvornår den udføres**: Under systemtest og manuel test.

#### Konkret kode der testes:

**Data Display Accuracy**:
- **Paller visning**:
  - Verificer at alle kolonner fra Paller tabel vises (PalleBeskrivelse, Laengde, Bredde, Hoejde, Vaegt, osv.)
  - Test at decimal værdier formateres korrekt (2 decimaler for Vaegt)
  - Test at boolean værdier (Aktiv) vises korrekt

- **Elementer visning**:
  - Verificer alle felter fra Elementer tabel vises
  - Test at RotationsRegel vises som læsbar tekst (ikke enum værdi)
  - Test at boolean flags (ErSpecialelement, ErGeometrielement) vises tydeligt

**Pakkeplan Output**:
- **PakkeplanResultat DTO**:
  - Verificer at antal paller matcher faktisk antal i PakkeplanPaller
  - Verificer at antal elementer stemmer overens
  - Test at SamletHoejde og SamletVaegt beregnes korrekt per palle
  - Test at Lag nummering er konsistent (1 = nederst)
  - Test at Plads nummering er korrekt (1-5)
  - Test at ErRoteret vises korrekt

**Settings Display**:
- Verificer at alle PalleOptimeringSettings vises
- Test at beskrivende labels bruges (ikke tekniske navne)
- Test at kun Aktiv settings kan vælges til nye pakkeplaner

### 4.9 Brugergrænseflade Test (UI Test)

**Formål**: At sikre at GUI'en/brugergrænsefladen fungerer som den skal, og at alle interaktive elementer virker korrekt.

**Hvorfor denne type giver mening**: Det er et krav at systemet virker, og det er en nem måde at opdage problemer på.

**Hvornår den udføres**: Løbende under udvikling og som del af manuel test.

#### Konkret kode der testes:

**Forms & Input Validation**:
- **Login Form** (AccountController):
  - Test at username/password input felter validerer
  - Test required field validation
  - Test error messages vises ved fejl
  - Test submit button sender korrekt data

- **Opret Palle Form** (PallerController):
  - Test alle input felter (PalleBeskrivelse, Laengde, Bredde, osv.)
  - Test numeric validation (positive værdier)
  - Test decimal input formatting
  - Test submit/cancel buttons

- **Opret Element Form** (ElementerController):
  - Test alle felter validerer korrekt
  - Test dropdown for Type og Serie
  - Test checkbox for ErSpecialelement, ErGeometrielement
  - Test RotationsRegel dropdown (Nej, Ja, Skal)

- **Optimering Form** (PalleOptimeringController):
  - Test element selection (checkboxes eller multi-select)
  - Test settings selection dropdown
  - Test OrdreReference input
  - Test submit knap og loading state

**Interactive Elements**:
- **Buttons**:
  - Test alle CRUD operation buttons (Opret, Rediger, Slet)
  - Test at confirmation dialogs vises ved sletning
  - Test disabled state for NormalUser

- **Tables & Lists**:
  - Test sorting på kolonner hvis implementeret
  - Test pagination hvis mange records
  - Test filtering/search funktionalitet

- **Visual Feedback**:
  - Test loading spinners under API kald
  - Test success/error messages efter operationer
  - Test validation error styling (røde borders)

**Responsive Design**:
- Test layout på forskellige skærmstørrelser
- Test at forms er brugbare på tablets (hvis relevant)

### 4.10 Accepttest (UAT)

**Formål**: At sikre, at produktet opfylder de specificerede krav og er klar til at blive leveret. At kunden kan se at alle krav er opfyldt, og kunden kan også selv teste systemet og sikre at det lever op til deres forventninger.

**Hvem der involveres**: Kunden involveres i accepttesten når systemtesten er færdig og godkendt af hele teamet. Her kan kunden få indblik i, hvordan systemet virker og om alt fungerer som tiltænkt fra kundens side.

**Hvornår**: Vi begynder accepttesten når systemtesten er godkendt og vi har færdiggjort hele programmet.

#### Konkret kode der testes (brugerscenarier):

**Scenarie 1: Administrator Workflow**:
1. Login som SuperUser
2. Opret nye palle typer via PallerController
3. Opret test elementer via ElementerController
4. Konfigurer settings via SettingsController
5. Generer pakkeplan og verificer output
6. Se historik over tidligere pakkeplaner

**Scenarie 2: Normal Bruger Workflow**:
1. Login som NormalUser
2. Kan se men ikke redigere paller og elementer
3. Kan generere pakkeplaner
4. Kan se historik

**Scenarie 3: Kompleks Optimering**:
1. Vælg blanding af elementer (forskellige typer, serier, størrelser)
2. Inkluder specialelementer og geometrielementer
3. Test rotationsregler (Nej, Ja, Skal)
4. Verificer at Settings regler overholdes:
   - MaksLag ikke overskrides
   - MaksHoejde ikke overskrides
   - MaksVaegt ikke overskrides
   - TilladVendeOpTilMaksKg respekteres
   - SorteringsPrioritering følges

**Acceptance Kriterier**:
- Alle funktionelle krav fra opgavebeskrivelsen er opfyldt
- Systemet bruger korrekt ASP.NET MVC arkitektur
- Entity Framework og MS SQL Server fungerer som forventet
- Rolle-baseret sikkerhed virker
- Performance er acceptabel

### 4.11 Manuel Test

**Formål**: At sikre at GUI'en/brugergrænsefladen fungerer som den skal, og det gør man ved manuelt at gå ind og teste at alle knapper og tekstfelter fungerer som de skal.

**Hvorfor**: Det er et krav at systemet virker, og så er det også en meget simpel og nem test at lave.

**Hvornår**: Den udføres løbende, så man altid er sikker på, at det hele fungerer. Og til sidst vil vi gennemgå det hele manuelt for at være sikker på at den endelige version har så lidt som muligt til ingen fejl ved deployment.

#### Manuel test checklist:

**HomeController Views**:
- [ ] Index view loader uden fejl
- [ ] Paller view viser data korrekt
- [ ] Elementer view fungerer
- [ ] Optimering view har alle controls
- [ ] Settings view kun tilgængelig for SuperUser

**AccountController**:
- [ ] Login form validerer input
- [ ] Register form opretter bruger
- [ ] Logout redirecter korrekt
- [ ] Fejlmeddelelser vises tydeligt

**CRUD Operations (alle controllers)**:
- [ ] Create forms validerer og gemmer data
- [ ] Read/List views viser data korrekt
- [ ] Update forms pre-populerer data
- [ ] Delete bekræfter før sletning
- [ ] Success/error messages vises

**Frontend-Backend Integration**:
- [ ] Alle API endpoints svarer korrekt
- [ ] Error handling vises i UI
- [ ] Loading states fungerer
- [ ] Data refresh efter opdateringer

**Cross-browser Test** (hvis relevant):
- [ ] Chrome
- [ ] Edge
- [ ] Firefox

## 5. Testmiljø

### Hardware / Software
Testene udføres kun på PC der bruger operativsystemet Windows. Alt andet ville ikke blive testet, da ingen i gruppen benytter sig af det, og at der ikke er specifikke krav for andet i opgaven.

Front og Backend framework bliver lavet i ASP.NET og vi benytter os af Entity Framework samt MS SQL Server som opgaven kræver.

**Teknologi Stack** (som defineret i arkitekturen):
- ASP.NET Core MVC
- Entity Framework Core (Code First)
- MS SQL Server
- ASP.NET Core Identity (for authentication/authorization)

### Testdata

**Master Data** (reference til ER-diagram seed data):
- **Paller**: 3 standard træpaller (75'er, 80'er, 100'er)
- **Elementer**: 8 test elementer (døre, vinduer, special)
- **Settings**: 1 standard settings profil
- **Brugere**: SuperUser og NormalUser test accounts

**Test Scenarios Data**:
- Dummy ordre referencer
- Forskellige element kombinationer
- Edge cases (max vægt, max højde, rotation krav)

### Versionsnumre

Vi bruger Git til versionsstyring, og testene bliver kørt på Git branches, som udgangspunkt ikke main branchen. Derudover benytter vi os af Jira til registrering af testcases.

**Test Framework**:
- xUnit eller NUnit for unit/integration tests
- Moq for mocking af dependencies
- Entity Framework InMemory Database for test isolation

## 6. Risici & Afhængigheder

Hvilke potentielle problemer kan påvirke testning?

- **Manglende testdata**: Løses via seed data funktionalitet (ElementService.ForceSeedElementer)
- **Database afhængigheder**: Alle foreign key relationer skal være på plads før komplekse tests
- **Lav erfaring med test**: Team har begrænset erfaring med automated testing
- **Entity Framework kompleksitet**: Navigation properties og lazy loading kan give uventede resultater
- **Optimeringsalgoritme kompleksitet**: PalleOptimeringService logik er kompleks og kan være svær at teste

**Afhængigheder mellem tests**:
- Integration tests kræver at unit tests er godkendt
- System tests kræver at database migrationer er kørt
- Performance tests kræver realistisk datamængde
- UAT kræver at alle tekniske tests er bestået

## 7. Succeskriterier

Testfasen er bestået når:

- **Ingen kritiske fejl**: Blocker bugs er rettet
- **Funktionalitet matcher krav**: Alle krav fra opgavebeskrivelsen er implementeret og testet
- **Alle accepttests består**: Kunden godkender systemet
- **Identificerede fejl er rettet**: Alle kendte bugs er løst eller dokumenteret
- **Kode kvalitet**:
  - Alle unit tests består
  - Integration tests mellem Controllers, Services og Database fungerer
  - Relationer i ER-diagrammet virker som specificeret
- **Sikkerhed**:
  - Authentication og authorization fungerer
  - Rolle-baseret adgangskontrol virker
- **Performance**:
  - Optimeringsalgoritmen performer tilfredsstillende
  - Database queries er optimerede

## 8. Reference til Systemdokumentation

For detaljeret information om systemets struktur, se:
- [Klassediagram](./klassediagram.md) - Viser MVC arkitektur, Controllers, Services og Models
- [ER-Diagram](./er-diagram.md) - Viser database struktur og relationer
- [README](./README.md) - System oversigt og workflow beskrivelse

## 9. Test Prioritering

**Høj prioritet** (kritisk for systemets funktion):
1. Unit tests for Service lag
2. Integration tests mellem Services og Database
3. Sikkerhedstest (authentication/authorization)
4. Systemtest af hoved-workflow (optimering)

**Medium prioritet** (vigtig for kvalitet):
1. Component tests for Controllers
2. Performance tests
3. Input validation tests
4. Database constraints tests

**Lav prioritet** (nice to have):
1. UI detaljer
2. Cross-browser tests
3. Edge case scenarios
4. Advanced performance optimization
