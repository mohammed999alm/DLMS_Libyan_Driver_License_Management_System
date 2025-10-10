using DLMS_DataAccessLayer;
using System;
using System.Data;
using System.Diagnostics;
using System.Text.Json.Serialization;
using static DLMS_BusinessLogicLayer.License;


namespace DLMS_BusinessLogicLayer
{

    internal enum enLicenseClassTypes
    {
        Grade1A = 1,
        Grade1B,
        Grade2,
        Grade3,
        Grade4A,
        Grade4B,
        SpecialGrade
    }

    internal enum enApplicationStatusTypes
    {
        NewApplication = 1,
        CanceledApplication,
        ProcessedApplication
    }
    public class LocalLicenseApplication : Application
    {

        private int _id;

        public int LocalAppId { get { return _id; } }

        private string _licenseClass;

        internal enLicenseClassTypes LicenseClassType { get; private set; }
        public string LicenseClassName
        {
            get { return _licenseClass; }
        }

        private int _licenseClassID;

        public int LicenseClassID
        {
            get { return _licenseClassID; }

            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _licenseClassID = value;

                    _licenseClass = LicenseClass.Find(_licenseClassID)?.Name;

                    LicenseClassType = (enLicenseClassTypes)value;
                }
            }
        }

        private byte _passedTests;

        public byte PassedTests { get { return _passedTests; } }


        private bool _retakeTestFlag;

        private Application _retakeTestApplication;

        //[JsonInclude]
        public TestAppointment? appointment { get; set; }

        //private Driver _driver;
        //private short _testFail

        private LocalLicenseApplication(int id, int licenseClassID, byte passedTests, int applicationID, int personID, Person person, string type,
            int typeID, string status, int createdByUserID, int? updatedByUserID, decimal paidFees,
            DateTime createdDate, DateTime? lastStatusDate, int? requestID, bool internalProcessAppointment = true)
            : base(applicationID, personID, person, type, typeID, status, createdByUserID,
                 updatedByUserID, paidFees, createdDate, lastStatusDate, requestID)
        {
            _mode = enStatusMode.AddMode;
            _id = id;

            LicenseClassID = licenseClassID;
            _licenseClass = LicenseClass.Find(_licenseClassID)?.Name;
            _passedTests = passedTests;

            if (internalProcessAppointment)
                appointment = TestAppointment.Find(TestAppointment.GetID_OfActiveAppointmentByLocalLicenseAppID(_id));

            _mode = enStatusMode.UpdateMode;
        }

        public LocalLicenseApplication()
        {
            _id = -1;
            _licenseClassID = -1;
            _licenseClass = null;
            _passedTests = 0;

            _mode = enStatusMode.AddMode;
        }


        public static LocalLicenseApplication Find(int id, bool internalProcessAppointment = true)
        {
            int licenseClassID = -1;
            int applicationID = -1;


            byte passedTests = 0;

            if (LocalLicenseApplicationDataAccess.FindByLocalLicenseApplicationID(id, ref applicationID, ref licenseClassID, ref passedTests))
            {
                Application application = Application.Find(applicationID);

                if (application == null)
                    return null;

                return new LocalLicenseApplication(id, licenseClassID, passedTests, application.ID,
                    application.PersonID, application.person, application.Type, application.TypeID,
                    application.Status, application.CreatedByUserID, application.UpdatedByUserID,
                    application.PaidFees, application.CreatedDate, application.LastStatusDate, application.RequestID, internalProcessAppointment);
            }

            return null;
        }


        public static LocalLicenseApplication FindByApplicationID(int applicationID, bool internalProcessAppointment = true)
        {
            int licenseClassID = -1;
            int id = -1;


            byte passedTests = 0;

            if (LocalLicenseApplicationDataAccess.FindByApplicationID(ref id,  applicationID, ref licenseClassID, ref passedTests))
            {
                Application application = Application.Find(applicationID);

                if (application == null)
                    return null;

                return new LocalLicenseApplication(id, licenseClassID, passedTests, application.ID,
                    application.PersonID, application.person, application.Type, application.TypeID,
                    application.Status, application.CreatedByUserID, application.UpdatedByUserID,
                    application.PaidFees, application.CreatedDate, application.LastStatusDate, application.RequestID, internalProcessAppointment);
            }

            return null;
        }


        private bool _AddNewApplication()
        {
            return (_id = LocalLicenseApplicationDataAccess.AddLocalLicenseApplication(ID, LicenseClassID)) > 0;
        }




        /*
             This method checks if a person meets the license requirements based on their age,
             license class, and special rules for Grade3 licenses:
             Grade3 licenses require an active Grade2 license with a threshold of 2 years.
        */
        private bool _IsPersonMeetingLicenseRequriments()
        {
            if (person == null) return false;

            LicenseClass licenseClass = LicenseClass.Find(LicenseClassID);

            if (licenseClass == null) return false;



            if (person.Age >= licenseClass.Age)
            {
                int driverID = Driver.GetDriverIDByPersonID(person.ID);

                // Grade3 licenses require an active Grade2 license with a threshold of 2 years
                if (LicenseClassType == enLicenseClassTypes.Grade3 && LicenseClassType == enLicenseClassTypes.Grade4A && LicenseClassType == enLicenseClassTypes.Grade4B)
                {
                    // Ensure the applicant is a driver (driverID > 0) before checking license activity
                    return driverID > 0 && License.IsLicenseWithDateThresholdByDriverAndClassIdExists(driverID,
                        (int)enLicenseClassTypes.Grade2, 2);
                }

                return true;
            }

            return false;
        }

        public override bool Save(int licenseClassID = -1)
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    {

                        switch (enAppType)
                        {
                            case enApplicationTypes.NewLicenseApp:
                                {
                                    if (_IsPersonMeetingLicenseRequriments())
                                    {

                                        if (base.Save(LicenseClassID))
                                        {
                                            return _AddNewApplication();
                                        }
                                    }
                                }
                                break;
                            case enApplicationTypes.RenewLicenseApp:
                                {
                                    if (base.Save())
                                    {

                                        if (license != null)
                                            LicenseClassID = license.LicenseClassID;

                                        else
                                        {
                                            License temp = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(Driver.GetDriverIDByPersonID(PersonID)));

                                            if (temp == null)
                                                return false;

                                            _mode = enStatusMode.AddMode;

                                            LicenseClassID = temp.LicenseClassID;    
                                        }
                                        

                                        return _AddNewApplication();
                                    }
                                    break;
                                }
                        }

                        return false;
                    }
                case enStatusMode.UpdateMode:

                    
                    if (PassedTests == 3 && enAppType == enApplicationTypes.NewLicenseApp)
                        return base.Save();

                    //else if (Status == ApplicationStatusType.Find((int)enApplicationStatusTypes.CanceledApplication)?.Type)
                    if (IsActive())
                        return base.Save();

                    else
                        return false;

                default:
                    return false;
            }
        }


        public static new bool Delete(int id)
        {
            LocalLicenseApplication app = LocalLicenseApplication.Find(id);

            if (app == null) return false;

            //if (app.Status == "مكتمل") return false;
            if (app.RequestID != null) return false;

            if (app.Status != ApplicationStatusType.Find((int)enApplicationStatusTypes.CanceledApplication).Type) return false;

            //if (app.Status == ApplicationStatusType.Find((int)enApplicationStatusTypes.NewApplication).Type) return false;

            //if (app.Status == ApplicationStatusType.Find((int)enApplicationStatusTypes.ProcessedApplication).Type) return false;

            return LocalLicenseApplicationDataAccess.DeleteLocalLicenseApplication(id);
        }



        public static new bool IsExist(int id)
        {
            return LocalLicenseApplicationDataAccess.IsExist(id);
        }

       


        public static new DataTable GetAll()
        {
            return LocalLicenseApplicationDataAccess.GetAll();
        }


        private void _SetPassedTestsByResults()
        {
            _passedTests = (byte)LocalLicenseApplicationDataAccess.GetPassedTests(LocalAppId);
        }

        private bool CreateTestAppointment(User createdByUser, TestType type, DateTime appointmentDate)
        {
            if (type == null) return false;

            if (createdByUser == null) return false;


            appointment = new TestAppointment();

            appointment.AppointmentDate = appointmentDate;
            appointment.CreatedByUserID = createdByUser.ID;
            appointment.TestTypeID = type.ID;




            appointment.LocalLicenseApplicationID = LocalAppId;

            if (Test.IsFailedTestExist(LocalAppId, type.ID))
            {
                _retakeTestApplication = _RetakeTestApplication(createdByUser.ID);

                appointment.RetakeTestID = _retakeTestApplication?.ID ?? null;
            }

            if (appointment.Save())
            {
                if (request != null && !request.IsPaid)
                {
                    request.FeeAmount += appointment.Fees;

                    request.Save();
                }

                return true;
            }
            return false;
        }
        private bool _ScheduleAppointment(int createdByUseerID, DateTime appointmentDate)
        {
            if (enAppType == enApplicationTypes.RenewLicenseApp && PassedTests == 1)
                return false;

            if (PassedTests == 3)
                return false;

            if (PassedTests == 0)
                return CreateTestAppointment(User.Find(createdByUseerID), TestType.Find(1), appointmentDate);


            if (PassedTests == 1)
                return CreateTestAppointment(User.Find(createdByUseerID), TestType.Find(2), appointmentDate);

            if (PassedTests == 2)
                return CreateTestAppointment(User.Find(createdByUseerID), TestType.Find(3), appointmentDate);

            return false;
        }


        public bool ValidateBeforeShedulingAppointment() 
        {
            ApplicationStatusType status = ApplicationStatusType.Find((int)enApplicationStatusTypes.NewApplication);

            if (status == null)
            {
                Debug.WriteLine("The Applications Status Not Found");
                return false;
            }

            if (Status != status.Type)
                return false;

            if (PassedTests == 3)
                return false;

            return true;
        }
        public bool ScheduleAppointment(int createByUserID, DateTime appointmentDate)
        {

            if (ValidateBeforeShedulingAppointment())
            {
                return (appointment?.IsLocked ?? true) ? _ScheduleAppointment(createByUserID, appointmentDate) : false;
            }
            return false;
        }

        public bool ReSheduleAppointment(int appointmentID, int byUserID, DateTime appointmentData) 
        { 
            TestAppointment appointment = TestAppointment.Find(appointmentID);

            if (ValidateBeforeShedulingAppointment())
            {
                if (appointment != null && !appointment.IsLocked) 
                {
                    appointment.AppointmentDate = appointmentData;
                    appointment.CreatedByUserID = byUserID; 

                    return appointment.Save();
                }
            }
            return false;
        }


        private bool _TakeTest(int userID, bool result, string notes)
        {
            if (appointment == null) return false;

            return User.IsExist(userID) ? appointment.TakeTest(userID, result, notes) : false;
        }


        public bool TakeTest(int createdByUserID, bool result, string notes)
        {
            if (_TakeTest(createdByUserID, result, notes))
            {
                _SetPassedTestsByResults();

                return true;
            }

            return false;
        }


        private bool _CreateDriver(int createdByUserID)
        {
            _driver = new Driver();

            _driver.PersonID = PersonID;
            _driver.CreatedByUserID = createdByUserID;

            _driver.CreationDate = DateTime.Now;

            return _driver.Save();
        }

        //private createLicense()

        //private bool _PriorCheckOnNewLicenseApp() 
        //{
        //    if (PassedTests < 3)
        //        return false;
        //}


        private bool _IssueRenewableLicense(int createdByUserID, string notes)
        {
            if (PassedTests < 1 && enAppType == enApplicationTypes.RenewLicenseApp) return false;

            _driver = Driver.FindByPersonId(PersonID);

            if (_driver == null) return false;  

            return base.IssueLicense(createdByUserID, notes);           
        }


        //Issue License for first time per license class 
        //for exmample issue license of grade 1(A)
        private bool _IssueNewLicense(int createdByUserID, string notes) 
        {

            if (PassedTests < 3 && enAppType == enApplicationTypes.NewLicenseApp)
                return false;

            if (!Driver.IsExistByPersonID(PersonID) && enAppType == enApplicationTypes.NewLicenseApp)
            {
                _CreateDriver(createdByUserID);
            }
            else
            {
                _driver = Driver.FindByPersonId(PersonID);
            }

            if (_driver == null) return false;

            string? issueReason = LicenseIssueReason.Find((int)enIssueReason.NewLicense)?.Name;

            if (issueReason != null)
            {
                return _IssueLicense(_driver.ID, LicenseClassID, createdByUserID, issueReason, notes);
            }

            return false;
        }
        private bool _IssueLicense(int createdByUserID, string notes)
        {
            switch (enAppType) 
            {
                case enApplicationTypes.NewLicenseApp:
                    return _IssueNewLicense(createdByUserID, notes);

                case enApplicationTypes.RenewLicenseApp:
                    return _IssueRenewableLicense(createdByUserID, notes);  
            }

            return false;
        }

        public override bool IssueLicense(int createdByUserID, string notes)
        {
            if (Status != ApplicationStatusType.Find((int)enApplicationStatusTypes.NewApplication).Type)
                return false;


            //if (_IssueLicense(createdByUserID, notes))
            //{
            //    this.Status = "مكتمل";

            //    this.LastStatusDate = DateTime.Now;

            //    this.UpdatedByUserID = createdByUserID;

            //    if (Save())
            //    {
            //        return true;
            //    }
            //}

            return _IssueLicense(createdByUserID, notes);

            //return false;
        }


        private bool _GetTestResult(bool result, string notes)
        {
            return appointment?.GetTestResult(result, notes) ?? false;
        }


        public bool GetTestResult(bool result, string notes)
        {
            if (_GetTestResult(result, notes))
            {
                _SetPassedTestsByResults();

                return true;
            }

            return false;
        }


        public DataTable GetTestAppointmentsList()
        {
            return TestAppointment.GetAllTestAppointmentesByLocalLicenseAppIdAndTestTypeID(_id, appointment?.TestTypeID ?? 0);
        }

        public DataTable GetAppointmentListBasedOnTestType(string type) 
        {
            return TestAppointment.GetAllTestAppointmentesByLocalLicenseAppIdAndTestType(LocalAppId, type);
        }



        public int GetTestAppointmentAttemptsBasedOnLocalLicenseAppIdAndTestType() 
        {     
            return GetTestAppointmentAttemptsBasedOnLocalLicenseAppIdAndTestType(LocalAppId, appointment?.TestTypeName);
        }


        public static int GetTestAppointmentAttemptsBasedOnLocalLicenseAppIdAndTestType(int appId, string type) 
        {
            return LocalLicenseApplicationDataAccess.GetTestAppointmentAttemptsBasedOnLocalLicenseAppIdAndTestType(appId, type);
        }


        public bool HasActiveAppointments() 
        {
            return TestAppointmentDataAccess.IsActiveAppointmentByLocalLicneseIDExist(LocalAppId);
        }

        public override bool Cancel(int? updatedByUserID)
        {
            if (base.Cancel(updatedByUserID))
            {
                if (appointment != null)
                {
                    appointment.IsLocked = true;

                    return appointment.Save();
                }

                return true;
            }

            return false;
        }



    }
}
