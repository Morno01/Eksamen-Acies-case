# Testplan - Palleoptimering System
*Acies A/S - EAMV Eksamensprojekt*

---

## 1. Formål

Formålet med testplanen er at sikre, at systemets funktionalitet, kvalitet og krav bliver verificeret og valideret gennem en struktureret testproces.

Denne testplan specificerer præcist **hvilke dele af koden** der skal testes med reference til:
- **[Klassediagram](./klassediagram.md)** - Viser systemets klasser, controllers og services
- **[ER-Diagram](./er-diagram.md)** - Viser databasestruktur og relationer

## 2. Teststrategi

### 2.1 Overordnet Tilgang

Projektet følger **spiralmodellen**, hvilket betyder at udviklingen foregår iterativt og risikobaseret. Dette gør det muligt at:
- Justere løsningen løbende
- Minimere risici gennem tidlig test
- Identificere kritiske fejl i hver iteration

Spiralmodellen passer godt til projektet fordi:
- Vi arbejder med nye teknologier (ASP.NET Core 6.0, Entity Framework)
- Kravene er komplekse (optimeringsalgoritme)
- Der er behov for løbende validering

### 2.2 Test Rækkefølge

Tests udføres i følgende rækkefølge:

```
Unit Tests → Integration Tests → System Tests → Manuel Tests → Accepttests (UAT)
```

Hver testfase skal være godkendt før næste påbegyndes.

### 2.3 Reference Diagrammer

Testplanen refererer til systemets arkitektur dokumenteret i:
- **[Klassediagram](./klassediagram.md)** - Viser systemets klasser og deres relationer
- **[ER-Diagram](./er-diagram.md)** - Viser database struktur og relationer

## 3. Testomfang (Scope)

### 3.1 Inkluderet i Test

#### Backend Funktioner
- Services (PalleService, ElementService, PalleOptimeringService, SettingsService)
- Controllers (API endpoints)
- Database operationer (CRUD)
- Optimeringsalgoritme
- Authorization (roller og rettigheder)

#### UI Funktionalitet
- Login/Logout flow
- Navigation mellem sider
- CRUD formularer (Paller, Elementer, Settings)
- Optimering interface
- Responsivitet (Bootstrap layout)

#### Datavalidering
- Model validation (Required fields, StringLength, Range)
- Business logic validation
- Database constraints
- Foreign key integrity

#### Integration mellem Moduler
- Controller → Service → DbContext flow
- Frontend JavaScript → API endpoints
- Identity integration
- Entity Framework migrations

#### Brugerscenarier
- SuperUser workflow: Opret → Rediger → Optimer → Se resultat
- NormalUser workflow: Se data (read-only)
- Krav verificering
- End-to-end system test

### 3.2 Ekskluderet fra Test

- Performance test (ikke et krav)
- Load test / stress test
- Browser compatibility (kun Chrome/Edge)
- Mobile responsiveness (ikke specificeret)
- Deployment automation

## 4. Testtyper og Testcases

### 4.1 Unit Tests

**Formål**: Isolere og teste mindre enheder i koden (funktioner, klasser eller metoder) for at se om de virker korrekt, uafhængigt af resten af koden.

**Hvornår**: Tidligt i udviklingsprocessen, så snart en metode/klasse er færdig.

**Værktøjer**: XUnit, Moq (mocking), Entity Framework In-Memory Database

#### 4.1.1 Service Layer Tests

Baseret på [Klassediagram](./klassediagram.md) - Service Layer komponenter.

##### PalleService Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| UT-PS-01 | GetAllePaller_ReturnsAllPaller | Hent alle paller fra database | Liste med 3 paller (seed data) |
| UT-PS-02 | GetAlleAktivePaller_ReturnsOnlyActive | Hent kun aktive paller | Kun paller med Aktiv=true |
| UT-PS-03 | GetPalle_ValidId_ReturnsPalle | Hent specifik palle med gyldigt ID | Palle objekt returneres |
| UT-PS-04 | GetPalle_InvalidId_ReturnsNull | Hent palle med ugyldigt ID | Null returneres |
| UT-PS-05 | OpretPalle_ValidData_SavesSuccessfully | Opret ny palle med gyldige data | Palle gemmes, Id tildeles |
| UT-PS-06 | OpretPalle_InvalidData_ThrowsException | Opret palle med manglende required felter | Exception kastes |
| UT-PS-07 | OpdaterPalle_ExistingPalle_UpdatesSuccessfully | Opdater eksisterende palle | Ændringer gemmes i database |
| UT-PS-08 | SletPalle_ExistingPalle_DeletesSuccessfully | Slet palle | Palle fjernes fra database |

**Kode Reference**: `MyProject/Services/PalleService.cs`

##### ElementService Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| UT-ES-01 | GetAlleElementer_ReturnsAllElements | Hent alle elementer | Liste med 8 elementer (seed data) |
| UT-ES-02 | GetElement_ValidId_ReturnsElement | Hent specifikt element | Element objekt returneres |
| UT-ES-03 | OpretElement_ValidData_SavesSuccessfully | Opret nyt element | Element gemmes med korrekt Type, Serie |
| UT-ES-04 | OpretFlereElementer_BulkInsert_SavesAll | Opret flere elementer på én gang | Alle elementer gemmes |
| UT-ES-05 | OpdaterElement_ExistingElement_UpdatesSuccessfully | Opdater element dimensioner | Ændringer gemmes |
| UT-ES-06 | SletElement_ExistingElement_DeletesSuccessfully | Slet element | Element fjernes |
| UT-ES-07 | OpretElement_NegativeWeight_ThrowsException | Opret element med negativ vægt | Validation fejler |

**Kode Reference**: `MyProject/Services/ElementService.cs`

##### PalleOptimeringSettingsService Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| UT-SS-01 | GetSettings_ReturnsAllSettings | Hent alle settings profiler | Liste returneres |
| UT-SS-02 | GetAktivSettings_ReturnsActiveProfile | Hent aktiv settings profil | Standard profil returneres |
| UT-SS-03 | OpdaterSettings_ValidData_UpdatesSuccessfully | Opdater MaksLag og regler | Ændringer gemmes |

**Kode Reference**: `MyProject/Services/PalleOptimeringSettingsService.cs`

##### PalleOptimeringService Tests (Kernelogik)

Baseret på optimeringsalgoritmen beskrevet i README.md.

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| UT-OS-01 | GenererPakkeplan_SingleElement_CreatesOnePalle | Generér plan med 1 element | 1 palle oprettes |
| UT-OS-02 | GenererPakkeplan_MultipleElements_OptimizesPacking | Generér plan med 5 elementer | Minimalt antal paller |
| UT-OS-03 | GenererPakkeplan_RespectsMaksLag | Tjek at MaksLag respekteres | Maks 2 lag pr. palle (default) |
| UT-OS-04 | GenererPakkeplan_RespectsMaksVaegt | Tjek vægtbegrænsning | Total vægt < palle.MaksVaegt |
| UT-OS-05 | GenererPakkeplan_RespectsMaksHoejde | Tjek højdebegrænsning | Total højde < palle.MaksHoejde |
| UT-OS-06 | GenererPakkeplan_RotationLogic | Tjek rotationsregler | Element roteres kun hvis tilladt |
| UT-OS-07 | GenererPakkeplan_SpecialElement_HandlesCorrectly | Special elementer håndteres | MaksElementerPrPalle respekteres |
| UT-OS-08 | GenererPakkeplan_GeometriElement_NoStacking | Geometri element må ikke stables | ErGeometrielement i øverste lag |
| UT-OS-09 | GenererPakkeplan_SavesPakkeplan | Pakkeplan gemmes i database | Pakkeplan, Paller, Elementer gemmes |

**Kode Reference**: `MyProject/Services/PalleOptimeringService.cs`

#### 4.1.2 Model Validation Tests

Baseret på [ER-Diagram](./er-diagram.md) - Database constraints.

##### Palle Model Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| UT-PM-01 | Palle_RequiredFields_Validates | Test required attributter | PalleBeskrivelse, Laengde, osv. required |
| UT-PM-02 | Palle_StringLength_Validates | Test StringLength constraints | Max 100 tegn for PalleBeskrivelse |
| UT-PM-03 | Palle_DecimalPrecision_Validates | Test decimal precision | Vaegt og MaksVaegt DECIMAL(18,2) |

**Kode Reference**: `MyProject/Models/Palle.cs`

##### Element Model Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| UT-EM-01 | Element_RequiredDimensions_Validates | Test required dimensioner | Hoejde, Bredde, Dybde, Vaegt required |
| UT-EM-02 | Element_RotationsRegel_ValidValues | Test RotationsRegel values | Kun "Nej", "Ja", "Skal" tilladt |
| UT-EM-03 | Element_Type_StringLength | Test Type StringLength | Max 100 tegn |

**Kode Reference**: `MyProject/Models/Element.cs`

### 4.2 Integration Tests

**Formål**: Sikre at forskellige dele og funktioner af koden kan arbejde sammen og udføre den ønskede handling uden fejl.

**Hvornår**: Efter unit tests, når flere komponenter skal arbejde sammen.

**Værktøjer**: XUnit, WebApplicationFactory (for API tests)

#### 4.2.1 Controller → Service Integration

Baseret på [Klassediagram](./klassediagram.md) - Controller → Service relationer.

##### PallerController Integration Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| IT-PC-01 | GetAllePaller_ReturnsOkWithData | GET /api/paller | HTTP 200, JSON array med paller |
| IT-PC-02 | GetPalle_ValidId_ReturnsOk | GET /api/paller/1 | HTTP 200, palle objekt |
| IT-PC-03 | GetPalle_InvalidId_ReturnsNotFound | GET /api/paller/999 | HTTP 404 |
| IT-PC-04 | OpretPalle_ValidData_ReturnsCreated | POST /api/paller | HTTP 201, Location header |
| IT-PC-05 | OpretPalle_InvalidData_ReturnsBadRequest | POST /api/paller (mangler data) | HTTP 400, ModelState errors |
| IT-PC-06 | OpdaterPalle_ValidData_ReturnsOk | PUT /api/paller/1 | HTTP 200 |
| IT-PC-07 | SletPalle_ExistingPalle_ReturnsNoContent | DELETE /api/paller/1 | HTTP 204 |

**Kode Reference**: `MyProject/Controllers/PallerController.cs`

##### ElementerController Integration Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| IT-EC-01 | GetAlleElementer_ReturnsOkWithData | GET /api/elementer | HTTP 200, JSON array |
| IT-EC-02 | OpretElement_ValidData_ReturnsCreated | POST /api/elementer | HTTP 201, element oprettet |
| IT-EC-03 | OpretFlereElementer_BulkInsert_ReturnsOk | POST /api/elementer/bulk | HTTP 200, alle elementer oprettet |
| IT-EC-04 | ForceSeedElementer_CreatesTestData | POST /api/elementer/force-seed | HTTP 200, 3 test elementer oprettet |

**Kode Reference**: `MyProject/Controllers/ElementerController.cs`

##### PalleOptimeringController Integration Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| IT-OC-01 | GenererPakkeplan_ValidRequest_ReturnsOk | POST /api/palleoptimering/generer | HTTP 200, pakkeplan genereret |
| IT-OC-02 | GenererPakkeplan_EmptyElementList_ReturnsBadRequest | POST med tom element liste | HTTP 400 |
| IT-OC-03 | GetPakkeplan_ExistingId_ReturnsOk | GET /api/palleoptimering/pakkeplaner/1 | HTTP 200, komplet pakkeplan |
| IT-OC-04 | GetAllePakkeplaner_ReturnsOkWithData | GET /api/palleoptimering/pakkeplaner | HTTP 200, liste af pakkeplaner |

**Kode Reference**: `MyProject/Controllers/PalleOptimeringController.cs`

#### 4.2.2 Database Integration

Baseret på [ER-Diagram](./er-diagram.md) - Foreign key relationer.

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| IT-DB-01 | Pakkeplan_ToPakkeplanPalle_Relationship | Test 1:many relation | Pakkeplan kan have flere PakkeplanPaller |
| IT-DB-02 | PakkeplanPalle_ToPakkeplanElement_Relationship | Test 1:many relation | PakkeplanPalle kan have flere elementer |
| IT-DB-03 | ForeignKey_Cascade_Delete | Slet pakkeplan | PakkeplanPaller og PakkeplanElementer slettes også |
| IT-DB-04 | Palle_Reference_Restrict | Forsøg at slette palle i brug | Fejl - palle bruges i pakkeplan |
| IT-DB-05 | Settings_Reference_Restrict | Forsøg at slette settings i brug | Fejl - settings bruges i pakkeplan |

**Kode Reference**: `MyProject/Data/PalleOptimeringContext.cs`

#### 4.2.3 Authorization Integration

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| IT-AU-01 | GET_Endpoint_NormalUser_Allowed | NormalUser GET /api/elementer | HTTP 200 |
| IT-AU-02 | POST_Endpoint_NormalUser_Forbidden | NormalUser POST /api/elementer | HTTP 403 Forbidden |
| IT-AU-03 | POST_Endpoint_SuperUser_Allowed | SuperUser POST /api/elementer | HTTP 201 Created |
| IT-AU-04 | Settings_Page_NormalUser_Forbidden | NormalUser /Home/Settings | Redirect til AccessDenied |
| IT-AU-05 | Unauthenticated_Redirect_To_Login | Ikke logget ind bruger | Redirect til /Account/Login |

**Kode Reference**:
- `MyProject/Controllers/*Controller.cs` (Authorize attributes)
- `MyProject/Program.cs` (Authorization setup)

### 4.3 System Tests

**Formål**: Sikre sammenspil i hele programmet - at al koden fungerer som den skal og alle forbindelser mellem funktionerne arbejder korrekt sammen.

**Hvornår**: Når al kode er færdigskrevet og foregående tests er godkendt.

**Metode**: End-to-end scenarier der tester hele systemet.

#### 4.3.1 SuperUser Workflow

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| ST-SU-01 | CompleteWorkflow_CreatePalleAndElements | 1. Login som SuperUser<br>2. Opret ny palle<br>3. Opret 5 nye elementer<br>4. Generér pakkeplan<br>5. Se resultat | Alle steps succeeds, pakkeplan vises korrekt |
| ST-SU-02 | EditExistingPalle_SaveChanges | 1. Login<br>2. Rediger palle (ændre MaksVaegt)<br>3. Gem<br>4. Verificer ændringer | Ændringer persisteres i database |
| ST-SU-03 | DeleteElement_VerifyRemoved | 1. Login<br>2. Slet et element<br>3. Verificer det ikke vises i liste | Element fjernet fra UI og database |
| ST-SU-04 | UpdateSettings_ReflectsInOptimization | 1. Login<br>2. Ændre MaksLag fra 2 til 3<br>3. Generér pakkeplan<br>4. Verificer 3 lag tilladt | Settings påvirker optimering |

#### 4.3.2 NormalUser Workflow

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| ST-NU-01 | ViewOnlyAccess_CannotCreate | 1. Login som NormalUser<br>2. Se Paller side<br>3. Verificer create form ikke vises<br>4. Forsøg API POST direkte | Form skjult, API returnerer 403 |
| ST-NU-02 | ViewAllData_NoEditButtons | 1. Login<br>2. Se Elementer, Paller, Serier<br>3. Verificer ingen edit/delete knapper | Kun read-only visning |

#### 4.3.3 Database Migrations og Seed Data

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| ST-DB-01 | FreshDatabase_MigrationsRun | 1. Slet database<br>2. Start applikation<br>3. Verificer migrations køres | Database oprettes automatisk |
| ST-DB-02 | SeedData_InsertsCorrectly | 1. Fresh database<br>2. Verificer seed data | 3 paller, 8 elementer, 1 settings, 2 brugere |
| ST-DB-03 | DbInitializer_CreatesRolesAndUsers | 1. Fresh database<br>2. Login som admin@acies.dk | Login succeeds, SuperUser rolle |

**Kode Reference**:
- `MyProject/Program.cs` (Auto-migration)
- `MyProject/Data/DbInitializer.cs`
- `MyProject/Data/PalleOptimeringContext.cs` (Seed data)

#### 4.3.4 Frontend Integration

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| ST-FE-01 | JavaScript_LoadsElementer_OnPageLoad | 1. Login<br>2. Naviger til Elementer side<br>3. Tjek browser console | API kaldt, elementer vist |
| ST-FE-02 | CreateForm_SubmitsViaAjax | 1. Udfyld element form<br>2. Submit<br>3. Tjek liste opdateres | Element tilføjes uden page refresh |
| ST-FE-03 | ErrorHandling_ShowsInUI | 1. Create element med invalid data<br>2. Submit | Fejlbesked vises i UI |

**Kode Reference**:
- `MyProject/Views/Home/Elementer.cshtml` (JavaScript)
- `MyProject/Views/Home/Paller.cshtml`

### 4.4 Manuel Tests

**Formål**: Sikre at GUI/brugergrænsefladen fungerer som den skal ved manuelt at teste alle knapper, tekstfelter og navigation.

**Hvornår**: Løbende gennem udviklingen og før deployment.

#### 4.4.1 UI/UX Tests

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| MT-UI-01 | Navigation_AllLinksWork | Klik på alle menu links | Alle sider loader korrekt |
| MT-UI-02 | Forms_AllFieldsEditable | Test alle input felter | Felter kan udfyldes, placeholder text vises |
| MT-UI-03 | Buttons_AllResponsive | Klik på alle knapper | Buttons trigger korrekt handling |
| MT-UI-04 | ResponsiveLayout_Resizing | Resize browser vindue | Bootstrap grid tilpasser sig |
| MT-UI-05 | Icons_DisplayCorrectly | Se alle sider | Font Awesome icons loader |
| MT-UI-06 | Loading_Indicators_Show | Submit langsom operation | Loading spinner vises |
| MT-UI-07 | Validation_ErrorMessages_Display | Submit form med fejl | Røde fejlbeskeder vises |

#### 4.4.2 Login/Logout Flow

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| MT-LO-01 | Login_ValidCredentials | Login med admin@acies.dk / admin | Redirect til /Home/Index |
| MT-LO-02 | Login_InvalidCredentials | Login med forkert password | Fejlbesked vises |
| MT-LO-03 | Logout_RedirectsToLogin | Klik logout knap | Redirect til login side |
| MT-LO-04 | RememberMe_PersistsSession | Login med "Husk mig" | Session bevares efter browser genstart |

**Kode Reference**: `MyProject/Views/Account/Login.cshtml`

#### 4.4.3 Browser Console Check

| Test ID | Test Navn | Beskrivelse | Forventet Resultat |
|---------|-----------|-------------|-------------------|
| MT-BC-01 | NoJavaScriptErrors | F12 → Console på alle sider | Ingen røde fejl |
| MT-BC-02 | DiagnosticLogging_Works | Se console på Elementer side | Logger viser "🔍 Henter elementer fra:" |
| MT-BC-03 | Network_AllRequests200 | F12 → Network tab | Alle API calls returnerer 200 |

### 4.5 Accepttest (UAT)

**Formål**: Sikre at produktet opfylder de specificerede krav og er klar til at blive leveret. Kunden tester systemet og verificerer det lever op til forventningerne.

**Hvem**: Acies A/S + EAMV eksaminator + projektgruppe

**Hvornår**: Efter systemtest er godkendt og programmet er færdigt.

#### 4.5.1 Funktionelle Krav

| Test ID | Krav ID | Test Navn | Beskrivelse | Succeskriterium |
|---------|---------|-----------|-------------|-----------------|
| UAT-FK-01 | FR-01 | CRUD Paller | Verificer opret, læs, opdater, slet paller | Alle operationer virker |
| UAT-FK-02 | FR-02 | CRUD Elementer | Verificer opret, læs, opdater, slet elementer | Alle operationer virker |
| UAT-FK-03 | FR-03 | Optimeringsalgoritme | Generér pakkeplan med 10 elementer | Pakkeplanen minimerer antal paller |
| UAT-FK-04 | FR-04 | Roller og Rettigheder | Test SuperUser vs NormalUser | NormalUser kun read-only |
| UAT-FK-05 | FR-05 | Settings Management | Opdater MaksLag, SorteringsPrioritering | Settings gemmes og bruges |
| UAT-FK-06 | FR-06 | Pakkeplan Visning | Se genereret pakkeplan med lag og placering | Al information vises korrekt |

#### 4.5.2 Ikke-Funktionelle Krav

| Test ID | Krav ID | Test Navn | Beskrivelse | Succeskriterium |
|---------|---------|-----------|-------------|-----------------|
| UAT-NK-01 | NFR-01 | Brugervenlighed | Ikke-teknisk bruger kan oprette element | Under 2 min. uden hjælp |
| UAT-NK-02 | NFR-02 | Response Tid | API kald til /api/elementer | < 2 sekunder |
| UAT-NK-03 | NFR-03 | Data Persistens | Opret data, genstart server, verificer data | Data bevares efter restart |
| UAT-NK-04 | NFR-04 | Sikkerhed | Forsøg at tilgå /api/paller uden login | 401 Unauthorized redirect |

#### 4.5.3 Brugerscenarier

| Test ID | Scenarie | Beskrivelse | Succeskriterium |
|---------|----------|-------------|-----------------|
| UAT-BS-01 | Daglig Brug - Optimering | Acies medarbejder modtager ordre med 15 elementer, genererer pakkeplan | Pakkeplan klar på under 5 min |
| UAT-BS-02 | Undtagelse - Special Element | Element med ErGeometrielement=true pakkes | Ingen stabling ovenpå geometri element |
| UAT-BS-03 | Ændring - Ny Palletype | SuperUser tilføjer ny 120'er palle | Palle tilgængelig i optimering |

## 5. Testmiljø

### 5.1 Hardware

- **Platform**: Windows 10/11 PC
- **Minimum**: 8 GB RAM, 2 GHz processor
- **Browser**: Chrome 120+ eller Edge 120+

### 5.2 Software

| Komponent | Version | Formål |
|-----------|---------|--------|
| .NET SDK | 6.0 | Runtime og compilation |
| ASP.NET Core | 6.0 | Web framework |
| Entity Framework Core | 6.0.25 | ORM og database migrations |
| MS SQL Server | LocalDB eller Azure SQL | Database |
| Bootstrap | 5.3.0 | Frontend UI framework |
| Font Awesome | 6.4.0 | Icons |
| XUnit | 2.4.0+ | Test framework |
| Moq | 4.18.0+ | Mocking framework |

### 5.3 Database

#### Development/Test
- **LocalDB**: `(localdb)\\mssqllocaldb`
- **Database Navn**: PalleOptimeringDb_Test
- **Migration**: Auto-applied on startup

#### Production (UAT)
- **Azure SQL**: bmm-server.database.windows.net
- **Database**: bmm
- **Connection**: Connection string i appsettings.json

### 5.4 Testdata

#### Seed Data (automatisk)
- 3 Paller (75'er, 80'er, 100'er)
- 8 Elementer (døre, vinduer, special)
- 1 Settings profil (Standard)
- 2 Brugere (admin@acies.dk, bruger@acies.dk)

#### Test Brugere

| Email | Password | Rolle | Formål |
|-------|----------|-------|--------|
| admin@acies.dk | admin | SuperUser | Fuld adgang til test |
| bruger@acies.dk | bruger | NormalUser | Read-only test |

### 5.5 Versionsstyring

- **Git**: Branches for feature development
- **Main Branch**: Beskyttet, kun merge efter godkendte tests
- **Test Branches**: `test/*` branches for test execution
- **Jira**: Registrering af testcases og bugs

## 6. Risici & Afhængigheder

### 6.1 Identificerede Risici

| Risiko ID | Beskrivelse | Sandsynlighed | Konsekvens | Mitigering |
|-----------|-------------|---------------|------------|------------|
| R-01 | Manglende testdata for edge cases | Middel | Høj | Opret dedikeret test dataset |
| R-02 | Azure SQL firewall bloker test | Lav | Middel | Brug LocalDB til unit/integration tests |
| R-03 | Lav erfaring med XUnit/Moq | Høj | Middel | Peer review, dokumentation |
| R-04 | Browser cache issues under test | Middel | Lav | Hard refresh (Ctrl+Shift+R) |
| R-05 | Migrations konflikt mellem branches | Middel | Middel | Koordiner migration oprettelse |
| R-06 | Optimeringsalgoritme kompleksitet | Høj | Høj | Iterativ udvikling, trin-for-trin test |

### 6.2 Afhængigheder

| Afhængighed | Type | Impact | Mitigering |
|-------------|------|--------|------------|
| Azure SQL tilgængelighed | Ekstern | Høj | Fallback til LocalDB |
| .NET 6.0 runtime | Software | Kritisk | Verificer installation før test |
| Entity Framework migrations | Intern | Høj | Test migrations separat |
| ASP.NET Identity setup | Intern | Høj | DbInitializer unit tests |
| Bootstrap/Font Awesome CDN | Ekstern | Lav | Lokal kopi som fallback |

## 7. Testudførelse

### 7.1 Test Execution Plan

#### Iteration 1 (Uge 1-2)
- [ ] Unit tests for Services
- [ ] Model validation tests
- [ ] Database migration test

#### Iteration 2 (Uge 3-4)
- [ ] Integration tests for Controllers
- [ ] Authorization tests
- [ ] Frontend JavaScript tests

#### Iteration 3 (Uge 5)
- [ ] System tests - komplette workflows
- [ ] Manuel UI/UX tests
- [ ] Performance baseline

#### Iteration 4 (Uge 6)
- [ ] UAT med Acies
- [ ] Bug fixes fra UAT
- [ ] Retest af kritiske fejl
- [ ] Final godkendelse

### 7.2 Bug Tracking

Bugs registreres i Jira med følgende felter:

- **Severity**: Critical, High, Medium, Low
- **Priority**: P1 (fix immediately), P2 (fix before UAT), P3 (nice to have)
- **Test ID**: Reference til testcase
- **Environment**: LocalDB / Azure SQL
- **Steps to Reproduce**
- **Expected vs Actual Result**
- **Screenshot/Log**

### 7.3 Test Rapportering

Efter hver test iteration:

1. **Test Summary Report**
   - Total tests: X
   - Passed: Y
   - Failed: Z
   - Pass rate: Y/X %

2. **Defect Report**
   - Critical bugs: X
   - All bugs by severity
   - Root cause analysis

3. **Coverage Report**
   - Code coverage % (mål: >70%)
   - Untested areas

## 8. Succeskriterier

Testfasen er **bestået** når:

### 8.1 Kvantitative Kriterier

- ✅ **≥95% af unit tests bestået**
- ✅ **≥90% af integration tests bestået**
- ✅ **100% af system tests bestået**
- ✅ **100% af accepttests bestået**
- ✅ **Ingen kritiske (P1) bugs**
- ✅ **Max 3 high (P2) bugs**
- ✅ **Code coverage ≥70%**

### 8.2 Kvalitative Kriterier

- ✅ **Funktionalitet matcher krav** fra kravspecifikation
- ✅ **SuperUser kan udføre alle CRUD operationer**
- ✅ **NormalUser har kun read-only adgang**
- ✅ **Optimeringsalgoritme producerer valide pakkeplaner**
- ✅ **Database constraints respekteres**
- ✅ **Authorization virker korrekt**
- ✅ **UI er brugervenlig og responsiv**

### 8.3 Acies Godkendelse

- ✅ **Acies produktejer godkender UAT**
- ✅ **System er deployment-klar**
- ✅ **Dokumentation er komplet** (README, Diagrammer, Testplan)

## 9. Reference Dokumentation

Denne testplan skal læses i sammenhæng med:

- **[Klassediagram](./klassediagram.md)** - Viser hvilke klasser og services der testes
- **[ER-Diagram](./er-diagram.md)** - Viser database struktur der valideres
- **[Arkitektur Oversigt](./README.md)** - Forklarer systemets opbygning
- **[Hovedprojekt README](../README.md)** - Installation og konfiguration

---

## Appendix A: Test Case Template

```
Test ID: [UT/IT/ST/MT/UAT]-[Komponent]-[Nummer]
Test Navn: [Beskrivende navn]
Testtype: [Unit/Integration/System/Manuel/UAT]
Komponent: [Service/Controller/Model/UI]
Prioritet: [Kritisk/Høj/Middel/Lav]

Forudsætninger:
- [Hvad skal være opsat først]

Test Steps:
1. [Step 1]
2. [Step 2]
3. [Step 3]

Testdata:
- Input: [Data]
- Expected: [Forventet output]

Forventet Resultat:
[Hvad skal ske]

Faktisk Resultat:
[Hvad skete der - udfyldes ved execution]

Status: [Pass/Fail/Blocked]
Tester: [Navn]
Dato: [DD-MM-YYYY]
```

## Appendix B: Test Coverage Matrix

| Komponent (fra Klassediagram) | Unit Test | Integration Test | System Test |
|-------------------------------|-----------|------------------|-------------|
| PalleService | ✅ UT-PS-01~08 | ✅ IT-PC-01~07 | ✅ ST-SU-01~04 |
| ElementService | ✅ UT-ES-01~07 | ✅ IT-EC-01~04 | ✅ ST-SU-01 |
| PalleOptimeringService | ✅ UT-OS-01~09 | ✅ IT-OC-01~04 | ✅ ST-SU-01 |
| PalleOptimeringSettingsService | ✅ UT-SS-01~03 | ✅ IT-OC-01 | ✅ ST-SU-04 |
| PalleOptimeringContext | ✅ UT-DB-01~05 | ✅ IT-DB-01~05 | ✅ ST-DB-01~03 |
| Authorization | N/A | ✅ IT-AU-01~05 | ✅ ST-NU-01~02 |
| Frontend (Elementer.cshtml) | N/A | ✅ ST-FE-01~03 | ✅ MT-UI-01~07 |
| Login Flow | N/A | N/A | ✅ MT-LO-01~04 |

## Appendix C: Database Test Queries

Brug disse SQL queries til at validere data efter tests:

```sql
-- Verificer seed data
SELECT COUNT(*) FROM Paller;              -- Skal være 3
SELECT COUNT(*) FROM Elementer;           -- Skal være 8
SELECT COUNT(*) FROM Settings;            -- Skal være 1
SELECT COUNT(*) FROM AspNetUsers;         -- Skal være 2

-- Verificer pakkeplan struktur (ER-diagram relationer)
SELECT
    p.Id,
    p.OrdreReference,
    p.AntalPaller,
    COUNT(DISTINCT pp.Id) AS AktuelAntalPaller,
    COUNT(pe.Id) AS TotalElementer
FROM Pakkeplaner p
LEFT JOIN PakkeplanPaller pp ON p.Id = pp.PakkeplanId
LEFT JOIN PakkeplanElementer pe ON pp.Id = pe.PakkeplanPalleId
GROUP BY p.Id, p.OrdreReference, p.AntalPaller;

-- Verificer foreign key constraints
SELECT
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fc.parent_object_id, fc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fc.referenced_object_id, fc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fc
    ON fk.object_id = fc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN
    ('Pakkeplaner', 'PakkeplanPaller', 'PakkeplanElementer');
```

---

**Version**: 1.0
**Dato**: 2025-12-05
**Ansvarlig**: Projektgruppe
**Godkendt af**: [Acies A/S / EAMV]
