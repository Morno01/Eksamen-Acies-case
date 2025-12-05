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

        public ElementerController(IElementService elementService)
        {
            _elementService = elementService;
        }

        /// <summary>
        /// Hent alle elementer
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Element>>> GetAlleElementer()
        {
            var elementer = await _elementService.GetAlleElementer();
            return Ok(elementer);
        }

        /// <summary>
        /// Hent et specifikt element
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Element>> GetElement(int id)
        {
            var element = await _elementService.GetElement(id);
            if (element == null)
                return NotFound();

            return Ok(element);
        }

        /// <summary>
        /// Opret et nyt element
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult<Element>> OpretElement([FromBody] Element element)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oprettetElement = await _elementService.OpretElement(element);
            return CreatedAtAction(nameof(GetElement), new { id = oprettetElement.Id }, oprettetElement);
        }

        /// <summary>
        /// Opret flere elementer på én gang
        /// </summary>
        [HttpPost("bulk")]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult<IEnumerable<Element>>> OpretFlereElementer([FromBody] IEnumerable<Element> elementer)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oprettedeElementer = await _elementService.OpretFlereElementer(elementer);
            return Ok(oprettedeElementer);
        }

        /// <summary>
        /// Opdater et eksisterende element
        /// </summary>
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

        /// <summary>
        /// Slet et element
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult> SletElement(int id)
        {
            var resultat = await _elementService.SletElement(id);
            if (!resultat)
                return NotFound();

            return NoContent();
        }
    }
}
