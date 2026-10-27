using GoCare.Services.Domain;

using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace GoCare.Controllers.Domain;

// Autocompletamento dei comuni ISTAT (nome o CAP): lo usa il frontend per gli indirizzi
// e per scegliere i comuni coperti dall'associazione (PA-03). Serve un utente autenticato.

[Route("comuni")]
[Tags("Comuni")]
[Authorize]
public sealed class SearchComuneController(SearchComuneService service) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct) =>
        Ok(await service.SearchAsync(q, ct));
}
