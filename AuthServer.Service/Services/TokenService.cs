using AuthServer.Core.Configuration;
using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedLibrary.Configurations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthServer.Service.Services;

public class TokenService : ITokenService
{
	private readonly UserManager<UserApp> userManager;
	private readonly CustomTokenOption tokenOption;

    public TokenService(UserManager<UserApp> userManager, IOptions<CustomTokenOption> options)
    {
        this.userManager = userManager;
		tokenOption = options.Value;
    }

	private string CreateRefreshToken()
	{
		var numberByte = new Byte[32];
		using var rnd = RandomNumberGenerator.Create();
		rnd.GetBytes(numberByte);

		return Convert.ToBase64String(numberByte);
	}

	private IEnumerable<Claim> GetClaims(UserApp userApp, List<string> audiences)
	{
		var userList = new List<Claim>() { 
			new Claim(ClaimTypes.NameIdentifier, userApp.Id),
			new Claim(JwtRegisteredClaimNames.Email, userApp.Email),
			new Claim(ClaimTypes.Name, userApp.UserName),
			new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
		};

		userList.AddRange(audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));

		return userList;
	}

	private IEnumerable<Claim> GetClientClaims(Client client)
	{
		var claims = new List<Claim>();
		claims.AddRange(client.Audiences.Select(x => new Claim(JwtRegisteredClaimNames.Aud, x)));
		claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
		claims.Add(new Claim(JwtRegisteredClaimNames.Sub, client.Id.ToString()));

		return claims;		
	}

    public TokenDto CreateToken(UserApp userApp)
	{
		var accessTokenExpiration = DateTime.Now.AddMinutes(tokenOption.AccessTokenExpiration);
		var refreshTokenExpiration = DateTime.Now.AddMinutes(tokenOption.RefreshTokenExpiration);
		var securityKey = SignService.GetSymmetricSecurityKey(tokenOption.SecurityKey);

		SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

		JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
			issuer: tokenOption.Issuer,
			expires: accessTokenExpiration,
			notBefore: DateTime.Now,
			claims: GetClaims(userApp, tokenOption.Audience),
			signingCredentials: signingCredentials);

		var handler = new JwtSecurityTokenHandler();
		var token = handler.WriteToken(jwtSecurityToken);

		var tokenDto = new TokenDto
		{
			AccessToken = token,
			RefreshToken = CreateRefreshToken(),
			AccessTokenExpiration = accessTokenExpiration,
			RefreshTokenExpiration = refreshTokenExpiration
		};

		return tokenDto;
	}

	public ClientTokenDto CreateClientToken(Client client)
	{
		var accessTokenExpiration = DateTime.Now.AddMinutes(tokenOption.AccessTokenExpiration);
		var securityKey = SignService.GetSymmetricSecurityKey(tokenOption.SecurityKey);

		SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature);

		JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
			issuer: tokenOption.Issuer,
			expires: accessTokenExpiration,
			notBefore: DateTime.Now,
			claims: GetClientClaims(client),
			signingCredentials: signingCredentials);

		var handler = new JwtSecurityTokenHandler();
		var token = handler.WriteToken(jwtSecurityToken);

		var tokenDto = new ClientTokenDto
		{
			AccessToken = token,
			AccessTokenExpiration = accessTokenExpiration,
		};

		return tokenDto;
	}
}
