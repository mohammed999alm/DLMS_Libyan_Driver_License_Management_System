using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    // Open endpoint, accessible to everyone
    [HttpGet("open")]
    public IActionResult OpenEndpoint()
    {
        return Ok("Anyone can access this endpoint.");
    }

    // Protected endpoint, requires authentication and Admin role
    [Authorize(Roles = "مدير النظام")]
    [HttpGet("protected")]
    public IActionResult ProtectedEndpoint()
    {
        return Ok("Only Admins can access this endpoint.");
    }


}