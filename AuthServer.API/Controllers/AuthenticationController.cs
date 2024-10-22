using AuthServer.Core.Dtos;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthServer.API.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class AuthenticationController : BaseController
{
	private readonly IAuthenticationService authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        this.authenticationService = authenticationService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateToken(LoginDto loginDto)
    {
        var result = await authenticationService.CreateTokenAsync(loginDto);
        return ActionResultInstance(result);
    }

	[HttpPost]
	public IActionResult CreateClientToken(ClientLoginDto clientLoginDto)
	{
		var result = authenticationService.CreateClientToken(clientLoginDto);
		return ActionResultInstance(result);
	}

    [HttpPost]
    public async Task<IActionResult> RevokeRefreshToken(RefreshTokenDto refreshTokenDto)
    {
        var result = await authenticationService.RevokeRefreshTokenAsync(refreshTokenDto.Token);
        return ActionResultInstance(result);
    }

	[HttpPost]
	public async Task<IActionResult> CreateTokenByRefreshToken(RefreshTokenDto refreshTokenDto)
	{
		var result = await authenticationService.CreateTokenByRefreshTokenAsync(refreshTokenDto.Token);
		return ActionResultInstance(result);
	}
}
