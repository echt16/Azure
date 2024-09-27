using lab3_1.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

public class AuthorizationService
{
    internal static ClaimsPrincipal DecodeToken(string token, string secretKey)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = JsonConvert.DeserializeObject<byte[]>(secretKey) ?? new byte[0];

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false
        };

        SecurityToken securityToken;
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);

        return principal;
    }

    internal static int GetIdOfCurrentUser(string token, string secretKey)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = JsonConvert.DeserializeObject<byte[]>(secretKey) ?? new byte[0];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            };

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            return int.Parse(principal.Claims.ElementAt(0).Value);
        }
        catch (Exception ex)
        {
            return -1;
        }
    }

    internal static AccountModel GetCurrentUser(string token, string secretKey)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            byte[] key = JsonConvert.DeserializeObject<byte[]>(secretKey) ?? new byte[0];

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false
            };

            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            return new AccountModel() { Id = int.Parse(principal.Claims.ElementAt(0).Value), Role = principal.Claims.ElementAt(1).Value };
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    internal static bool CheckAuthorization(string tokenStr, string keyStr, string role)
    {
        try
        {
            if (tokenStr == null || tokenStr == "" || keyStr == null || keyStr == "")
            {
                return false;
            }

            ClaimsPrincipal claimsPrincipal = DecodeToken(tokenStr, keyStr);
            return claimsPrincipal.IsInRole(role);
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    internal static bool CheckAuthorization(string tokenStr, string keyStr)
    {
        try
        {
            if (tokenStr == null || tokenStr == "" || keyStr == null || keyStr == "")
            {
                return false;
            }

            ClaimsPrincipal claimsPrincipal = DecodeToken(tokenStr, keyStr);
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    internal static AuthorizationModel GenerateToken(int userId, string role)
    {
        var claims = new[]
        {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(GenerateRandomSecretKey(32));
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return new AuthorizationModel()
        {
            Token = tokenHandler.WriteToken(token),
            Key = JsonConvert.SerializeObject(key)
        };
    }

    private static string GenerateRandomSecretKey(int length)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        StringBuilder stringBuilder = new StringBuilder();
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            byte[] uintBuffer = new byte[sizeof(uint)];

            while (length-- > 0)
            {
                rng.GetBytes(uintBuffer);
                uint num = BitConverter.ToUInt32(uintBuffer, 0);
                stringBuilder.Append(validChars[(int)(num % (uint)validChars.Length)]);
            }
        }
        return stringBuilder.ToString();
    }
}
