using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {

        private string _imagePath = "root/images";

        [HttpGet("{personID}/{imageName}")]
        IActionResult GetImage(int personID, string imageName)
        {
            string filePath = Path.Combine(_imagePath, personID.ToString(), imageName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var image = System.IO.File.OpenRead(filePath);
            return File(image, "image/jpeg");
        }
    }
}
