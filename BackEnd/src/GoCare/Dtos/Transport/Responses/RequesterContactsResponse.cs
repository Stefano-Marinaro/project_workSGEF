namespace GoCare.Dtos.Transport.Responses;

// recapiti per organizzare il viaggio: li vede solo l'associazione che l'ha accettato (PA-04)
public sealed record RequesterContactsResponse(
    string? RequesterName,
    string? RequesterSurname,
    string? RequesterPhone,
    string RequesterEmail,
    string ReferencePhone,      // recapito indicato nella richiesta di trasporto
    string? ReferenceEmail,
    string BeneficiaryName,
    string BeneficiarySurname,
    string BeneficiaryPhone);
