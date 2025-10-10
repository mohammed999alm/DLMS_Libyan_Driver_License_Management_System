using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_DTO;

public class CitizenRequestStatusDto
{

    public int RequestID { get; set; }
    public string NationalNumber { get; set; }  
    public string Status { get; set; }  

    public string Message { get; set; }

    public string Type { get; set; }    

    public string CreateAt { get; set; }

    public CitizenRequestStatusDto(int requestID, string nationalNumber, string status, string message, string type, string createAt)
    {
        RequestID = requestID;
        NationalNumber = nationalNumber;
        Status = status;
        Message = message;
        Type = type;
        CreateAt = createAt;
    }
}



