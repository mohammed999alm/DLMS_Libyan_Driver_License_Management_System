namespace DLMS_SERVER.ValidatorClasses
{
    public class ValidateResult
    {

        public bool IsValid {  get; set; }  
        public string? ErrorMessage { get; set; }

        public int? StatusCode { get; set; }


        public static ValidateResult Success() => new ValidateResult { IsValid = true };

        public static ValidateResult Fail (int statusCode, string errMsg) => new ValidateResult { IsValid = false, ErrorMessage = errMsg, StatusCode = statusCode};
    }
}
