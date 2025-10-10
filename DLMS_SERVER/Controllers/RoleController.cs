using DLMS_BusinessLogicLayer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Authorization;

namespace DLMS_SERVER.Controllers
{
    [Authorize(Roles = "مدير النظام")]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        [HttpGet("All")] public IActionResult GetAll()
        
        {
        
            DataTable roles = Role.GetAll();

            if (roles == null || roles.Rows.Count == 0) return NotFound("ليس هناك اي امتيازات في النظام");

            string jsonRolesFormat = JsonConvert.SerializeObject(roles);

            return Ok(jsonRolesFormat);
        }


        [HttpGet("FindByRoleName/{roleTag}")] public IActionResult GetByRoleTag(string roleTag)  
        {
            Role role = Role.Find(roleTag);

            if (role == null) return NotFound("هذا الإمتياز غير مسجل في النظام");

            return Ok(role);
        }

    }
}
