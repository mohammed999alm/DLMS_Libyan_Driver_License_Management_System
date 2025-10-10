using DLMS_DataAccessLayer;
using System.Data;
using System.Diagnostics;
using GlobalUtility;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using DLMS_DTO;



namespace DLMS_BusinessLogicLayer
{


    public class Person
    {

        private class Contact
        {
            public string PhoneNumber { get; set; }

            public string TempForUpdate {  get; set; }
            public string Email { get; set; }

            private DataTable _contactList;
            public int OwnerPersonID { get; set; }

            public Contact(int id, string phone, string email)
            {
                OwnerPersonID = id;
                PhoneNumber = phone;
                Email = email;
            }


            public Contact(int id)
            {
                OwnerPersonID = id;
            }

            private DataTable _GetPhones()
            {
                return Phone.FindPhonesByPersonID(OwnerPersonID);
            }

            private DataTable _GetEmail()
            {
                return EmailAddress.FindEmailAddresssByPersonID(OwnerPersonID);
            }


            public static bool DeletePhone(string phone)
            {
                return PhoneDataAccess.DeletePhone(phone);
            }

            public static bool DeleteEmail(string email)
            {
                return EmailAddress.DeleteEmailAddress(email);
            }

            public static bool DeleteContact(string columnCatpion, string contact)
            {
                if (columnCatpion == "البريد الإلكتروني")
                    return DeleteEmail(contact);
                else
                    return DeletePhone(contact);
            }

            private bool _MergeContactsInfo()
            {
                DataColumn emailColumn = new DataColumn("رقم الهاتف", typeof(string));
                DataColumn phoneColumn = new DataColumn("البريد الإلكتروني", typeof(string));

                _contactList = new DataTable();
                _contactList.Columns.Add(phoneColumn);
                _contactList.Columns.Add(emailColumn);

                DataTable emailList = _GetEmail();

                DataTable phoneList = _GetPhones();

                if (phoneList == null && emailList == null)
                    return false;

                try
                {

                    int rowsCounter = Math.Max(emailList?.Rows.Count ?? 0, phoneList?.Rows.Count ?? 0);

                    for (int i = 0; i < rowsCounter; ++i)
                    {
                        DataRow contactRow = _contactList.NewRow();

                        if (i < emailList?.Rows.Count)
                            contactRow["البريد الإلكتروني"] = emailList.Rows[i]["البريد الإلكتروني"];

                        if (i < phoneList?.Rows.Count)
                            contactRow["رقم الهاتف"] = phoneList.Rows[i]["رقم الهاتف"];


                        _contactList.Rows.Add(contactRow);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"{ex.Message}  :  {ex.ToString()}");
                    return false;
                }
            }
            public DataTable GetAllContacts()
            {
                return (_MergeContactsInfo()) ? _contactList : null;
            }

            private bool _AddNewPhone()
            {
                if (string.IsNullOrEmpty(PhoneNumber))
                    return false;

                Phone phone = new Phone { PersonID = OwnerPersonID, PhoneNumber = PhoneNumber };

                return phone.Save();
            }

            private bool _UpdatePhone()
            {
                if (string.IsNullOrEmpty(PhoneNumber))
                    return false;

                Phone phone = Phone.Find(TempForUpdate);

                phone.PhoneNumber = PhoneNumber;

                return phone?.Save() ?? false;
            }

            private bool _AddNewEmail()
            {
                if (string.IsNullOrEmpty(Email))
                    return false;

                EmailAddress email = new EmailAddress { PersonID = OwnerPersonID, Email = Email };

                return email.Save();
            }


            private bool _UpdateEmail()
            {
                if (string.IsNullOrEmpty(Email))
                    return false;

                EmailAddress email = EmailAddress.Find(TempForUpdate);

                email.Email = Email;

                return email?.Save() ?? false;
            }


            public bool CreateContact()
            {
                if (!string.IsNullOrEmpty(PhoneNumber) && !string.IsNullOrEmpty(Email))
                    return (_AddNewEmail() && _AddNewPhone());
                else
                    return (_AddNewEmail() || _AddNewPhone());
            }

            public bool UpdateContact()
            {
                return (_UpdatePhone() || _UpdateEmail());
            }


            public static bool DeleteContactByPersonID(int personID)
            {
                if (Phone.IsExistByPersonID(personID) && EmailAddress.IsExistByPersonID(personID))
                {
                    return Phone.DeletePhoneByPersonID(personID) && EmailAddress.DeleteEmailAddressByPersonID(personID);
                }

                else if (Phone.IsExistByPersonID(personID) || EmailAddress.IsExistByPersonID(personID))
                {
                    return Phone.DeletePhoneByPersonID(personID) || EmailAddress.DeleteEmailAddressByPersonID(personID);
                }

                return true;
            }




        }



        private int _id;
        public int ID { get { return _id; } }

        private Contact _contact;

        private Country country;

        private Municipality municipality;

        //[JsonPropertyName("NationalNumber")]
        public string NationalNumber { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string ThirdName { get; set; }
        public string LastName { get; set; }

        public string FullName { get { return $"{FirstName} {SecondName} {ThirdName} {LastName}"; } }

        private DateTime _dateOfBirth;
        public DateTime DateOfBirth
        {
            get { return _dateOfBirth; }

            set
            {
                _dateOfBirth = value;

                Age = (byte)Util.GetDifferenceInYears(_dateOfBirth, DateTime.Now);
            }
        }
        public string Gender { get; set; }

        public string Address { get; set; }

        public int NationalityID { get; set; }

        public int MunicipalityID { get; set; }

        private string _countryName;

        //[JsonIgnore]
        //public string CountryName { get { return _countryName; } }

        private string _municipalityName;


 
        public string? CountryName { get { return _countryName; } }

    
        public string? MunicipalityName { get { return _municipalityName; } }
        public string? ImagePath { get; set; }



        private enStatusMode _mode;

        

        public byte Age { get; private set; }


        private Person(int id, string nationalNumber, string firstName, string secondName,
            string thirdName, string lastName, DateTime dateOfBirth, string gender,
            string address, Country country, Municipality municipality, string imagePath)
        {
            _id = id;
            NationalNumber = nationalNumber;
            FirstName = firstName;
            SecondName = secondName;
            ThirdName = thirdName;
            LastName = lastName;
            DateOfBirth = dateOfBirth;
            Gender = gender;
            Address = address;

            this.country = country;
            this.municipality = municipality;

            NationalityID = country.ID;
            MunicipalityID = municipality.ID;

            _countryName = country.Name;
            _municipalityName = municipality.Name;

            ImagePath = imagePath;
            _mode = enStatusMode.UpdateMode;

            
            _contact = new Contact(id);
        }

        public Person(string phoneNumber, string email = "")
        {
            _id = -1;
            NationalNumber = "";
            FirstName = "";
            SecondName = "";
            ThirdName = "";
            LastName = "";
            DateOfBirth = DateTime.MinValue;
            Gender = "";
            Address = "";
            ImagePath = "";
            MunicipalityID = -1;
            NationalityID = -1;
            //country = null;
            //municipality = null;
            _mode = enStatusMode.AddMode;
            _contact = new Contact(_id, phoneNumber, email);

        }

        public Person()
        {
            _id = -1;
            //NationalNumber = "";
            //FirstName = "";
            //SecondName = "";
            //ThirdName = "";
            //LastName = "";
            //DateOfBirth = DateTime.MinValue;
            //Gender = "";
            //Address = "";
            //ImagePath = "";
            //MunicipalityID = -1;
            //NationalityID = -1;
            //country = null;
            //municipality = null;
            _mode = enStatusMode.AddMode;
        }


        public static Person Find(int id)
        {
            string nationalNumber = null,
                firstName = null,
                secondName = null,
                thirdName = null,
                lastName = null,
                address = null,
                imagePath = null;

            DateTime dateOfBirth = DateTime.MinValue;

            string gender = null;

            int nationalityID = 0, municipalityID = 0;



            if (PersonDataAccess.FindByPersonID(id, ref nationalNumber, ref firstName, ref secondName,
                ref thirdName, ref lastName, ref dateOfBirth, ref gender, ref address,
                ref nationalityID, ref municipalityID, ref imagePath))
            {

                return new Person(id, nationalNumber, firstName, secondName, thirdName, lastName,
                    dateOfBirth, gender, address,
                    Country.Find(nationalityID),
                    Municipality.Find(municipalityID), imagePath);
            }

            return null;
        }



        public static Person Find(string nationalNumber)
        {
            int id = -1;

            string firstName = null,
                secondName = null,
                thirdName = null,
                lastName = null,
                address = null,
                imagePath = null;

            DateTime dateOfBirth = DateTime.MinValue;

            string gender = null;

            int nationalityID = 0, municipalityID = 0;


            if (PersonDataAccess.FindByNationalNumber(ref id, nationalNumber, ref firstName, ref secondName,
                ref thirdName, ref lastName, ref dateOfBirth, ref gender, ref address,
                ref nationalityID, ref municipalityID, ref imagePath))
            {

                return new Person(id, nationalNumber, firstName, secondName, thirdName, lastName,
                    dateOfBirth, gender, address,
                    Country.Find(nationalityID),
                    Municipality.Find(municipalityID), imagePath);
            }

            return null;
        }



        public static DataTable GetPeople()
        {
            return PersonDataAccess.GetPeopleList();
        }



        public DataTable GetContactList()
        {
            return _contact?.GetAllContacts();
        }

        private bool _AddNewPerson()
        {
            if ((_id = PersonDataAccess.AddNewPerson(NationalNumber, FirstName, SecondName, ThirdName, LastName, DateOfBirth, Gender
               , Address, NationalityID, MunicipalityID, ImagePath)) > 0)
            {
                Country country = Country.Find(NationalityID);

                Municipality municipality = Municipality.Find(MunicipalityID);

                _countryName = country.Name;
                _municipalityName = municipality.Name;


                if (_contact != null)
                {
                    _contact.OwnerPersonID = _id;

                    _contact.CreateContact();
                }


                return true;
            }

            return false;
        }


        private bool _UpdatePerson()
        {
            if ((PersonDataAccess.UpdatePerson(_id, NationalNumber, FirstName, SecondName, ThirdName, LastName, DateOfBirth, Gender
            , Address, NationalityID, MunicipalityID, ImagePath)))
            {
                Country country = Country.Find(NationalityID);

                Municipality municipality = Municipality.Find(MunicipalityID);

                _countryName = country.Name;
                _municipalityName = municipality.Name;

                return true;
            }

            return false;
        }

        public bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    return _AddNewPerson();

                case enStatusMode.UpdateMode:
                    return _UpdatePerson();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return PersonDataAccess.IsExist(id);
        }

        public static bool IsExistPhone(string phone)
        {
            return Phone.IsExist(phone);
        }

        public static bool DeleteEmail(string email)
        {
            return EmailAddress.DeleteEmailAddress(email);
        }

        public static bool IsExistEmail(string email)
        {
            return EmailAddress.IsExist(email);
        }

        public static bool DeletePhone(string phone)
        {
            return Phone.DeletePhone(phone);
        }

        public bool CreateNewContact(string phone = "", string email = "")
        {
            if (_contact == null)
                _contact = new Contact(ID, phone, email);

            //if (_contact != null)
            //{
                _contact.Email = email;
                _contact.PhoneNumber = phone;

                return _contact.CreateContact();
            //}

            //return false;
        }

        public bool UpdateContact(string oldValue, string phone = "", string email = "")
        {
            if (_contact != null)
            {
                

                _contact.Email = email;
                _contact.PhoneNumber = phone;
                _contact.TempForUpdate = oldValue;

                return _contact.UpdateContact();
            }

            return false;
        }

        public static bool IsExist(string nationalNumber)
        {
            return PersonDataAccess.IsExist(nationalNumber);
        }


        public static bool DeletePerson(int id)
        {
            if (User.IsExistByPersonID(id) || Driver.IsExistByPersonID(id))
                return false;

            return PersonDataAccess.DeletePerson(id);
        }

 

        public bool HasRequests() 
        {
            return HasRequests(NationalNumber);
        }
        public static bool HasRequests(string nationalNumber)
        {
            return Request.IsRequestExistByNationalNumber(nationalNumber);
        }

      
        public static bool HasApplication(int id)
        {
            return ApplicationDataAccess.IsApplicationExistByPersonID(id);
        }

        public bool HasApplication()
        {
            return HasApplication(ID);
        }

        public bool DeleteContact(string columnCaption, string contact)
        {
            return Contact.DeleteContact(columnCaption, contact);
        }


        public bool HasActiveRequest() 
        {
            return HasActiveRequest(NationalNumber);
        }
        public static bool HasActiveRequest(string nationalNumber) 
        {
            return Request.IsActiveRequestExistByNationalNumber(nationalNumber);    
        }

        public bool HasActiveApplicationDeepSearch() 
        {
            return HasActiveActiveApplictionDeepSearch(NationalNumber);
        }

        public static bool HasActiveActiveApplictionDeepSearch(string nationalNumber) 
        {
            return Request.HasActiveApplication(nationalNumber);
        }
        public bool HasActiveApplication()
        {
            return ApplicationDataAccess.IsActiveApplicationExistByPersonID(ID);
        }

        public static bool HasActiveApplication(int personID)
        {
            return ApplicationDataAccess.IsActiveApplicationExistByPersonID(personID);
        }
        internal bool HasActiveRetakeTestApplication(string type)
        {
            return ApplicationDataAccess.IsActiveRetakeApplicationExistByPersonIDAndApplicationType(ID, type);
        }
        public int GetActiveApplicationID()
        {
            return ApplicationDataAccess.GetActiveApplicationIDByPersonID(_id);
        }


        public List<RequestDetailedDto> GetAllRequestsByNationalNumber()
        {
            return Request.GetAllRequestsByNationalNumber(NationalNumber);
        }


        internal int GetRequestIDFromTheLatestRequest() 
        {
            return Request.GetRequestIDFromTheLatestRequestByNationalNumber(NationalNumber);
        }


        public bool Equals(Person other) 
        {
            if (other is null) return false;

            return  
                   FullName == other.FullName &&
                   Address == other.Address &&
                   NationalityID == other.NationalityID &&
                   MunicipalityID == other.MunicipalityID &&
                   Gender == other.Gender &&
                   DateOfBirth == other.DateOfBirth &&
                   NationalNumber == other.NationalNumber &&
                   ImagePath == other.ImagePath;
        }
    }
}
