using GoCare.Models.Domain;

using Microsoft.EntityFrameworkCore;

namespace GoCare.Data;

// Importa i comuni ISTAT (nome, codice, provincia, CAP) da un CSV committato nel repo,
// cosi' che un collega che clona il progetto li abbia gia' senza rifare a mano tutto
// il lavoro di import fatto una volta sola (vedi RECAP_BACKEND.md per i dettagli).
public static class ComuniSeeder
{
    public static async Task SeedAsync(GoCareDbContext db, string contentRootPath, CancellationToken ct)
    {
        if (await db.Comuni.AnyAsync(ct))
            return; // gia' popolata, non reimportare

        var csvPath = Path.Combine(contentRootPath, "Data", "Seed", "comuni.csv");
        if (!File.Exists(csvPath))
            return; // file assente (es. ambiente senza il seed): non blocchiamo l'avvio

        var comuni = new List<Comune>();

        foreach (var line in await File.ReadAllLinesAsync(csvPath, ct))
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            // Trim('"'): psql mette tra virgolette i campi CSV vuoti (per distinguerli da NULL) -
            // senza toglierle, un comune senza CAP finirebbe con un CapCodes = [""] invece di [].
            var parts = line.Split(',').Select(p => p.Trim('"')).ToArray();
            if (parts.Length != 4)
                continue;

            var capCodes = parts[3].Length == 0
                ? []
                : parts[3].Split(';').ToList();

            var comune = new Comune(Guid.NewGuid(), parts[0], parts[1], parts[2]);
            comune.SetCapCodes(capCodes);
            comuni.Add(comune);
        }

        db.Comuni.AddRange(comuni);
        await db.SaveChangesAsync(ct);
    }
}
