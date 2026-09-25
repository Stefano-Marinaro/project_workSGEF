using System.Security.Claims;

using GoCare.Dtos.Auth.Requests;
using GoCare.Dtos.Auth.Responses;
using GoCare.Errors;
using GoCare.Services.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoCare.Controllers.Auth;

[ApiController]
[Route("auth")]
public sealed class AuthController(AuthService authservice) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<AuthTokenResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken) = await authservice.LoginAsync(request.Email, request.Password, ct);

        return Ok(new AuthTokenResponse(accessToken, refreshToken));
    }

    [HttpPost("register/user")]
    public async Task<ActionResult<RegisterResponse>> RegisterUser(RegisterUserRequest request, CancellationToken ct)
    {
        var accountId = await authservice.RegisterUserAsync(request.Email, request.Password, ct);

        return Created($"/auth/account/{accountId}", new RegisterResponse(accountId));
    }

    [HttpPost("register/association")]
    public async Task<ActionResult<RegisterResponse>> RegisterAssociation(RegisterAssociationRequest request, CancellationToken ct)
    {
        var accountId = await authservice.RegisterAssociationAsync(request.Email, request.Password, ct);

        return Created($"/auth/account/{accountId}", new RegisterResponse(accountId));
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail(VerifyEmailRequest request, CancellationToken ct)
    {
        await authservice.VerifyAccount(request.Token, ct);
        return NoContent();
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request, CancellationToken ct)
    {
        await authservice.ForgotPasswordAsync(request.Email, ct);
        return NoContent();
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request, CancellationToken ct)
    {
        await authservice.ResetPasswordAsync(request.Token, request.NewPassword, ct);
        return NoContent();
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken ct)
    {
        await authservice.LogoutAsync(request.RefreshToken, ct);
        return NoContent();
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<AuthTokenResponse>> Refresh(RefreshRequest request, CancellationToken ct)
    {
        var (accessToken, refreshToken) = await authservice.RefreshAsync(request.RefreshToken, ct);

        return Ok(new AuthTokenResponse(accessToken, refreshToken));
    }

    [HttpPost("verify-email/resend")]
    public async Task<IActionResult> ResendVerificationEmail(
        ResendVerificationEmailRequest request,
        CancellationToken ct)
    {
        await authservice.ResendVerificationEmailAsync(request.Email, ct);
        return NoContent();
    }

    [Authorize]
    [HttpPost("change-email")]
    public async Task<IActionResult> ChangeEmail(ChangeEmailRequest request, CancellationToken ct)
    {
        var subject = User.FindFirstValue("sub");

        if (!Guid.TryParse(subject, out var accountId))
            throw new ForbiddenException("Il token non contiene un identificativo account valido.");

        await authservice.RequestEmailChangeAsync(
            accountId,
            request.NewEmail,
            request.CurrentPassword,
            ct);

        return NoContent();
    }

    [HttpPost("change-email/confirm")]
    public async Task<IActionResult> ConfirmEmailChange(
        ConfirmEmailChangeRequest request,
        CancellationToken ct)
    {
        await authservice.ConfirmEmailChangeAsync(request.Token, ct);
        return NoContent();
    }
}



