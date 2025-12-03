using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services;

namespace MyProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PallerController : ControllerBase
    {
        private readonly IPalleService _palleService;

        public PallerController(IPalleService palleService)
        {
            _palleService = palleService;
        }

        /// <summary>
        /// Hent alle paller
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Palle>>> GetAllePaller()
        {
            var paller = await _palleService.GetAllePaller();
            return Ok(paller);
        }

        /// <summary>
        /// Hent alle aktive paller
        /// </summary>
        [HttpGet("aktive")]
        public async Task<ActionResult<IEnumerable<Palle>>> GetAlleAktivePaller()
        {
            var paller = await _palleService.GetAlleAktivePaller();
            return Ok(paller);
        }

        /// <summary>
        /// Hent en specifik palle
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Palle>> GetPalle(int id)
        {
            var palle = await _palleService.GetPalle(id);
            if (palle == null)
                return NotFound();

            return Ok(palle);
        }

        /// <summary>
        /// Opret en ny palle
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Palle>> OpretPalle([FromBody] Palle palle)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oprettetPalle = await _palleService.OpretPalle(palle);
            return CreatedAtAction(nameof(GetPalle), new { id = oprettetPalle.Id }, oprettetPalle);
        }

        /// <summary>
        /// Opdater en eksisterende palle
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<Palle>> OpdaterPalle(int id, [FromBody] Palle palle)
        {
            if (id != palle.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eksisterendePalle = await _palleService.GetPalle(id);
            if (eksisterendePalle == null)
                return NotFound();

            var opdateretPalle = await _palleService.OpdaterPalle(palle);
            return Ok(opdateretPalle);
        }

        /// <summary>
        /// Slet en palle
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> SletPalle(int id)
        {
            var resultat = await _palleService.SletPalle(id);
            if (!resultat)
                return NotFound();

            return NoContent();
        }
    }
}
