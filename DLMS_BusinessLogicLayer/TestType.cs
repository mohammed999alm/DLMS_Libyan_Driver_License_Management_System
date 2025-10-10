using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class TestType
    {

        private int _id;
        public int ID { get { return _id; } }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Fees { get; set; }



        private enStatusMode _mode;

        private TestType(int id, string name, string description, decimal fees)
        {
            _id = id;
            Name = name;
            Description = description;
            Fees = fees;

            _mode = enStatusMode.UpdateMode;
        }

        //public TestType()
        //{
        //    _id = -1;
        //    Name = "";
        //    Description = "";
        //    Fees = 0;

        //    _mode = enStatusMode.AddMode;
        //}

        public static TestType Find(int id)
        {

            string name = "";
            string desc = "";
            decimal fees = 0;

            if (TestTypeDataAccess.FindByTestTypeID(id, ref name, ref desc, ref fees))
                return new TestType(id, name, desc, fees);

            return null;
        }
        public static TestType Find(string name)
        {

            int id = -1;
            string desc = "";
            decimal fees = 0;

            if (TestTypeDataAccess.FindByTestTypeName(ref id, name, ref desc, ref fees))
                return new TestType(id, name, desc, fees);

            return null;
        }


        public static DataTable GetAllTestTypes()
        {
            return TestTypeDataAccess.GetAllTestTypes();
        }



        //private bool _AddNewTestType()
        //{
        //    return (_id = TestTypeDataAccess.AddNewTestType(Name, Description, Fees)) > 0;
        //}

        private bool _UpdateTestType()
        {
            return TestTypeDataAccess.UpdateTestType(_id, Description, Fees);
        }

        public bool Save()
        {
            switch (_mode)
            {
                //case enStatusMode.AddMode:
                //    {
                //        if (_AddNewTestType())
                //        {
                //            _mode = enStatusMode.UpdateMode;
                //            return true;
                //        }

                //        return false;
                //    }

                case enStatusMode.UpdateMode:
                    return _UpdateTestType();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return TestTypeDataAccess.IsExist(id);
        }

        public static bool IsExist(string TestType)
        {
            return TestTypeDataAccess.IsExist(TestType);
        }


        public static List<string>? GetAllTypes() 
        {
            return TestTypeDataAccess.GetAllTypes();
        }

        public static DataTable GetAll() 
        {
            return TestTypeDataAccess.GetAllTestTypes();
        }

        //public static bool DeleteTestType(int id)
        //{
        //    return TestTypeDataAccess.DeleteTestType(id);
        //}

    }
}
