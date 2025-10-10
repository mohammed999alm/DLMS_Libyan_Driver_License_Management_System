using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DLMS_BusinessLogicLayer
{

    enum enStatusMode { AddMode, UpdateMode }
    public class Phone
    {

        private int _id;
        public int ID { get { return _id; } }

        public int PersonID { get; set; }

        public string PhoneNumber { get; set; }



        private enStatusMode _mode;

        private Phone(int id, int personID, string phone)
        {
            _id = id;
            PersonID = personID;
            PhoneNumber = phone;
            _mode = enStatusMode.UpdateMode;
        }

        public Phone()
        {
            _id = -1;
            PersonID = -1;
            PhoneNumber = "";


            _mode = enStatusMode.AddMode;
        }

        public static Phone Find(int id)
        {

            int personID = -1;
            string phone = "";

            if (PhoneDataAccess.FindByPhoneID(id, ref phone, ref personID))
                return new Phone(id, personID, phone);

            return null;
        }

        public static Phone Find(string phone)
        {
            int id = -1;
            int personID = -1;

            if (PhoneDataAccess.FindByPhone(ref id, phone, ref personID))
                return new Phone(id, personID, phone);

            return null;
        }



        public static DataTable GetAllPhones()
        {
            return PhoneDataAccess.GetAllPhones();
        }

        public static DataTable FindPhonesByPersonID(int id)
        {
            return PhoneDataAccess.FindPhonesByPersonID(id);
        }

        private bool _AddNewPhone()
        {
            return (_id = PhoneDataAccess.AddPhone(PersonID, PhoneNumber)) > 0;
        }

        private bool _UpdatePhone()
        {
            return PhoneDataAccess.UpdatePhone(_id, PersonID, PhoneNumber);
        }

        public bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    return _AddNewPhone();

                case enStatusMode.UpdateMode:
                    return _UpdatePhone();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return PhoneDataAccess.IsExist(id);
        }

        public static bool IsExist(string phone)
        {
            return PhoneDataAccess.IsExist(phone);
        }

        public static bool IsExistByPersonID(int id)
        {
            return PhoneDataAccess.IsExistByPersonID(id);
        }



        public static bool DeletePhone(int id)
        {
            return PhoneDataAccess.DeletePhone(id);
        }

        public static bool DeletePhone(string phone)
        {
            return PhoneDataAccess.DeletePhone(phone);
        }

        public static bool DeletePhoneByPersonID(int personID)
        {
            return PhoneDataAccess.DeletePhoneByPersonID(personID);
        }
    }
}
