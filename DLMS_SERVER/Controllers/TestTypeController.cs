using DLMS_BusinessLogicLayer;
using DLMS_DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace DLMS_SERVER.Controllers
{

    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [Route("api/[controller]")]
    [ApiController]
    public class TestTypeController : ControllerBase
    {


        private void CopyObjectData(TestType oldData, TestTypeDto newData) 
        {
            oldData.Description = newData.Description;  
            oldData.Fees = newData.Fees;
        }

        [Authorize(Roles = "مدير النظام")]
        [HttpPut("{id}/edit")]
        public IActionResult EditTestType(int id, [FromBody] TestTypeDto test) 
        {
            if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتعرف النظام على المستخدم");

            if (!User.IsInRole("مدير النظام")) 
            {
                return StatusCode(403, "مدير النظام من له الصلاحية التامة على هذا الإجراء");
            }

            if (test == null) return BadRequest("لا يجب أن يتم إرسال البيانات فارغة");


            if (test.Fees < 2) return StatusCode(409, "لا يجب ان تقل الرسوم المحددة للإختبار عن دينارين");

            var oldData = TestType.Find(id);



            if (test.Fees == oldData.Fees && test.Description == oldData.Description) 
                return StatusCode(409, "لم تقم بأي تعديل حتى يتم الحفظ");


            if (oldData == null) return  NotFound($"ليس هناك نوع إختبار مفهرس بالرقم {id}");

            CopyObjectData(oldData, test);

            if (!oldData.Save()) return StatusCode(500, "حدث خطأ في الخادم لم تتم العملية بنجاح");

            return Ok("تمت عملية التعديل بنجاح");
        }



        [HttpGet("{id}")]
        public IActionResult GetTestTypeByID(int id) 
        {
            TestType type = TestType.Find(id);

            if (type == null) return NotFound($"ليس هناك نوع إختبار مفهرس بالرقم {id}");

            return Ok(type);    
        }

        [HttpGet("Types")]
        public IActionResult GetTypes()
        {
            var list = TestType.GetAllTypes();

            if (list == null || list.Count == 0) return NotFound("ليس هناك أنواع إختبار مسجلة في النظام");

            return Ok(list);
        }

        [HttpGet("Types-Details")]
        public IActionResult GetTestTypes()
        {
            var list = TestType.GetAllTestTypes();

            if (list == null || list.Rows.Count == 0) return NotFound("ليس هناك أنواع إختبار مسجلة في النظام");

            string jsonText = JsonConvert.SerializeObject(list);

            return Ok(jsonText);
        }
    }
}
