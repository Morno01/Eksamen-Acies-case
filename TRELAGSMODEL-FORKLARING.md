# Trelagsmodellen i MyProject

## Oversigt
Trelagsmodellen (Three-Tier Architecture) er et arkitekturmønster der opdeler applikationen i tre separate lag, hver med sit eget ansvarsområde. Dette projekt følger trelagsmodellen gennem ASP.NET Core MVC-mønsteret.

## De tre lag i jeres projekt

### 1. **Præsentationslaget (Presentation Layer)**
**Placering:** `/Views` mappen

**Ansvar:**
- Brugergrænsefladen (UI)
- Visning af data til brugeren
- Modtagelse af brugerinput
- HTML, CSS, JavaScript

**I jeres projekt:**
- Razor Views (.cshtml filer) der viser data
- Håndterer kun visning - ingen forretningslogik
- Kommunikerer med Controller-laget

**Eksempel:**
```html
@model Product
<h1>@Model.Name</h1>
<p>Pris: @Model.Price kr.</p>
```

---

### 2. **Forretningslogik/Applikationslaget (Business Logic Layer)**
**Placering:** `/Controllers` mappen

**Ansvar:**
- Håndtering af brugeranmodninger
- Forretningslogik og regler
- Validering af data
- Koordinering mellem Presentation og Data lag
- Beslutningslogik

**I jeres projekt:**
- Controller-klasser der håndterer HTTP requests
- Processerer brugerinput fra Views
- Kalder Model-lag for data
- Returnerer passende Views med data

**Eksempel:**
```csharp
public class ProductController : Controller
{
    // GET: /Product/Details/5
    public IActionResult Details(int id)
    {
        // Forretningslogik: Hent produkt fra data-lag
        var product = _productService.GetById(id);

        // Validering
        if (product == null)
            return NotFound();

        // Send til præsentationslag (View)
        return View(product);
    }
}
```

---

### 3. **Datalaget (Data Access Layer)**
**Placering:** `/Model` mappen

**Ansvar:**
- Datamodeller (klasser der repræsenterer data)
- Database-adgang (via Repository pattern eller DbContext)
- Data persistering og hentning
- CRUD operationer (Create, Read, Update, Delete)

**I jeres projekt:**
- Model-klasser (domain entities)
- Database context (hvis I bruger Entity Framework)
- Repository-klasser (valgfrit)
- Data Transfer Objects (DTOs)

**Eksempel:**
```csharp
// Model/Product.cs
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

// Model/ApplicationDbContext.cs (hvis I bruger EF Core)
public class ApplicationDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
}
```

---

## Dataflow i trelagsmodellen

```
Bruger
  ↓
[PRÆSENTATIONSLAG - Views]
  ↓ (HTTP Request)
[FORRETNINGSLOGIK - Controllers]
  ↓ (Hent/Gem data)
[DATALAG - Models + Database]
  ↑ (Returnér data)
[FORRETNINGSLOGIK - Controllers]
  ↑ (Data til visning)
[PRÆSENTATIONSLAG - Views]
  ↑
Bruger
```

## Fordele ved trelagsmodellen

1. **Separation of Concerns:** Hvert lag har sit eget ansvar
2. **Vedligeholdelse:** Lettere at finde og rette fejl
3. **Testbarhed:** Hvert lag kan testes isoleret
4. **Genanvendelighed:** Forretningslogik kan genbruges
5. **Skalerbarhed:** Lagene kan skaleres uafhængigt
6. **Teamwork:** Forskellige teams kan arbejde på forskellige lag

## Bedste praksis for jeres projekt

### Models (Datalag)
- Hold models simple - kun data properties
- Brug Data Annotations til validering
- Overvej Repository pattern til database-adgang

### Controllers (Forretningslogik)
- En controller per ressource/entity
- Hold actions fokuserede og små
- Brug Dependency Injection
- Valider altid brugerinput

### Views (Præsentationslag)
- Ingen forretningslogik i views
- Brug ViewModels hvis nødvendigt
- Hold views DRY (Don't Repeat Yourself)
- Brug partials til genbrug

## Eksempel på komplet flow

### Scenario: Vis produktdetaljer

**1. Bruger:** Klikker på "Se produkt #5"

**2. Præsentationslag (View):**
```html
<a href="/Product/Details/5">Se produkt</a>
```

**3. Forretningslogik (Controller):**
```csharp
public class ProductController : Controller
{
    private readonly IProductRepository _repository;

    public IActionResult Details(int id)
    {
        var product = _repository.GetById(id);
        if (product == null)
            return NotFound();
        return View(product);
    }
}
```

**4. Datalag (Model + Repository):**
```csharp
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public Product GetById(int id)
    {
        return _context.Products.Find(id);
    }
}
```

**5. Tilbage til Præsentationslag (View):**
```html
@model Product
<h1>@Model.Name</h1>
<p>@Model.Description</p>
<p>Pris: @Model.Price kr.</p>
```

## Sammenhæng med MVC

Trelagsmodellen og MVC overlapper:
- **Model** (MVC) = Datalag (3-tier)
- **Controller** (MVC) = Forretningslogik (3-tier)
- **View** (MVC) = Præsentationslag (3-tier)

Men trelagsmodellen er mere en arkitektonisk opdeling, mens MVC er et designmønster.

## Konklusion

Jeres projekt følger allerede trelagsmodellen gennem mappestrukturen:
- `/Views` → Præsentationslag
- `/Controllers` → Forretningslogik
- `/Model` → Datalag

Ved at holde denne opdeling klar og følge ansvarsprincipperne, får I en velstruktureret, vedligeholdelsesvenlig applikation.
