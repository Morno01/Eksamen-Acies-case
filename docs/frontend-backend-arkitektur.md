# Frontend og Backend Arkitektur - Palleoptimering System

## Oversigt

Frontend og backend skaber sammen et system, hvor brugere (i vores tilfælde Acies medarbejdere) kan:
- Se paller, elementer og genererede pakkeplaner
- Oprette og redigere master data (kun SuperUser)
- Generere optimerede pakkeplaner
- Få overblik over lagerstatus via dashboard
- Eksportere pakkeplaner til produktion

## 4.1 System Arkitektur

### 4.1.1 Frontend - ASP.NET Core Razor Views + JavaScript

Frontend er det grafiske brugerinterface (GUI), som vores brugere interagerer med. Det er bygget med:

#### Teknologier
- **Razor Views (.cshtml)** - Server-side rendering af HTML
- **JavaScript (Vanilla)** - Client-side dynamisk indhold
- **Bootstrap 5.3.0** - Responsivt CSS framework
- **Font Awesome 6.4.0** - Icons til UI

#### Struktur

**Views** (lokation: `MyProject/Views/`)
```
Views/
├── Account/
│   ├── Login.cshtml              # Login formular
│   └── Register.cshtml            # Registrerings formular
├── Home/
│   ├── Index.cshtml               # Dashboard / forsiden
│   ├── Paller.cshtml              # Palle management side
│   ├── Elementer.cshtml           # Element management side
│   ├── Optimering.cshtml          # Pakkeplan generering
│   ├── Serier.cshtml              # Serie oversigt
│   └── Settings.cshtml            # Indstillinger (kun SuperUser)
├── Shared/
│   ├── _Layout.cshtml             # Master layout med navigation
│   └── _ViewImports.cshtml        # Tag helpers og namespaces
```

**Reference til [Klassediagram](./klassediagram.md):**
- Frontend kommunikerer med **Controllers** (PallerController, ElementerController, osv.)

#### Frontend Ansvar

Frontend/præsentationslaget skal:

1. **Vise Data**
   - Henter data fra backend via REST API (fetch calls)
   - Renderer data i tabeller, lister og dashboards
   - Eksempel: `Elementer.cshtml` henter elementer via `/api/elementer`

2. **Håndtere Brugerinput**
   - Formularer til CRUD operationer (Create, Read, Update, Delete)
   - Validation af input før API kald
   - Eksempel: Element oprettelsesformular validerer dimensioner

3. **Dynamisk Opdatering**
   - JavaScript opdaterer UI uden page refresh
   - Loading indicators mens data hentes
   - Fejlbeskeder ved API fejl

4. **Authorization UI**
   - Skjuler/viser elementer baseret på brugerrolle
   - SuperUser ser "Opret" formularer
   - NormalUser ser kun read-only visninger

#### Eksempel: Elementer Side

**Kode reference:** `MyProject/Views/Home/Elementer.cshtml`

```javascript
// 1. Hent data fra backend
async function loadElementer() {
    const response = await fetch(`${API_BASE}/api/elementer`);
    const elementer = await response.json();

    // 2. Vis data i UI
    elementer.forEach(el => {
        // Render element i liste
    });
}

// 3. Opret nyt element
document.getElementById('elementForm').addEventListener('submit', async (e) => {
    e.preventDefault();

    const element = {
        reference: document.getElementById('reference').value,
        hoejde: parseInt(document.getElementById('hoejde').value),
        // ... flere felter
    };

    // 4. Send til backend
    const response = await fetch(`${API_BASE}/api/elementer`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(element)
    });

    if (response.ok) {
        // 5. Opdater UI
        loadElementer();
    }
});
```

### 4.1.2 Backend - ASP.NET Core + Entity Framework + Azure SQL

Backend består af følgende lag (se [Klassediagram](./klassediagram.md)):

#### 1. Controller Lag (API Endpoints)

**Lokation:** `MyProject/Controllers/`

Controllers modtager HTTP requests og returnerer responses.

**Vigtigste Controllers:**

| Controller | Ansvar | Kode Reference |
|------------|--------|----------------|
| **PallerController** | REST API for paller CRUD | `Controllers/PallerController.cs` |
| **ElementerController** | REST API for elementer CRUD | `Controllers/ElementerController.cs` |
| **PalleOptimeringController** | Genererer pakkeplaner | `Controllers/PalleOptimeringController.cs` |
| **SettingsController** | Håndterer optimeringsindstillinger | `Controllers/SettingsController.cs` |
| **AccountController** | Login, logout, registration | `Controllers/AccountController.cs` |
| **HomeController** | Navigation til views | `Controllers/HomeController.cs` |

**Eksempel: ElementerController**

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ElementerController : ControllerBase
{
    private readonly IElementService _elementService;

    // GET /api/elementer - Hent alle elementer
    [HttpGet]
    [Authorize(Roles = "SuperUser,NormalUser")]
    public async Task<ActionResult<IEnumerable<Element>>> GetAlleElementer()
    {
        var elementer = await _elementService.GetAlleElementer();
        return Ok(elementer);
    }

    // POST /api/elementer - Opret nyt element
    [HttpPost]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult<Element>> OpretElement([FromBody] Element element)
    {
        var oprettetElement = await _elementService.OpretElement(element);
        return CreatedAtAction(nameof(GetElement), new { id = oprettetElement.Id }, oprettetElement);
    }
}
```

**Reference til [Klassediagram](./klassediagram.md):**
- Controllers bruger **Services** (IPalleService, IElementService, osv.)
- Controllers er ansvarlige for HTTP håndtering, ikke forretningslogik

#### 2. Service Lag (Forretningslogik)

**Lokation:** `MyProject/Services/`

Services indeholder al forretningslogik og koordinerer mellem controllers og database.

**Vigtigste Services:**

| Service | Ansvar | Kode Reference |
|---------|--------|----------------|
| **PalleService** | CRUD for paller, validering | `Services/PalleService.cs` |
| **ElementService** | CRUD for elementer, validering | `Services/ElementService.cs` |
| **PalleOptimeringService** | Kernealgoritme for optimering | `Services/PalleOptimeringService.cs` |
| **PalleOptimeringSettingsService** | Håndterer indstillinger | `Services/PalleOptimeringSettingsService.cs` |

**Forretningslogik Eksempler:**

1. **Validering**
   - Tjek at palle dimensioner er positive
   - Verificer at element vægt ikke overskrider palle kapacitet
   - Valider rotationsregler

2. **Beregninger**
   - Find mindste palle der kan rumme element
   - Beregn samlet vægt på palle
   - Beregn samlet højde inkl. palle

3. **Optimering** (PalleOptimeringService)
   - Sorter elementer efter konfigurerede regler
   - Find optimal placering på paller
   - Respekter MaksLag, MaksVaegt, MaksHoejde
   - Håndter rotation baseret på RotationsRegel
   - Special håndtering af ErGeometrielement (ingen stabling ovenpå)

**Eksempel: PalleOptimeringService (forenklet)**

```csharp
public class PalleOptimeringService : IPalleOptimeringService
{
    private readonly PalleOptimeringContext _context;

    public async Task<PakkeplanResultat> GenererPakkeplan(PakkeplanRequest request)
    {
        // 1. Hent data fra database
        var elementer = await _context.Elementer
            .Where(e => request.ElementIds.Contains(e.Id))
            .ToListAsync();

        var settings = await _context.Settings.FindAsync(request.SettingsId);
        var aktivePaller = await _context.Paller.Where(p => p.Aktiv).ToListAsync();

        // 2. Forretningslogik - Sortering
        var sorteretElementer = SorterElementer(elementer, settings.SorteringsPrioritering);

        // 3. Forretningslogik - Optimering
        var pakkeplaner = new List<PakkeplanPalle>();

        foreach (var element in sorteretElementer)
        {
            // Find palle eller opret ny
            var palle = FindEllerOpretPalle(pakkeplaner, element, aktivePaller, settings);

            // Placer element på palle
            PlacerElementPaaPalle(palle, element, settings);
        }

        // 4. Gem i database
        var pakkeplan = new Pakkeplan
        {
            OrdreReference = request.OrdreReference,
            AntalPaller = pakkeplaner.Count,
            AntalElementer = elementer.Count,
            Paller = pakkeplaner
        };

        _context.Pakkeplaner.Add(pakkeplan);
        await _context.SaveChangesAsync();

        return new PakkeplanResultat { Pakkeplan = pakkeplan };
    }
}
```

**Reference til [Klassediagram](./klassediagram.md):**
- Services bruger **PalleOptimeringContext** til database adgang
- PalleOptimeringService afhænger af andre services

#### 3. Data Lag (Entity Framework + Database)

**Database Context:** `MyProject/Data/PalleOptimeringContext.cs`

**Entity Framework** håndterer al kommunikation med databasen.

**Reference til [ER-Diagram](./er-diagram.md):**
- Viser alle **tabeller** (Paller, Elementer, Pakkeplaner, osv.)
- Viser **foreign key relationer** mellem tabeller
- Viser **constraints** (required fields, string lengths)

**DbContext Definition:**

```csharp
public class PalleOptimeringContext : IdentityDbContext<ApplicationUser>
{
    // DbSets (tabeller)
    public DbSet<Palle> Paller { get; set; }
    public DbSet<Element> Elementer { get; set; }
    public DbSet<PalleOptimeringSettings> Settings { get; set; }
    public DbSet<Pakkeplan> Pakkeplaner { get; set; }
    public DbSet<PakkeplanPalle> PakkeplanPaller { get; set; }
    public DbSet<PakkeplanElement> PakkeplanElementer { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Konfigurer relationer (se ER-diagram)
        // Seed data
    }
}
```

**Database Operationer:**

1. **Create**
   ```csharp
   _context.Elementer.Add(element);
   await _context.SaveChangesAsync();
   ```

2. **Read**
   ```csharp
   var elementer = await _context.Elementer.ToListAsync();
   var element = await _context.Elementer.FindAsync(id);
   ```

3. **Update**
   ```csharp
   _context.Entry(element).State = EntityState.Modified;
   await _context.SaveChangesAsync();
   ```

4. **Delete**
   ```csharp
   _context.Elementer.Remove(element);
   await _context.SaveChangesAsync();
   ```

**Database:** Azure SQL Server
- **Server:** bmm-server.database.windows.net
- **Database:** bmm
- **Connection:** Via Entity Framework med connection string

**Reference til [ER-Diagram](./er-diagram.md):**

Eksempel på relationer i databasen:
- **Pakkeplan (1) → (*) PakkeplanPalle** - Én pakkeplan har mange paller
- **PakkeplanPalle (1) → (*) PakkeplanElement** - Én palle har mange elementer
- **PakkeplanElement → Element** - Reference til master data

### 4.1.3 Kommunikation mellem Frontend og Backend

Her er et simpelt flow:

#### Eksempel 1: Hent Elementer (GET)

```
1. (Frontend) Bruger åbner Elementer siden
   ↓
2. (Frontend) JavaScript kalder loadElementer()
   → fetch('https://localhost:5001/api/elementer')
   ↓
3. (Backend) ElementerController modtager HTTP GET request
   → Kalder _elementService.GetAlleElementer()
   ↓
4. (Backend) ElementService henter data
   → _context.Elementer.ToListAsync()
   ↓
5. (Database) Entity Framework udfører SQL query:
   → SELECT * FROM Elementer
   ↓
6. (Database) Azure SQL returnerer data
   ↓
7. (Backend) ElementService returnerer List<Element>
   ↓
8. (Backend) Controller serialiserer til JSON og returnerer HTTP 200
   ↓
9. (Frontend) JavaScript modtager JSON array
   → Renderer elementer i UI
```

**Kode Reference:**
- Frontend: `MyProject/Views/Home/Elementer.cshtml` (linje 147-195)
- Controller: `MyProject/Controllers/ElementerController.cs` (linje 25-33)
- Service: `MyProject/Services/ElementService.cs` (linje 16-19)

#### Eksempel 2: Opret Element (POST)

```
1. (Frontend) Bruger udfylder element formular og klikker "Opret"
   ↓
2. (Frontend) JavaScript submit event handler
   → Validerer input (dimensioner, vægt)
   → fetch('https://localhost:5001/api/elementer', { method: 'POST', body: JSON })
   ↓
3. (Backend) ElementerController modtager HTTP POST request
   → Tjekker authorization ([Authorize(Roles = "SuperUser")])
   → Validerer ModelState
   ↓
4. (Backend) Controller kalder _elementService.OpretElement(element)
   ↓
5. (Backend) ElementService forretningslogik
   → Validerer dimensioner (positive værdier)
   → _context.Elementer.Add(element)
   → await _context.SaveChangesAsync()
   ↓
6. (Database) Entity Framework genererer SQL:
   → INSERT INTO Elementer (Reference, Type, Serie, ...) VALUES (...)
   ↓
7. (Database) Azure SQL gemmer data og returnerer Id
   ↓
8. (Backend) Service returnerer Element med Id
   ↓
9. (Backend) Controller returnerer HTTP 201 Created med Location header
   ↓
10. (Frontend) JavaScript modtager success response
    → Viser success besked
    → Kalder loadElementer() for at opdatere liste
```

**Kode Reference:**
- Frontend: `MyProject/Views/Home/Elementer.cshtml` (linje 111-144)
- Controller: `MyProject/Controllers/ElementerController.cs` (linje 48-57)
- Service: `MyProject/Services/ElementService.cs` (linje 26-31)

#### Eksempel 3: Generér Pakkeplan (POST)

```
1. (Frontend) Bruger vælger elementer og klikker "Generér Pakkeplan"
   ↓
2. (Frontend) JavaScript sender request
   → fetch('/api/palleoptimering/generer', {
       method: 'POST',
       body: JSON.stringify({
         elementIds: [1, 2, 3, 4, 5],
         settingsId: 1,
         ordreReference: "ORD-2025-001"
       })
     })
   ↓
3. (Backend) PalleOptimeringController modtager request
   → Tjekker authorization
   → Validerer at elementIds ikke er tom
   ↓
4. (Backend) Controller kalder _optimeringService.GenererPakkeplan(request)
   ↓
5. (Backend) PalleOptimeringService - Kompleks forretningslogik

   5a. Hent data fra database
       → Elementer, Settings, Aktive Paller

   5b. Sorter elementer
       → Efter settings.SorteringsPrioritering
       → Type, Specialelement, Pallestorrelse, osv.

   5c. Find mindste palle for hvert element
       → Sammenlign dimensioner
       → Tjek rotation muligheder

   5d. Placer elementer på paller
       → Respekter MaksLag (default: 2)
       → Respekter MaksVaegt og MaksHoejde
       → Håndter ErGeometrielement (ingen stabling ovenpå)
       → Beregn optimal rotation

   5e. Opret Pakkeplan objekt
       → Pakkeplan med PakkeplanPaller
       → PakkeplanPaller med PakkeplanElementer
       → Gem i database via Entity Framework
   ↓
6. (Database) Entity Framework gemmer komplet pakkeplan
   → INSERT Pakkeplan
   → INSERT PakkeplanPaller (flere rækker)
   → INSERT PakkeplanElementer (mange rækker)
   ↓
7. (Backend) Service returnerer PakkeplanResultat
   ↓
8. (Backend) Controller serialiserer til JSON
   → Inkluderer antal paller, elementer, detaljeret placering
   ↓
9. (Frontend) JavaScript modtager resultat
   → Renderer pakkeplanen grafisk
   → Viser antal paller
   → Viser elementer pr. palle med lag og placering
```

**Kode Reference:**
- Frontend: `MyProject/Views/Home/Optimering.cshtml`
- Controller: `MyProject/Controllers/PalleOptimeringController.cs`
- Service: `MyProject/Services/PalleOptimeringService.cs`

#### HTTP Metoder

Systemet bruger standard REST conventions:

| HTTP Metode | Formål | Eksempel Endpoint | Authorization |
|-------------|--------|-------------------|---------------|
| **GET** | Hent data | `/api/elementer` | SuperUser + NormalUser |
| **POST** | Opret ny | `/api/elementer` | Kun SuperUser |
| **PUT** | Opdater eksisterende | `/api/elementer/1` | Kun SuperUser |
| **DELETE** | Slet | `/api/elementer/1` | Kun SuperUser |

#### Response Formats

**Success Response (200 OK):**
```json
{
  "id": 1,
  "reference": "DØR-001",
  "type": "Dør",
  "serie": "Premium",
  "hoejde": 2100,
  "bredde": 900,
  "vaegt": 45.5
}
```

**Error Response (400 Bad Request):**
```json
{
  "errors": {
    "Hoejde": ["Højde skal være større end 0"],
    "Vaegt": ["Vægt er påkrævet"]
  }
}
```

**Authorization Error (403 Forbidden):**
```json
{
  "message": "Du har ikke rettigheder til denne operation"
}
```

## 4.2 Security og Authorization

### ASP.NET Core Identity

**Kode Reference:** `MyProject/Program.cs` (linje 18-34)

Systemet bruger ASP.NET Core Identity til authentication og authorization:

```csharp
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<PalleOptimeringContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
});
```

### Roller

**Reference til [ER-Diagram](./er-diagram.md):** AspNetRoles tabel

| Rolle | Beskrivelse | Rettigheder |
|-------|-------------|-------------|
| **SuperUser** | Administrator | CRUD på alle entiteter, Settings adgang |
| **NormalUser** | Standard bruger | Kun læseadgang (GET endpoints) |

### Authorization Attributter

**Kode Eksempel:**

```csharp
// Kræv login for alt
[Authorize]
public class ElementerController : ControllerBase
{
    // Både SuperUser og NormalUser kan læse
    [HttpGet]
    [Authorize(Roles = "SuperUser,NormalUser")]
    public async Task<ActionResult> GetAlleElementer() { }

    // Kun SuperUser kan oprette
    [HttpPost]
    [Authorize(Roles = "SuperUser")]
    public async Task<ActionResult> OpretElement() { }
}
```

### Seed Data

**Kode Reference:** `MyProject/Data/DbInitializer.cs`

Systemet opretter automatisk test brugere:

| Email | Password | Rolle | Formål |
|-------|----------|-------|--------|
| admin@acies.dk | admin | SuperUser | Fuld adgang |
| bruger@acies.dk | bruger | NormalUser | Read-only |

## 4.3 Dataflow Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                         FRONTEND                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ Login.cshtml │  │Elementer.html│  │Optimering.html│     │
│  │              │  │              │  │              │     │
│  │ Login form   │  │ CRUD forms   │  │ Pakkeplan UI │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
│         │                  │                  │             │
│         │ JavaScript       │ JavaScript       │ JavaScript  │
│         │ fetch()          │ fetch()          │ fetch()     │
└─────────┼──────────────────┼──────────────────┼─────────────┘
          │                  │                  │
          │ HTTP/HTTPS       │ HTTP/HTTPS       │ HTTP/HTTPS
          │ (JSON)           │ (JSON)           │ (JSON)
          ↓                  ↓                  ↓
┌─────────────────────────────────────────────────────────────┐
│                     CONTROLLER LAG                          │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │  Account     │  │  Elementer   │  │Palleoptimering│    │
│  │  Controller  │  │  Controller  │  │  Controller  │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
│         │                  │                  │             │
│         │ DI               │ DI               │ DI          │
└─────────┼──────────────────┼──────────────────┼─────────────┘
          ↓                  ↓                  ↓
┌─────────────────────────────────────────────────────────────┐
│                      SERVICE LAG                            │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │ UserManager  │  │   Element    │  │Palleoptimering│    │
│  │ SignInManager│  │   Service    │  │   Service    │     │
│  └──────┬───────┘  └──────┬───────┘  └──────┬───────┘     │
│         │                  │                  │             │
│         │ EF Core          │ EF Core          │ EF Core     │
└─────────┼──────────────────┼──────────────────┼─────────────┘
          ↓                  ↓                  ↓
┌─────────────────────────────────────────────────────────────┐
│                     DATA LAG                                │
│              PalleOptimeringContext (DbContext)             │
│                                                             │
│  DbSet<ApplicationUser>  DbSet<Element>  DbSet<Pakkeplan>  │
└─────────────┬───────────────────────────────────────────────┘
              │
              │ ADO.NET / SQL Connection
              ↓
┌─────────────────────────────────────────────────────────────┐
│                  AZURE SQL DATABASE                         │
│                                                             │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │AspNetUsers│ │ Elementer │ │  Paller  │ │Pakkeplaner│  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│                                                             │
│  bmm-server.database.windows.net                           │
└─────────────────────────────────────────────────────────────┘
```

## 4.4 Teknologi Stack Oversigt

| Lag | Teknologi | Formål | Kode Reference |
|-----|-----------|--------|----------------|
| **Frontend** | Razor Views (.cshtml) | Server-side HTML rendering | `Views/` |
| **Frontend** | JavaScript | Client-side dynamik | `Views/Home/*.cshtml` (script sections) |
| **Frontend** | Bootstrap 5.3 | Responsive UI framework | `Views/Shared/_Layout.cshtml` |
| **Controller** | ASP.NET Core MVC | HTTP request/response håndtering | `Controllers/` |
| **Service** | C# Classes | Forretningslogik | `Services/` |
| **Data Access** | Entity Framework Core 6.0 | ORM (Object-Relational Mapping) | `Data/PalleOptimeringContext.cs` |
| **Database** | Azure SQL Server | Persistent data storage | bmm-server.database.windows.net |
| **Authentication** | ASP.NET Core Identity | Brugere og roller | `Models/ApplicationUser.cs` |
| **API Format** | JSON | Data udveksling | Automatisk serialisering |
| **Authorization** | Role-based | Access control | `[Authorize(Roles = "...")]` |

## 4.5 Reference til Diagrammer

For at forstå arkitekturen fuldstændigt, se:

1. **[Klassediagram](./klassediagram.md)**
   - Viser alle klasser: Controllers, Services, Models
   - Viser relationer mellem komponenter
   - Illustrerer dependency injection flow

2. **[ER-Diagram](./er-diagram.md)**
   - Viser database tabeller: Paller, Elementer, Pakkeplaner, osv.
   - Viser foreign key relationer
   - Dokumenterer constraints og data types

3. **[Arkitektur Oversigt](./README.md)**
   - Komplet system oversigt
   - Workflow beskrivelser
   - Deployment information

---

**Konklusion:**

Systemet følger standard ASP.NET Core MVC arkitektur med klar adskillelse af ansvar:
- **Frontend**: UI og brugerinteraktion (Razor + JavaScript)
- **Controllers**: HTTP routing og request håndtering
- **Services**: Forretningslogik og beregninger
- **Data Access**: Entity Framework til database kommunikation
- **Database**: Azure SQL til persistent storage

Denne arkitektur gør systemet:
- **Testbart** - Hver komponent kan testes isoleret
- **Vedligeholdbart** - Klar struktur, nem at finde kode
- **Skalerbart** - Services kan optimeres uafhængigt
- **Sikkert** - Role-based authorization på alle lag
