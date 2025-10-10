using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class EmailAddress
    {

        private int _id;
        public int ID { get { return _id; } }

        public int PersonID { get; set; }

        public string Email { get; set; }



        private enStatusMode _mode;

        private EmailAddress(int id, int personID, string EmailAddress)
        {
            _id = id;
            PersonID = personID;
            Email = EmailAddress;
            _mode = enStatusMode.UpdateMode;
        }

        public EmailAddress()
        {
            _id = -1;
            PersonID = -1;
            Email = "";


            _mode = enStatusMode.AddMode;
        }

        public static EmailAddress Find(int id)
        {

            int personID = -1;
            string EmailAddress = "";

            if (EmailAddressDataAccess.FindByEmailAddressID(id, ref EmailAddress, ref personID))
                return new EmailAddress(id, personID, EmailAddress);

            return null;
        }

        public static EmailAddress Find(string EmailAddress)
        {
            int id = -1;
            int personID = -1;

            if (EmailAddressDataAccess.FindByEmailAddress(ref id, EmailAddress, ref personID))
                return new EmailAddress(id, personID, EmailAddress);

            return null;
        }



        public static DataTable GetAllEmailAddresss()
        {
            return EmailAddressDataAccess.GetAllEmailAddresss();
        }

        public static DataTable FindEmailAddresssByPersonID(int id)
        {
            return EmailAddressDataAccess.FindEmailAddresssByPersonID(id);
        }

        private bool _AddNewEmailAddress()
        {
            return (_id = EmailAddressDataAccess.AddEmailAddress(PersonID, Email)) > 0;
        }

        private bool _UpdateEmailAddress()
        {
            return EmailAddressDataAccess.UpdateEmailAddress(_id, PersonID, Email);
        }

        public bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    return _AddNewEmailAddress();

                case enStatusMode.UpdateMode:
                    return _UpdateEmailAddress();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return EmailAddressDataAccess.IsExist(id);
        }

        public static bool IsExist(string email)
        {
            return EmailAddressDataAccess.IsExist(email);
        }

        public static bool IsExistByPersonID(int id)
        {
            return EmailAddressDataAccess.IsExistByPersonID(id);
        }
        public static bool DeleteEmailAddress(int id)
        {
            return EmailAddressDataAccess.DeleteEmailAddress(id);
        }

        public static bool DeleteEmailAddress(string email)
        {
            return EmailAddressDataAccess.DeleteEmailAddress(email);
        }

        public static bool DeleteEmailAddressByPersonID(int personID)
        {
            return EmailAddressDataAccess.DeleteEmailAddressByPersonID(personID);
        }
    }
}
