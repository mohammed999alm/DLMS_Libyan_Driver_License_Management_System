using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_DTO
{

    public class LicenseClassDto
    {
        public int ID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public byte Age { get; set; }

        public byte ValidatyLength { get; set; }

        public decimal Fees { get; set; }
    }
}
