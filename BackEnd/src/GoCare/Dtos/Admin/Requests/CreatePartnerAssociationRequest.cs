using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Admin.Requests;

// dati con cui l'admin registra un'associazione partner (UC 11.1): la password la sceglie poi l'associazione
public sealed record CreatePartnerAssociationRequest(
    string Email,
    string Name,
    AddressRequest Headquarter,
    List<string> Phones,
    List<string> CoveredComuni,          // codici ISTAT, es. "054001"
    string? AvailabilityHours
);
