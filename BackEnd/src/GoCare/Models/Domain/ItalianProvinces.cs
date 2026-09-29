namespace GoCare.Models.Domain;

public static class ItalianProvinces
{
    // Sigle in maiuscolo. L'elenco può cambiare nel tempo (es. riordino delle province sarde): si aggiorna qui.
    public static readonly IReadOnlySet<string> All = new HashSet<string>(StringComparer.Ordinal)
    {
        "AG", "AL", "AN", "AO", "AP", "AQ", "AR", "AT", "AV",
        "BA", "BG", "BI", "BL", "BN", "BO", "BR", "BS", "BT", "BZ",
        "CA", "CB", "CE", "CH", "CL", "CN", "CO", "CR", "CS", "CT", "CZ",
        "EN",
        "FC", "FE", "FG", "FI", "FM", "FR",
        "GE", "GO", "GR",
        "IM", "IS",
        "KR",
        "LC", "LE", "LI", "LO", "LT", "LU",
        "MB", "MC", "ME", "MI", "MN", "MO", "MS", "MT",
        "NA", "NO", "NU",
        "OR",
        "PA", "PC", "PD", "PE", "PG", "PI", "PN", "PO", "PR", "PT", "PU", "PV", "PZ",
        "RA", "RC", "RE", "RG", "RI", "RM", "RN", "RO",
        "SA", "SI", "SO", "SP", "SR", "SS", "SU", "SV",
        "TA", "TE", "TN", "TO", "TP", "TR", "TS", "TV",
        "UD",
        "VA", "VB", "VC", "VE", "VI", "VR", "VT", "VV"
    };

    public static bool IsValid(string? code) =>
        code is not null && All.Contains(code.Trim().ToUpperInvariant());
}
