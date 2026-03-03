using Module.Auth.Dtos;

namespace Module.Auth.Services;

public interface ITokenService
{
    Task<TokenResponse> LoginAsync(string username, string password, CancellationToken ct = default);
    Task<TokenResponse> RefreshAsync(string refreshToken, CancellationToken ct = default);
    Task LogoutAsync(string refreshToken, CancellationToken ct = default);
}
