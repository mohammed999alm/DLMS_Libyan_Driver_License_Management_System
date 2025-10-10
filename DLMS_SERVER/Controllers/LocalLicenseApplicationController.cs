using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using System.Data;
using Newtonsoft.Json;
using Microsoft.Identity.Client;
using Humanizer.Localisation.TimeToClockNotation;
using static DLMS_SERVER.Controllers.ApplicationController;
using System.Security.Claims;
using System.ComponentModel.Design;
using GlobalUtility;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using Microsoft.AspNetCore.Authorization;

namespace DLMS_SERVER.Controllers;

[Authorize(Roles = "مدير النظام,مستخدم النظام")]
[Route("api/[controller]")]
[ApiController]
public class LocalLicenseApplicationController : ControllerBase
{

    
    [HttpGet("Get/{id}")] public IActionResult GetByID(int id)
    {
        LocalLicenseApplication lApp = LocalLicenseApplication.Find(id);

        if (lApp == null) return NotFound($"ليس هناك طلب مفهرس بالرقم {id}");

        return Ok(lApp);
    }


    [HttpGet("Get-By-Main-App-ID/{id}")]
    public IActionResult GetByAppID(int id)
    {
        LocalLicenseApplication lApp = LocalLicenseApplication.FindByApplicationID(id);

        if (lApp == null) return NotFound($"ليس هناك طلب مفهرس بالرقم {id}");

        return Ok(lApp);
    }

    [HttpGet("{appID}/Get-Appointment/{appointmentID}")]
    public IActionResult GetAppointmentByAppointmentID(int appID, int appointmentID)
    {

        if (!LocalLicenseApplication.IsExist(appID)) return NotFound($"ليس هناك طلب مفهرس بالرقم {appID}");

        var appointment = TestAppointment.Find(appointmentID);

        if (appointment == null || appointment.LocalLicenseApplicationID != appID)
            return NotFound($"لا يوجد موعد بهذا المعرف {appointmentID} ضمن الطلب رقم {appID}");

        return Ok(appointment);
    }



    [HttpGet("{localLicenseAppID}/Appointments")]
    public ActionResult GetAppointments(int localLicenseAppID, int testTypeID)
    {
        DataTable appointments = TestAppointment.GetAllTestAppointmentesByLocalLicenseAppIdAndTestTypeID(localLicenseAppID, testTypeID);

        if (appointments == null || appointments.Rows.Count <= 0)
            return NotFound($"لم يتم العثور على أي سجلات لمواعيد مقترنة برقم هذا الطلب {localLicenseAppID} في النظام");

        string jsonText = JsonConvert.SerializeObject(appointments);

        return Ok(jsonText);
    }


    [HttpGet("{localLicenseAppID}/Appointments-List")]
    public ActionResult GetAppointments(int localLicenseAppID, string type)
    {
        DataTable appointments = TestAppointment.GetAllTestAppointmentesByLocalLicenseAppIdAndTestType(localLicenseAppID, type);

        if (appointments == null || appointments.Rows.Count <= 0)
            return NotFound($"لم يتم العثور على أي سجلات لمواعيد مقترنة برقم هذا الطلب {localLicenseAppID} في النظام");

        string jsonText = JsonConvert.SerializeObject(appointments);

        return Ok(jsonText);
    }



    [HttpGet("Get All")] public IActionResult GetAll()
    {
        DataTable dt = LocalLicenseApplication.GetAll();

        if (dt == null || dt.Rows.Count <= 0) return NotFound("ليس هناك أي طلبات  استخراج رخص محلية في النظام");

        string jsonText = JsonConvert.SerializeObject(dt);

        return Ok(jsonText);
    }


    [HttpPost("Add New Application")]
    public IActionResult AddNewApplication([FromBody] DLMS_DTO.LocalLicenseApplicationDto app)
    {
        if (app == null) return BadRequest("يجب أن لا تكون البيانات المرسلة فارغة");

        LicenseClass licenseClass = LicenseClass.Find(app.LicenseClassID);

        if (licenseClass == null)
        {
            return BadRequest("نوع الترخيص غير موجود في النظام يرجى إدخال فئة ترخيص صحيحة موجودة ضمن الدولة الليبية");
        }

        Person person = Person.Find(app.PersonID);

        if (person == null) return NotFound($"ليس هناك شخص مفهرس في النظام بالرقم التالي {app.PersonID}");

        if (person.Age < licenseClass.Age)
            return StatusCode(409, "عمر المتقدم لا يستوفي العمر المشروط لفئة الترخيص");

        //if (person.HasActiveApplicationDeepSearch())
        //    return StatusCode(409, "هذا الشخص لديه طلب فعال");


        if (person.HasActiveApplication()) return StatusCode(409, "هذا الشخص لديه طلب فعال");

        if (person.HasActiveRequest()) return StatusCode(409, "هذا الشخص لديه طلب وارد لم يتم معالجته  بعد");


        int driverID = Driver.GetDriverIDByPersonID(app.PersonID);

        var license = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driverID));

        if (license != null && license.IsDetained())
        {
            return StatusCode(409, "يجب فك حجز الرخصة أو دفع الغرامة الخاصة بالحجز أولا حتى يتسنى لك الحصول على رخصة جديدة");
        }


        if (driverID != -1 && License.IsExistByLicenseClassAndDriverID(driverID, app.LicenseClassID))
        {
            return StatusCode(409, "لا يمكن التقديم على فئة رخصة يملك المواطن ترخيصا منها");
        }





        if (licenseClass.ID == (int)enLicenseClassTypes.Grade3 || licenseClass.ID == (int)enLicenseClassTypes.Grade4A || licenseClass.ID == (int)enLicenseClassTypes.Grade4B)
        {
            if (!License.IsLicenseWithDateThresholdByDriverAndClassIdExists(driverID, (int)enLicenseClassTypes.Grade2))
            {
                return StatusCode(409, "لا بد أن يكون للمتقدم  رخصة من الدرجة الثانية لسنتين على الأقل");
            }
        }

        DLMS_BusinessLogicLayer.LocalLicenseApplication application = new DLMS_BusinessLogicLayer.LocalLicenseApplication
        {
            PersonID = app.PersonID,
            TypeID = app.TypeID,
            LicenseClassID = app.LicenseClassID,
            CreatedByUserID = app.CreatedByUserID,
            UpdatedByUserID = app.UpdatedByUserID,
            RequestID = app.RequestID
        };

        if (!application.Save()) return StatusCode(500, "حدث خطأ في الخادم لم تتم عملية الإضافة");

        return CreatedAtAction(nameof(GetByID), new { ID = application.ID }, application);
    }

    public class SheduleAppointment
    {
        public int CreatedByUserID { get; set; }

        public DateTime AppointmentDate { get; set; }
    }

    [HttpPost("{id}/SheduleTestAppointment")] public IActionResult SheduleTestAppointment(int id, [FromBody] SheduleAppointment appointment)
    {
        var lApp = LocalLicenseApplication.Find(id);


        

        if (lApp == null) return NotFound($"ليس هناك طلب استخراج رخصة مفهرس برقم {id}");

        if (!lApp.IsActive()) return StatusCode(409, $"هذا الطلب {lApp.Status} ولا يمكن جدولة موعد عن طريقه. ");

        if (lApp.HasActiveAppointments()) return StatusCode(409, "هذا الطلب مقرون بموعد إختبار ساري المفعول مسبقا");

        if (lApp.TypeID == (int)enApplicationTypes.NewLicenseApp && lApp.PassedTests == 3) 
        {
            return StatusCode(409, "لا يمكن أن تتم المعاملة كون صاحب هذا الطلب قد أكمل الإختبارات الثلاثة ونجح فيهم");
        }

        if (lApp.TypeID == (int)enApplicationTypes.RenewLicenseApp && lApp.PassedTests == 1) 
        {
            return StatusCode(409, "لا يمكن أن تتم العملية كون أن صاحب الطلب قد إجتاز الفحص الطبي المطلوب لتجديد الرخصة");
        }

        if (appointment.AppointmentDate < DateTime.Today)
            return StatusCode(409, "لا يمكن حجز موعد في الماضي");



        if (!lApp.ScheduleAppointment(appointment.CreatedByUserID, appointment.AppointmentDate))
            return StatusCode(500, "حدثت مشكلة أثناء عملية أخذ الموعد");

        return CreatedAtAction(nameof(GetByID), new { id = lApp.LocalAppId }, appointment);
    }



    [HttpPut("{id}/ResheduleTestAppointment/{appointmentID}")]
    public IActionResult ResheduleTestAppointment(int id, int appointmentID, [FromBody] SheduleAppointment appointment)
    {
        var lApp = LocalLicenseApplication.Find(id);



        if (lApp == null) return NotFound($"ليس هناك طلب استخراج رخصة مفهرس برقم {id}");

        if (!lApp.IsActive()) return StatusCode(409, $"هذا الطلب  {lApp.Status} ولا يمكن إعادة جدولة موعد عن طريقه.");


        if (!TestAppointment.IsExist(appointmentID)) return NotFound($"ليس هناك موعد مفهرس بالرقم {appointmentID}");

        
		if (appointment.AppointmentDate < DateTime.Today)
			return StatusCode(409, "لا يمكن حجز موعد في الماضي");

		if (!lApp.ReSheduleAppointment(appointmentID, appointment.CreatedByUserID, appointment.AppointmentDate))
            return StatusCode(500, "حدثت مشكلة أثناء عملية أخذ الموعد");

        return Ok("تمت عملية جدولة الموعد بنجاح");
    }

    [HttpGet("{id}/Appointment-test-attempts/")] public IActionResult GetTestAttemtps(int id, string testType)
    {
        int attempts = LocalLicenseApplication.GetTestAppointmentAttemptsBasedOnLocalLicenseAppIdAndTestType(id, testType);

        if (attempts != -1)
            return Ok(attempts);

        return StatusCode(500, "حدث خطأ في الخادم");
    }
    public class TestTaken
    {
        public int CreatedByUserID { get; set; }

        public bool Result { get; set; }

        public string? Notes { get; set; }
    }



    [HttpPost("{id}/TakeTest")]
    public IActionResult TakeTest(int id, [FromBody] TestTaken test)
    {
        var lApp = LocalLicenseApplication.Find(id);

        if (lApp == null) return NotFound($"ليس هناك طلب استخراج رخصة مفهرس برقم {id}");

        if (!lApp.IsActive()) return StatusCode(409, $"هذا الطلب {lApp.Status} ولا يمكن تسجيل إختبار عن طريقه. ");

        if (test == null) return BadRequest("لا يجب أن ترسل البيانات فارغة للخادم");

        if (lApp.request != null && !lApp.request.IsPaid)  
        {
            return StatusCode(409, "لا يمكن إستكمال هذه العملية حتى يتم دفع رسوم الطلب الوارد");
        }

        if (!lApp.HasActiveAppointments()) return StatusCode(409, "يجب حجز موعد أولا حتى يتم أخذ الإختبار");

        if (lApp.appointment is null) return  StatusCode(409, "يجب حجز موعد أولا حتى يتم أخذ الإختبار");

		if (lApp.appointment.AppointmentDate > DateTime.Now)
			return StatusCode(409, "لا يمكن أخذ الإختبار قبل أن يأتي وقت الموعد");



		if (!lApp.TakeTest(test.CreatedByUserID, test.Result, test.Notes))
            return StatusCode(500, "حدثت مشكلة أثناء عملية تسجيل الإختبار");


        //lApp.appointment = GetAppointmentByAppointmentID(lApp.ID, lApp.ID);
        return CreatedAtAction(nameof(GetAppointmentByAppointmentID), new { appID = lApp.LocalAppId, appointmentID = lApp?.appointment.ID }, lApp);
    }




    [HttpPost("{id}/IssueLicense/")]
    public IActionResult IssueLicenseEndPoint(int id, [FromBody] IssueLicenseDto obj)
    {
        
     
        if (obj is null) return BadRequest("يجب ألا يكون بيانات الحقول الخاصة بمنشئي الرخصة فارغة");

        DLMS_BusinessLogicLayer.LocalLicenseApplication app = DLMS_BusinessLogicLayer.LocalLicenseApplication.Find(id);

        if (app == null) return NotFound($"ليس هناك طلب مفهرس بالمعرف رقم {id}");

        if (!app.IsActive()) return StatusCode(409,$"هذا الطلب {app.Status} ولا يمكن إنشاء رخصة عن طريقه.");

        if (app.PassedTests < 3)
            return StatusCode(409, "يجب على مقدم الطلب إجتياز الإختبارات الثلاثة حتى يتسنى له الحصول على رخصة جديدة");


        if (app.person is not null) 
        {

            var licenseClass = LicenseClass.Find(app.LicenseClassID);

            if (licenseClass is not null && app.person.Age < licenseClass.Age)
                return StatusCode(409, "عمر المتقدم لا يستوفي شروط الترخيص تواصل مع الدعم الفني وتحقق من الشخص الذي قام بالتلاعب بتاريخ ميلاد المتقدم حتى يتم محاسبته");

		}

        if (!app.IssueLicense(obj.UserID, obj.Notes)) return StatusCode(500, "حدثت مشكلة أثناء عملية إنشاء الرخصة");

        return Ok($"تم إصدار رخصة جديدة");
    }




    [HttpPut("{id}/Cancel-Application")]
    public IActionResult CancelApplication(int id, [FromBody]int userID) 
    {

        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "ليس لديك الصلاحية لإتمام هذه العملية لعدم تعرف النظام عليك");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "لا يمكن لغير مدير النظام إتمام هذه العملية");
        }

        LocalLicenseApplication lApp = LocalLicenseApplication.Find(id);

        if (lApp == null) return NotFound($"هذا الطلب المعرف بالرقم غير مسجل في النظام {id}");

        //if (!lApp.IsActive()) return StatusCode(409, "هذا الطلب لا يمكن التعديل على حالته وجعله طلب ملغي");

        if (!lApp.IsActive()) return StatusCode(409, $"هذا الطلب {lApp.Status} ولا يمكن إتمام أي عملية عليه.");


        //int userId = int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId) ? userId : -1;

        if (!lApp.Cancel(userID)) return StatusCode(500, "لم تتم العملية حدث خطأ في الخادم");

        return Ok("تمت عملية إلغاء الطلب بنجاح");
    }


    
    [HttpDelete("{id}/Delete")] public IActionResult DeleteApplication(int id) 
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "ليس لديك الصلاحية لإتمام هذه العملية لعدم تعرف النظام عليك");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "لا يمكن لغير مدير النظام إتمام هذه العملية");
        }

        var lApp = LocalLicenseApplication.Find(id);

        if (lApp == null) return NotFound($"لم يتم العثور على الطلب إستخراج رخصة جديدة المقيد بالرقم المفهرس  {id}");

        if (!Application.CouldBeDeletedAppID(lApp.ID))
            return StatusCode(409, "لا يمكن حذف طلب مكتمل");

        if (lApp.IsActive())
            return StatusCode(409, "يجب إلغاء الطلب أولا حتى تتم عملية حذفه");

        if (lApp.RequestID != null)
            return StatusCode(409,"هذا الطلب لا يمكن حذفه إلا عن طريق حذف الطلب الوارد المرتبط بهذا الطلب");




        if (LocalLicenseApplication.Delete(id))
        {

            LoggerUtil.LogTransaction(User.Identity.Name, "LocalLicenseApplication", lApp, new
            {
                DeletedBy = User.Identity.Name,
                DeletedPersonID = id,
                Timestamp = DateTime.UtcNow
            }, nameof(PeopleController), nameof(DeleteApplication));

            return Ok("تم حذف الطلب من النظام");
        }

        return StatusCode(500, "حدث خطأ أثناء عملية الحذف");
    }
}
