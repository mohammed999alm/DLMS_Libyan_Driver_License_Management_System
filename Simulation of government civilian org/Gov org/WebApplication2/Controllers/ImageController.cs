using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BussinessLogicLayer;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication2.Controllers
{
    [Authorize(Roles = "مدير النظام")]
    [ApiController]
    [Route("api/[controller]")]
    public class ImageController : ControllerBase
    {

        private string _imagePath = "root/images";

        [HttpGet("{personID}/{imageName}")]
        public IActionResult GetImage(int personID, string imageName) 
        {
            string filePath = Path.Combine(_imagePath, personID.ToString(), imageName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            var image = System.IO.File.OpenRead(filePath);
            return File(image, "image/jpeg");
        }

        [HttpGet("test/{id}")]
        public IActionResult Test(int id)
        {
            Person person = Person.Find(id);
            if (person == null) 
            {
                return NotFound();
            }

            GetImage(person.ID, person.ImagePath);
            return Ok("Test controller is working!");
        }


        [HttpPost("upload/{imageName}")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile imageFile, string imageName)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return BadRequest("No file uploaded or file is empty.");
            }

            if (!System.IO.Directory.Exists(_imagePath))
            {
                Directory.CreateDirectory(_imagePath);
            }

            try
            {
                string filePath = Path.Combine(_imagePath, imageName);

                // Log the file upload process
                Console.WriteLine($"Uploading file: {imageFile.FileName} ({imageFile.Length} bytes)");

                using (Stream stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);

                }

                return Ok(new { FilePath = filePath });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, "Internal server error during file upload.");
            }
        }
    }
}
