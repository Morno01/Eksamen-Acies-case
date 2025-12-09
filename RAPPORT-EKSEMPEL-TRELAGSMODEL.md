# Eksempel på rapport-afsnit om trelagsmodellen

## 3. Systemarkitektur

### 3.1 Trelagsmodellen

Systemets arkitektur er baseret på trelagsmodellen (three-tier architecture), som er et anerkendt arkitekturmønster inden for softwareudvikling. Trelagsmodellen opdeler applikationen i tre distinkte lag, hver med sit eget veldefinerede ansvarsområde (Fowler, 2002). Denne separation sikrer en klar struktur, hvor ændringer i ét lag ikke nødvendigvis påvirker de andre lag, hvilket øger systemets vedligeholdelsesevne og skalerbarhed.

#### 3.1.1 Præsentationslaget

Præsentationslaget udgør den øverste del af arkitekturen og har til formål at håndtere brugergrænsefladen og interaktionen med slutbrugeren. I dette projekt er præsentationslaget implementeret ved hjælp af ASP.NET Core Razor Views, som er placeret i `/Views` mappen.

Præsentationslagets primære ansvar er at:
- Præsentere data for brugeren på en intuitiv og brugervenlig måde
- Modtage og videresende brugerinput til de underliggende lag
- Sikre en responsiv og tilgængelig brugergrænseflade

I henhold til Model-View-Controller (MVC) mønsteret, som ASP.NET Core bygger på, indeholder præsentationslaget udelukkende præsentationslogik og er fri for forretningslogik (Microsoft, 2023). Dette sikrer en klar adskillelse mellem, hvordan data vises, og hvordan data behandles. Et eksempel på dette ses i projektets produktvisning, hvor Razor Views modtager produktdata fra controlleren og præsenterer disse uden selv at foretage datamanipulation eller databasekald.

```csharp
@model Product
<div class="product-details">
    <h1>@Model.Name</h1>
    <p class="description">@Model.Description</p>
    <p class="price">Pris: @Model.Price kr.</p>
</div>
```

I ovenstående eksempel ses det tydeligt, hvordan viewet udelukkende beskæftiger sig med præsentation af data, uden at indeholde logik til datahentning eller -behandling.

#### 3.1.2 Forretningslogiklaget

Forretningslogiklaget fungerer som systemets centrale "hjerne" og implementeres i dette projekt gennem Controller-klasser i `/Controllers` mappen. Dette lag har ansvaret for at koordinere dataflowet mellem præsentationslaget og datalaget, samt at implementere systemets forretningsregler og valideringslogik.

Forretningslogiklagets hovedansvar omfatter:
- Modtagelse og behandling af HTTP-anmodninger fra præsentationslaget
- Implementering af forretningsregler og valideringslogik
- Koordinering af datahentning fra datalaget
- Beslutningslogik vedrørende, hvilke views der skal returneres

I projektet ses dette eksempelvis i `ProductController`, hvor forretningslogikken håndterer brugerens anmodning om at se produktdetaljer:

```csharp
public class ProductController : Controller
{
    private readonly IProductRepository _repository;

    public ProductController(IProductRepository repository)
    {
        _repository = repository;
    }

    public IActionResult Details(int id)
    {
        // Forretningslogik: Hent produkt via repository
        var product = _repository.GetById(id);

        // Validering: Kontroller om produktet findes
        if (product == null)
        {
            return NotFound();
        }

        // Returner view med data
        return View(product);
    }
}
```

Controlleren implementerer her flere vigtige principper: den anvender Dependency Injection til at modtage repository-instansen, hvilket letter testbarhed (Seemann, 2011), og den sikrer validering af data før præsentation. Dette lag fungerer således som en mellemmand mellem brugergrænsefladen og datakilden.

#### 3.1.3 Datalaget

Datalaget repræsenterer den nederste del af trelagsmodellen og har til opgave at håndtere al datapersistering og -hentning. I projektet er datalaget implementeret i `/Model` mappen og omfatter både domænemodeller og dataadgangslogik.

Datalagts primære ansvar inkluderer:
- Definition af domænemodeller, der repræsenterer systemets entiteter
- Håndtering af CRUD-operationer (Create, Read, Update, Delete)
- Abstraktion af databaseadgang gennem Repository-mønsteret
- Sikring af dataintegritet

Et eksempel på en domænemodel i projektet:

```csharp
public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Produktnavn er påkrævet")]
    [StringLength(100)]
    public string Name { get; set; }

    [Range(0.01, 999999.99)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string Description { get; set; }
}
```

Modellen anvender Data Annotations til validering, hvilket sikrer, at data overholder definerede regler allerede på modelniveau. Dataadgangen håndteres gennem Repository-mønsteret, som abstraherer den konkrete databaseimplementering:

```csharp
public interface IProductRepository
{
    Product GetById(int id);
    IEnumerable<Product> GetAll();
    void Add(Product product);
    void Update(Product product);
    void Delete(int id);
}

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Product GetById(int id)
    {
        return _context.Products.Find(id);
    }

    // Øvrige metoder...
}
```

Repository-mønsteret skaber en abstraktionslagn mellem forretningslogikken og den konkrete dataadgang, hvilket letter unit testing og gør det muligt at udskifte datakilden uden at påvirke de øvrige lag (Fowler, 2002).

### 3.2 Dataflow i systemet

Når en bruger interagerer med systemet, følger dataflowet en veldefineret vej gennem de tre lag:

1. **Brugeranmodning**: Brugeren interagerer med præsentationslaget (f.eks. klikker på "Se produkt")
2. **Routing**: ASP.NET Core's routing-mekanisme dirigerer anmodningen til den relevante controller-action
3. **Forretningslogik**: Controlleren validerer input og kalder repository-metoden
4. **Dataadgang**: Repository henter data fra databasen via Entity Framework Core
5. **Returdata**: Data sendes tilbage gennem lagene: Repository → Controller → View
6. **Præsentation**: Viewet renderer data som HTML og vises for brugeren

Dette unidirektionelle flow sikrer, at hvert lag kun kommunikerer med det nærmeste lag, hvilket reducerer afhængigheder og øger systemets modularitet (Sommerville, 2015).

### 3.3 Fordele ved trelagsmodellen i projektet

Implementeringen af trelagsmodellen i dette projekt giver flere konkrete fordele:

**Separation of Concerns**: Hvert lag har et veldefineret ansvar, hvilket gør kodebasen mere overskuelig. En udvikler, der skal ændre brugergrænsefladen, behøver ikke at have detaljeret kendskab til databasestrukturen.

**Vedligeholdelsesevne**: Ændringer i ét lag påvirker sjældent de andre lag. Hvis databasen f.eks. migreres fra SQL Server til PostgreSQL, skal kun datalaget opdateres, mens forretningslogik og præsentation forbliver uændrede.

**Testbarhed**: Hvert lag kan testes isoleret. Controllers kan unit testes ved at mocke repository-interfaces, hvilket giver hurtigere og mere pålidelige tests.

**Genanvendelighed**: Forretningslogikken kan potentielt genbruges på tværs af forskellige præsentationsformer (web, API, desktop).

### 3.4 Udfordringer og begrænsninger

Selvom trelagsmodellen giver mange fordele, medfører den også visse udfordringer:

**Kompleksitet**: For små applikationer kan trelagsmodellen være overkill og tilføje unødvendig kompleksitet. I dette projekt er fordelen dog klar, da systemet forventes at vokse.

**Performance overhead**: De ekstra abstraktionslag kan medføre mindre performance-overhead sammenlignet med en mere direkte tilgang. Dette er dog sjældent et problem i praksis og opvejes typisk af arkitekturens fordele.

**Læringskurve**: Nye udviklere skal forstå hele arkitekturen for at kunne bidrage effektivt til projektet.

### 3.5 Konklusion

Trelagsmodellen udgør fundamentet for dette projekts arkitektur og sikrer en struktureret, vedligeholdelsesvenlig og skalerbar løsning. Gennem den klare opdeling i præsentationslag, forretningslogiklag og datalag opnås en høj grad af modularitet, som letter både udvikling, test og fremtidig udvidelse af systemet. ASP.NET Core's MVC-framework understøtter naturligt denne arkitektur, hvilket gør implementeringen ligetil og idiomatisk.

---

## Referencer

Fowler, M. (2002). *Patterns of Enterprise Application Architecture*. Addison-Wesley.

Microsoft. (2023). *ASP.NET Core MVC Overview*. Microsoft Documentation. https://learn.microsoft.com/en-us/aspnet/core/mvc/overview

Seemann, M. (2011). *Dependency Injection in .NET*. Manning Publications.

Sommerville, I. (2015). *Software Engineering* (10th ed.). Pearson Education.

---

## Tips til din egen rapport

**Struktur:**
- Start med teoretisk baggrund (hvad er trelagsmodellen?)
- Forklar hvert lag individuelt med eksempler fra dit projekt
- Beskriv dataflow
- Diskuter fordele og ulemheder
- Afslut med konklusion

**Akademisk stil:**
- Brug faglige termer konsekvent
- Referer til kilder (bøger, dokumentation, artikler)
- Brug aktiv form: "Systemet implementerer..." frem for "Vi har implementeret..."
- Inkluder kodeeksempler, men forklar dem tekstuelt
- Brug figurer/diagrammer hvis muligt

**Undgå:**
- Første person ("jeg", "vi")
- Uformelt sprog
- Påstande uden dokumentation
- For lange kodeeksempler uden forklaring

**Hvad du kan tilføje:**
- UML-diagrammer over arkitekturen
- Sekvensdiagrammer der viser dataflow
- Sammenligninger med alternative arkitekturer
- Konkrete metrics (f.eks. testdækning per lag)
