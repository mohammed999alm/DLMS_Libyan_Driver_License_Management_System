using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using Newtonsoft.Json;
using System.Data;

namespace DLMS_SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MunicipalityController : ControllerBase
    {

        [HttpGet("ID/{id}")]
        public IActionResult GetById(int id)
        {
            Municipality Municipality = Municipality.Find(id);

            if (Municipality == null) return NotFound($"Municipality With ID ({id}) Is Not Found!!!");

            return Ok(Municipality);
        }


        [HttpGet("Name/{name}")]
        public IActionResult GetByName(string name)
        {
            Municipality Municipality = Municipality.Find(name);

            if (Municipality == null) return NotFound($"Municipality With Name ({name}) Is Not Found!!!");

            return Ok(Municipality);
        }


        [HttpGet("Municipalities")]
        public IActionResult GetAll()
        {
            DataTable municipalities = Municipality.GetAll();

            if (municipalities == null) return NotFound("Error");

            string jsontext = JsonConvert.SerializeObject(municipalities);

            return Ok(jsontext);
        }
    }
}
