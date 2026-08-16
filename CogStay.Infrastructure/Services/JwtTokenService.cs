using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using CogStay.Application.Contracts.Services;
using CogStay.Application.DTOs;

namespace CogStay.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public AuthResponseDTO GenerateTokens(
        string userId,
        int integerId,
        string fullName,
        string email,
        string role,
        bool emailVerified,
        bool phoneVerified,
        bool isActive)
    {
        var secretKey = _configuration["JWT_SIGNING_KEY"] 
            ?? _configuration["Jwt:SigningKey"] 
            ?? "CogStaySecretSigningKeySuperSecure32BytesLongString!";

        var issuer = _configuration["JWT_ISSUER"] ?? _configuration["Jwt:Issuer"] ?? "CogStayAPI";
        var audience = _configuration["JWT_AUDIENCE"] ?? _configuration["Jwt:Audience"] ?? "CogStayApp";

        var minutesStr = _configuration["JWT_ACCESS_TOKEN_MINUTES"] ?? _configuration["Jwt:AccessTokenMinutes"] ?? "60";
        double minutes = double.TryParse(minutesStr, out var m) ? m : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("IntegerId", integerId.ToString()),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim("EmailVerified", emailVerified.ToString().ToLower()),
            new Claim("PhoneVerified", phoneVerified.ToString().ToLower()),
            new Claim("IsActive", isActive.ToString().ToLower())
        };

        var expires = DateTime.UtcNow.AddMinutes(minutes);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: credentials);

        var tokenHandler = new JwtSecurityTokenHandler();
        var accessTokenStr = tokenHandler.WriteToken(tokenDescriptor);

        // Secure Refresh Token Generation
        var refreshTokenBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(refreshTokenBytes);
        var refreshTokenStr = Convert.ToBase64String(refreshTokenBytes);

        return new AuthResponseDTO
        {
            Token = accessTokenStr,
            RefreshToken = refreshTokenStr,
            ExpiresAt = expires,
            UserId = userId,
            IntegerId = integerId,
            FullName = fullName,
            Email = email,
            Role = role,
            EmailVerified = emailVerified,
            PhoneVerified = phoneVerified,
            IsActive = isActive
        };
    }

    public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var secretKey = _configuration["JWT_SIGNING_KEY"] 
            ?? _configuration["Jwt:SigningKey"] 
            ?? "CogStaySecretSigningKeySuperSecure32BytesLongString!";

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false // Keep false so expired tokens can be read for refresh
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }
            return principal;
        }
        catch
        {
            return null;
        }
    }
}
