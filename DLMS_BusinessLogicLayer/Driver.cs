using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class Driver
    {
        private int _id;
        public int ID { get { return _id; } }

        public int PersonID { get; set; }

        public Person? person { get; private set; }
        public int CreatedByUserID { get; set; }

        public DateTime CreationDate { get; set; }

        public byte NumberActiveLicense { get; private set; }

        private enStatusMode _mode;

        private Driver(int id, int personID, int createdByUserID, DateTime creationDate)
        {
            _id = id;
            PersonID = personID;
            person = Person.Find(personID);

            CreatedByUserID = createdByUserID;

            NumberActiveLicense = (byte)GetNumberOfActiveLicenses();

            CreationDate = creationDate;

            _mode = enStatusMode.UpdateMode;
        }

        public Driver()
        {
            _id = -1;
            PersonID = -1;
            CreatedByUserID = -1;
            CreationDate = DateTime.MinValue;

            _mode = enStatusMode.AddMode;
        }

        public static Driver Find(int id)
        {

            int personID = -1;
            int createdByUserID = -1;
            DateTime creationDate = DateTime.MinValue;

            if (DriverDataAccess.FindByDriverID(id, ref personID, ref createdByUserID, ref creationDate))
                return new Driver(id, personID, createdByUserID, creationDate);

            return null;
        }

        public static Driver FindByPersonId(int personID)
        {

            int id = -1;
            int createdByUserID = -1;
            DateTime creationDate = DateTime.MinValue;

            if (DriverDataAccess.FindByPersonID(ref id, personID, ref createdByUserID, ref creationDate))
                return new Driver(id, personID, createdByUserID, creationDate);

            return null;
        }



        public static DataTable GetAllDrivers()
        {
            return DriverDataAccess.GetAll();
        }


        private bool _AddNewDriver()
        {
            return (_id = DriverDataAccess.AddDriver(PersonID, CreatedByUserID, CreationDate)) > 0;
        }

        //private bool _UpdateDriver()
        //{
        //    return DriverDataAccess.UpdateDriver(_id, PersonID, DriverNumber);
        //}

        public bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    return _AddNewDriver();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return DriverDataAccess.IsExist(id);
        }


 

        public static bool IsExistByPersonID(int id)
        {
            return DriverDataAccess.IsExistByPersonID(id);
        }

        public static int GetDriverIDByPersonID(int id)
        {
            return DriverDataAccess.GetDriverIDByPersonID(id);
        }

        public static int GetNumberOfActiveLicense(int id)
        {
            return DriverDataAccess.GetNumberOfActiveLicensesByDriverID(id);
        }

        private int GetNumberOfActiveLicenses()
        {
            return GetNumberOfActiveLicense(_id);
        }

        public DataTable GetLicenses()
        {
            return License.GetAll(ID);
        }


        public DataTable GetInternationalLicenses()
        {
            return InternationalLicense.GetAll(ID);
        }





        //public static bool DeleteDriver(int id)
        //{
        //    return DriverDataAccess.DeleteDriver(id);
        //}

        //public static bool DeleteDriver(string Driver)
        //{
        //    return DriverDataAccess.DeleteDriver(Driver);
        //}

        //public static bool DeleteDriverByPersonID(int personID)
        //{
        //    return DriverDataAccess.DeleteDriverByPersonID(personID);
        //}
    }
}
