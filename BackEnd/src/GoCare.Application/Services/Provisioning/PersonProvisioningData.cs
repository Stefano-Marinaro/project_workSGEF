namespace GoCare.Application.Services.Provisioning;

public sealed record PersonProvisioningData(
      string Name, string Surname, DateOnly BirthDate, string Email, string Phone);
