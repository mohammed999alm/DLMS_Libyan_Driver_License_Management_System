using DLMS_BusinessLogicLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DLMS_SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InternationalLicenseController : ControllerBase
    {

        [HttpGet("{id}")] public IActionResult GetByID(int id) 
        {
            var license = InternationalLicense.Find(id);

            if (license == null) return NotFound($"ليس هناك رخصة دولية مفهرسة بالرقم {id}");

            return Ok(license); 
        }
    }
}
