namespace GoCare.Models.Auth;

public static class ExpirableExtensions 
{
    public static bool IsExpired(this IExpirable e, DateTimeOffset now) => now >= e.ExpiresAt;
}
