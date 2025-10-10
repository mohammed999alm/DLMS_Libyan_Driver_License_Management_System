using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DataAccessLayer;
using DTO_Layer;

namespace BussinessLogicLayer
{
    public class Person
    {

        public string NationalNumber { get; set; }
        public int ID { get; set; }

        public string FirstName { get; set; }

        public string SecondName { get; set; }

        public string ThirdName { get; set; }
        public string LastName { get; set; }

        public DateTime DateOfBirth { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }

        public string Address { get; set; }

        public string Gender { get; set; }

        public int NationalityID { get; set; }

        public int MunicipalityID { get; set; }

        public string MunicipalityName {  get; set; }

        public string Country { get; set; }
        public string ImagePath { get; set; }

        public static DataTable GetAllPeople()
        {
            //DataAccessLayer.DataAccessSettings.Initialize();
            return PeopleDataAccess.GetAllPeople();
        }

        public static List<PersonDTO> GetAll()
        {
            //DataAccessLayer.DataAccessSettings.Initialize();
            return PeopleDataAccess.GetAllPeopleDto();
        }


        private Person(int id, string nationalNumber, string firstName, string secondName,
            string thirdName, string lastName, string gender, string address, string phone,
            string email, int nationalID, string country, int municipalityID, string municipalityName, string imagePath, DateTime dateOfBirth)
        {
            ID = id;
            NationalNumber = nationalNumber;
            FirstName = firstName;
            SecondName = secondName;
            ThirdName = thirdName;
            LastName = lastName;
            Email = email;
            PhoneNumber = phone;
            Address = address;
            Gender = gender;
            NationalityID = nationalID;
            MunicipalityID = municipalityID;
            MunicipalityName = municipalityName;
            Country = country;
            ImagePath = imagePath;
            DateOfBirth = dateOfBirth;
        }


        public static Person Find(int id)
        {
            string nationalNumber = "";
            string firstname = string.Empty;
            string secondname = string.Empty;
            string thirdname = string.Empty;
            string lastName = string.Empty;
            string gender = string.Empty;
            string address = string.Empty;
            string phone = string.Empty;
            string email = string.Empty;
            int nationalID = 0;
            int municipalityID = 0;
            string municipalityName = string.Empty;
            string country = string.Empty;
            string imagePath = string.Empty;
            DateTime dateOfBirth = DateTime.MinValue;

            if (PeopleDataAccess.FindPeople(id, ref nationalNumber, ref firstname, ref secondname, ref thirdname,
                ref lastName, ref gender, ref address, ref phone, ref email, ref nationalID, ref country,ref municipalityID, ref municipalityName,
                ref imagePath, ref dateOfBirth))
            {
                return new Person(id, nationalNumber, firstname, secondname,
                    thirdname, lastName, gender, address, phone, email, nationalID, country, municipalityID, municipalityName, imagePath, dateOfBirth);
            }

            return null;
        }


        public static Person Find(string nationalNumber)
        {
            int id = -1;
            string firstname = string.Empty;
            string secondname = string.Empty;
            string thirdname = string.Empty;
            string lastName = string.Empty;
            string gender = string.Empty;
            string address = string.Empty;
            string phone = string.Empty;
            string email = string.Empty;
            int nationalID = 0;
            int municipalityID = 0;
            string municipalityName = string.Empty;
            string country = string.Empty;
            string imagePath = string.Empty;

            DateTime dateOfBirth = DateTime.MinValue;

            if (PeopleDataAccess.FindPeopleByNationalNumber(ref id, nationalNumber, ref firstname, ref secondname, ref thirdname,
                ref lastName, ref gender, ref address, ref phone, ref email, ref nationalID, ref country, ref municipalityID,ref municipalityName,
                ref imagePath ,ref dateOfBirth))
            {
                return new Person(id, nationalNumber, firstname, secondname,
                      thirdname, lastName, gender, address, phone, email, nationalID, country, municipalityID, municipalityName, imagePath, dateOfBirth);
            }

            return null;
        }
    }
}
