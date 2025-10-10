using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DLMS_DataAccessLayer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using static System.Net.Mime.MediaTypeNames;

namespace DLMS_BusinessLogicLayer
{
    public class TestAppointment
    {

        private enStatusMode _mode;

        private int _id;

        public int ID { get { return _id; } }

        private int _localLicenseApplicationID;
        public int LocalLicenseApplicationID
        {
            get { return _localLicenseApplicationID; }

            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _localLicenseApplicationID = value;
                    application = LocalLicenseApplication.Find(value, false);

                    if (application != null)
                    {
                        _licenseClassName = application.LicenseClassName;
                    }
                }
            }
        }


        [JsonIgnore]
        [BindNever]
        public LocalLicenseApplication? application { get; private set; }

        private int _testTypeID;
        public int TestTypeID
        {
            get { return _testTypeID; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _testTypeID = value;

                    _testType = TestType.Find(_testTypeID)?.Name;

                    _fees = TestType.Find(_testTypeID)?.Fees ?? 0;
                }
            }
        }


        private string? _licenseClassName;

        public string? LicenseClassName { get { return _licenseClassName; } }

        private DateTime _appointmentDate;

        public DateTime AppointmentDate
        {
            get { return _appointmentDate; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                    _appointmentDate = value;
                else if (_mode == enStatusMode.UpdateMode && !IsLocked)
                    _appointmentDate = value;
            }
        }

        private int _createdByUserID;

        public int CreatedByUserID
        {
            get { return _createdByUserID; }

            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _createdByUserID = value;
                }
                else if (_mode == enStatusMode.UpdateMode && !IsLocked) 
                {
                    
                }
            }
        }


        private int? _retakeTestID;

        public int? RetakeTestID
        {
            get { return _retakeTestID; }

            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _retakeTestID = value;
                }
            }
        }

        private string _testType;

        public string TestTypeName { get { return _testType; } }


        private bool _isLocked;

        public bool IsLocked
        {
            get { return _isLocked; }

            set
            {
                if (_mode == enStatusMode.UpdateMode)
                {
                    _isLocked = value;
                }
            }
        }

        private decimal _fees;

        public decimal Fees { get { return _fees; } }

        [JsonInclude]
      
        public Test? test { get; set; }


        private TestAppointment(int id, int localLicenseApplicationID, int testTypeID, DateTime appointmentDate,
           int createByUserID, int? retakeTestID, bool isLocked, decimal fees)
        {

            _id = id;
            LocalLicenseApplicationID = localLicenseApplicationID;
            TestTypeID = testTypeID;
            _testType = TestType.Find(testTypeID)?.Name;

            test = Test.FindByAppointmentID(ID);

            _fees = fees;

            _licenseClassName = LocalLicenseApplication.Find(_localLicenseApplicationID, false)?.LicenseClassName;
            AppointmentDate = appointmentDate;
            CreatedByUserID = createByUserID;
            RetakeTestID = retakeTestID;
            RetakeTestID = retakeTestID;
            _isLocked = isLocked;
            _mode = enStatusMode.UpdateMode;
        }

        public TestAppointment()
        {
            _id = -1;
            _localLicenseApplicationID = -1;
            _testTypeID = -1;
            _fees = 0;
            CreatedByUserID = -1;
            RetakeTestID = null;
            _isLocked = false;
            AppointmentDate = DateTime.Now;

            _mode = enStatusMode.AddMode;
        }


        public static TestAppointment Find(int id)
        {
            int localLicenseApplicationID = -1;
            int testTypeID = -1;
            DateTime appointmentDate = DateTime.UtcNow;
            int createdByUserID = -1;
            decimal fees = 0;

            int? retakeTestAppId = null;

            bool isLocked = false;


            if (TestAppointmentDataAccess.FindByTestAppointmentID(id, ref testTypeID, ref localLicenseApplicationID,
                ref appointmentDate, ref isLocked, ref fees, ref createdByUserID, ref retakeTestAppId))
            {

                return new TestAppointment(id, localLicenseApplicationID, testTypeID, appointmentDate, createdByUserID, retakeTestAppId, isLocked, fees);
            }

            return null;
        }


        public static TestAppointment Find(int? retakeTestAppID)
        {
            int localLicenseApplicationID = -1;
            int testTypeID = -1;
            DateTime appointmentDate = DateTime.UtcNow;
            int createdByUserID = -1;
            decimal fees = 0;

            int id = -1;

            bool isLocked = false;


            if (TestAppointmentDataAccess.FindByRetakeTestAppointmentID(ref id, ref testTypeID, ref localLicenseApplicationID,
                ref appointmentDate, ref isLocked, ref fees, ref createdByUserID, retakeTestAppID))
            {

                return new TestAppointment(id, localLicenseApplicationID, testTypeID, appointmentDate, createdByUserID, retakeTestAppID, isLocked, fees);
            }

            return null;
        }

        private bool _AddNewTestAppointment()
        {
            return (_id = TestAppointmentDataAccess.AddNewTestAppointment(TestTypeID, LocalLicenseApplicationID,
                AppointmentDate, IsLocked, Fees, CreatedByUserID, RetakeTestID)) > 0;
        }

        private bool _UpdateTestAppointment()
        {
            return TestAppointmentDataAccess.UpdateTestAppointment(ID, CreatedByUserID, AppointmentDate, IsLocked);
        }


        internal bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:

                    if (_AddNewTestAppointment())
                    {
                        _mode = enStatusMode.UpdateMode;

                        return true;
                    }

                    return false;

                case enStatusMode.UpdateMode:
                    if (IsActiveAppointmentExistByLocalLicenseAppID(LocalLicenseApplicationID))
                    {
                        return _UpdateTestAppointment();
                    }
                    return false;

                default:
                    return false;
            }
        }
        public static DataTable GetAllByApplicantPersonID(int personID)
        {
            return TestAppointmentDataAccess.GetAllTestAppointmentesByPersonID(personID);
        }


        public static bool IsExist(int id)
        {
            return TestAppointmentDataAccess.IsExist(id);
        }

        public static bool DeleteByID(int id)
        {
            return TestAppointmentDataAccess.DeleteTestAppointmentByID(id);
        }


        internal static bool DeleteByLocalLicenseApplication(int id)
        {
            return TestAppointmentDataAccess.DeleteTestAppointmentByLocalLicenseApplicationID(id);
        }

        private bool _TakeTest(int userID, bool result, string notes)
        {
            test = new Test();


            test.CreatedByUserID = userID;
            test.TestAppointmentID = ID;
            test.Result = result;
            test.Notes = notes;

            return test.Save();
        }

        internal bool TakeTest(int createdByUserID, bool result, string notes)
        {
            if (application != null && (bool)(!application.request?.IsPaid ?? false))
            {
                return false;
            }

            if (_TakeTest(createdByUserID, result, notes))
            {
                this._isLocked = true;

                if (Save())
                {
                    if (RetakeTestID != null)
                    {
                        Application app = Application.Find(RetakeTestID ?? 0);

                        app.UpdatedByUserID = createdByUserID;
                        app.LastStatusDate = DateTime.Now;

                        //Status With ID 3 = Completed Application
                        app.Status = ApplicationStatusType.Find(3)?.Type;

                        return app.Save();
                    }

                    return true;
                }
            }

            return false;
        }



        internal bool GetTestResult(bool result, string notes)
        {
            if (test == null)
                return false;

            test.Result = result;
            test.Notes = notes;

            if (test.Save())
            {
                this._isLocked = true;

                _UpdateTestAppointment();

                return true;
            }

            return false;
        }


        public static int GetID_OfActiveAppointmentByLocalLicenseAppID(int id)
        {
            return TestAppointmentDataAccess.GetID_OfActiveAppointmentByLocalLicneseIDExist(id);
        }


        public static DataTable GetAllTestAppointmentesByLocalLicenseAppIdAndTestTypeID(int id, int typeID)
        {
            return TestAppointmentDataAccess.GetAllTestAppointmentesByLocalLicenseAppIdAndTestTypeID(id, typeID);
        }

        public static DataTable GetAllTestAppointmentesByLocalLicenseAppIdAndTestType(int localAppID, string type) 
        {
            return TestAppointmentDataAccess.GetAllTestAppointmentesByLocalLicenseAppIdAndTestType(localAppID, type);
        }

        public static bool IsActiveAppointmentExistByLocalLicenseAppID(int licenseID) 
        {
            return TestAppointmentDataAccess.IsActiveAppointmentByLocalLicneseIDExist(licenseID);
        }
    }
}
