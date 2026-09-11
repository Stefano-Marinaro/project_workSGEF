namespace GoCare.Application.Services.Auth;

public interface IPasswordService
{
    string Hash(string plainPassword);
    bool Verify(string plainPassword, string hash);
}
