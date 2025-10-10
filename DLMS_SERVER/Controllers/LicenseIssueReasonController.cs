using DLMS_BusinessLogicLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace DLMS_SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LicenseIssueReasonController : ControllerBase
    {

        [HttpGet("Get All")]
        public IActionResult GetAll()
        {
            List<string> issueReasons = LicenseIssueReason.GetAll();

            if (issueReasons == null || issueReasons.Count <= 0) return NotFound("ليس هناك اي بيانات مسجلة عن أسباب استخراج الرخص في النظام");

            return Ok(issueReasons);
        }

        [HttpGet("Find Licence Issue Reason By ID")]
        public IActionResult GetLicenseClass(int issueTypeID)
        {
            var obj = LicenseIssueReason.Find(issueTypeID);

            if (obj == null) return NotFound($"ليس هناك نوع من سبب من أسباب استخراج التراخيص في النظام مفهرس برقم {issueTypeID}");

            return Ok(obj);
        }



        [HttpGet("Find Licence Class By Name")]
        public IActionResult GetLicenseClass(string issueType)
        {
            var obj = LicenseIssueReason.Find(issueType);

            if (obj == null) return NotFound($"ليس هناك نوع من سبب من أسباب استخراج التراخيص في النظام بهذا الإسم   {issueType}");

            return Ok(obj);
        }
    }
}
