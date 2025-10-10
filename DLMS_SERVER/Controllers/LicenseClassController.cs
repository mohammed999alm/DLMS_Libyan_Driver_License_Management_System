using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using System.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;
using System.Reflection;

namespace DLMS_SERVER.Controllers;

[Authorize(Roles = "مدير النظام,مستخدم النظام")]
[Route("api/[controller]")]
[ApiController]
public class LicenseClassController : ControllerBase
{

    [AllowAnonymous]
    [HttpGet("Get All")] public IActionResult GetAll() 
    {
        DataTable dt = LicenseClass.GetAll();

        if (dt == null || dt.Rows.Count <= 0) return NotFound("ليس هناك اي بيانات مسجلة عن نوع الرخص في النظام");

        string jsonText = JsonConvert.SerializeObject(dt);

        return Ok(jsonText);
    }

    [AllowAnonymous]
    [HttpGet("Get All2")] public IActionResult GetAll2() 
    {
        List<string> classes = LicenseClass.GetAllClasses();

        if (classes == null || classes.Count <= 0) return NotFound("ليس هناك اي بيانات مسجلة عن نوع الرخص في النظام");

        return Ok(classes); 
    }


    [AllowAnonymous]

    [HttpGet("Get All With Details")]
    public IActionResult GetAllWithDetails()
    {
        List<DLMS_DTO.LicenseClassDto> classes = LicenseClass.GetAllWithDetails();

        if (classes == null || classes.Count <= 0) return NotFound("ليس هناك اي بيانات مسجلة عن نوع الرخص في النظام");

        return Ok(classes);
    }

    [AllowAnonymous]
    [HttpGet("{licenseClassID}")] public IActionResult GetLicenseClass(int licenseClassID) 
    {
        var obj = LicenseClass.Find(licenseClassID);

        if (obj == null) return NotFound($"ليس هناك نوع من أنواع الترخيص في النظام مفهرس برقم {licenseClassID}");

        return Ok(obj);
    }



    [HttpGet("Find Licence Class By Name")]
    public IActionResult GetLicenseClass(string classType)
    {
        var obj = LicenseClass.Find(classType);

        if (obj == null) return NotFound($"ليس هناك نوع من أنواع الترخيص في النظام مفهرس برقم {classType}");

        return Ok(obj);
    }



    private void CopyObjectData(LicenseClass oldData, LicenseClass newData)
    {
        oldData.Description = newData.Description;
        oldData.Fees = newData.Fees;
        oldData.ValidatyLength = newData.ValidatyLength;
        oldData.Age = newData.Age;
    }

 

    [Authorize(Roles = "مدير النظام")]
    [HttpPut("{id}/edit")]
    public IActionResult EditTestType(int id, [FromBody] LicenseClass type)
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتعرف النظام على المستخدم");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "مدير النظام من له الصلاحية التامة على هذا الإجراء");
        }

        if (type == null) return BadRequest("لا يجب أن يتم إرسال البيانات فارغة");


        if (type.Fees < 5) return StatusCode(409, "لا يجب ان تقل الرسوم المحددة للرخصة عن 5 دنانير");

        if (type.Age < 18) return StatusCode(409, "لا يجب أن يقل العمر المخصص لحاملي الرخصة عن 18 العام");

		if (type.Age > 35) return StatusCode(409, "لا يجب أن يزيد العمر المخصص لحاملي الرخصة عن 35 عامََا");


		//if (type.Age > 35) return StatusCode(409, "لا يجب أن يزيد العمر المخصص لحاملي الرخصة عن 18 العام");


		if (type.ValidatyLength < 2) return StatusCode(409, "لا يجب أن تقل المدة المحددة لصلاحية فئة الرخصة عن عامين");

		if (type.ValidatyLength > 10) return StatusCode(409, "لا يجب أن تزيد المدة المحددة لصلاحية فئة الرخصة عن 10 سنوات");



		var oldData = LicenseClass.Find(id);

        if (oldData == null) return NotFound($"ليس هناك نوع فئة تراخيص مفهرسة بالرقم {id}");

        if (oldData.Equals(type)) 
            return StatusCode(409, "لم تقم بأي تعديل حتى يتم الحفظ");



        CopyObjectData(oldData, type);

        if (!oldData.Save()) return StatusCode(500, "حدث خطأ في الخادم لم تتم العملية بنجاح");

        return Ok("تمت عملية التعديل بنجاح");
    }
}
