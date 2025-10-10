
using System.Security.Cryptography;
using System.Text;

namespace GlobalUtility
{
    public static class Util
    {

        public static int GetDifferenceInYears(DateTime startDate, DateTime endDate)
        {
            int years = endDate.Year - startDate.Year;

            if (endDate.Month == startDate.Month && endDate.Day < startDate.Day ||
                endDate.Month < startDate.Month)
                years--;

            return years;
        }

        public static string HashPassword(string password) 
        {
            SHA256 sha512 = SHA256.Create();    

            string hashedPassword = BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-","").ToLower();

            return hashedPassword;
        }
    }
}
