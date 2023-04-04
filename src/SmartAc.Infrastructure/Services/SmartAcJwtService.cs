using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartAc.Application.Abstractions.Services;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartAc.Infrastructure.Services;

public sealed class SmartAcJwtService : ISmartAcJwtService
{
    public string JwtScopeApiKey => "ApiKey";

    public string JwtScopeDeviceIngestionService => "DeviceIngestionApi";

    public string JwtScopeDeviceAdminService => "DeviceAdminApi";

    private readonly string _key;

    private readonly string _issuer;

    private readonly string _audience;

    public SmartAcJwtService(IConfiguration configuration)
    {
        _key = configuration["Jwt:Key"];
        _issuer = configuration["Jwt:Issuer"];
        _audience = configuration["Jwt:Audience"];
    }

    public (string tokenId, string token) GenerateJwtFor(string targetId, string role)
    {
        var tokenId = Guid.NewGuid().ToString();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, targetId),
            new Claim(JwtRegisteredClaimNames.Jti, tokenId),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new Claim(ClaimTypes.Role, JwtScopeApiKey),
            new Claim(ClaimTypes.Role, role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(180),
            signingCredentials: credentials);

        return (tokenId, new JwtSecurityTokenHandler().WriteToken(token));
    }
}