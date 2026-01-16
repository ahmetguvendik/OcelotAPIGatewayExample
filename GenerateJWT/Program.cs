using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;

var secretKey = "BuCokGizliBirAnahtarOlmalidirVeEnAz32KarakterUzunlugundaOlmalidir";
var issuer = "ServiceA";
var audience = "ServiceA";

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var claims = new[]
{
    new Claim(JwtRegisteredClaimNames.Sub, "test-user"),
    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
};

var token = new JwtSecurityToken(
    issuer: issuer,
    audience: audience,
    claims: claims,
    expires: DateTime.UtcNow.AddHours(1),
    signingCredentials: creds
);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
Console.WriteLine(tokenString);
