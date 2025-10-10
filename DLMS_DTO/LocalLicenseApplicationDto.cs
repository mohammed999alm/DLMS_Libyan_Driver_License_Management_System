using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_DTO
{
    public class LocalLicenseApplicationDto : ApplicationDto
    {
        public int LocalAppID { get; set; } 

        public int? PassedTests {  get; set; }   

        public int LicenseClassID { get; set; }

        public string? LicenseClassName { get; set; }
    }
}
