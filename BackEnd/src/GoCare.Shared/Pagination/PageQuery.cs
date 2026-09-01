namespace GoCare.Shared.Pagination;

// Questa pagina ha lo scopo di normalizzare i numeri ricevuti dal client: possibilità di inserire solo valori sicuri in una queryString 
public sealed record PageQuery
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 20;

    public int Page { get; }
    public int PageSize { get; }

    public PageQuery(int page = 1, int pageSize = DefaultPageSize)
    {
        Page = page < 1 ? 1 : page;    // operatore ternario: condizione ? valore_se_vera : valore_se_falsa
        PageSize = pageSize is < 1 or > MaxPageSize ? DefaultPageSize : pageSize;  // alternativa: PageSize = (pageSize < 1 || pageSize > MaxPageSize) ? DefaultPageSize : pageSize;
    }

    public int Skip => (Page - 1) * PageSize; // serve a capire in quale pagina si è, nello specifico quante righe devi saltare:
                                              //    Page = 1 -> (1 - 1) * 20 = 0 -> salti 0 righe.
                                              //    Page = 2 -> (2 - 1) * 20 = 20 → salti le prime 20, parti dalla 21.
}
