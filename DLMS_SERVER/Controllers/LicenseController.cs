using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using DLMS_BusinessLogicLayer;
using Newtonsoft.Json;
using System.Data;
using Microsoft.AspNetCore.Authorization;

namespace DLMS_SERVER.Controllers;


[Authorize(Roles = "مدير النظام,مستخدم النظام,شرطي مرور")]
[Route("api/[controller]")]
[ApiController]
public class LicenseController : ControllerBase
{



    [HttpGet("Find")] public IActionResult GetByID(int licenseID)
    {
        var license = License.Find(licenseID);

        if (license == null) return NotFound($"ليس هناك رخصة مفهرسة برقم {licenseID} مسجلة في النظام");

        return Ok(license);
    }


    [HttpGet("Find-By-Application-ID")]
    public IActionResult GetByAppID(int appID)
    {
        var license = License.FindByApplicationID(appID);

        if (license == null) return NotFound($"ليس هناك رخصة مفهرسة برقم {appID} مسجلة في النظام");

        return Ok(license);
    }
    public class clsDetainLicense
    {
        //public int ByUserID { get; set; }

        public decimal FineFees { get; set; }

        public string? notes { get; set; }
    }

    [HttpPut("{licenseID}/DetainLicense")]
    public IActionResult DetainLicense(int licenseID, [FromBody] clsDetainLicense detained)
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتم التعرف على المستخدم من قبل الخادم");


        var user = DLMS_BusinessLogicLayer.User.Find(User.Identity.Name);

        if (user is null) return StatusCode(500, "خطأ في الخادم فشل في جلب معلومات المستخدم");

        License license = License.Find(licenseID);


		if (detained == null) return BadRequest("لا يجب أن يتم إرسال البيانات فارغة");

        if (detained.FineFees < 20) return StatusCode(409,
            "ليس هناك غرامة مرورية يمكن أن تكون قيمتها أقل من 20 دينارا");


		if (!license.IsActiveLicense()) return StatusCode(409, "يجب أن تكون الرخصة فعالة ليتم حجزها");

        if (license == null) return NotFound($"ليس هناك رخصة مفهرسة برقم {licenseID}");

        if (!license.DetainLicense(user.ID, detained.FineFees, detained.notes))
            return StatusCode(500, "فشلت عملية حجز الترخيص");

        return Ok("تمت عملية الحجز بنجاح");
    }

}
