using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;
using NuGet.Protocol;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using GlobalUtility;
using DLMS_BusinessLogicLayer;
using NLog.Targets;
using DLMS_DTO;
using DLMS_SERVER.ValidatorClasses;
using DLMS_SERVER.Extensions;
using System.Globalization;
using Microsoft.AspNetCore.Server.HttpSys;

namespace DLMS_SERVER.Controllers;

[Authorize(Roles = "مدير النظام,مستخدم النظام")]
[Route("api/[controller]")]
[ApiController]
public class RequestController : ControllerBase
{



    [HttpDelete("{id}/Delete")]
    public IActionResult DeleteApplication(int id)
    {
        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "ليس لديك الصلاحية لإتمام هذه العملية لعدم تعرف النظام عليك");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "لا يمكن لغير مدير النظام إتمام هذه العملية");
        }


        var req = DLMS_BusinessLogicLayer.Request.Find(id);

        if (req is null) return NotFound($"لم يتم العثور على الطلب الوارد المقيد بالرقم المفهرس  {id}");

        if (Application.IsExistByRequestID(id))
        {
            if (!Application.CouldBeDeletedByRequestID(id))
                return StatusCode(409, "لا يمكن حذف طلب مكتمل");
        }
        req.application = (req.application is null) ? Application.Find((int?)req.ID) : req.application;

        if (req.application is not null && req.application.IsActive())
            return StatusCode(409, "يجب إلغاء المعاملة المرتبطة بالطلب الوارد أولا حتى يتسنى لك حذف الطلب الوارد");

        if (DLMS_BusinessLogicLayer.Request.DeleteByRequestID(id))
        {

            LoggerUtil.LogTransaction(User.Identity.Name, "Request", req, new
            {
                DeletedBy = User.Identity.Name,
                DeletedPersonID = id,
                Timestamp = DateTime.UtcNow
            }, nameof(PeopleController), nameof(DeleteApplication));

            return Ok("تم حذف الطلب من النظام");
        }

        return StatusCode(500, "حدث خطأ أثناء عملية الحذف");
    }


    [HttpGet("Get All")] public IActionResult Get()
    {
        DataTable dt = DLMS_BusinessLogicLayer.Request.GetAll();

        if (dt == null || dt.Rows.Count <= 0) return NotFound("ليس هناك أي طلبات عبر البوابة الإلكترونية");

        string jsonText = JsonConvert.SerializeObject(dt);

        return Ok(jsonText);
    }


    [AllowAnonymous]
    [HttpGet("{nationalNumber}/Get-All")]
    public IActionResult GetAll(string nationalNumber)
    {
        Person person = Person.Find(nationalNumber);

        if (person == null) return NotFound($"ليس هناك شخص مسجل في النظام مقيد بالرقم الوطني {nationalNumber} يرجى التأكد من صحة الرقم الوطني");


        var reqeusts = person.GetAllRequestsByNationalNumber();

        if (reqeusts == null || reqeusts.Count <= 0) return NotFound("ليس هناك أي طلبات تخص المواطن مسجلة في النظام");

        return Ok(reqeusts);
    }



    [Produces("application/json")]
    [AllowAnonymous]
    [HttpGet("{nationalNumber}/Request-State")]
    public IActionResult GetRequestState(string nationalNumber)
    {
        Person person = Person.Find(nationalNumber);

        if (person == null) return NotFound($"ليس هناك شخص مسجل في النظام مقيد بالرقم الوطني {nationalNumber} يرجى التأكد من صحة الرقم الوطني");

        var request = DLMS_BusinessLogicLayer.Request.Find(person.GetRequestIDFromTheLatestRequest());

        if (request == null) return NotFound($"لم يتم العثور على بيانات أخر طلب  مرسل من قبل الشخص المقيد بالرقم الوطني {nationalNumber}");

        ObjectResult serverStatusCodeError =
         StatusCode(500,
            $"حدث خطأ غير متوقع أثناء معالجة حالة الطلب المرسل من قبل الشخص المقيد بالرقم الوطني {nationalNumber}. يرجى المحاولة لاحقًا أو التواصل مع الدعم الفني.");

        if (request.status == null)
            return serverStatusCodeError;

        string specificDateFormat = request.CreateDate.ToString("yyyy/MM/dd | HH:mm:ss", CultureInfo.InvariantCulture);

        switch (request._enStatus)
        {
            case enRequestStatus.Approved:
                return Ok(new CitizenRequestStatusDto(request.ID, nationalNumber, request.status.Name,
                    "تمت الموافقة عليه الرجاء الذهاب إلى أقرب مركز مروري لإستكمال باقي إجراءات الطلب", request.TheTypeOfRequest, specificDateFormat));
                
            case enRequestStatus.Pending:
                return Ok(new CitizenRequestStatusDto(request.ID, nationalNumber, request.status.Name, 
                    "لم تتم الموافقة عليه بعد", request.TheTypeOfRequest, specificDateFormat));
                
            case enRequestStatus.Declined:
                return Ok(new CitizenRequestStatusDto(request.ID, nationalNumber, request.status.Name,
                         " الطلب تم رفضه من قبل أصحاب القرار", request.TheTypeOfRequest, specificDateFormat));

            case enRequestStatus.Completed:
                return Ok(new CitizenRequestStatusDto(request.ID, nationalNumber, request.status.Name,
                           "الطلب مكتمل", request.TheTypeOfRequest, specificDateFormat));

            default:
                return serverStatusCodeError;
        }
    }




    [HttpGet("Get By ID")] public IActionResult GetByID(int requestID)
    {
        var request = DLMS_BusinessLogicLayer.Request.Find(requestID);

        if (request == null) return NotFound($"ليس هناك طلب عبر الإنترنت مفهرس برقم {requestID}");

        return Ok(request);
    }


    private async Task<ServerJsonCustomObject> FetchPersonGovAsync(string nationalNumber)
    {
        string url = $"https://localhost:7256/api/People/NationalNumber/{nationalNumber}";

        ServerJsonCustomObject obj = await DLMS_BusinessLogicLayer.Request.GetPersonByIdAsynchronously(url);

        return obj;
    }


    [AllowAnonymous]
    [HttpPost("Create-License-Request")] public async Task<IActionResult> AddNewRequest([FromBody]DLMS_DTO.RequestDto request)
    {

        if (request == null) return BadRequest("لا يجب أن تكون البيانات المرسلة فارغة");


        if (request.RequestTypeID != (int)enApplicationTypes.NewLicenseApp)
        {
            return BadRequest("رقم الطلب غير صحيح لا يمكن معالجة نوع طلب غير مخصص لإنشاء ترخيص جديد ");
        }

        LicenseClass licenseClass = LicenseClass.Find(request.LicenseClassID);

        if (licenseClass == null) 
        {
            return BadRequest("نوع الترخيص غير موجود في النظام يرجى إدخال فئة ترخيص صحيحة موجودة ضمن الدولة الليبية");
        }


        bool contactNotNull =
        ((!string.IsNullOrEmpty(request.PhoneNumber) && !string.IsNullOrWhiteSpace(request.PhoneNumber))
            || (!string.IsNullOrEmpty(request.Email) && !string.IsNullOrWhiteSpace(request.Email)));
        

        if (contactNotNull)
        {
            ValidateResult result = ContactValidator.ValidateContacts(request.PhoneNumber, request.Email);

            if (!result.IsValid)
            {
                return StatusCode(result.StatusCode.DefaultOrValue(), result.ErrorMessage);
            }
        }
        //ValidateResult result = ContactValidator.ValidateContacts(request.PhoneNumber, request.Email, true);

        //if (!result.IsValid) 
        //{
        //    return StatusCode(result.StatusCode.DefaultOrValue(), result.ErrorMessage);
        //}

        Person person = Person.Find(request.NationalNumber);

        if (person is null)
        {

            ValidateResult result = ContactValidator.ValidateContacts(request.PhoneNumber, request.Email, true);

            if (!result.IsValid)
            {
                return StatusCode(result.StatusCode.DefaultOrValue(), result.ErrorMessage);
            }

            ServerJsonCustomObject jsonObject = await FetchPersonGovAsync(request.NationalNumber);

            if (jsonObject.Status)
            {
                try
                {
                    person = JsonConvert.DeserializeObject<Person>(jsonObject.Data);

                    if (person is null)
                        return StatusCode(500, "حدث خطأ في الخادم أثناء معالجة بيانات المواطن");

                
                    if (person.Age < 18)
					    return StatusCode(409, "لا يجب أن يكون المواطن المراد تسجيله في الجهة قاصرا لم يصل السن القانونية");

				
					if ((!person.Save()))
                    {
                        return StatusCode(500, "فشل حفظ بيانات المواطن في قاعدة البيانات");
                    }
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, nameof(RequestController), nameof(AddNewRequest));

                    return StatusCode(500, "حدث خطأ في الخادم أثناء معالجة بيانات المواطن");
                }

            }
            else
            {
                return BadRequest("المواطن غير موجود في قاعدة البيانات الحكومية يرجى التحقق من صحة الرقم الوطني");
            }
        }

        if (contactNotNull) 
        {
            if (!person.CreateNewContact(request.PhoneNumber, request.Email)) 
            {
                  return StatusCode(500, "فشل حفظ بيانات جهة الإتصال الخاصة بالمواطن في قاعدة البيانات");
            }
        }

        if (person.Age < licenseClass.Age)
            return StatusCode(409, "عمر المتقدم لا يستوفي العمر المشروط لفئة الترخيص");

        if (person.HasActiveApplicationDeepSearch()) 
            return StatusCode(409, "هذا الشخص لديه طلب فعال");

        

        int driverID = Driver.GetDriverIDByPersonID(person.ID);

        var license = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driverID));

        if (license != null && license.IsDetained())
        {
            return StatusCode(409, "يجب فك حجز الرخصة أو دفع الغرامة الخاصة بالحجز أولا حتى يتسنى لك الحصول على رخصة جديدة");
        }

        if (driverID != -1 && License.IsExistByLicenseClassAndDriverID(driverID, request.LicenseClassID))
        {
            return StatusCode(409, "لا يمكن تقديم على فئة رخصة يملك المواطن ترخيصا منها");
        }

        if (licenseClass.ID == (int)enLicenseClassTypes.Grade3 || licenseClass.ID == (int)enLicenseClassTypes.Grade4A || licenseClass.ID == (int)enLicenseClassTypes.Grade4B) 
        {
            if (!License.IsLicenseWithDateThresholdByDriverAndClassIdExists(driverID, (int)enLicenseClassTypes.Grade2))
            {
                return StatusCode(409, "لا بد أن يكون للمتقدم  رخصة من الدرجة الثانية لسنتين على الأقل");
            }
        }

        var theRequest = new DLMS_BusinessLogicLayer.Request
        {
            NationalNumber = request.NationalNumber,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            RequestTypeID = request.RequestTypeID,
            LicenseClassID = request.LicenseClassID
        };

        if (theRequest.Save() == false) return StatusCode(500, "حدث خطأ أثناء عملية  الإضافة");

        return CreatedAtAction(nameof(GetByID), new { ID = theRequest.ID }, theRequest);
    }



    private DLMS_BusinessLogicLayer.Request LoadAppDtoToRequestBLL(RequestDto2 request) 
    {

        var theRequest = new DLMS_BusinessLogicLayer.Request
        {
            NationalNumber = request.NationalNumber,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            RequestTypeID = request.RequestTypeID,
            LicenseID = request.LicenseID
        };

        return theRequest;
    }

    private IActionResult AddRnewApp(License license, RequestDto2 app)
    {

        if (!license.Renewable())
            return StatusCode(409, "لا يمكن تجديد رخصة لا تستوفي متطلبات التجديد وهي أن تكون منتهية او لا يزال أقل من سنة على إنتهائها");

        if (license.IsDetained()) return StatusCode(409, "الرخصة محجوزة ويجب فك حجزها أولا ليتم إستخراج أي بديل لها");

        DLMS_BusinessLogicLayer.Request request = LoadAppDtoToRequestBLL(app);

        if (!request.Save()) return StatusCode(500, "لم تتم عملية إضافة الطلب");

        return CreatedAtAction(nameof(GetByID), new { ID = request.ID }, request);
    }

    private IActionResult ReplaceLicenseApp(License license, RequestDto2 app)
    {
        if (license.IsDetained()) return StatusCode(409, "الرخصة محجوزة ويجب فك حجزها أولا ليتم إستخراج أي بديل لها");

        if (!license.IsActiveLicense()) return StatusCode(409, $"بناء على نوع هذا الطلب  يجب أن تكون الرخصة فعالة");

        DLMS_BusinessLogicLayer.Request request = LoadAppDtoToRequestBLL(app);

        if (!request.Save()) return StatusCode(500, "لم تتم عملية إضافة الطلب");

        return CreatedAtAction(nameof(GetByID), new { ID = request.ID }, request);
    }

    [AllowAnonymous]
    [HttpPost("Create-License-Replacement-Request")]
    public IActionResult AddNewRequest([FromBody]DLMS_DTO.RequestDto2 request)
    {

        if (request == null) return BadRequest("لا يجب أن تكون البيانات المرسلة فارغة");

        var person = Person.Find(request.NationalNumber);

        if (person is null) return BadRequest("الرقم الوطني غير مسجل في النظام يرجى التأكد من صحة الرقم الوطني قبل إدخاله");

        var license = License.Find(request.LicenseID);

        if (license is null) return BadRequest("رقم الرخصة غير مسجل في النظام يرجى التأكد من صحة رقم الرخصة أثناء إدخاله");

        if (person.HasActiveApplicationDeepSearch()) return BadRequest("لديك طلب فعال مقيد برقمك الوطني بالفعل");

        Driver driver = Driver.FindByPersonId(person.ID);

        if (driver == null) return StatusCode(409, "لا يمكن إستكمال الطلب الشخص المقيد بالرقم الوطني المرسل ليس بسائق مسجل في النظام");

        var latestLicense = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driver.ID));

        if (latestLicense == null)
            return StatusCode(500, "حدث خطأ لم يتم العثور على أخر رخصة متحصل عليها من قبل السائق المقيد بالرقم الوطني المرسل");

        if (latestLicense.ID != license.ID)
            return StatusCode(422, "رقم الرخصة الذي تم إدخاله غير صحيح ولا يتوافق مع أخر رخصة مقيدة بالرقم الوطني المرسل");

        if ((!string.IsNullOrEmpty(request.PhoneNumber) && !string.IsNullOrWhiteSpace(request.PhoneNumber)) 
            || (!string.IsNullOrEmpty(request.Email) && !string.IsNullOrWhiteSpace(request.Email)))
        {
            ValidateResult result = ContactValidator.ValidateContacts(request.PhoneNumber, request.Email);

            if (!result.IsValid) 
            {
                return StatusCode(result.StatusCode.DefaultOrValue(), result.ErrorMessage);
            }

            if (!person.CreateNewContact(request.PhoneNumber, request.Email)) 
            {
                  return StatusCode(500, "فشل حفظ بيانات جهة الإتصال الخاصة بالمواطن في قاعدة البيانات");
            }
        }

        switch (request.RequestTypeID) 
        {
            case (int)enApplicationTypes.RenewLicenseApp:
                return AddRnewApp(license, request);

            case (int)enApplicationTypes.DamagedLicenseApp:
            case (int)enApplicationTypes.LostLicenseApp:
                return ReplaceLicenseApp(license, request);

            default:
                return BadRequest("هذه الخدمة غير معروفة أو غير متوفرة إلكترونيا");
        }
        
    }


    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [HttpPut("{id}/Approve")]
    public IActionResult Approve(int id, [FromBody]int userID)
    {
        var req = DLMS_BusinessLogicLayer.Request.Find(id);

        if (req == null) return BadRequest($"ليس هناك طلب عبر الإنترنت مفهرس برقم {id}");

        //int userID = int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userID) ? userID : -1;

        if (!req.IsActive()) return StatusCode(409, "هذا الطلب غير فعال ولا يمكن التعديل عليه ");

        if (req._enStatus == enRequestStatus.Approved) return StatusCode(409, "هذا الطلب تمت الموافقة عليه فيما سبق");

        if (req.Approve(userID))
            return Ok("تم قبول الطلب الوارد بنجاح");


        return StatusCode(500, "لم تتم العملية بنجاح");
    }


    [HttpPut("{id}/Reject")]
    public IActionResult Reject(int id)
    {
        var req = DLMS_BusinessLogicLayer.Request.Find(id);

        if (req == null) return BadRequest($"ليس هناك طلب عبر الإنترنت مفهرس برقم {id}");

        //int userID = int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userID) ? userID : -1;

        //if (req._enStatus == enRequestStatus.Completed)
        //    return StatusCode(409, "لم يتم إتمام هذا الإجراء لأن هذا الطلب مكتمل ولا يمكن  التعديل على حالته");

        //if (req._enStatus == enRequestStatus.Declined)

        if (!req.IsActive()) return StatusCode(409, "هذا الطلب غير فعال ولا يمكن التعديل عليه");

        req.application = (req.application is null) ? Application.Find((int?)req.ID) : req.application;

        if (req.application is not null && req.IsActive())
            return StatusCode(409, "يجب إلغاء المعاملة المرتبطة بالطلب الوارد أولا للتعديل على حالته");

        if (req.Decline())
            return Ok("تم رفض الطلب الوارد");

        return StatusCode(500, "لم تتم العملية بنجاح");
    }


    //[Authorize(Roles = "مدير النظام")]
    [HttpPut("{id}/Complete")]
    public IActionResult Complete(int id)
    {

        if (User.Identity is null || User.Identity.Name is null) return StatusCode(401, "ليس لديك الصلاحية لإتمام هذه العملية لعدم تعرف النظام عليك");

        if (!User.IsInRole("مدير النظام"))
        {
            return StatusCode(403, "لا يمكن لغير مدير النظام إتمام هذه العملية");
        }


        var req = DLMS_BusinessLogicLayer.Request.Find(id);

        if (req == null) return BadRequest($"ليس هناك طلب عبر الإنترنت مفهرس برقم {id}");

        //int userID = int.TryParse(User?.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userID) ? userID : -1;

        if (!req.IsActive()) return StatusCode(409, "هذا الطلب غير فعال ولا يمكن التعديل عليه");

        if (req._enStatus != enRequestStatus.Approved) return StatusCode(409, "لا يمكن إستكمال الطلب قبل أن تتم عملية معالجته");

        if (req.FeeAmount == null || req.FeeAmount < 20) return StatusCode(409, "لا يمكن إستكمال طلب لم تتم عملية حساب رسومه"); 

        if (req.Completed())
            return Ok("تم الإنتهاء من المعاملة");


        return StatusCode(500, "لم تتم العملية بنجاح");
    }

    [AllowAnonymous]

    [HttpGet("generate-pdf")]
    public IActionResult GetGeneratedPdf()
    {

        var reqeust = DLMS_BusinessLogicLayer.Request.Find(7);

        if (reqeust == null) return NotFound();

        var pdfDoc = PDF_Builder.GenerateInMemory(reqeust.person?.FullName,
            reqeust.TheTypeOfRequest + " من " + reqeust.licenseClass?.Name, reqeust.person?.NationalNumber);

        if (pdfDoc == null) return StatusCode(500, "Unable to serve the process");

        var pdfBytes = pdfDoc.BinaryData;

        return File(pdfBytes, "application/pdf", "CitizenReport.pdf");
    }


    [AllowAnonymous]
    [HttpGet("generate-pdfv2")]
    public async Task<IActionResult> GetGeneratedPdfv2()
    {

        var reqeust = DLMS_BusinessLogicLayer.Request.Find(7);

        if (reqeust == null) return NotFound();

        var pdfBytes = await PDF_Builder.GeneratePdfAsByteArrayAsync(reqeust.person?.FullName, 
            reqeust.TheTypeOfRequest + " من " + reqeust.licenseClass?.Name, reqeust.person?.NationalNumber);

        if (pdfBytes == null) return StatusCode(500, "Unable to serve the process");

        return File(pdfBytes, "application/pdf", "CitizenReport.pdf");
    }
}
