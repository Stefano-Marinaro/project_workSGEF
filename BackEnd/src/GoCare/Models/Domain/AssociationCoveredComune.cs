namespace GoCare.Models.Domain;

// Un comune coperto da un'associazione (PA-03): una riga per ogni coppia associazione-comune.
// Il comune è identificato dal codice ISTAT, in Data/Seed/comuni.csv stabile in ogni database (comuni.id invece cambia a ogni seed).
public sealed class AssociationCoveredComune
{
    private AssociationCoveredComune() { } // costruttore vuoto: EF (materializzazione dal DB)

    public AssociationCoveredComune(Guid associationId, string comuneCodiceIstat)
    {
        AssociationId = associationId;
        ComuneCodiceIstat = comuneCodiceIstat;
    }

    public Guid AssociationId { get; }
    public string ComuneCodiceIstat { get; } = null!;
}