namespace GoCare.Abstractions;

public interface IPushSender
{
    Task SendAsync(string deviceToken, string title, string body, CancellationToken ct = default);
}
