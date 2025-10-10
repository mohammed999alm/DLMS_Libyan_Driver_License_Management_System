using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using DLMS_BusinessLogicLayer;
using System.Data;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace DLMS_SERVER.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CountryController : ControllerBase
    {

        [HttpGet("ID/{id}")] public IActionResult GetById(int id) 
        {
            Country country = Country.Find(id);

            if (country == null) return NotFound($"Country With ID ({id}) Is Not Found!!!");

            return Ok(country);
        }


        [HttpGet("Name/{name}")]
        public IActionResult GetByName(string name)
        {
            Country country = Country.Find(name);

            if (country == null) return NotFound($"Country With Name ({name}) Is Not Found!!!");

            return Ok(country);
        }


        [HttpGet("Countries")]
        public IActionResult GetAll()
        {
            DataTable countries = Country.GetAll();

            if (countries == null) return NotFound("Error");

            string jsontext = JsonConvert.SerializeObject(countries);

            return Ok(jsontext);
        }

    }
}
