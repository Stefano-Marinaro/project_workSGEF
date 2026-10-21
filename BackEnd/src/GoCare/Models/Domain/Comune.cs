namespace GoCare.Models.Domain;

public sealed class Comune
{
    private Comune() { } // costruttore vuoto: EF (materializzazione dal DB)

    public Comune(Guid id, string codiceIstat, string nomeComune, string provincia)
    {
        Id = id;
        CodiceIstat = codiceIstat;
        NomeComune = nomeComune;
        Provincia = provincia;
        CapCodes = [];
    }

    public Guid Id { get; }
    public string CodiceIstat { get; private set; } = null!;
    public string NomeComune { get; private set; } = null!;
    public string Provincia { get; private set; } = null!;
    public List<string> CapCodes { get; private set; } = [];

    public void SetCapCodes(List<string> capCodes) => CapCodes = capCodes;
}
