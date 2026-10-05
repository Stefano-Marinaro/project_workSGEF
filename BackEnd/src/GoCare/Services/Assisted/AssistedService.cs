using GoCare.Data;

using Microsoft.AspNetCore.Mvc;

namespace GoCare.Services.Assisted;

public class AssistedService(GoCareDbContext db)
{
    public async Task<IActionResult> CreateAssisted(Guid id, CancellationToken ct)
    {
        var caregiver = 
    }
}
