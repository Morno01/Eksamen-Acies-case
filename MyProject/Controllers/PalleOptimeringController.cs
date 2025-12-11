using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyProject.Services;
using MyProject.Services.DTOs;

namespace MyProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PalleOptimeringController : ControllerBase
    {
        private readonly IPalleOptimeringService _optimeringService;

        public PalleOptimeringController(IPalleOptimeringService optimeringService)
        {
            _optimeringService = optimeringService;
        }

        [HttpPost("generer")]
        [Authorize(Roles = "SuperUser")]
        public async Task<ActionResult<PakkeplanResultat>> GenererPakkeplan([FromBody] PakkeplanRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (request.ElementIds == null || !request.ElementIds.Any())
                return BadRequest("ElementIds må ikke være tom");

            var resultat = await _optimeringService.GenererPakkeplan(request);

            if (resultat.Status == "Error")
                return BadRequest(resultat);

            return Ok(resultat);
        }

        [HttpGet("pakkeplaner")]
        [Authorize(Roles = "SuperUser,NormalUser")]
        public async Task<ActionResult> GetAllePakkeplaner()
        {
            var pakkeplaner = await _optimeringService.GetAllePakkeplaner();
            return Ok(pakkeplaner);
        }

        [HttpGet("pakkeplaner/{id}")]
        [Authorize(Roles = "SuperUser,NormalUser")]
        public async Task<ActionResult> GetPakkeplan(int id)
        {
            var pakkeplan = await _optimeringService.GetPakkeplan(id);
            if (pakkeplan == null)
                return NotFound();

            return Ok(pakkeplan);
        }
    }
}
