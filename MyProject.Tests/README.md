# MyProject.Tests - Test Suite

Dette test projekt indeholder omfattende tests for Palleoptimering systemet, baseret på [testplanen](../docs/testplan.md).

## 📋 Oversigt

Test projektet følger testplanen og dækker:
- **Unit Tests** - Isolerede tests af Services og Models
- **Integration Tests** - Tests af Controller → Service → Database integration
- **Security Tests** - Authentication og Authorization tests
- **Model Validation Tests** - Data validering og constraints

## 🏗️ Projekt Struktur

```
MyProject.Tests/
├── UnitTests/
│   ├── PalleServiceTests.cs           # Service layer tests for Palle
│   └── ElementServiceTests.cs         # Service layer tests for Element
├── IntegrationTests/
│   └── PalleControllerIntegrationTests.cs  # Fuld stak integration tests
├── SecurityTests/
│   ├── AuthenticationTests.cs         # Login, Register, Logout tests
│   └── AuthorizationTests.cs          # Rolle-baseret adgangskontrol
└── MyProject.Tests.csproj             # Test projekt konfiguration
```

## 🔧 Teknologier

- **xUnit** - Test framework
- **Moq** - Mocking library til isolation af dependencies
- **Entity Framework InMemory** - In-memory database til isolerede tests
- **Microsoft.AspNetCore.Mvc.Testing** - Integration testing support

## 🧪 Test Kategorier

### 1. Unit Tests (Section 4.1 i testplan)

#### PalleServiceTests.cs
Tester **PalleService** klassen isoleret:
- ✅ `GetAllePaller()` - Returnerer alle paller
- ✅ `GetAlleAktivePaller()` - Filtrerer kun aktive paller
- ✅ `GetPalle(id)` - Henter enkelt palle (valid/invalid ID)
- ✅ `OpretPalle()` - Opretter ny palle med validering
- ✅ `OpdaterPalle()` - Opdaterer eksisterende palle
- ✅ `SletPalle()` - Sletter palle

**Reference til system**: docs/klassediagram.md - PalleService klasse

#### ElementServiceTests.cs
Tester **ElementService** klassen:
- ✅ `GetAlleElementer()` - Returnerer alle elementer
- ✅ `GetElement(id)` - Henter enkelt element
- ✅ `OpretElement()` - Opretter nyt element med validering
- ✅ `OpretFlereElementer()` - Bulk insert funktionalitet
- ✅ `OpdaterElement()` - Opdaterer element
- ✅ `SletElement()` - Sletter element
- ✅ Model validering - RotationsRegel enum (Nej, Ja, Skal)
- ✅ Boolean flags - ErSpecialelement, ErGeometrielement

**Reference til database**: docs/er-diagram.md - Elementer tabel

### 2. Integration Tests (Section 4.2 i testplan)

#### PalleControllerIntegrationTests.cs
Tester fuld stak integration: **Controller → Service → Database**

- ✅ `GetAllePaller()` - HTTP GET returnerer data fra database
- ✅ `GetPalle(id)` - 200 OK ved fund, 404 NotFound ved manglende
- ✅ `OpretPalle()` - 201 Created og gem til database
- ✅ `OpdaterPalle()` - 204 NoContent og opdater database
- ✅ `SletPalle()` - 204 NoContent og slet fra database
- ✅ Database constraints - RESTRICT på master data

**Reference til arkitektur**:
- docs/klassediagram.md - PallerController → IPalleService → PalleOptimeringContext
- docs/er-diagram.md - Paller tabel og relationer

### 3. Security Tests (Section 4.5 i testplan)

#### AuthenticationTests.cs
Tester **AccountController** og ASP.NET Core Identity:

- ✅ `Login()` - Valid credentials → success
- ✅ `Login()` - Invalid credentials → fejl
- ✅ `Register()` - Opretter ny bruger i AspNetUsers
- ✅ `Logout()` - Invaliderer session
- ✅ Password hashing - PasswordHash kolonne
- ✅ Account lockout - LockoutEnabled, AccessFailedCount

**Reference til database**: docs/er-diagram.md - AspNetUsers tabel

#### AuthorizationTests.cs
Tester rolle-baseret adgangskontrol:

- ✅ **SuperUser rolle** - Fuld adgang til alle CRUD operationer
- ✅ **NormalUser rolle** - Kun GET requests (read-only)
- ✅ Ikke-authenticated brugere - Nægtet adgang
- ✅ Multiple roller per bruger - AspNetUserRoles junction tabel
- ✅ HomeController.Settings() - Kun SuperUser
- ✅ GET endpoints - Tilgængelige for begge roller
- ✅ POST/PUT/DELETE - Kun SuperUser

**Reference til database**: docs/er-diagram.md - AspNetRoles, AspNetUserRoles

## 🚀 Kørsel af Tests

### Alle tests
```bash
dotnet test
```

### Specifik test kategori
```bash
dotnet test --filter "FullyQualifiedName~UnitTests"
dotnet test --filter "FullyQualifiedName~IntegrationTests"
dotnet test --filter "FullyQualifiedName~SecurityTests"
```

### Med code coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 Test Coverage

Baseret på testplanen dækker vi:

| Test Type | Dækning | Reference |
|-----------|---------|-----------|
| Unit Tests | Service lag (PalleService, ElementService) | Section 4.1 |
| Integration Tests | Controller → Service → Database | Section 4.2 |
| Security Tests | Authentication + Authorization | Section 4.5 |
| Model Validation | Palle, Element models | Section 4.1 |

## 🔗 Referencer til Systemdokumentation

Alle tests refererer til:
1. **Testplan**: docs/testplan.md - Beskriver hvad der skal testes
2. **Klassediagram**: docs/klassediagram.md - Viser system arkitektur
3. **ER-Diagram**: docs/er-diagram.md - Viser database struktur

## 📝 Mock Implementationer

Da projektet er under udvikling, indeholder test filerne mock implementationer af:
- `PalleOptimeringContext` (DbContext)
- `Palle`, `Element` models
- `PalleService`, `ElementService` (interfaces + implementations)
- `PallerController`
- `AccountController`
- `ApplicationUser` (ASP.NET Identity)

**Vigtigt**: Når de rigtige klasser er implementeret i MyProject, skal disse mocks erstattes med project references.

## ✅ Test Best Practices

Tests følger AAA pattern:
1. **Arrange** - Setup test data og dependencies
2. **Act** - Udfør den handling der testes
3. **Assert** - Verificer resultatet

Hver test har:
- Beskrivende navn der forklarer hvad der testes
- XML kommentarer med reference til testplan og diagrammer
- Clear assertion messages

## 🎯 Succeskriterier (fra testplan)

Tests hjælper med at verificere:
- ✅ Ingen kritiske fejl
- ✅ Funktionalitet matcher krav
- ✅ Database relationer fungerer (FK, CASCADE, RESTRICT)
- ✅ Sikkerhed (authentication + rolle-baseret authorization)
- ✅ Model validering og constraints

## 🔜 Manglende Tests (TODO)

Baseret på testplanen mangler:
- [ ] **Systemtest** (Section 4.3) - End-to-end workflows
- [ ] **Performancetest** (Section 4.6) - Query performance, optimeringsalgoritme
- [ ] **Navigeringstest** (Section 4.7) - HomeController navigation
- [ ] **Indholdstest** (Section 4.8) - Data display accuracy
- [ ] **UI Test** (Section 4.9) - Forms og interactive elements
- [ ] **Accepttest** (Section 4.10) - Brugerscenarier

Disse tests kan tilføjes når frontend og fuld implementation er klar.

## 📚 Yderligere Læsning

- [xUnit Documentation](https://xunit.net/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [EF Core Testing](https://docs.microsoft.com/en-us/ef/core/testing/)
- [ASP.NET Core Integration Tests](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
