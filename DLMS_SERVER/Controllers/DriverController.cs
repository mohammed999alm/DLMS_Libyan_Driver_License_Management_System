using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using System.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;
using Microsoft.AspNetCore.Authorization;

namespace DLMS_SERVER.Controllers
{

    [Authorize(Roles = "مدير النظام,مستخدم النظام")]
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {

        [HttpGet("Get All Licenses")] public IActionResult GetAllLicenses(int? driverID = null, int? personID = null) 
        {
            Driver? driver = driverID.HasValue ? Driver.Find(driverID.Value) : personID.HasValue ? Driver.FindByPersonId(personID.Value) : null;


            if (driver == null) 
                return NotFound("ليس سائق مفهرس برقم {driverID} في النظام");

            DataTable licenses = driver.GetLicenses();

            if (licenses == null || licenses.Rows.Count <= 0) return NotFound("ليس هناك تراخيص مقيدة برقم هذا السائق");

            string jsonText = JsonConvert.SerializeObject(licenses);    

            return Ok(jsonText);
        }



        [HttpGet("Get All International Licenses")]
        public IActionResult GetAllInternationalLicenses(int? driverID = null, int? personID = null)
        {
            Driver? driver = driverID.HasValue ? Driver.Find(driverID.Value) : personID.HasValue ? Driver.FindByPersonId(personID.Value) : null;

            if (driver == null)
                return NotFound("ليس سائق مفهرس برقم {driverID} في النظام");

            DataTable licenses = driver.GetInternationalLicenses();

            if (licenses == null || licenses.Rows.Count <= 0) return NotFound("ليس هناك تراخيص مقيدة برقم هذا السائق");

            string jsonText = JsonConvert.SerializeObject(licenses);

            return Ok(jsonText);
        }


        [HttpGet("Get-All-Drivers")]
        public IActionResult GetAllDrivers() 
        {
            DataTable drivers = Driver.GetAllDrivers();

            if (drivers is null || drivers.Rows.Count <= 0) return NotFound("ليس هناك بيانات تخص السائقين في النظام");

            string jsonText = JsonConvert.SerializeObject(drivers);

            return Ok(jsonText);
        }


        //[HttpGet("Get-All-License")]
        //public IActionResult GetAllLicensesByPersonID(int personID)
        //{
        //    Driver driver = Driver.FindByPersonId(personID);

        //    if (driver == null)
        //        return NotFound("ليس سائق مفهرس برقم {personID} في النظام");

        //    DataTable licenses = driver.GetLicenses();

        //    if (licenses == null || licenses.Rows.Count <= 0) return NotFound("ليس هناك تراخيص مقيدة برقم هذا السائق");

        //    string jsonText = JsonConvert.SerializeObject(licenses);

        //    return Ok(jsonText);
        //}



        //[HttpGet("Get-All-International-Licenses")]
        //public IActionResult GetAllInternationalLicensesByPersonID(int personID)
        //{
        //    Driver driver = Driver.FindByPersonId(personID);

        //    if (driver == null)
        //        return NotFound("ليس سائق مفهرس برقم {personID} في النظام");

        //    DataTable licenses = driver.GetInternationalLicenses();

        //    if (licenses == null || licenses.Rows.Count <= 0) return NotFound("ليس هناك تراخيص مقيدة برقم هذا السائق");

        //    string jsonText = JsonConvert.SerializeObject(licenses);

        //    return Ok(jsonText);
        //}

        [HttpGet("Find")]
        public IActionResult GetDriver(int driverID)
        {
            Driver driver = Driver.Find(driverID);

            if (driver == null)
                return NotFound("ليس هناك سائق مفهرس برقم {driverID} في النظام");

            return Ok(driver);
        }


        [HttpGet("Find By PersonID")]
        public IActionResult GetDriverByPersonID(int personID)
        {
            Driver driver = Driver.FindByPersonId(personID);

            if (driver == null)
                return NotFound("ليس هناك سائق مفهرس برقم معرف الشخص  {personID} في النظام");

            return Ok(driver);
        }
    }
}
