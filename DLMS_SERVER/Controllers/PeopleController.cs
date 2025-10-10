using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using System.Data;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http.Metadata;
using System.Linq.Expressions;
using System;
using System.IO;
using System.Security.Claims;
using NuGet.Protocol;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.AspNetCore.Authorization;
using GlobalUtility;
using DLMS_SERVER.ValidatorClasses;
using DLMS_SERVER.Extensions;



namespace DLMS_SERVER.Controllers;

[Authorize(Roles = "مدير النظام,مستخدم النظام,شرطي مرور")]
[Route("api/[controller]")]
[ApiController]
public class PeopleController : ControllerBase
{

    private readonly string  _imageFolderPath = "www.root/Images";


    [HttpGet("ID/{id}")] public IActionResult GetPersonByID(int id)
    {
        Person person = Person.Find(id);

        if (person == null)
			return NotFound(" هذا الشخص الذي يحمل معرفا" + id + " غير مسجل في النظام");

		return Ok(person);
    }


	[HttpGet("NationalNumber/{nationalNumber}")]
	public IActionResult GetPersonByID(string nationalNumber)
	{
		Person person = Person.Find(nationalNumber);

		if (person == null)
			return NotFound(" هذا الشخص الذي يحمل رقما وطنيا" + nationalNumber + " غير مسجل في النظام");

		return Ok(person);
	}

	[AllowAnonymous]
    [HttpGet("{personID}/ProfilePicture")]
    public IActionResult GetPersonProfile(int personID)
    {
        if (!Person.IsExist(personID)) return NotFound(" هذا الشخص الذي يحمل معرفا" + personID + " غير مسجل في النظام");
        Person person = Person.Find(personID);

        if (person.ImagePath == null) return NotFound("هذا الشخص ليس لديه صورة شخصية");

        string filePath = Path.Combine(_imageFolderPath, person.NationalNumber, person.ImagePath);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("لم يتم العثور على الصورة الشخصية");
        }

        var image = System.IO.File.OpenRead(filePath);
        return File(image, "image/jpeg");
    }


    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [HttpGet("All")] public IActionResult GetAll(int id)
    {
        DataTable people = Person.GetPeople();

        if (people == null)
            return NotFound("ليس هناك بيانات مسجلة عن الأشخاص في النظام");


        string jsonText = JsonConvert.SerializeObject(people);

        return Ok(jsonText);
    }


    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [HttpPost("Add")] public IActionResult AddPerson(Person person)
    {
        if (person == null)
            return BadRequest("لا يمكن ان تكون بيانات الشخص فارغة");

        if (Person.IsExist(person.NationalNumber))
            return StatusCode(409, "الرقم الوطني موجود مسبقا الرجاء إدخال رقم وطني صالح");

        if (!Validator.IsNationalNumberValid(person.NationalNumber, person.Gender, person.DateOfBirth.Year.ToString()))
            return StatusCode(409,
                "الرقم الوطني يجب ان يتكون من 12 الرقم على ان يحتوي على سنة الميلاد ويبدأ برقم 1 للذكر و 2 للأنثى");

        if (!person.Save())
            return StatusCode(500, "حدث خطأ أثناء عملية الإضافة");

        return CreatedAtAction(nameof(GetPersonByID), new { ID = person.ID }, person);
    }


    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [HttpPost("Upload/Image/{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePicture(IFormFile imageFile, int id)
    {
        Person person = Person.Find(id);

        if (person == null) return NotFound($"الشخص المفهرس بالرقم ({id}) غير موجود في النظام");

        if (person.ImagePath == null) return NotFound("امتداد الصورة ورابطها غير موجود");

        if (System.IO.File.Exists(Path.Combine(_imageFolderPath, person.NationalNumber, person.ImagePath)))
            return NotFound($"هذه الصورة مخزنة  مسبقا ضمن ملف هذا الشخص المقيد بالرقم الوطني {person.NationalNumber}");

        string filePath = await UploadImage(imageFile, person.NationalNumber, person.ImagePath);

        if (filePath.Contains("Success"))
            return Ok("تم رفع الصورة بنجاح");
        if (string.IsNullOrEmpty(filePath))
            return NotFound("لم يتم إرسال الصورة");

        else 
        {
            return StatusCode(500,"حدث خطأ أثناء عملية الإضافة");
        }
    }

   
    private async Task<string> UploadImage(IFormFile imageFile, string folder, string fileName) 
    {

        try
        {
            string oldFile = null;

            if (imageFile != null && imageFile.Length > 0)
            {
                string folderPath = Path.Combine(_imageFolderPath, folder);

                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
                else 
                {
                    foreach (var file in Directory.GetFiles(folderPath)) 
                    {
                        oldFile = file;
                    }
                }

                string filePath = Path.Combine(folderPath, fileName);

                using (Stream stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                if (oldFile != null)
                   System.IO.File.Delete(oldFile);

                return $"Success/{filePath}";
            }

            return null;

        }

        catch (Exception ex)
        {
            return $"An Exception Occured : {ex.Message}";
        }
    }


    private bool DeleteProfileImageDrictory(string folderPath) 
    {
        if (Directory.Exists(folderPath))
        {
            Directory.Delete(folderPath, true);

            return true;
        }

        return false;
    }



    //[HttpPost("Add2")]
    ////[Consumes("multipart/form-data")] // Corrected Content-Type header

    //public async Task<IActionResult> AddPerson2(Person person, IFormFile profilePicture)
    //{
    //    if (person == null)
    //        return BadRequest("لا يمكن ان تكون بيانات الشخص فارغة");

    //    string filePath = await UploadImage(profilePicture, person.NationalNumber, person.ImagePath);




    //    if (!person.Save())
    //    {
    //        if (filePath != null && filePath.Contains("Success")) 
    //            DeleteProfileImageDrictory(Path.Combine(_imageFolderPath, person.NationalNumber));
            
    //        return StatusCode(500, "حدث خطأ أثناء عملية الإضافة");
    //    }

    //    return CreatedAtAction(nameof(GetPersonByID), new { ID = person.ID }, person);
    //}


    private void copyObject(Person oldData, Person newData)
    {
        oldData.NationalNumber = newData.NationalNumber;
        oldData.NationalityID = newData.NationalityID;
        oldData.FirstName = newData.FirstName;
        oldData.SecondName = newData.SecondName;
        oldData.ThirdName = newData.ThirdName;
        oldData.LastName = newData.LastName;
        oldData.Address = newData.Address;
        oldData.DateOfBirth = newData.DateOfBirth;
        oldData.Gender = newData.Gender;
        oldData.ImagePath = newData.ImagePath;
        oldData.MunicipalityID = newData.MunicipalityID;
    }

    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [HttpPut("Update/{id}")] public IActionResult UpdatePerson(int id, Person person)
    {
        int userID = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userID) ? userID : -1;

        DLMS_BusinessLogicLayer.User user = DLMS_BusinessLogicLayer.User.Find(userID);

        if (user == null) return StatusCode(409, "بيانات هذا المستخدم غير صالحة للقيام بالعملية");

        var userPerformend = DLMS_BusinessLogicLayer.User.FindByPersonID(id);

        if (userPerformend != null)
        {
            if (!User.IsInRole("مدير النظام")) return StatusCode(403, "ليس لديك صلاحية لهذا الإجراء");
        }


        if (!Validator.IsNationalNumberValid(person.NationalNumber, person.Gender, person.DateOfBirth.Year.ToString()))
            return StatusCode(409,
                "الرقم الوطني يجب ان يتكون من 12 الرقم على ان يحتوي على سنة الميلاد ويبدأ برقم 1 للذكر و 2 للأنثى");

        Person personObj = Person.Find(id);

        Person oldData = Person.Find(id);

        if (personObj == null)
            return NotFound($"الشخص المفهرس بالرقم {id} غير موجود");

        string imagePath = personObj.ImagePath;

        if (personObj.Equals(person)) 
        {
            return StatusCode(409, "لم تقم بأي تعديل حتى تتم عملية الحفظ");
        }
        


        copyObject(personObj, person);

        if (string.IsNullOrEmpty(personObj.ImagePath) && !string.IsNullOrWhiteSpace(imagePath)) 
        {
            string filePath = Path.Combine(_imageFolderPath, personObj.NationalNumber, Path.GetFileName(imagePath));

            if (System.IO.File.Exists(filePath))  
            {
                System.IO.File.Delete(filePath);
            }
        } 
        if (!personObj.Save())
            return StatusCode(500, "حدث خطأ أثناء عملية التعديل");

        LoggerUtil.LogTransaction(User?.Identity?.Name, nameof(Person), oldData, person, nameof(PeopleController), nameof(UpdatePerson));

        return Ok("تمت العملية بنجاح");
    }


    //Only Admin
    [HttpDelete("Delete/{id}")]
    public IActionResult DeletePerson(int id)
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "ليس لديك الصلاحية لإتمام هذه العملية لعدم تعرف النظام عليك");

        if (!User.IsInRole("مدير النظام")) 
        {
            return StatusCode(403, "لا يمكن لغير مدير النظام إتمام هذه العملية");
        }
        
        var user = DLMS_BusinessLogicLayer.User.Find(User.Identity.Name);

        if (user == null) return StatusCode(500, "حدث خطأ في الخادم لم يتم العثور على بيانات المستخدم القائم على العملية");

        if (user.PersonID == id) return StatusCode(409, "لا يمكن للمستخدم القيام بهذا الإجراء الذي من شأنه أن يقوم بحذف بياناته الشخصية");
        
        if (!Person.IsExist(id)) 
        {
            return NotFound($"ليس هناك شخص مفهرس في النظام بالرقم {id}");
        }
        if (Driver.IsExistByPersonID(id) || DLMS_BusinessLogicLayer.User.IsExistByPersonID(id)) 
        {
            return StatusCode(409,"لا يمكنك حذف بيانات هذا الشخص");
        }

        Person person = Person.Find(id);

        if (Person.DeletePerson(id))
        {
            LoggerUtil.LogTransaction(User.Identity.Name, "Person", person, new
            {
                DeletedBy = user.PersonID,
                DeletedPersonID = id,
                Timestamp = DateTime.UtcNow
            }, nameof(PeopleController), nameof(DeletePerson));
            return Ok("تم حذف الشخص من النظام");

        }
        else 
        {
            return StatusCode(500, "حدث خطأ أثناء عملية الحذف");
        }
    }
    

    [HttpGet("{id}/Contact Info/")] public IActionResult GetContacts(int id)
    {
        DataTable contacts = Person.Find(id)?.GetContactList();

        if (contacts == null)
            return NotFound($"لم يتم العثور على جهات الإتصال للشخص المهرس بالرقم {(id)}");

        string jsonText = JsonConvert.SerializeObject(contacts);

        return Ok(jsonText);
    }

    public class Contact
    {
        public string? phone;
        public string? email;
    }


    private IActionResult ValidateContacts(Contact contact) 
    {

        if (
            (string.IsNullOrEmpty(contact.phone) || string.IsNullOrWhiteSpace(contact.phone))
            &&
            (string.IsNullOrEmpty(contact.email) || string.IsNullOrWhiteSpace(contact.email))

            )
        {
            return BadRequest(" لم يتم إرسال أي بيانات تخص رقم الهاتف أو البريد الإلكتروني");
        }

        if (!string.IsNullOrEmpty(contact.phone) && !string.IsNullOrWhiteSpace(contact.phone))
        {
            if (!Validator.IsPhoneNumberValid(contact.phone))
                return StatusCode(409, "رقم الهاتف الذي ادخلته غير صالح");

            if (Phone.IsExist(contact.phone))
                return StatusCode(409, "هذا الرقم مسجل مسبقا داخل النظام");
        }
        if (!string.IsNullOrEmpty(contact.email) && !string.IsNullOrWhiteSpace(contact.email))
        {
            if (!Validator.IsEmailAddressValid(contact.email))
                return StatusCode(409, "البريد الإلكتروني الذي أدخلته ليس بريدا إلكترونيا صالحا");

            if (Phone.IsExist(contact.email))
                return StatusCode(409, "هذا الرقم مسجل مسبقا داخل النظام");
        }

        return Ok();
    }

    
    [HttpPost("{id}/Create-Contact/")] public IActionResult AddContact(int id, [FromBody] Contact contact)
    {

        Person person = Person.Find(id);

        if (person == null) return NotFound($"المستخدم مع الرقم المسجل {id} غير موجود");


        if (contact == null)
            return BadRequest("لا ينبغي أن تكون كل الحقول فارغة");

        //IActionResult result = ValidateContacts(contact);


        //if (result is not OkResult) 
        //{
        //    return result;
        //}

        ValidateResult result = ContactValidator.ValidateContacts(contact.phone, contact.email);

        if (!result.IsValid)
            return StatusCode(result.StatusCode.DefaultOrValue(), result.ErrorMessage);


        if (!person.CreateNewContact(contact.phone, contact.email))
            return StatusCode(500, "حدث خطأ أثناء عملية الإضافة");


        return Ok("تمت عملية الإضافة بنجاح");
    }


    [HttpPut("{id}/Update Contact/{oldValue}")] public IActionResult UpdateContact(int id, string oldValue, [FromBody] Contact contact)
    {
        Person person = Person.Find(id);

        if (person == null) return NotFound($"المستخدم مع الرقم المسجل {id} غير موجود");

        if (contact == null)
        {
            return BadRequest("لا ينبغي أن تكون الحقول فارغة");
        }

        //IActionResult result = ValidateContacts(contact);

        //if (result is not OkResult) 
        //{
        //    return result;
        //}

        ValidateResult result = ContactValidator.ValidateContacts(contact.phone, contact.email);

        if (!result.IsValid)
        {
            return StatusCode(result.StatusCode.GetValueOrDefault(409), result.ErrorMessage);
        }

        if (!person.UpdateContact(oldValue, contact.phone, contact.email))
        {
            return StatusCode(500, "حدث خطأ أثناء عملية التعديل");
        }

        return Ok("تمت العملية بنجاح");
    }


    [HttpDelete("{id}/Delete Contact/{CaptionName}/{record}")] public  IActionResult DeleteContact(int id, string CaptionName, string record) 
    {
        Person person = Person.Find(id);

        if (person == null) return NotFound($"المستخدم مع الرقم المسجل {id} غير موجود");

        if (!person.DeleteContact(CaptionName, record)) 
        {
            return StatusCode(500, "حدث خطأ أثناء عملية الحذف");
        }

        return Ok("تمت عملية الحذف بنجاح");
    }

    
}
