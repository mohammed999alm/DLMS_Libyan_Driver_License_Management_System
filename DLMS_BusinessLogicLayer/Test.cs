using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class Test
    {
        private int _id;

        public int ID { get { return _id; } }

        public bool Result { get; set; }

        public string? Notes { get; set; }

        public int TestAppointmentID { get; set; }

        public int CreatedByUserID { get; set; }

        private enStatusMode _mode;

        private Test(int id, string notes, int createdByUserID, int testAppointmentID, bool result)
        {
            _id = id;
            Notes = notes;
            CreatedByUserID = createdByUserID;
            Result = result;
            TestAppointmentID = testAppointmentID;

            _mode = enStatusMode.UpdateMode;
        }


        internal Test()
        {
            _id = -1;
            Notes = string.Empty;
            CreatedByUserID = -1;
            Result = false;
            TestAppointmentID = -1;

            _mode = enStatusMode.AddMode;
        }

        public static Test Find(int id)
        {
            string notes = string.Empty;

            bool result = false;

            int testAppointmentID = -1;

            int createdByUserID = -1;

            if (TestDataAccess.FindByTestID(id, ref notes, ref result, ref testAppointmentID, ref createdByUserID))
            {
                return new Test(id, notes, createdByUserID, testAppointmentID, result);
            }

            return null;
        }


        public static Test FindByAppointmentID(int testAppointmentID)
        {
            string notes = string.Empty;

            bool result = false;

            int id = -1;


            int createdByUserID = -1;

            if (TestDataAccess.FindByTestAppointmentID(ref id, ref notes, ref result,  testAppointmentID, ref createdByUserID))
            {
                return new Test(id, notes, createdByUserID, testAppointmentID, result);
            }

            return null;
        }


        private bool _AddNewTest()
        {
            return (_id = TestDataAccess.AddNewTest(Result, Notes, TestAppointmentID, CreatedByUserID)) > 0;
        }


        private bool _UpdateTest()
        {
            return TestDataAccess.UpdateTest(ID, Notes, Result);
        }


        public bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    {
                        if (_AddNewTest())
                        {
                            _mode = enStatusMode.UpdateMode;

                            return true;
                        }

                        return false;
                    }

                case enStatusMode.UpdateMode:
                    return _UpdateTest();

                default: return false;
            }
        }


        internal static bool DeleteByLocalLicenseApplicationID(int id)
        {
            return TestDataAccess.DeleteTestByLocalLicenseAppID(id);
        }


        public static bool IsPassedTestExist(int localLicenseApplicationID, int testTypeID)
        {
            return TestDataAccess.IsPassedTestExist(localLicenseApplicationID, testTypeID);
        }

        public static bool IsFailedTestExist(int localLicenseApplicationID, int testTypeID)
        {
            return TestDataAccess.IsFailedTestExist(localLicenseApplicationID, testTypeID);
        }

    }
}
