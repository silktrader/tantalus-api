using Tantalus.Entities;
using Tantalus.Models;

namespace Tantalus.Services;

public interface IUserService {
    Task<UserLoginResponse?> Login(string name, string password);
    Task Register(UserRegisterRequest request);
    void RevokeToken(Guid userId, string token);
    UserLoginResponse RefreshToken(string tokenValue);
    Task<bool> Exists(string userName);
    void Delete(Guid userId);
}