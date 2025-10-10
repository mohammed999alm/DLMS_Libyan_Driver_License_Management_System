namespace DLMS_DTO
{
    public class ApplicationDto
    {
        public int ID { get; set; }
        public int PersonID { get; set; }
        public string? Type { get; set; }
        public int TypeID { get; set; }
        public string? Status { get; set; }
        public int CreatedByUserID { get; set; }
        public string? CreatedByUser { get; set; }
        public int? UpdatedByUserID { get; set; }
        public string? UpdatedByUser { get; set; }
        public decimal? PaidFees { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? LastStatusDate { get; set; }
        public int? RequestID { get; set; }
    }

}
