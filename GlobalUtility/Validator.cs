using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace GlobalUtility
{
    public static  class Validator
    {


        static string SetPhoneNumber(string? phoneNumber)
        {
            string prefix = "+218";

            if (phoneNumber?.StartsWith("0") ?? false)
                phoneNumber = phoneNumber.Substring(1);

            return prefix + phoneNumber;
        }
        public static bool IsPhoneNumberValid(string? phoneNumber)
        {
            phoneNumber = SetPhoneNumber(phoneNumber);

            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrWhiteSpace(phoneNumber))
                return false;
            
            return Regex.IsMatch(phoneNumber, @"^\+218(91|92|93|94|95)\d{7}$");
        }



        public static bool IsNationalNumberValid(string nationalNumber, string gender, string yearOfBirth)
        {
            if (gender.ToLower() == "ذكر")
            {
                if (Regex.IsMatch(nationalNumber, $@"^1{yearOfBirth}\d{{7}}$"))
                {
                    return true;
                }
            }
            else if (gender.ToLower() == "أنثى")
            {
                if (Regex.IsMatch(nationalNumber, $@"^2{yearOfBirth}\d{{7}}$"))
                {
                    return true;
                }
            }

            return false;
        }


        public static bool IsEmailAddressValid(string? email)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
                return false;

            return Regex.IsMatch(email, @"^[a-zA-Z0-9_.-]+@(gmail|hotmail|yahoo|outlook)\.com$");
        }
    }
}
