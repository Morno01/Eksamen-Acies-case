using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services;

namespace MyProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ElementerController : ControllerBase
    {
        private readonly IElementService _elementService;
        private readonly ILogger<ElementerController> _logger;

        public ElementerController(IElementService elementService, ILogger<ElementerController> logger)
        {
            _elementService = elementService;
            _logger = logger;
        }


        [HttpGet]
        [Authorize(Roles = "SuperUser,NormalUser")]
        public async Task<ActionResult<IEnumerable<Element>>> GetAlleElementer()
        {
            _logger.LogInformation("🔍 GetAlleElementer kaldt af bruger: {User}", User.Identity?.Name);
            var elementer = await _elementService.GetAlleElementer();
            _logger.LogInformation("📊 Returnerer {Count} elementer", elementer.Count());
            return Ok(elementer);
        }


        [HttpGet("{id}")]
        [Authorize(Roles = "SuperUser,NormalUser")]
        public async Task<ActionResult<Element>> GetElement(int id)
        {
            var element = await _elementService.GetElement(id);
            if (element == null)
                return NotFound();

            return Ok(element);
        }


        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult<Element>> OpretElement([FromBody] Element element)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oprettetElement = await _elementService.OpretElement(element);
            return CreatedAtAction(nameof(GetElement), new { id = oprettetElement.Id }, oprettetElement);
        }


        [HttpPost("bulk")]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult<IEnumerable<Element>>> OpretFlereElementer([FromBody] IEnumerable<Element> elementer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oprettedeElementer = await _elementService.OpretFlereElementer(elementer);
            return Ok(oprettedeElementer);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult<Element>> OpdaterElement(int id, [FromBody] Element element)
        {
            if (id != element.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eksisterendeElement = await _elementService.GetElement(id);
            if (eksisterendeElement == null)
                return NotFound();

            var opdateretElement = await _elementService.OpdaterElement(element);
            return Ok(opdateretElement);
        }


        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult> SletElement(int id)
        {
            var resultat = await _elementService.SletElement(id);
            if (!resultat)
                return NotFound();

            return NoContent();
        }


        [HttpPost("force-seed")]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult> ForceSeedElementer()
        {
            _logger.LogWarning("⚠️ Force seed elementer kaldt af: {User}", User.Identity?.Name);

            var testElementer = new List<Element>
            {
                new Element
                {
                    Reference = "TEST-DØR-001",
                    Type = "Dør",
                    Serie = "Premium",
                    Hoejde = 2100,
                    Bredde = 900,
                    Dybde = 100,
                    Vaegt = 45.5m,
                    ErSpecialelement = false,
                    ErGeometrielement = false,
                    RotationsRegel = "Ja"
                },
                new Element
                {
                    Reference = "TEST-VIND-001",
                    Type = "Vindue",
                    Serie = "Premium",
                    Hoejde = 1200,
                    Bredde = 1200,
                    Dybde = 100,
                    Vaegt = 35.0m,
                    ErSpecialelement = false,
                    ErGeometrielement = false,
                    RotationsRegel = "Ja"
                },
                new Element
                {
                    Reference = "TEST-DØR-002",
                    Type = "Dør",
                    Serie = "Standard",
                    Hoejde = 2000,
                    Bredde = 800,
                    Dybde = 100,
                    Vaegt = 40.0m,
                    ErSpecialelement = false,
                    ErGeometrielement = false,
                    RotationsRegel = "Ja"
                }
            };

            var oprettedeElementer = await _elementService.OpretFlereElementer(testElementer);
            _logger.LogInformation("✓ Force seeded {Count} test elementer", testElementer.Count);

            return Ok(new
            {
                message = $"Oprettet {testElementer.Count} test elementer",
                elementer = oprettedeElementer
            });
        }
    }
}
