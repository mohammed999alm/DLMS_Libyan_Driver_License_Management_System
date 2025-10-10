using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using DLMS_BusinessLogicLayer;
using Newtonsoft.Json;
using DLMS_DTO;
using Microsoft.Identity.Client;
using System.Diagnostics.CodeAnalysis;
using Humanizer.Localisation.TimeToClockNotation;
using System.Text.Encodings;
using GlobalUtility;
using Microsoft.AspNetCore.Authorization;
//using System.Text;

namespace DLMS_SERVER.Controllers;

[Authorize(Roles ="مدير النظام,مستخدم النظام")]
[Route("api/[controller]")]
[ApiController]
public class ApplicationController : ControllerBase
{


    private IActionResult _DeleteRenewApp(Application app) 
    {
        var lApp = LocalLicenseApplication.FindByApplicationID(app.ID);
        
        if (LocalLicenseApplication.Delete(lApp.LocalAppId))
        {

            LoggerUtil.LogTransaction(User.Identity.Name, "Person", lApp, new
            {
                DeletedBy = User.Identity.Name,
                DeletedPersonID = lApp.ID,
                Timestamp = DateTime.UtcNow
            }, nameof(PeopleController), nameof(DeleteApplication));

            return Ok("تم حذف الطلب من النظام");
        }

        return StatusCode(500, "حدث خطأ أثناء عملية الحذف");
    }

    [HttpDelete("{id}/Delete")]
    public IActionResult DeleteApplication(int id)
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "ليس لديك الصلاحية لإتمام هذه العملية لعدم تعرف النظام عليك");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "لا يمكن لغير مدير النظام إتمام هذه العملية");
        }

        var app = Application.Find(id);

        if (app == null) return NotFound($"لم يتم العثور على الطلب المقيد بالرقم المفهرس  {id}");

        if (!Application.CouldBeDeletedAppID(app.ID))
            return StatusCode(409, "لا يمكن حذف طلب مكتمل");

        if (app.IsActive())
            return StatusCode(409, "يجب إلغاء الطلب أولا حتى تتم عملية حذفه");

        if (app.RequestID != null)
            return StatusCode(409, "هذا الطلب لا يمكن حذفه إلا عن طريق حذف الطلب الوارد المرتبط بهذا الطلب");

        if (app.TypeID == (int)enApplicationTypes.RenewLicenseApp)
            return _DeleteRenewApp(app);
        

        if (Application.Delete(id))
        {

            LoggerUtil.LogTransaction(User.Identity.Name, "Person", app, new
            {
                DeletedBy = User.Identity.Name,
                DeletedPersonID = id,
                Timestamp = DateTime.UtcNow
            }, nameof(PeopleController), nameof(DeleteApplication));

            return Ok("تم حذف الطلب من النظام");
        }

        return StatusCode(500, "حدث خطأ أثناء عملية الحذف");
    }

    [HttpGet("ID/{id}")] public IActionResult GetByID(int id) 
    {
        DLMS_BusinessLogicLayer.Application app = DLMS_BusinessLogicLayer.Application.Find(id);

        if (app == null)
          return NotFound($"ليس هناك طلب مفهرس بالرقم ({id})");
        
        return Ok(app);
    }

    [HttpGet("Reqeust-ID/{id}")]
    public IActionResult GetByRequestID(int? id)
    {
        DLMS_BusinessLogicLayer.Application app = DLMS_BusinessLogicLayer.Application.Find(id);

        if (app == null)
            return NotFound($"ليس هناك طلب مقيد برقم الطلب الوارد المفهرس برقم {id}");

        return Ok(app);
    }


    private Application LoadAppDtoToAppBLL(ApplicationDto app) 
    {

        DLMS_BusinessLogicLayer.Application application = new DLMS_BusinessLogicLayer.Application
        {
            PersonID = app.PersonID,
            TypeID = app.TypeID,
            CreatedByUserID = app.CreatedByUserID,
            UpdatedByUserID = app.UpdatedByUserID,
            RequestID = app.RequestID
        };

        return application;
    }

    private LocalLicenseApplication LoadAppDtoToLocalAppBLL(ApplicationDto app)
    {

        DLMS_BusinessLogicLayer.LocalLicenseApplication application = new DLMS_BusinessLogicLayer.LocalLicenseApplication
        {
            PersonID = app.PersonID,
            TypeID = app.TypeID,
            CreatedByUserID = app.CreatedByUserID,
            UpdatedByUserID = app.UpdatedByUserID,
            RequestID = app.RequestID
        };

        return application;
    }


    private IActionResult AddRnewApp(License license,ApplicationDto app)
    {

        if (!license.Renewable())
            return StatusCode(409, "لا يمكن تجديد رخصة لا تستوفي متطلبات التجديد وهي أن تكون منتهية او لا يزال أقل من سنة على إنتهائها");

        if (license.IsDetained()) return StatusCode(409, "الرخصة محجوزة ويجب فك حجزها أولا ليتم إستخراج أي بديل لها");
        
        LocalLicenseApplication application = LoadAppDtoToLocalAppBLL(app);

        if (!application.Save()) return StatusCode(500, "لم تتم عملية إضافة الطلب");

        return CreatedAtAction(nameof(GetByID), new { ID = application.ID }, application);
    }

    private IActionResult ReplaceLicenseApp(License license, ApplicationDto app) 
    {
        if (license.IsDetained()) return StatusCode(409, "الرخصة محجوزة ويجب فك حجزها أولا ليتم إستخراج أي بديل لها");

        if (!license.IsActiveLicense()) return StatusCode(409, $"بناء على نوع هذا الطلب  {app.Type} يجب أن تكون الرخصة فعالة");

        Application application = LoadAppDtoToAppBLL(app);

        if (!application.Save()) return StatusCode(500, "لم تتم عملية إضافة الطلب");

        return CreatedAtAction(nameof(GetByID), new { ID = application.ID }, application);
    }

    private IActionResult NewInternationalLicenseApp(License license, ApplicationDto app) 
    {
        //if (InternationalLicense.IsAcitveLicenseExistByDriverID(license.DriverID))
        //{
        //    return StatusCode(409,$"هذا الشخص لديه بالفعل رخصة دولية فعالة");
        //}

        if (license.IsDetained()) return StatusCode(409, "الرخصة محجوزة ويجب فك حجزها أولا ليتم إستخراج رخصة دولية عن طريقها");

        if (!license.IsActiveLicense()) return StatusCode(409, $"بناء على نوع هذا الطلب {app.Type} يجب أن تكون الرخصة فعالة");

        Application application = LoadAppDtoToAppBLL(app);

        if (!application.Save()) return StatusCode(500, "لم تتم عملية إضافة الطلب");

        return CreatedAtAction(nameof(GetByID), new { ID = application.ID }, application);
    }

    private IActionResult AddReleaseApp(License license, ApplicationDto app)
    {
        if (!license.IsDetained()) 
            return StatusCode(409, "الرخصة ليست محجوزة الرجاء التأكد من إعطاء بيانات صحيحة تخص رخصة محجوزة لإستكمال طلب هذه الخدمة");

        Application application = LoadAppDtoToAppBLL(app);

        if (!application.Save()) return StatusCode(500, "لم تتم عملية إضافة الطلب");

        return CreatedAtAction(nameof(GetByID), new { ID = application.ID }, application);
    }

    [HttpPost("Add New Application")] 
    public IActionResult AddNewApplication([FromBody] DLMS_DTO.ApplicationDto app) 
    {
        if (app == null) return BadRequest("يجب ألا تكون بيانات الطلب فارغة");

        if (Person.HasActiveApplication(app.PersonID)) return StatusCode(409, "هذا الشخص لديه بالفعل طلب فعال");

        int driverID = Driver.GetDriverIDByPersonID(app.PersonID);

        if (driverID == -1) return BadRequest(" لا يمكن لشخص ليس سائقا ان يقدم على خدمة تخص أصحاب التراخيص الذين يحملون رخص سابقة");

        License license = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driverID));

        if (license == null) return BadRequest("فشل في إستجلاب بيانات الرخصة تأكد من بيانات الرخصة");

        switch (app.TypeID) 
        {
            case (int)enApplicationTypes.NewLicenseApp:
                return BadRequest("خدمات إستخراج الرخص المحلية لأول مرة لا تتم من هذا الرابط ");

            case (int)enApplicationTypes.RetakeTestApp:
                return BadRequest("تتم هذه العملية داخليا");

            case (int)enApplicationTypes.RenewLicenseApp:
                return AddRnewApp(license, app);

            case (int)enApplicationTypes.LostLicenseApp:
            case (int)enApplicationTypes.DamagedLicenseApp:
                return ReplaceLicenseApp(license, app);

            case (int)enApplicationTypes.NewInternationalLicense:
                return NewInternationalLicenseApp(license, app);

            case (int)enApplicationTypes.ReleaseLicenseApp:
                return AddReleaseApp(license, app);
        }

        return BadRequest("نوع الطلب غير معروف");
    }

    public class IssueLicenseDto
    {
        public int UserID { get; set; }
        public string? Notes { get; set; }
    }




    [HttpPost("{id}/IssueLicense/")]
    public IActionResult IssueLicenseEndPoint(int id,[FromBody] IssueLicenseDto obj)
    {
        if (obj is null) return BadRequest("يجب ألا يكون بيانات الحقول الخاصة بمنشئي الرخصة فارغة");
        DLMS_BusinessLogicLayer.Application app = DLMS_BusinessLogicLayer.Application.Find(id);

        if (app.Type == (ApplicationType.Find((int)enApplicationTypes.ReleaseLicenseApp)?.Type) ||
            app.Type == (ApplicationType.Find((int)enApplicationTypes.NewInternationalLicense)?.Type)
            )
            return BadRequest($"هذا الطلب نوعه  {app.Type} وليس مخصصا لإنشاء رخص محلية جديدة");

    
        if (!app.IsActive()) return StatusCode(409, $"هذا الطلب  {app.Status} ولا يمكن إنشاء رخصة عن طريقه.");

        if (app == null) return NotFound($"ليس هناك طلب مفهرس بالمعرف رقم {id}");

        if (app.Type == (ApplicationType.Find((int)enApplicationTypes.RenewLicenseApp)?.Type))
        {
            var lApp = LocalLicenseApplication.FindByApplicationID(id);


            if (lApp.PassedTests < 1)
                return StatusCode(409, "يجب على المتقدم أن يقوم بفحص طبي أولا وينجح فيه حتى يتمكن من تجديد الرخصة");

            if (!lApp.IssueLicense(obj.UserID, obj.Notes)) return StatusCode(500, "حدثت مشكلة أثناء عملية إنشاء الرخصة");

            var temp = License.FindByApplicationID(app.ID);

            return Ok(temp);


        }


        if (!app.IssueLicense(obj.UserID, obj.Notes)) return StatusCode(500, "حدثت مشكلة أثناء عملية إنشاء الرخصة");

        var license = License.FindByApplicationID(app.ID);

        return Ok(license);
    }

    [HttpPut("{id}/Cancel")] public IActionResult CancelApplication(int id, [FromBody]int byUserID) 
    {

        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "ليس لديك الصلاحية لإتمام هذه العملية لعدم تعرف النظام عليك");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "لا يمكن لغير مدير النظام إتمام هذه العملية");
        }


        DLMS_BusinessLogicLayer.Application app = DLMS_BusinessLogicLayer.Application.Find(id);

        if (!app.IsActive()) return StatusCode(409, $"هذا الطلب {app.Status} ولا يمكن إتمام أي عملية عليه.");


        if (!app.Cancel(byUserID)) 
        {
            return StatusCode(500, "حدث خطأ أثناء العملية لم يتم إلغاء الطلب");
        }

        return StatusCode(200, "تمت عملية إلغاء الطلب");
    }

    [HttpGet("Get All")] public IActionResult GetAll() 
    {
        DataTable apps = DLMS_BusinessLogicLayer.Application.GetAll();

        if (apps == null || apps.Rows.Count == 0) return NotFound("ليس هناك أي طلبات في النظام");

        string jsonText = JsonConvert.SerializeObject(apps);    

        return Ok(jsonText);
    }



    [HttpGet("Get")]
    public IActionResult GetAll(string type)
    {
        DataTable apps = DLMS_BusinessLogicLayer.Application.GetAll(type);

        if (apps == null || apps.Rows.Count == 0) return NotFound($"ليس هناك أي طلبات {type} في النظام");


        string jsonText = JsonConvert.SerializeObject(apps);

        return Ok(jsonText);
    }


    [HttpGet("IsExist/{id}")] public IActionResult IsExist(int id) 
    {
        if (DLMS_BusinessLogicLayer.Application.IsExist(id))
            return base.Ok("نعم الطلب موجود");

        return NotFound($"الطلب المفهرس برقم {id} غير موجود");
    }


    [HttpGet("IsExistPersonID/{id}")]
    public IActionResult IsExistByPersonID(int id)
    {
        if (DLMS_BusinessLogicLayer.Application.IsExistByPersonID(id))
            return base.Ok("نعم الطلب موجود");

        return NotFound($"الطلب المفهرس برقم {id} غير موجود");
    }





    [HttpPost("Issue International License")]
    public IActionResult IssueInternationalLicense(int id, [FromBody] IssueLicenseDto obj)
    {
        if (obj is null) return BadRequest("يجب ألا يكون بيانات الحقول الخاصة بمنشئي الرخصة فارغة");
        DLMS_BusinessLogicLayer.Application app = DLMS_BusinessLogicLayer.Application.Find(id);

        
        if (app == null) return NotFound($"ليس هناك طلب مفهرس بالمعرف رقم {id}");

        if (app.Type != ApplicationType.Find((int)enApplicationTypes.NewInternationalLicense)?.Type)
            return BadRequest($"هذا الطلب نوعه {app.Type}  وليس مخصصا لفك حجز الرخص ");


        if (!app.IsActive()) return StatusCode(409, $"هذا الطلب {app.Status} ولا يمكن إنشاء رخصة عن طريقه.");

        if (!app.IssueInternationalLicense(obj.UserID)) return StatusCode(500, "حدثت مشكلة أثناء عملية إنشاء الرخصة");


        var temp = app.internationalLicense is null ? InternationalLicense.FindByApplicatonID(app.ID) : app.internationalLicense;   

        return Ok(temp);
    }


    public struct ReleaseData 
    {
        public int ApplicationID { get; set; }
        public int ReleasedByUserID { get; set; }
    }
    //[HttpPut("Release License")] public IActionResult ReleaseLicense(int id, int releasedByUserID) 
    //{
    //    DLMS_BusinessLogicLayer.Application app = DLMS_BusinessLogicLayer.Application.Find(id);

    //    if (app == null) return NotFound($"ليس هناك طلب مفهرس بالمعرف رقم {id}");

    //    if (!app.ReleaseDetainedLicense(releasedByUserID)) return StatusCode(500, "حدثت مشكلة أثناء عملية فك حجز الرخصة");

    //    return StatusCode(200, "تمت العملية بنجاح");

    //}


    [HttpPut("Release License")]
    public IActionResult ReleaseLicense([FromBody]ReleaseData data)
    {
        DLMS_BusinessLogicLayer.Application app = DLMS_BusinessLogicLayer.Application.Find(data.ApplicationID);

        if (app == null) return NotFound($"ليس هناك طلب {data.ApplicationID} مفهرس بالمعرف رقم ");

        if (app.Type != ApplicationType.Find((int)enApplicationTypes.ReleaseLicenseApp)?.Type)
            return BadRequest($"هذا الطلب نوعه {app.Type}  وليس مخصصا لفك حجز الرخص");

        int driverID = Driver.GetDriverIDByPersonID(app.PersonID);

        if (driverID != 0)
        {
            License licnese = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driverID));

            if (licnese == null) return NotFound("الرخصة غير موجودة");

            if (!licnese.IsDetained()) return StatusCode(409, "الرخصة غير محجوزة ليتم فك حجزها");
        }

        if (!app.IsActive()) return StatusCode(409, $"هذا الطلب ولا يمكن فك حجز الرخصة طريقه. {app.Status}");

        if (!app.ReleaseDetainedLicense(data.ReleasedByUserID)) return StatusCode(500, "حدثت مشكلة أثناء عملية فك حجز الرخصة");

        return StatusCode(200, "تمت العملية بنجاح");
    }



    [HttpGet("Get-Application-Status-Types")] public IActionResult GetStatusTypes() 
    {
        var list = ApplicationStatusType.GetAllStatus();

        if (list == null || list.Count == 0) return NotFound("ليس هناك حالات طلب مسجلة في النظام");

        return Ok(list);
    }

}
