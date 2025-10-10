using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using System.Data;
using Newtonsoft.Json;
using NLog.Targets;
using DLMS_DTO;
using Microsoft.AspNetCore.Authorization;
namespace DLMS_SERVER.Controllers;

[Route("api/[controller]")]
[ApiController]


public class ApplicationTypeController : ControllerBase
{
  
    [HttpGet("Get Types")] public IActionResult GetAll() 
    {
        DataTable types = ApplicationType.GetAllApplicationTypes();

        if (types == null || types.Rows.Count == 0) return NotFound("لا يوجد خدمات  في النظام");



        string jsonText = JsonConvert.SerializeObject(types);

        return Ok(jsonText);
    }

    [HttpGet("Find By ID")] public IActionResult GetByID(int id) 
    {

        if (id <= 0) return BadRequest($"Invalid ID {id}");

		if (id < 0 || id > 7) return StatusCode(403, "لا يمكن إتمام هذا الإجراء");


		ApplicationType type = ApplicationType.Find(id);

        if (type == null) return NotFound("هذه الخدمة غير موجودة");

        return Ok(type);
    }


    [HttpGet("Find By Name")]
    public IActionResult GetByName(string name)
    {

        ApplicationType type = ApplicationType.Find(name);

        

        if (type == null) return NotFound("هذه الخدمة غير موجودة");

		if (type.ID > 7) return StatusCode(403, "لا يمكن عرض البيانات");


		return Ok(type);
    }




    private void CopyObjectData(ApplicationType oldData, ApplicationType newData)
    {
        oldData.Type = newData.Type;
        oldData.Fee = newData.Fee;
    }

    [Authorize(Roles = "مدير النظام")]
    [HttpPut("{id}/edit")]
    public IActionResult EditTestType(int id, [FromBody] ApplicationType type)
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتعرف النظام على المستخدم");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "مدير النظام من له الصلاحية التامة على هذا الإجراء");
        }


        if (type == null) return BadRequest("لا يجب أن يتم إرسال البيانات فارغة");

        if (id < 0 || id > 7) return StatusCode(403, "لا يمكن إتمام هذا الإجراء");

        if (type.Fee < 2) return StatusCode(409, "لا يجب ان تقل الرسوم المحددة للخدمة عن دينارين");

        var oldData = ApplicationType.Find(id);



        if (type.Fee == oldData.Fee && type.Type == oldData.Type)
            return StatusCode(409, "لم تقم بأي تعديل حتى يتم الحفظ");


        if (oldData == null) return NotFound($"ليس هناك نوع خدمة مفهرسة بالرقم {id}");

        CopyObjectData(oldData, type);

        if (!oldData.Save()) return StatusCode(500, "حدث خطأ في الخادم لم تتم العملية بنجاح");

        return Ok("تمت عملية التعديل بنجاح");
    }

}
