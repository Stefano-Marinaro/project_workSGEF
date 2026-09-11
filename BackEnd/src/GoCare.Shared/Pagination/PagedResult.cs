namespace GoCare.Shared.Pagination;
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize)
{
    public int TotalPages =>
        PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize); //Int davanti: converte a intero, Math.Ceiling: approssimazione eccesso.
}                                                                              //Serve per dire al client quante pagine esistono


