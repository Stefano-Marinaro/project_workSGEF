namespace GoCare.Dtos.Auth.Responses;

// Solo l'id creato: la password non deve mai comparire in una response (log, cache, devtools, ecc.).
public sealed record RegisterResponse(Guid AccountId);
