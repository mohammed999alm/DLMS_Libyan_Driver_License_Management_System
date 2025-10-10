using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using Microsoft.AspNetCore.Authorization;

namespace DLMS_SERVER.Controllers
{

    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [Route("api/[controller]")]
    [ApiController]
    public class DetainedLicenseController : ControllerBase
    {

        [HttpGet("Get-By-ID")] 
        public IActionResult GetByID(int id) 
        {
            DetainedLicense detainedLicense = DetainedLicense.Find(id);

            if (detainedLicense == null) return NotFound($"ليس هناك حجز رخصة مفهرس برقم {id}");

            return Ok(detainedLicense);
        }

        [HttpGet("Get-By-LicenseID")]

        public IActionResult GetByLicenseID(int id)
        {
            DetainedLicense detainedLicense = DetainedLicense.FindByLicenseID(id);

            if (detainedLicense == null) return NotFound($"ليس هناك حجز للرخصة المفهرسة برقم  {id}");

            return Ok(detainedLicense);
        }
    }
}
