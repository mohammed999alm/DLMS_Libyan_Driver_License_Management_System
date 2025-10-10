using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using WebApplication2.DataRepo;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "مدير النظام,مستخدم النظام,شرطي مرور")]
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseController : ControllerBase
    {


        [Authorize(Roles = "مدير النظام,مستخدم النظام")]

        [HttpGet("Find/{id}")] public IActionResult Get(int id) 
        {
            var license = LicenseRepo.Find(id);
            
            if (license == null) return NotFound($"There is not license have this id = {id}"); 

            return Ok(license);
        }


        [Authorize(Roles = "مدير النظام,مستخدم النظام,شرطي مرور")]
        [HttpGet("Test")] public IActionResult GetTest()
        {
            return Ok("يسمح بدخول شرطي المرور");
        }

        [AllowAnonymous]
        [HttpGet("Test2")]
        public IActionResult GetTest2()
        {
            return Ok("يسمح بدخول الجميع");
        }



        [AllowAnonymous]
        [HttpGet("WhoAmI")]
        public IActionResult WhoAmI()
        {
            var name = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            // Extract token from Authorization header
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

            return Ok($"User: {name}, Role: {role}, Token: {token}");
        }
    }
}
