using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ContentHub.Application.Abstractions.Authentication;
using ContentHub.Data.Entities.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ContentHub.Infrastructure.Authentication;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Generate(
        User user,
        IReadOnlyCollection<string> roles,
        Guid? sessionId = null)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("userId", user.Id.ToString())
        };

        if (sessionId.HasValue)
        {
            claims.Add(new Claim("sessionId", sessionId.Value.ToString()));
        }

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_options.Secret));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
