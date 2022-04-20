using System.Text.Json.Serialization;
using Tantalus.Entities;

namespace Tantalus.Models;

public record UserLoginResponse {
    public UserLoginResponse(User user, string accessToken, RefreshToken refreshToken) {
        Id = user.Id;
        FullName = user.FullName;
        Name = user.Name;
        AccessToken = accessToken;
        RefreshToken = refreshToken.Value;
    }

    public Guid Id { get; }
    public string FullName { get; }
    public string Name { get; }
    public string AccessToken { get; }
    
    [JsonIgnore]
    public string RefreshToken { get; }
}