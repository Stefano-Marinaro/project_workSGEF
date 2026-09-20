namespace GoCare.Models.Auth;

public interface IExpirable
{
    DateTimeOffset ExpiresAt { get; }
}
