//using Microsoft.AspNetCore.Mvc;

//namespace WebApplication2.Controllers
//{
//    public class PeopleController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }
//    }
//}



using Microsoft.AspNetCore.Mvc;
using BussinessLogicLayer;
using System.Collections.Generic;
using System.Data;
using System.Text.Json.Serialization;
using System.Text.Json;
using Newtonsoft.Json;

using Microsoft.Extensions.Options;
using DTO_Layer;
using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PeopleController : ControllerBase
    {
        // GET: api/people
        [HttpGet]
        //public IActionResult GetAllPeople()
        //{
        //    DataTable people = Person.GetAllPeople();
        //    if (people == null || people.Rows.Count == 0)
        //    {
        //        return NotFound("No people found.");
        //    }

        //    List<> personList = new List<Person>();
        //    foreach (DataRow row in people.Rows)
        //    {
        //        personList.Add(
        //          row.ToString()
        //        ));
        //    }
        //    return Ok(personList);
        //}

        // GET: api/people/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPerson(int id)
        {
            var person =  Person.Find(id);
            
            if (person == null)
            {
                return NotFound($"No person found with ID {id}");
            }


            person.ImagePath = $"https://localhost:7256/api/Image/{person.ID}/{person.ImagePath}"; 


            return Ok(person);
        }


        [HttpGet("All/DataTable")]
        public IActionResult GetAll1()
        {
            var dt = Person.GetAllPeople();
            if (dt == null)
            {
                return NotFound($"There's No People In The System");
            }

            //List<Person> people = new List<Person>();


            //foreach (DataRow row in dt.Rows)
            //{
            //    people.Add(Person.Find((int)row[0]));
            //}


            //person.ImagePath = $"https://localhost:7256/api/Image/{person.ID}/{person.ImagePath}";

            string jsonData = JsonConvert.SerializeObject(dt);


            return Ok(jsonData);
        }

        [HttpGet("All")]
        public IActionResult GetAll()
        {
            PersonDTO personDTO = new PersonDTO();  
            List<PersonDTO> people = Person.GetAll();
            var dt = Person.GetAllPeople();
            if (dt == null)
            {
                return NotFound($"There's No People In The System");
            }

            return Ok(people);
        }

        // GET: api/people/{nationalNumber}
        [HttpGet("NationalNumber/{nationalNumber}")]
        public IActionResult GetPerson(string nationalNumber)
        {
            var person = Person.Find(nationalNumber);
            if (person == null)
            {
                return NotFound($"No person found with NationalNumber {nationalNumber}");
            }

            //JsonSerializerOptions 

            return Ok(person);
        }
    }
}
