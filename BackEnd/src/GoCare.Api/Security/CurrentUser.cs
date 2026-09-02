using System.Security.Claims;

using GoCare.Shared.Abstractions;

namespace GoCare.Api.Security;

public sealed class CurrentUser(IHttpContextAccessor accessor) : ICurrentUser // per creare un CurrentUser serve un IHttpContextAccessor.
                                                                              // ICurrentUser: nostra interfaccia ->  Il compilatore ora pretende che esistano AccountId, IsAuthenticated, IsInRole con le firme giuste
{
    private ClaimsPrincipal? User => accessor.HttpContext?.User; // "se HttpContext è null, l'intera espressione è null; altrimenti prendi .User"

    public Guid? AccountId =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) // cerca il primo Claim di tipo NameIdentifier, ritorna una stringa
        ? id                                                                       // che se non valida come Guid = null / oppure Guid valido = id;
        : null;                                                                           

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false; // "autenticato se e solo se c'è un'identità e dice di essere autenticata; in ogni altro caso, no"

    public bool IsInRole(string role) => User?.IsInRole(role) ?? false;
}
