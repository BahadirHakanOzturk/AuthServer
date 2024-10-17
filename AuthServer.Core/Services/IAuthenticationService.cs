using AuthServer.Core.Dtos;
using SharedLibrary.Dtos;

namespace AuthServer.Core.Services;

public interface IAuthenticationService
{
	Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto);
	Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string	refreshToken);
	Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken);
	Task<Response<ClientTokenDto>> CreateTokenByClientAsync(ClientLoginDto clientLoginDto);

}
