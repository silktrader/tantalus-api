// ReSharper disable UnusedMember.Global
namespace Tantalus.Data;

#pragma warning disable CS8618
public class Settings {
    public string Secret { get; set; }
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public int ExpiredTokensDuration { get; set; }
    public int AccessTokensDuration { get; set; }
}
#pragma warning restore CS8618
