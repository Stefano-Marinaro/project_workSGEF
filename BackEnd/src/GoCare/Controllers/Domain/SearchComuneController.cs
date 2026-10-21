using GoCare.Services.Domain;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Domain;

// Controller temporaneo di verifica: vedi SearchComuneService. Nessun [Authorize] di proposito,
// solo per poterlo chiamare facilmente dal mockup - va protetto prima di diventare definitivo.
[ApiController]
[Route("comuni")]
[Tags("Comuni (mockup)")]
public sealed class SearchComuneController(SearchComuneService service) : ControllerBase
{
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q, CancellationToken ct) =>
        Ok(await service.SearchAsync(q, ct));
}
