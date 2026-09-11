namespace GoCare.Application.Models.Auth;

public interface IExpirable
{
    DateTimeOffset ExpiresAt { get; }
}
