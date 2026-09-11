namespace GoCare.Shared.Abstractions;

public interface ICurrentUser
{
    Guid? AccountId { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
