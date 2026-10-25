namespace GoCare.Dtos.Domain.Responses;

// un comune coperto, come lo vede il frontend: il codice per salvarlo, nome e provincia per mostrarlo//
// sealed non permette che nessuna classe erediti da questa
public sealed record CoveredComuneResponse(string CodiceIstat, string Nome, string Provincia);