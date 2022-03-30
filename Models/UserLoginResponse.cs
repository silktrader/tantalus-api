using System.Text.Json.Serialization;
using Tantalus.Entities;

namespace Tantalus.Models;

public class UserLoginResponse {
    public UserLoginResponse(User user, string accessToken, RefreshToken refreshToken) {
        Id = user.Id;
        FullName = user.FullName;
        UserName = user.Name;
        AccessToken = accessToken;
        RefreshToken = refreshToken.Value;
    }

    public Guid Id { get; }
    public string FullName { get; }
    public string UserName { get; }
    public string AccessToken { get; }
    
    [JsonIgnore]
    public string RefreshToken { get; }
}