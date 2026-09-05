using GoCare.Application.Models.Enums;

namespace GoCare.Application.Models.Domain;

public sealed class Association(Guid id, string name, Address headquarter, List<string> phones, string email, List<string> coveredProvinces)
{
    public Guid Id { get; } = id;
    public string Name { get; private set; } = name;
    public Address Headquarter { get; private set; } = headquarter;
    public List<string> Phones { get; private set; } = phones;
    public string Email { get; private set; } = email;
    public string? Hours { get; private set; }
    public EAccreditationStatus Status { get; private set; }
    public DateTimeOffset? DeletedAt { get; private set; }
    public List<string> CoveredProvinces { get; private set; } = coveredProvinces;

}
