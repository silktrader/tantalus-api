using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Tantalus.Data;
using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public class UserService : IUserService {
    private readonly DataContext _dataContext;
    private readonly Settings _settings;

    public UserService(DataContext dataContext, IOptions<Settings> settings) {
        _dataContext = dataContext;
        _settings = settings.Value;
    }

    public async Task<UserLoginResponse?> Login(string name, string password) {
        var user = await _dataContext.Users.SingleOrDefaultAsync(user => user.Name == name);

        // invalid user name or password
        if (user == null || ComputeHash(password, Convert.FromBase64String(user.PasswordSalt)) != user.HashedPassword)
            return null;

        var accessToken = GenerateAccessToken(user.Id);
        var refreshToken = GenerateRefreshToken(user);
        user.RefreshTokens.Add(refreshToken);

        // remove old inactive refresh tokens based on exported TTL
        user.RefreshTokens.RemoveAll(token =>
            !token.IsActive && token.CreationDate.AddDays(_settings.ExpiredTokensDuration) <= DateTime.UtcNow
        ); // tk user dapper

        _dataContext.Update(user);
        await _dataContext.SaveChangesAsync();

        return new UserLoginResponse(user, accessToken, refreshToken);
    }

    public async Task<bool> Exists(string name) {
        return await _dataContext.Users.AnyAsync(user => user.Name == name);
    }

    public async Task Register(UserRegisterRequest registerRequest) {
        var saltBytes = GenerateSalt();
        var user = new User(
            id: Guid.NewGuid(),
            name: registerRequest.Name,
            email: registerRequest.Email,
            hashedPassword: ComputeHash(registerRequest.Password, saltBytes),
            passwordSalt: Convert.ToBase64String(saltBytes),
            fullName: registerRequest.FullName,
            creationDate: DateTime.UtcNow
        );

        _dataContext.Users.Add(user);
        await _dataContext.SaveChangesAsync();
    }

    public void Delete(Guid userId) {

        var user = _dataContext.Users.Find(userId);
        if (user == null) throw new KeyNotFoundException();

        _dataContext.Users.Remove(user);
        _dataContext.SaveChanges();
    }

    public UserLoginResponse RefreshToken(string tokenValue) {
        //var user = GetUserByRefreshToken(tokenValue);
        //var refreshToken = user.RefreshTokens.Single(x => x.Contents == token);
        var refreshToken = _dataContext.RefreshTokens.Include(token => token.User).SingleOrDefault(token => token.Value == tokenValue);
        if (refreshToken is not { IsActive: true }) throw new Exception("Invalid token");
        var user = refreshToken.User;

        // revoke all descendant tokens in case this token has been compromised
        if (refreshToken.WasRevoked) {
            RevokeDescendantTokens(refreshToken, Entities.RefreshToken.RevocationReason.RevokedAncestor);
            _dataContext.Update(user);
            _dataContext.SaveChanges();
        }

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = RotateRefreshToken(refreshToken, user);
        user.RefreshTokens.Add(newRefreshToken);

        // remove old refresh tokens from user
        RemoveOldRefreshTokens(user);

        _dataContext.Update(user);
        _dataContext.SaveChanges();

        var accessToken = GenerateAccessToken(user.Id);
        return new UserLoginResponse(user, accessToken, newRefreshToken);
    }

    public void RevokeToken(Guid userId, string token) {
        var user = _dataContext.Users.Find(userId);
        if (user == null) throw new Exception();
        
        var refreshToken = user.RefreshTokens.SingleOrDefault(refreshToken => refreshToken.Value == token);
        if (refreshToken is not { IsActive: true }) throw new Exception();

        // revoke token and save
        RevokeRefreshToken(refreshToken, Entities.RefreshToken.RevocationReason.Manual);
        _dataContext.Update(user);
        _dataContext.SaveChanges();
    }

    // remove old inactive refresh tokens from user based on TTL in app settings
    private void RemoveOldRefreshTokens(User user) {
        user.RefreshTokens.RemoveAll(token => !token.IsActive && token.CreationDate.AddDays(_settings.ExpiredTokensDuration) <= DateTime.UtcNow);
    }

    // recursively traverse the refresh token chain and ensure all descendants are revoked
    private static void RevokeDescendantTokens(RefreshToken refreshToken, RefreshToken.RevocationReason reason) {
        if (string.IsNullOrEmpty(refreshToken.ReplacedBy)) return;

        var childToken = refreshToken.User.RefreshTokens.SingleOrDefault(token => token.Value == refreshToken.ReplacedBy);
        if (childToken == null) return;

        if (childToken.IsActive) RevokeRefreshToken(childToken, reason);
        else RevokeDescendantTokens(childToken, reason);
    }

    private string GenerateAccessToken(Guid userId) {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(new[]
                { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) }),
            Expires = DateTime.UtcNow.AddMinutes(_settings.AccessTokensDuration),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_settings.Secret)),
                    SecurityAlgorithms.HmacSha512Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private static RefreshToken GenerateRefreshToken(User user) {
        // https://stackoverflow.com/a/643511
        return new RefreshToken(
            value: Convert.ToBase64String(RandomNumberGenerator.GetBytes(64)), // seems to be safe on with .NET NewGuid()
            userId: user.Id,
            creationDate: DateTime.UtcNow,
            expiryDate: DateTime.UtcNow.AddDays(7)
        );
    }

    // create a random 256-bit salt
    private static byte[] GenerateSalt() => RandomNumberGenerator.GetBytes(32);

    private static string ComputeHash(string password, byte[] salt) => 
        Convert.ToBase64String(KeyDerivation.Pbkdf2(password, salt, KeyDerivationPrf.HMACSHA512, 100000, 32));

    private static RefreshToken RotateRefreshToken(RefreshToken refreshToken, User user) {
        var newRefreshToken = GenerateRefreshToken(user);
        RevokeRefreshToken(refreshToken, Entities.RefreshToken.RevocationReason.Replaced,
            newRefreshToken.Value);
        return newRefreshToken;
    }

    private static void RevokeRefreshToken(RefreshToken token,
        RefreshToken.RevocationReason? reason = null, string? replacedByToken = null) {
        token.RevocationDate = DateTime.UtcNow;
        token.ReasonRevoked = reason;
        token.ReplacedBy = replacedByToken;
    }
}