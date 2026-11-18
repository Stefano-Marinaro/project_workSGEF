using GoCare.Errors;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
[Tags("Auth")]
public sealed class RegisterAssociationController : ControllerBase
{
    [HttpPost("register/association")]
    public IActionResult RegisterAssociation() =>
        throw new ForbiddenException("La registrazione pubblica delle associazioni non è disponibile. Contatta l’amministratore.");
}
