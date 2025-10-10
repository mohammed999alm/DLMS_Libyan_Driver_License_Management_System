//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.ModelBinding;

//namespace DLMS_SERVER.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ImageController : ControllerBase
//    {
//        private string _imageFolderPath = "www.root/Images";


//        [HttpGet("Images/{personID}/{imageName}")]
//        public IActionResult GetImageServer(int personID, string imageName)
//        {
//            string filePath = Path.Combine(_imageFolderPath, personID.ToString(), imageName);

//            if (!System.IO.File.Exists(filePath))
//            {
//                return NotFound();
//            }

//            var image = System.IO.File.OpenRead(filePath);
//            return File(image, "image/jpeg");
//        }




//        [HttpPost("upload/{id}/{imageName}")]
//        [Consumes("multipart/form-data")] // Corrected Content-Type header
//        public async Task<IActionResult> UploadImage(IFormFile imageFile, int id, string imageName)
//        {
//            string filePath = Path.Combine(_imageFolderPath, id.ToString());

//            if (imageFile == null || imageFile.Length == 0)
//            {
//                return BadRequest("No file uploaded or file is empty.");
//            }

//            if (!System.IO.Directory.Exists(filePath))
//            {
//                Directory.CreateDirectory(filePath);
//            }

//            try
//            {
//                filePath = Path.Combine(filePath, imageName);

//                // Log the file upload process
//                Console.WriteLine($"Uploading file: {imageFile.FileName} ({imageFile.Length} bytes)");

//                using (Stream stream = new FileStream(filePath, FileMode.Create))
//                {
//                    await imageFile.CopyToAsync(stream);
//                }

//                return Ok(new { FilePath = filePath });
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error: {ex.Message}");
//                return StatusCode(500, "Internal server error during file upload.");
//            }
//        }

//        //[HttpPost("upload/{id}/{imageName}")]
//        //[Consumes("multipart/from-data")]
//        //public async Task<IActionResult> UploadImage(IFormFile imageFile, int id, string imageName)
//        //{
//        //    string filePath = Path.Combine(_imageFolderPath, id.ToString());

//        //    if (imageFile == null || imageFile.Length == 0)
//        //    {
//        //        return BadRequest("No file uploaded or file is empty.");
//        //    }

//        //    if (!System.IO.Directory.Exists(filePath))
//        //    {
//        //        Directory.CreateDirectory(filePath);
//        //    }

//        //    try
//        //    {
//        //        filePath = Path.Combine(filePath, imageName);

//        //        // Log the file upload process
//        //        Console.WriteLine($"Uploading file: {imageFile.FileName} ({imageFile.Length} bytes)");

//        //        using (Stream stream = new FileStream(filePath, FileMode.Create))
//        //        {
//        //            await imageFile.CopyToAsync(stream);

//        //        }

//        //        return Ok(new { FilePath = filePath });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"Error: {ex.Message}");
//        //        return StatusCode(500, "Internal server error during file upload.");
//        //    }
//        //}


//        //[HttpPost("upload")]
//        //[Consumes("multipart/from-data")]
//        //public async Task<IActionResult> UploadImage([FromForm] IFormFile imageFile)
//        //{
//        //    //string filePath = Path.Combine(_imageFolderPath, id.ToString());

//        //    if (imageFile == null || imageFile.Length == 0)
//        //    {
//        //        return BadRequest("No file uploaded or file is empty.");
//        //    }

//        //    if (!System.IO.Directory.Exists(_imageFolderPath))
//        //    {
//        //        Directory.CreateDirectory(_imageFolderPath);
//        //    }

//        //    try
//        //    {
//        //        string filePath = Path.Combine(_imageFolderPath, imageFile.FileName);

//        //        // Log the file upload process
//        //        Console.WriteLine($"Uploading file: {imageFile.FileName} ({imageFile.Length} bytes)");

//        //        using (Stream stream = new FileStream(filePath, FileMode.Create))
//        //        {
//        //            await imageFile.CopyToAsync(stream);

//        //        }

//        //        return Ok(new { FilePath = filePath });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"Error: {ex.Message}");
//        //        return StatusCode(500, "Internal server error during file upload.");
//        //    }
//        //}


//        ////[BindNever]
//        //[HttpPost("upload/{id}/{imageName}")]
//        //public async Task<IActionResult> UploadImage(IFormFile imageFile, string imageName)
//        //{
//        //    if (imageFile == null || imageFile.Length == 0)
//        //    {
//        //        return BadRequest("No file uploaded or file is empty.");
//        //    }

//        //    if (!System.IO.Directory.Exists(_imageFolderPath))
//        //    {
//        //        Directory.CreateDirectory(_imageFolderPath);
//        //    }

//        //    try
//        //    {
//        //        string filePath = Path.Combine(_imageFolderPath, imageName);

//        //        // Log the file upload process
//        //        Console.WriteLine($"Uploading file: {imageFile.FileName} ({imageFile.Length} bytes)");

//        //        using (Stream stream = new FileStream(filePath, FileMode.Create))
//        //        {
//        //            await imageFile.CopyToAsync(stream);

//        //        }

//        //        return Ok(new { FilePath = filePath });
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        Console.WriteLine($"Error: {ex.Message}");
//        //        return StatusCode(500, "Internal server error during file upload.");
//        //    }
//        //}


//        [HttpDelete("Delete/{id}/{imageName}")]
//        public async Task<IActionResult> DeleteImage(int id, string imageName)
//        {
//            string filePath = Path.Combine(_imageFolderPath, id.ToString(), imageName);



//            if (System.IO.Directory.Exists(filePath))
//            {
//                Directory.Delete(filePath, true);

//                return Ok("Image Deleted Successfully");
//            }

//            return NotFound("Image Not Found Be Sure About Image Path");
//        }


//        //[HttpOptions]
//        //public IActionResult HandlePreflight()
//        //{
//        //    return Ok();
//        //}




//    }
//}
