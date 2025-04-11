using System.Security.Cryptography;
using GymManagerAPI.Data.Common;
using GymManagerAPI.Data.Context;
using GymManagerAPI.Data.DTOs;
using GymManagerAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace GymManagerAPI.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext applicationDbContext;
        private readonly JwtService jwtService;

        public AuthService(ApplicationDbContext applicationDbContext, JwtService jwtService)
        {
            this.applicationDbContext = applicationDbContext;
            this.jwtService = jwtService;
        }

        public async Task<OperationResult<TokenResult>> AuthenticateUserAsync(UserLoginDTO userLoginDTO)
        {
            // Verify the user by username
            var user = await applicationDbContext.Users
                .AsNoTracking()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserName == userLoginDTO.UserName && u.IsActive);

            if (user == null)
            {
                return OperationResult<TokenResult>.Fail(401, "Invalid username or password.");
            }

            // Verify the password
            if (!PasswordHasher.VerifyPasswordHash(userLoginDTO.Password, user.PasswordHash, user.PasswordSalt))
            {
                return OperationResult<TokenResult>.Fail(401, "Invalid username or password.");
            }

            //generate an access and a refresh token
            var tokenResult = await GenerateAccessAndRefreshTokenAsync(user);

            return OperationResult<TokenResult>.Ok(data: tokenResult);
        }

        public async Task<OperationResult<RefreshToken>> RevocateRefreshTokenAsync(string token)
        {
            var refreshToken = await applicationDbContext.RefreshTokens.FirstOrDefaultAsync(x => x.Token == token);

            if (refreshToken == null)
            {
                return OperationResult<RefreshToken>.Fail(400, "Invalid refresh token");
            }

            refreshToken.IsRevoked = true;

            applicationDbContext.Update(refreshToken);

            await applicationDbContext.SaveChangesAsync();

            return OperationResult<RefreshToken>.Ok();
        }

        //helpers
        public async Task<TokenResult> GenerateAccessAndRefreshTokenAsync(User user)
        {
            //generate an access and a refresh token
            var accessToken = jwtService.GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken(user.Id);

            //save the refreshtoken generated in the db
            await applicationDbContext.RefreshTokens.AddAsync(refreshToken);
            await applicationDbContext.SaveChangesAsync();

            var tokenResult = new TokenResult()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken.Token
            };

            return tokenResult;
        }

        public RefreshToken GenerateRefreshToken(int userId)
        {
            var refreshToken = new RefreshToken()
            {
                Token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)),
                ExpirationDate = DateTime.UtcNow.AddDays(1),
                UserId = userId,
                IsRevoked = false
            };

            return refreshToken;
        }

        public async Task<RefreshToken> GetRefreshToken(string token)
        {
            return await applicationDbContext.RefreshTokens
                .AsNoTracking()
                .Include(x => x.User)
                .ThenInclude(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .FirstOrDefaultAsync(x => x.Token == token);
        }
    }
}
