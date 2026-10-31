using GoCare.Dtos.Transport.Requests;

namespace GoCare.Dtos.Admin.Requests;

// stessi campi di PUT /association/profile, più l'email (UC 11.2 · AD-02)
public sealed record UpdatePartnerAssociationRequest(
    string Email,
    string Name,
    AddressRequest Headquarter,
    List<string> Phones,
    List<string> CoveredComuni,          // codici ISTAT, es. "054001"
    string? AvailabilityHours
);
