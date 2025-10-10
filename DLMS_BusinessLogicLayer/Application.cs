using Azure.Core;
using DLMS_DataAccessLayer;
using GlobalUtility;
using IronPdf.Logging;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.SqlServer.Server;
using NLog.Filters;
using NLog.Time;
using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;


namespace DLMS_BusinessLogicLayer
{

    internal enum enApplicationTypes
    {
        NewLicenseApp = 1, RenewLicenseApp,
        DamagedLicenseApp, LostLicenseApp, ReleaseLicenseApp,
        NewInternationalLicense,
        RetakeTestApp
    }

    public class Application
    {


        const string ACCEPTED_RETAKE_TEST_APPLICATION = "تقديم طلب إعادة الإختبار";
        const string ACCEPTED_RETAKE_IF_APPLICATION_IS_LOCAL_LICENSE = "تقديم طلب  استخراج رخصة محلية جديدة";



        private protected  enApplicationTypes enAppType;

        private int _id;
        public int ID { get { return _id; } }

        int? _referencedToMainAppID;
        internal int? ReferencedToMainAppID { get { return _referencedToMainAppID; } 
            private set 
            {
                if (_mode == enStatusMode.AddMode)
                {
                    if (enAppType != enApplicationTypes.RetakeTestApp)
                        _referencedToMainAppID = null;
                    else
                        _referencedToMainAppID = value;
                }
            } 
        }   


        private int _personID;
        public int PersonID
        {
            get { return _personID; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _personID = value;

                    person = Person.Find(value);
                }
            }
        }


        //[JsonIgnore]
        //[BindNever]
        public Person? person { get; private set; }
        public string Type { get; set; }


        private ApplicationType type;

        private int _typeID;
        public int TypeID
        {
            get { return _typeID; }

            set
            {
                _typeID = value;
                type = ApplicationType.Find(value);
                if (type != null)
                {
                    Type = type.Type;

                    PaidFees = type.Fee;

                    enAppType = (enApplicationTypes)value;
                }
            }
        }

        public string Status { get; set; }

        private int _createdByUserID;
        public int CreatedByUserID
        {
            get { return _createdByUserID; }
            set
            {
                _createdByUserID = value;
                var user = User.Find(value);

                if (user != null) CreatedByUser = user.Username;
            }
        }

        public string CreatedByUser { get;  set; }

        private int? _updatedByUserID;
        public int? UpdatedByUserID
        {
            get { return _updatedByUserID; }
            set
            {
                if (value.HasValue)
                {
                    var user = User.Find((int)value);

                    if (user != null)
                    {
                        _updatedByUserID = value;

                        CreatedByUser = user.Username;
                    }
                }
            }
        }
        public string UpdatedByUser { get;  set; }

        public decimal PaidFees { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? LastStatusDate { get; set; }


        private protected enStatusMode _mode;

        private protected Driver _driver;

        public License? license { get; private set; }

        public License? oldLicense { get; private set; }

        public InternationalLicense? internationalLicense { get; private set; }


        private int? _requestID;

        public int? RequestID
        {
            get { return _requestID; }

            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _requestID = value;

                    if (value == null) return;

                    if (IsInternalProcess)
                        request = Request.Find(_requestID.Value);
                }
            }
        }

        [JsonIgnore]
        [BindNever]
        public Request? request { get; private set; }

        private bool IsInternalProcess = false;
        protected Application(int id, int personID, Person person, string type,
            int typeID, string status, int createdByUserID, int? updatedByUserID, decimal paidFees,
            DateTime createdDate, DateTime? lastStatusDate, int? requestID, bool internalProcessApp = true)
        {
            IsInternalProcess = internalProcessApp;

            _id = id;
            PersonID = personID;
            this.person = person;
            Type = type;
            TypeID = typeID;
            Status = status;
            CreatedByUserID = createdByUserID;
            CreatedByUser = User.Find(createdByUserID)?.Username;
            UpdatedByUserID = updatedByUserID;
            UpdatedByUser = User.Find(updatedByUserID ?? 0)?.Username;
            PaidFees = paidFees;
            CreatedDate = createdDate;
            LastStatusDate = lastStatusDate;


            RequestID = requestID;

            if (internalProcessApp)
            {

                license = License.FindByApplicationID(ID);

                internationalLicense = InternationalLicense.FindByApplicatonID(ID);
  
                ResolveSourceLicense();
                
            }

            //Reset Internal Process to default
            IsInternalProcess = false;
            _mode = enStatusMode.UpdateMode;
        }

       

        public Application()
        {
            _id = -1;
            PersonID = -1;
            this.person = null;
            Type = null;
            TypeID = -1;
            Status = null;
            CreatedByUser = null;
            CreatedByUserID = -1;
            UpdatedByUser = null;
            UpdatedByUserID = null;
            PaidFees = -1;
            CreatedDate = DateTime.Now;
            LastStatusDate = null;
            _mode = enStatusMode.AddMode;
        }


        private void ResolveSourceLicense()
        {
          

            Driver driver = Driver.FindByPersonId(PersonID);

            if (driver is null) return;

            if (license is not null && license.OldLicenseID.HasValue)
                oldLicense = License.Find(license.OldLicenseID.Value);

            else if (internationalLicense is not null)
                oldLicense = License.Find(internationalLicense.LocalLicenseID);

            else
                oldLicense = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driver.ID));
        }

        public static Application? Find(int id, bool internalProcessApp = true)
        {
            int personID = -1;
            int typeID = -1;
            DateTime applicationDate = DateTime.MinValue;
            int statusID = -1;
            DateTime? lastStatusDate = null;
            decimal paidFees = 0;
            int createdByUserID = -1;
            int? updatedByUserID = null;
            int? requestID = null;

            if (ApplicationDataAccess.FindByApplicationID(id, ref personID, ref typeID, ref applicationDate, ref statusID, ref lastStatusDate,
                ref paidFees, ref createdByUserID, ref updatedByUserID, ref requestID))
            {
                Person person = Person.Find(personID);
                string type = ApplicationType.Find(typeID).Type;
                string status = ApplicationStatusType.Find(statusID).Type;

                return new Application(id, personID, person, type, typeID, status,
                    createdByUserID, updatedByUserID, paidFees, applicationDate, lastStatusDate, requestID, internalProcessApp);
            }

            return null;
        }


        public static Application? Find(int? requestID, bool internalProcessApp = true)
        {
            int personID = -1;
            int typeID = -1;
            DateTime applicationDate = DateTime.MinValue;
            int statusID = -1;
            DateTime? lastStatusDate = null;
            decimal paidFees = 0;
            int createdByUserID = -1;
            int? updatedByUserID = null;
            int id = -1;

            if (ApplicationDataAccess.FindByRequestID(ref id, ref personID, ref typeID, ref applicationDate, ref statusID, ref lastStatusDate,
                ref paidFees, ref createdByUserID, ref updatedByUserID,  requestID))
            {
                Person person = Person.Find(personID);
                string type = ApplicationType.Find(typeID).Type;
                string status = ApplicationStatusType.Find(statusID).Type;

                return new Application(id, personID, person, type, typeID, status,
                    createdByUserID, updatedByUserID, paidFees, applicationDate, lastStatusDate, requestID, internalProcessApp);
            }

            return null;
        }

        public virtual bool Cancel(int? updatedByUserID)
        {
            if (!IsActive()) return false;

            this.Status = ApplicationStatusType.Find((int)enApplicationStatusTypes.CanceledApplication)?.Type ?? "ملغي";
            this.LastStatusDate = DateTime.Now;
            this.UpdatedByUserID = updatedByUserID;

            if (this.Type == ACCEPTED_RETAKE_TEST_APPLICATION)
            {
                TestAppointment appointment = TestAppointment.Find(this.ID);

                if (appointment != null)
                {
                    appointment.IsLocked = true;

                    if (!appointment.Save())
                        return false;
                }

            }


            if (RequestID != null && request != null && request.IsActive())
            {
                request.Decline();
            }
            return this.Save();
        }


        private bool _CheckBeforeLicenseIssue()
        {
            _driver = Driver.FindByPersonId(PersonID);

            //if (driver == null) return false;

            //if (this.enAppType == enApplicationTypes.RenewLicenseApp)
            //{

            //}

            if (_driver != null)
            {

                license = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(_driver.ID));

                //if (license != null)
                //{
                //    license.IsActive = false;

                //    license.Save();

                //    return true;
                //}

                return true;
            }

            return false;
        }



        private protected bool _IssueLicense(int driverID, int licenseClassId,
            int createdByUserID, string issueReason, string notes)
        {
            License license = new License();

            license.ApplicationID = this.ID;
            license.DriverID = driverID;
            license.LicenseClassID = licenseClassId;
            license.IssueReason = issueReason;
            license.IssueDate = DateTime.Now;
            license.ExpirationDate = DateTime.Now.AddYears(license.licenseClass?.ValidatyLength ?? 0);

            license.Notes = notes;

            license.IsActive = true;

            license.CreatedByUserID = createdByUserID;

            License oldLicense = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driverID));


            if (license.Save())
            {
                if (oldLicense != null)
                {
                    if (oldLicense.IsActiveLicense())
                    {
                        oldLicense.IsActive = false;

                        oldLicense.Save();
                    }
                }

                return _ApplicationCompleted(createdByUserID);
            }

            return false;
        }

        private bool _IsLicenseIssued(License license)
        {
            if (license == null) return false;

            if (request != null && !request.IsPaid)
            {

                if (license.Save())
                {
                    request.UpdateDate = DateTime.Now;
                    request.FeeAmount += license.PaidFess;


                    return request.Save();
                }

                return true;
            }
            else
                return license.Save();
        }
        private bool _IssueLicense(string notes)
        {
            License license = null;

            switch (enAppType)
            {
                case enApplicationTypes.RenewLicenseApp:
                    license = this.license.Renew(this, notes);
                    return _IsLicenseIssued(license);

                case enApplicationTypes.LostLicenseApp:
                case enApplicationTypes.DamagedLicenseApp:
                    license = this.license.ReplaceLicense(this, notes);
                    return _IsLicenseIssued(license);
            }

            return false;
        }
        public virtual bool IssueLicense(int createdByUserID, string notes)
        {
            if (Status != ApplicationStatusType.Find((int)enApplicationStatusTypes.NewApplication).Type) return false;


            UpdatedByUserID = createdByUserID;

            if (_CheckBeforeLicenseIssue())
            {
                if (license != null)
                {
                    if (_IssueLicense(notes))
                    {

                        license.IsActive = false;

                        if (license.IsActiveLicense())
                        {
                            if (license.Save())
                            {
                                //int driverID = Driver.GetDriverIDByPersonID(PersonID);

                                license = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(_driver?.ID ?? 0));

                                //this.LastStatusDate = DateTime.Now;
                                ////this.UpdatedByUserID = createdByUserID;
                                //this.Status = ApplicationStatusType.Find((int)enApplicationStatusTypes.ProcessedApplication).Type;

                                return _ApplicationCompleted(createdByUserID);

                                //return Save();
                            }
                        }
                        else
                        {
                            return _ApplicationCompleted(createdByUserID);
                        }
                    }
                }
            }

            return false;

        }


        private bool _IssueInternationalLicense(int createdByUserID)
        {
            int driverID = Driver.GetDriverIDByPersonID(PersonID);

            //if (InternationalLicense.IsAcitveLicenseExistByDriverID(driverID)) return false;

            int localLicenseID = License.GetLicenseIdOfExistingLicenseByDriverID(driverID);


            InternationalLicense license = new InternationalLicense();

            license.ApplicationID = ID;
            license.LocalLicenseID = localLicenseID;
            license.CreatedByUserID = createdByUserID;
            //license.IssueDate = DateTime.Now;
            license.DriverID = driverID;
            license.IsActive = true;



            return license.Save();
        }
        public bool IssueInternationalLicense(int createdByUserID)
        {
            if (Status != ApplicationStatusType.Find((int)enApplicationStatusTypes.NewApplication).Type) return false;

            if (enAppType != enApplicationTypes.NewInternationalLicense) return false;

            if (_IssueInternationalLicense(createdByUserID))
            {
                //LastStatusDate = DateTime.Now;
                //UpdatedByUserID = createdByUserID;
                //Status = ApplicationStatusType.Find("مكتمل").Type;


                return _ApplicationCompleted(createdByUserID);

                //return Save();
            }

            return false;
        }


        public bool ReleaseDetainedLicense(int releasedByUserID)
        {
            if (!User.IsExist(releasedByUserID)) return false;


            if (Status != ApplicationStatusType.Find((int)enApplicationStatusTypes.NewApplication).Type
                || enAppType != enApplicationTypes.ReleaseLicenseApp) return false;

            license = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(Driver.GetDriverIDByPersonID(PersonID)));

            if (license != null && license.Release(releasedByUserID, ID))
            {
                _ApplicationCompleted(releasedByUserID);

                return true;
            }
            return false;
        }


        private bool _ApplicationCompleted(int updatedByUserID)
        {

            Status = ApplicationStatusType.Find((int)enApplicationStatusTypes.ProcessedApplication)?.Type ?? "مكتمل";
            LastStatusDate = DateTime.Now;
            UpdatedByUserID = updatedByUserID;

            return Save();
        }


        private bool IsEligibleForRetakeTestApp => this.enAppType == enApplicationTypes.NewLicenseApp || this.enAppType == enApplicationTypes.RenewLicenseApp;
        protected Application? _RetakeTestApplication(int createByUserID)
        {
            if (!IsEligibleForRetakeTestApp) 
            {
                var ex = new InvalidConstraintException("The retake test application should be referenced to main application with type of new local license applicaiton");
                LoggerUtil.LogError(ex, nameof(Application), nameof(_RetakeTestApplication),
                            new Dictionary<string, object> { { "CreatedByUserID", createByUserID }, { "AppType", Type},
                            {"enAppType", enAppType }
                            });

                return null;
            }

            Application application = new Application();

            application.PersonID = PersonID;
            application.person = person;
            application.Status = "جديد";

            //Retake Test Application primary key is 7 
            application.TypeID = ApplicationType.Find((int)enApplicationTypes.RetakeTestApp).ID;

            application.CreatedDate = DateTime.Now;
            application.PaidFees = ApplicationType.Find(application.TypeID).Fee;
            application.CreatedByUserID = createByUserID;

            application.ReferencedToMainAppID = this.ID;

            if (application.Save())
                return application;

            return null;
        }
        public static DataTable GetAll()
        {
            return ApplicationDataAccess.GetAllApplications();
        }

        public static DataTable GetAll(string appType)
        {
            return ApplicationDataAccess.GetAllApplications(appType);
        }

        private bool _AddNewApplication()
        {
            Status = ApplicationStatusType.Find((int)enApplicationStatusTypes.NewApplication)?.Type;

            return (_id = ApplicationDataAccess.AddNewApplication(PersonID, TypeID, CreatedDate, ApplicationStatusType.Find(Status).ID,
                LastStatusDate, PaidFees, CreatedByUserID, UpdatedByUserID, RequestID, ReferencedToMainAppID)) > 0;
        }

        private bool _UpdateApplication()
        {
            return ApplicationDataAccess.UpdateApplication(ID, ApplicationStatusType.Find(Status)?.ID ?? 0, LastStatusDate,
                 UpdatedByUserID);
        }


        //private bool _AddLocalLicenseAppForRenewalPurpose()
        //{
        //    LocalLicenseApplication lApp = new();

        //    lApp.ID = this.ID;

        //}
        private bool _AddApplication()
        {
            if (_AddNewApplication())
            {

                _mode = enStatusMode.UpdateMode;
                //Type = ApplicationType.Find(TypeID).Type;
                return true;
            }

            return false;
        }


        private bool _AddNewLicenseApp(int licenseClassID)
        {

            if (License.IsExistByLicenseClassAndDriverID(Driver.GetDriverIDByPersonID(PersonID),
                licenseClassID))
            {
                return false;
            }

            return _AddApplication();
        }

        private bool _AddRenewLicenseApp()
        {
            int licenseID = License.GetLicenseIdOfExistingLicenseByDriverID(Driver.
             GetDriverIDByPersonID(PersonID));

            License license = License.Find(licenseID);

            if (license == null)
            {
                Debug.WriteLine("There's No License To Renew It");
                return false;
            }

            return license.Renewable() && _AddApplication();
        }

        private bool _AddLostOrDamagedLicenseApp()
        {
            if (License.IsAcitveLicenseExistByDriverID(Driver.GetDriverIDByPersonID(PersonID)))
            {
                return _AddApplication();
            }

            return false;
        }

        private bool _AddRetakeTestApplication()
        {
            if (person != null && person.HasActiveRetakeTestApplication(ACCEPTED_RETAKE_TEST_APPLICATION))
                return false;

            if (Find(person?.GetActiveApplicationID() ?? -1)?.Type ==
            ACCEPTED_RETAKE_IF_APPLICATION_IS_LOCAL_LICENSE)
            {
                return _AddApplication();
            }
            else
            {
                return false;
            }
        }

        private bool _AddNewInternaationalLicense()
        {
            int driverID = Driver.GetDriverIDByPersonID(PersonID);

			//if (!InternationalLicense.IsAcitveLicenseExistByDriverID(driverID) && License.IsAcitveLicenseExistByDriverID(driverID))
			if (License.IsAcitveLicenseExistByDriverID(driverID))
			{
				return _AddNewApplication();
            }

            return false;
        }

        private bool _AddNewReleaseLicenseApplication()
        {
            int temp = Driver.GetDriverIDByPersonID(PersonID);

            if (temp == -1) return false;

            temp = License.GetLicenseIdOfExistingLicenseByDriverID(temp);


            if (temp == -1 || !License.IsDetained(temp)) return false;

            return _AddApplication();
        }
        private bool _SaveNewApplications(int licenseClassID, int licenseID = -1)
        {
            switch (enAppType)
            {
                case enApplicationTypes.NewLicenseApp:
                    return _AddNewLicenseApp(licenseClassID);

                case enApplicationTypes.RenewLicenseApp:
                    return _AddRenewLicenseApp();

                case enApplicationTypes.ReleaseLicenseApp:
                    return _AddNewReleaseLicenseApplication();

                case enApplicationTypes.LostLicenseApp:
                //return _AddLostOrDamagedLicenseApp();

                case enApplicationTypes.DamagedLicenseApp:
                    return _AddLostOrDamagedLicenseApp();

                case enApplicationTypes.NewInternationalLicense:
                    return _AddNewInternaationalLicense();

                case enApplicationTypes.RetakeTestApp:
                    return _AddRetakeTestApplication();

                default:
                    return false;
            }
        }
        public virtual bool Save(int licenseClassID = -1)
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:

                    if (Type == ACCEPTED_RETAKE_TEST_APPLICATION)
                    {
                        return _SaveNewApplications(licenseClassID);
                    }

                    else if (person != null && !person.HasActiveApplication())
                    {
                        return _SaveNewApplications(licenseClassID);
                    }

                    return false;

                case enStatusMode.UpdateMode:
                    if (person != null && person.HasActiveApplication())
                    {
                        return _UpdateApplication();
                    }
                    return false;

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return ApplicationDataAccess.IsExist(id);
        }


        public bool IsActive() 
        {
            return IsActiveApplicationExistByApplicationID(ID);
        }
        public static bool IsActiveApplicationExistByApplicationID(int id)
        {
            return ApplicationDataAccess.IsActiveApplicationExistByApplicationID(id);
        }
        public static bool Delete(int id)
        {
            Application? app = Find(id);

            if (app == null) return false;

            if (app.RequestID != null) return false;
            //if (app.Status == "مكتمل") return false;
            if (app.Status != ApplicationStatusType.Find((int)enApplicationStatusTypes.CanceledApplication).Type) return false;

            //if (app.Status == ApplicationStatusType.Find((int)enApplicationStatusTypes.ProcessedApplication)?.Type) return false;

            return ApplicationDataAccess.DeleteApplication(id);
        }

        public static bool IsExistByPersonID(int id)
        {
            return ApplicationDataAccess.IsApplicationExistByPersonID(id);
        }


        public bool IsExistByRequestID() 
        {
            if (RequestID == null) return false;

            return IsExistByRequestID(RequestID.Value);
        }
        public static bool IsExistByRequestID(int reqID) 
        {
            return ApplicationDataAccess.IsExistByRequestID(reqID);   
        }

        internal static bool CouldBeDeletedAppID(int appID)
        {
            return ApplicationDataAccess.CouldBeDeletedByAppID(appID);    
        }



        internal static bool CouldBeDeletedByRequestID(int reqID)
        {
            return ApplicationDataAccess.CouldBeDeletedByRequestID(reqID);
        }

    }
}
