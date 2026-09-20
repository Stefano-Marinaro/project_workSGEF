namespace GoCare.Services.Provisioning;

public sealed record PersonProfileData(
      string Name, string Surname, DateOnly BirthDate, string Phone);
