using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Models;
using MyProject.Services;

namespace MyProject.Controllers
{
    [Authorize(Roles = "SuperUser")]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly IPalleOptimeringSettingsService _settingsService;

        public SettingsController(IPalleOptimeringSettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        /// <summary>
        /// Hent alle settings
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PalleOptimeringSettings>>> GetAlleSettings()
        {
            var settings = await _settingsService.GetAlleSettings();
            return Ok(settings);
        }

        /// <summary>
        /// Hent aktive settings
        /// </summary>
        [HttpGet("aktiv")]
        public async Task<ActionResult<PalleOptimeringSettings>> GetAktivSettings()
        {
            var settings = await _settingsService.GetAktivSettings();
            if (settings == null)
                return NotFound("Ingen aktive settings fundet");

            return Ok(settings);
        }

        /// <summary>
        /// Hent specifik settings
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PalleOptimeringSettings>> GetSettings(int id)
        {
            var settings = await _settingsService.GetSettings(id);
            if (settings == null)
                return NotFound();

            return Ok(settings);
        }

        /// <summary>
        /// Opret nye settings
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PalleOptimeringSettings>> OpretSettings([FromBody] PalleOptimeringSettings settings)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var oprettetSettings = await _settingsService.OpretSettings(settings);
            return CreatedAtAction(nameof(GetSettings), new { id = oprettetSettings.Id }, oprettetSettings);
        }

        /// <summary>
        /// Opdater eksisterende settings
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<PalleOptimeringSettings>> OpdaterSettings(int id, [FromBody] PalleOptimeringSettings settings)
        {
            if (id != settings.Id)
                return BadRequest("ID mismatch");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var eksisterendeSettings = await _settingsService.GetSettings(id);
            if (eksisterendeSettings == null)
                return NotFound();

            var opdateretSettings = await _settingsService.OpdaterSettings(settings);
            return Ok(opdateretSettings);
        }

        /// <summary>
        /// Slet settings
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> SletSettings(int id)
        {
            var resultat = await _settingsService.SletSettings(id);
            if (!resultat)
                return NotFound();

            return NoContent();
        }
    }
}
