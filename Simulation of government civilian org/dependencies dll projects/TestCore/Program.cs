//// See https://aka.ms/new-console-template for more information
//Console.WriteLine("Hello, World!");



using  System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Azure;
using BussinessLogicLayer;


namespace TestCore 
{
    internal class Test 
    {


        static string ReadPhoneNumber(string message) 
        {
            string prefix = "+218";

            Console.WriteLine(message);

            string? phoneNumber = Console.ReadLine();

            if (phoneNumber?.StartsWith("0") ?? false) 
                phoneNumber = phoneNumber.Substring(1);

            return prefix + phoneNumber;
        }
        static bool IsPhoneNumberValid(string phoneNumber, string[] companyCode) 
        {
            
            char[] phoneNumberString = phoneNumber.ToCharArray();

            if (phoneNumberString.Length != 13) return false;

            for (short i = 0; i < phoneNumberString.Length; i++) 
            {
                if (i == 0 && phoneNumberString[i] != '+') 
                {
                    return false;
                }

                if (i > 0 && !char.IsDigit(phoneNumberString[i]))
                    return false;
            }

            foreach (string code in companyCode)
            {
                if (phoneNumber.StartsWith($"+218{code}"))
                {
                    return true;
                }
            }
            return false;
        }

        static string[] CompanyPrefixCodeNumber() 
        {
            return new[]{ "91", "92", "93", "94", "95"};
        }

        static bool IsPhoneNumberValid(string phoneNumber) 
        {
            return Regex.IsMatch(phoneNumber, @"^\+218(91|92|93|94|95)\d{7}$");

        }

  

        static bool IsNationalNumberValid(string nationalNumber, string  gender, string yearOfBirth) 
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


        static bool IsEmailAddressValid(string email) 
        {
            return Regex.IsMatch(email, @"^[a-zA-Z0-9_.-]+@(gmail|hotmail|yahoo|outlook)\.com$");
        }

        public  class Person 
        {
            public int ID { get; set; }
            public string FirstName { get; set; }

            public string LastName { get; set; }

            public bool IsMarried { get; set; }


            public void PrintPersonCard() 
            {
                Console.WriteLine("\n\n_________________________________________________________");
                Console.WriteLine("ID          :  {0}", ID);
                Console.WriteLine("First Name  :  {0}", FirstName);
                Console.WriteLine("Last Name   :  {0}", LastName);
                Console.WriteLine("Is Married  :  {0}", IsMarried);
                Console.WriteLine("_________________________________________________________");

            }
        }

        interface IApplicationService
        {
            public void Validate(int age);


            public void CommonValidation(int age) 
            {
                if (age < 18)
                    Console.WriteLine("Nope you can't be a driver man you are still underage brat");
                else
                    Console.WriteLine("You are not a brat but we will see if you could get license or not");
            }
        }

        class NewLicenseApp : IApplicationService 
        {

            public  void Validate(int age) 
            {

                ((IApplicationService)this).CommonValidation(age);

                if (age > 80)
                    Console.WriteLine("Unfortunately you could not got driver license because you are too old to get a one");
                else
                    Console.WriteLine("Nice you can get your driver license now congratulation");
            }
        }
        public static void Main(string[] args)
        {


            NewLicenseApp app = new NewLicenseApp();

            app.Validate(85);
            return;
            //string phone = ReadPhoneNumber("Please Enter Your Phone Number ? : ");


            //if (IsPhoneNumberValid(phone, CompanyPrefixCodeNumber()))
            //{
            //    Console.WriteLine($"This is {phone} valid PhoneNumber");
            //}
            //else 
            //{
            //    Console.WriteLine("Not Valid Phone Number");
            //}

            //if (IsPhoneNumberValid(phone))
            //{
            //    Console.WriteLine($"This is {phone} valid PhoneNumber");
            //}
            //else
            //{
            //    Console.WriteLine("Not Valid Phone Number");
            //}


            //string nationalNumber = "119960874542";

            //if (IsNationalNumberValid(nationalNumber, "ذكر", "1996"))
            //{
            //    Console.WriteLine("It's a valid national number");
            //}
            //else 
            //{
            //    Console.WriteLine("No it's not a valid national number");
            //}




            //string email = "980MohammedAlm999@outlook.com";

            //if (IsEmailAddressValid(email))
            //{
            //    Console.WriteLine("It's {0} a valid Email Address", email);
            //}
            //else
            //{
            //    Console.WriteLine("No it's not a valid Email Address");
            //}

            Console.WriteLine("Hello World");

            //CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            //var cultureInfo = new CultureInfo("en-US");
            //CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            //CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;
            //DataTable dt = Person.GetAllPeople();




            //string fullName;
            //foreach (DataRow row in dt.Rows)
            //{
            //    fullName = $"{row["FirstName"]} {row["LastName"]}";
            //    //fullName = row["FirstName"].ToString() + row["SecondName"].ToString(); 
            //    Console.WriteLine(
            //        "Full Name : {0, -25}  | Phone Number : {1, -25}", fullName, row["PhoneNumber"]
            //       );
            //}



            List<Person> list = new List<Person>() { new Person { ID = 1, FirstName = "Mohammed", LastName = "Ali", IsMarried = false} ,
                new Person { ID = 2, FirstName = "Alia", LastName = "Jamil", IsMarried = true }
            ,new Person { ID = 3, FirstName = "Samira", LastName = "Ali", IsMarried = true}
            ,new Person { ID = 4, FirstName = "Mohammed", LastName = "Abdu", IsMarried = true}
            , new Person { ID = 5, FirstName = "Samer", LastName = "Ali", IsMarried = true}
            , new Person { ID = 6, FirstName = "Salma", LastName = "Mahmood", IsMarried = true}
            , new Person { ID = 7, FirstName = "Adam", LastName = "Smith", IsMarried = false} };


            List<Person> copy;

            copy = (from u in list where !u.IsMarried orderby u.ID descending select u).ToList();


            foreach (var c in copy)
            {
                c.PrintPersonCard();
            }
        }


       
    }
}

