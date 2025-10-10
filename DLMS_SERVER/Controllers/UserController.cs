using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using System.Data;
using Newtonsoft.Json;
using Azure.Identity;
using Newtonsoft.Json.Schema;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Authorization;
using GlobalUtility;
using System.Security.Claims;

namespace DLMS_SERVER.Controllers;


[Authorize(Roles = "مدير النظام,مستخدم النظام,شرطي مرور,مدير النظام إعادة تعيين كلمة المرور")]
[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{

    [Authorize(Roles = "مدير النظام")]

    [HttpGet("all")] public IActionResult GetAll() 
    {
        DataTable users = DLMS_BusinessLogicLayer.User.GetAllUsers();

        if (users == null) return StatusCode(404, "ليس هناك مستخدمين في النظام");

        string jsonUsers = JsonConvert.SerializeObject(users);  

        return Ok(jsonUsers);
    }


    [Authorize(Roles = "مدير النظام")]
    [HttpGet("with no history/all")]
    public IActionResult GetAllWithNoHistory()
    {
        DataTable users = DLMS_BusinessLogicLayer.User.GetUsersWithNoHistory();


        if (users == null || users.Rows.Count < 1) return StatusCode(404, "ليس هناك مستخدمين لم يقوموا بأي نشاط في النظام");

        string jsonUsers = JsonConvert.SerializeObject(users);

        return Ok(jsonUsers);
    }


    [Authorize(Roles = "مدير النظام")]
    [HttpDelete("Delete/{id}")] public IActionResult DeleteUserWithNoHistory(int id) 
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتم التعرف على المستخدم من قبل النظام");

        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == (object)id) 
        {
            return StatusCode(409, "لا يمكن للمستخدم أن يحذف نفسه");
        }

        if (!DLMS_BusinessLogicLayer.User.IsExist(id)) return NotFound("المستخدم غير مسجل في النظام");


        if (!DLMS_BusinessLogicLayer.User.IsExistWithNoHistoryUser(id)) return BadRequest("لا يمكنك حذف مستخدم قام ببعض العمليات في النظام");

        if (DLMS_BusinessLogicLayer.User.DeleteUser(id))
            return Ok("تم حذف المستخدم بنجاح");

        return StatusCode(500, "فشلت عملية حذف المستخدم"); 
    }



   
    [Authorize(Roles = "مدير النظام")]
    [HttpPost("Add")] public IActionResult AddNewUser(User user) 
    {
        if (user == null) return BadRequest( "يجب عليك أن لا ترسل بيانات فارغة عن المستخدم الذي تريد إضافته");

		if (DLMS_BusinessLogicLayer.User.IsExist(user.Username))
			return StatusCode(409, $"اسم المستخدم مستخدم مسبقا في النظام الرجاء ادخال اسم غير مستخدم فيما سبق");

		if (DLMS_BusinessLogicLayer.User.IsExistByPersonID(user.PersonID))
            return StatusCode(409, $"الشخص المفهرس بالرقم {user.PersonID} هو مُسًتَخًدِمً مسجل فيما سبق");

        user.Password = GlobalUtility.PasswordHasher.HashPassword(user.Password);

        if (!user.Save())
            return StatusCode(500, "فشلت عملية إضافة المستخدم");


        return CreatedAtAction(nameof(GetUser), new { ID = user.ID}, user);
    }




    private void _CopyUserObject(DLMS_BusinessLogicLayer.User oldData, DLMS_BusinessLogicLayer.User latestData)
    {
        if (oldData == null) return;


        latestData.Password = PasswordHasher.HashPassword(latestData.Password);

        oldData.UserRole = latestData.UserRole;
        oldData.ActiveStatus = latestData.ActiveStatus; 
        oldData.Password = latestData.Password;
    }


    [Authorize(Roles = "مدير النظام")]
    [HttpPut("{id}/Update")] public IActionResult UpdateUser(int id, User user) 
    {

        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتم التعرف على المستخدم من قبل النظام");    

        
        if (user == null) 
            return BadRequest(StatusCode(415, "يجب عليك أن لا ترسل بيانات فارغة عن المستخدم الذي تريد تعديله"));   

         
        if (!DLMS_BusinessLogicLayer.User.IsExist(id))
            return NotFound($"غير مسجل في النظام {id} هذا المستخدم الذي يحمل المعرف ");

        if (User.Identity.Name == user.Username) return StatusCode(409, "لا يسمح للمستخدم أن يقوم بالتعديل على بياناته الخاصة");

         var existingUser = DLMS_BusinessLogicLayer.User.Find(id);

        if (existingUser == null) return BadRequest("هناك خطأ في جلب بيانات المستخدم");

        _CopyUserObject(existingUser, user);

        if (!existingUser.Save())
            return StatusCode(500, "لم تتم عملية التعديل على المستخدم");

        return Ok("تمت عملية التعديل على المستخدم بنجاح");    
    }


    private IActionResult ValidateUserModification(int id, User user, ref User? existingUser) 
    {
        if (user == null)
            return BadRequest(StatusCode(415, "يجب عليك أن لا ترسل بيانات فارغة عن المستخدم الذي تريد تعديله"));

        if (!DLMS_BusinessLogicLayer.User.IsExist(id))
            return NotFound($"غير مسجل في النظام {id} هذا المستخدم الذي يحمل المعرف ");

        existingUser = DLMS_BusinessLogicLayer.User.Find(id);

        if (existingUser == null) return BadRequest("هناك خطأ في جلب بيانات المستخدم");

        return Ok();

    }



    

    [HttpPut("{id}/Change-Password")]
    public IActionResult ChangePassword(int id, [FromBody] string newPassword)
    {

        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتم التعرف على المستخدم من قبل النظام");

        var user = DLMS_BusinessLogicLayer.User.Find(id);

        if (user is null) return NotFound($"غير مسجل في النظام {id} هذا المستخدم الذي يحمل المعرف ");

        if (user.Username.ToLower() != User.Identity.Name.ToLower()) return StatusCode(409, "لا يمكن للمستخدم أن يقوم بتغيير رمز سري يخص حسابا أخر");

        if (user.Password == newPassword || PasswordHasher.VerifyPassword(newPassword, user.Password))
            return StatusCode(409, "لم تقم بتغيير كلمة المرور إن أردت تغيير كلمة المرور فعليك بإدخال كلمة مرور جديدة");

        user.Password = newPassword;

        user.Password = PasswordHasher.HashPassword(user.Password);


        if (!user.Save())
            return StatusCode(500, "لم تتم عملية التعديل على المستخدم");

        return Ok("تمت عملية تغيير كلمة المرور بنجاح");
    }


    [Authorize(Roles = "مدير النظام إعادة تعيين كلمة المرور")]
    [HttpPut("{id}/Reset-Password")]
    public IActionResult ResetPassword(int id, [FromBody] string newPassword)
    {

        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "لم يتم التعرف على المستخدم من قبل النظام");

        var user = DLMS_BusinessLogicLayer.User.Find(id);

        if (user is null) return NotFound($"غير مسجل في النظام {id} هذا المستخدم الذي يحمل المعرف ");

        if (user.Username.ToLower() != User.Identity.Name.ToLower()) return StatusCode(409, "لا يمكن للمستخدم أن يقوم بتغيير رمز سري يخص حسابا أخر");

        if (user.Password == newPassword || PasswordHasher.VerifyPassword(newPassword, user.Password))
            return StatusCode(409, "لم تقم بتغيير كلمة المرور إن أردت تغيير كلمة المرور فعليك بإدخال كلمة مرور جديدة");


        user.Password = newPassword;

        user.Password = PasswordHasher.HashPassword(user.Password);


        if (!user.Save())
            return StatusCode(500, "لم تتم عملية التعديل على المستخدم");

        return Ok("تمت عملية تغيير كلمة المرور بنجاح");
    }

    public class LoginRequest
    {
        public string UserName { get; set; }    
        public string Password { get; set; }
    }

    //[HttpPost("Login")]
    //public IActionResult Login([FromBody] LoginRequest request)
    //{
    //    var user = DLMS_BusinessLogicLayer.User.Find(request.UserName, request.Password);

    //    if (user == null) return BadRequest("إسم المستخدم أو كلمة المرور غير صحيحة");


    //    return Ok(user);
    //}



    [Authorize(Roles = "مدير النظام")]
    [HttpGet("{id}")]
    public IActionResult GetUser(int id)
    { 
        var user = DLMS_BusinessLogicLayer.User.Find(id);
        
        if (user == null) return BadRequest($"لا يوجد نتائج");
        

        return Ok(user);
    }


    [Authorize(Roles = "مدير النظام")]
    [HttpGet("ByPersonID/{id}")]
    public IActionResult GetUserByPerson(int id)
    {
        var user = DLMS_BusinessLogicLayer.User.FindByPersonID(id);

        if (user == null) return BadRequest($"لا يوجد نتائج");


        return Ok(user);
    }



    [Authorize(Roles = "مدير النظام,مستخدم النظام,شرطي مرور")]
    [HttpGet("Find-By-Username/{username}")]
    public IActionResult GetUserByUsername(string username)
    {
        var user = DLMS_BusinessLogicLayer.User.Find(username);

        if (user == null) return BadRequest($"لا يوجد نتائج");


        return Ok(user);
    }


    [Authorize(Roles = "مدير النظام")]
    [HttpGet("Exist/userId/{id}")]
    public IActionResult IsExistByID(int id)
    {
        if (DLMS_BusinessLogicLayer.User.IsExist(id))
            return Ok("هذا المستخدم موجود");
        else
            return NotFound("هذا المستخدم غير موجود");
    }


    [Authorize(Roles = "مدير النظام")]
    [HttpGet("Exist/personId/{id}")]
    public IActionResult IsExistByPersonID(int id)
    {
        if (DLMS_BusinessLogicLayer.User.IsExistByPersonID(id))
            return Ok("هذا المستخدم موجود");
        else
            return NotFound("هذا المستخدم غير موجود");
    }



    [Authorize(Roles = "مدير النظام")]
    [HttpGet("Exist/Username/{username}")]
    public IActionResult IsExist(string username)
    {
        if (DLMS_BusinessLogicLayer.User.IsExist(username))
            return Ok("هذا المستخدم موجود");
        else
            return NotFound("هذا المستخدم غير موجود");
    }
}
