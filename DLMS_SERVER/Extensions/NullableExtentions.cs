namespace DLMS_SERVER.Extensions
{
    public static class NullableExtensions
    {

        public static int DefaultOrValue(this int? value, int defaultValue = 400) 
        {
            return value ?? defaultValue;   
        }
    }
}
