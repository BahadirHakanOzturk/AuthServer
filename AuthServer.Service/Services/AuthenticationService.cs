using AuthServer.Core.Configuration;
using AuthServer.Core.Dtos;
using AuthServer.Core.Models;
using AuthServer.Core.Repositories;
using AuthServer.Core.Services;
using AuthServer.Core.UnitOfWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SharedLibrary.Dtos;

namespace AuthServer.Service.Services;

public class AuthenticationService : IAuthenticationService
{
	private readonly List<Client> clients;
	private readonly ITokenService tokenService;
	private readonly UserManager<UserApp> userManager;
	private readonly IUnitOfWork unitOfWork;
	private readonly IGenericRepository<UserRefreshToken> userRefreshTokenRepository;

	public AuthenticationService(IOptions<List<Client>> optionsClient, ITokenService tokenService, UserManager<UserApp> userManager, IUnitOfWork unitOfWork, IGenericRepository<UserRefreshToken> userRefreshTokenRepository)
    {
        clients = optionsClient.Value;
		this.userManager = userManager;
		this.unitOfWork = unitOfWork;
		this.userRefreshTokenRepository = userRefreshTokenRepository;
    }

    public async Task<Response<TokenDto>> CreateTokenAsync(LoginDto loginDto)
	{
		if(loginDto == null) throw new ArgumentNullException(nameof(loginDto));

		var user = await userManager.FindByEmailAsync(loginDto.Email);

		if (user == null) return Response<TokenDto>.Fail("Email or Password is wrong.", 400, true);

		if (!await userManager.CheckPasswordAsync(user, loginDto.Password)) return Response<TokenDto>.Fail("Email or Password is wrong.", 400, true);

		var token = tokenService.CreateToken(user);
		var userRefreshToken = await userRefreshTokenRepository.Where(x => x.UserId == user.Id).SingleOrDefaultAsync();
		
		if(userRefreshToken == null)
		{
			await userRefreshTokenRepository.AddAsync(new UserRefreshToken { UserId = user.Id, Code = token.RefreshToken, Expiration = token.RefreshTokenExpiration });
		}
		else
		{
			userRefreshToken.Code = token.RefreshToken;
			userRefreshToken.Expiration = token.RefreshTokenExpiration;
		}

		await unitOfWork.CommitAsync();

		return Response<TokenDto>.Success(token, 200);
	}

	public Response<ClientTokenDto> CreateClientToken(ClientLoginDto clientLoginDto)
	{
		var client = clients.SingleOrDefault(x => x.Id.ToString() == clientLoginDto.ClientId && x.Secret == clientLoginDto.ClientSecret);

		if (client == null) return Response<ClientTokenDto>.Fail("Client Id or Client Secret not found.",404, true);

		var token = tokenService.CreateClientToken(client);

		return Response<ClientTokenDto>.Success(token,200);
	}

	public async Task<Response<TokenDto>> CreateTokenByRefreshTokenAsync(string refreshToken)
	{
		var rToken = await userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

		if(rToken == null) return Response<TokenDto>.Fail("Refresh token not found.",404,true);

		var user = await userManager.FindByIdAsync(rToken.UserId);

		if (user == null) return Response<TokenDto>.Fail("User Id not found.", 404, true);

		var tokenDto = tokenService.CreateToken(user);

		rToken.Code = tokenDto.RefreshToken;
		rToken.Expiration = tokenDto.RefreshTokenExpiration;

		await unitOfWork.CommitAsync();

		return Response<TokenDto>.Success(tokenDto,200);
	}

	public async Task<Response<NoDataDto>> RevokeRefreshTokenAsync(string refreshToken)
	{
		var rToken = await userRefreshTokenRepository.Where(x => x.Code == refreshToken).SingleOrDefaultAsync();

		if (rToken == null) return Response<NoDataDto>.Fail("Refresh token not found.", 404, true);

		userRefreshTokenRepository.Remove(rToken);
		await unitOfWork.CommitAsync();

		return Response<NoDataDto>.Success(200);
	}
}
