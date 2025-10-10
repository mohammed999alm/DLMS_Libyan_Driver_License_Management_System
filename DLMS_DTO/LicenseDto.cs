using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DLMS_DTO;

public class LicenseDto
{
	public int ApplicationID { get; set; }
	public int DriverID { get; set; }
	public int LicenseClassID { get; set; }
	public int IssueReasonID { get; set; } 
	public DateTime IssueDate { get; set; }
	public DateTime ExpirationDate { get; set; }
	public bool IsActive { get; set; }
	public string Notes { get; set; } = string.Empty;
	public decimal PaidFess { get; set; }
	public int CreatedByUserID { get; set; }



}
