using DLMS_BusinessLogicLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GlobalUtility;

namespace DLMS_SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {



		public static class UserRoles
		{
			public const string Cop = "شرطي مرور";
			public const string Admin = "مدير نظام";
			public const string SystemUser = "مستخدم نظام";
		}


		[HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequestDTO loginRequest)
        {
            return _Login(loginRequest);
        }


        private IActionResult _Login(LoginRequestDTO loginRequest, bool recoveryMode = false) 
        {

            DLMS_BusinessLogicLayer.User existingUser = DLMS_BusinessLogicLayer.User.Find(loginRequest.Username);

            if (existingUser != null)
            {
                if (!existingUser.IsActive())
                {
                    return Unauthorized("هذا الحساب معطل يجب ان تتواصل مع مدير النظام أو من له الشأن في إعادة تفعيل الحسابات");
                }
                if (existingUser.FailAttempts < 3)
                {
                    User? user = null;

                    if (recoveryMode)
                        user = DLMS_BusinessLogicLayer.User.Find(loginRequest.Username, loginRequest.Password);
                    else
                    {
                        if (PasswordHasher.VerifyPassword(loginRequest.Password, existingUser.Password)) 
                        {
                            user = existingUser;
                        }
                    }


                    if (user != null)
                    {
                        
                        if (existingUser.FailAttempts > 0) { existingUser.FailAttempts = 0; existingUser.Save(); }

                        if (!recoveryMode)
                        {
                            if (user.UserRole == UserRoles.Cop && loginRequest.AudienceType == "DLMS_Desktop")
                                return Unauthorized("هذا الحساب لا يمكن استخدامه في نسخة سطح المكتب");

                            if (user.UserRole != UserRoles.Cop && loginRequest.AudienceType == "DLMS_MobileApp")
                                return Unauthorized("هذا الحساب لا يمكن استخدامه في نسخة الهاتف المحمول");
                        }
                        else
                        {
                            user.UserRole += " إعادة تعيين كلمة المرور";
                            loginRequest.AudienceType = "DLMS_Desktop";
                        }
                        var token = GenerateJwtToken(user, loginRequest);
                        return Ok(new { Token = token });
                    }
                    else
                    {
                        existingUser.FailAttempts++;

                        if (existingUser.Save())
                            return Unauthorized($"كلمة المرور خاطئة تبقت لديك  {existingUser.FailAttempts - 3} محاولة(محاولات)");

                        return Unauthorized("كلمة المرور غير صحيحة");
                    }
                }
                else
                {
                    return Unauthorized("تم إيقاف حسابك لتجاوزك العدد المسموح من المحاولات الخاطئة");
                }
            }

            return Unauthorized("إسم المستخدم غير صحيح الرجاء التأكد من إسم المستخدم");
        }



        [HttpPost("Recovery")]
        public IActionResult RecoveryLogin([FromBody] LoginRequestDTO loginRequest)
        {
            return _Login(loginRequest, true);
        }


        public class LoginRequestDTO
        {
            public string Username { get; set; }
            public string Password { get; set; }
            public string? AudienceType { get; set; }

		}
        public class TokenData
        {
            public string Token { get; set; }

            public DateTime Expiration { get; set; }

            public string Role { get; set; }
        }

        private TokenData GenerateJwtToken(User user, LoginRequestDTO logData)
        {
            var expiration = DateTime.UtcNow.AddMinutes(30);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("mySuperSecureSecretKeyWith256Bits123456789"); 
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.UserRole),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString())
            }),
                Expires = expiration,
                Issuer = "myIssuer",
                Audience = logData.AudienceType,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new TokenData { Token = tokenHandler.WriteToken(token), Expiration = expiration, Role = user.UserRole };
        }
    }
}
