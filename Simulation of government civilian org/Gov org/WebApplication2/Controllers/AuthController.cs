//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Text;

//[ApiController]
//[Route("api/[controller]")]
//public class AuthController : ControllerBase
//{
//    [HttpGet("generate-token")]
//    public IActionResult GenerateToken()
//    {
//        var token = GenerateJwtToken();
//        return Ok(new { Token = token });
//    }

//    private string GenerateJwtToken()
//    {
//        var tokenHandler = new JwtSecurityTokenHandler();
//        var key = Encoding.UTF8.GetBytes("mySuperSecureSecretKeyWith256Bits123456789"); // Key must be >= 32 characters
//        var tokenDescriptor = new SecurityTokenDescriptor
//        {
//            Subject = new ClaimsIdentity(new[]
//            {
//            new Claim(ClaimTypes.Name, "TestUser"),
//            new Claim(ClaimTypes.Role, "Admin") // Assign Admin role
//        }),
//            Expires = DateTime.UtcNow.AddHours(1),
//            Issuer = "myIssuer",
//            Audience = "myAudience",
//            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
//        };
//        var token = tokenHandler.CreateToken(tokenDescriptor);
//        return tokenHandler.WriteToken(token);
//    }
//}


using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication2.DataRepo;
using WebApplication2.TestClasses;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] User loginRequest)
    {
        // Validate credentials (simplified for demo purposes)
        User? user = UserRepo.Login(loginRequest.Username, loginRequest.Password);
        if (user != null)
        {
            // Generate a JWT token
            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }
        else
        {
            return Unauthorized("Invalid credentials");
        }
    }


    public class TokenData 
    {
        public string Token { get; set; }

        public DateTime Expiration { get; set; }    

        public string Role {  get; set; }
    }

    private TokenData GenerateJwtToken(User user)
    {
        var expiration = DateTime.Now.AddMinutes(2);
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes("mySuperSecureSecretKeyWith256Bits123456789"); // At least 32 characters
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role) // Assign role dynamically if needed
            }),
            Expires =expiration,
            Issuer = "myIssuer",
            Audience = "myAudience",
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
        };

        
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return  new TokenData { Token = tokenHandler.WriteToken(token), Expiration = expiration, Role = user.Role};
    }
}

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}