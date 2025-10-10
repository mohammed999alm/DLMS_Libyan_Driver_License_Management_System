using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_DTO
{
    public class RequestDto
    {
        [Required]
        public string NationalNumber { get; set; }

        [Required]
        public int RequestTypeID { get; set; }

        [Required]
        public int LicenseClassID { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
    }

    public class RequestDto2 
    {
        [Required]
        public string NationalNumber { get; set; }

        [Required]
        public int RequestTypeID { get; set; }

        [Required]
        public int LicenseID { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }
    }


    public class RequestDetailedDto
    {
        public int RequestID { get; set; }                  
        public string NationalNumber { get; set; }            
        public string ApplicantName { get; set; }            
        public string ApplicationTypeTitle { get; set; }      
        public string? ClassName { get; set; }                
        public int? LicenseID { get; set; }                   
        public string StatusType { get; set; }              
        public DateTime CreatedDate { get; set; }            
        public DateTime? PaymentTimeStamp { get; set; }      
        public DateTime? UpdatedDate { get; set; }           

        public decimal? FeesAmount { get; set; }             
        public bool? IsPaid { get; set; }                    
    }
}
