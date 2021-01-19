using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ellersoft.SignalR.Core
{
    public class JwtMiddleware
    {
        public IConfiguration Configuration { get; }
        public string Audience { get; }
        public string Issuer { get; }
        public SecurityKey Key { get; }

        public JwtMiddleware(IConfiguration configuration)
        {
            Configuration = configuration;
            Audience = configuration["Jwt:Audience"];
            Issuer = configuration["Jwt:Issuer"];
            Key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["Jwt:Key"]));
        }
        
        private SigningCredentials GetSigningCredentials() =>
            new SigningCredentials(Key, SecurityAlgorithms.HmacSha256Signature);

        private SecurityTokenDescriptor GetDescriptor(ClaimsPrincipal user) => 
            GetDescriptor(user.Claims);

        private SecurityTokenDescriptor GetDescriptor(IEnumerable<Claim> claims) =>
            new SecurityTokenDescriptor
            {
                Audience = Audience,
                Issuer = Issuer,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = GetSigningCredentials()
            };

        public TokenValidationParameters GetValidationParameters(TokenValidationParameters initialParameters)
        {
            initialParameters.IssuerSigningKey = Key;
            initialParameters.ValidAudience = Audience;
            initialParameters.ValidIssuer = Issuer;
            initialParameters.ValidateAudience = Audience != null;
            initialParameters.ValidateIssuer = Issuer != null;
            initialParameters.ValidateIssuerSigningKey = true;
            initialParameters.ValidateLifetime = true;
            return initialParameters;
        }

        private string GenerateToken(SecurityTokenDescriptor descriptor)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(descriptor));
        }

        public string GenerateToken(ClaimsPrincipal user) =>
            GenerateToken(GetDescriptor(user));

        public static Task GetTokenFromQueryString(MessageReceivedContext context, string basePath)
        {
            var accessToken = context.Request.Query["access_token"];

            // If the request is for our hub...
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments(basePath))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    }
}
