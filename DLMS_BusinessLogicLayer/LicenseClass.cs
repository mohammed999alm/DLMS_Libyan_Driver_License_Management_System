using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DLMS_DataAccessLayer;

namespace DLMS_BusinessLogicLayer
{
    public class LicenseClass 
    {

        private int _id;

        public int ID { get { return _id; } }

        public string Name { get; set; }

        public string Description { get; set; }

        public byte Age { get; set; }

        public byte ValidatyLength { get; set; }

        public decimal Fees { get; set; }

        private enStatusMode _mode;
        private LicenseClass(int id, string name, string description, byte age, byte validatyRange, decimal fees)
        {
            _id = id;
            Name = name;
            Description = description;
            Age = age;
            ValidatyLength = validatyRange;
            Fees = fees;

            _mode = enStatusMode.UpdateMode;
        }


        public LicenseClass()
        {
            _id = -1;
            Name = string.Empty;
            Description = string.Empty;

            Age = 18;

            ValidatyLength = 0;

            Fees = 0;

            _mode = enStatusMode.AddMode;
        }


        public static LicenseClass Find(int id)
        {

            string name = null;
            string description = null;
            byte age = 0;
            byte validatyRange = 0;
            decimal fees = 0;

            if (LicenseClassDataAccess.FindByLicenseClassID(id, ref name, ref description,
                ref age, ref validatyRange, ref fees))
            {
                return new LicenseClass(id, name, description, age, validatyRange, fees);
            }

            return null;
        }

        public static LicenseClass Find(string name)
        {

            int id = -1;
            string description = null;
            byte age = 0;
            byte validatyRange = 0;
            decimal fees = 0;

            if (LicenseClassDataAccess.FindByLicenseClassName(ref id, name, ref description,
                ref age, ref validatyRange, ref fees))
            {
                return new LicenseClass(id, name, description, age, validatyRange, fees);
            }

            return null;
        }

        public static DataTable GetAll()
        {
            return LicenseClassDataAccess.GetAllLicenseClasses();
        }

        public static List<string> GetAllClasses()
        {
            return LicenseClassDataAccess.GetAll();
        }

        public static List<DLMS_DTO.LicenseClassDto> GetAllWithDetails() 
        {
            return LicenseClassDataAccess.GetAllWithDetails();
        }


        //private bool _AddNewLicenseClass() 
        //{
        //    return (_id = LicenseClassDataAccess.AddNewLicenseClass(Name, Description, Age, ValidatyLength, Fees)) > 0;
        //}

        private bool _UpdateLicenseClass()
        {
            return LicenseClassDataAccess.UpdateLicenseClass(ID, Description, Age, ValidatyLength, Fees);
        }


        public bool Save()
        {
            switch (_mode)
            {

                //case enStatusMode.AddMode: 
                //    {
                //        if (_AddNewLicenseClass()) 
                //        {
                //            _mode = enStatusMode.UpdateMode;

                //            return true;
                //        }

                //        return false;
                //    }

                case enStatusMode.UpdateMode:
                    return _UpdateLicenseClass();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return LicenseClassDataAccess.IsExist(id);
        }

        public static bool IsExist(string name)
        {
            return LicenseClassDataAccess.IsExist(name);
        }


        public  bool Equals(LicenseClass other)
        {
            if (other == null) return false;
            return Description == other.Description &&
                   Fees == other.Fees &&
                   ValidatyLength == other.ValidatyLength &&
                   Age == other.Age &&
                   Name == other.Name
                   ;
        }

    

        //public static bool DeleteByID(int id) 
        //{
        //    return LicenseClassDataAccess.DeleteLicenseClass(id);
        //}
    }
}
