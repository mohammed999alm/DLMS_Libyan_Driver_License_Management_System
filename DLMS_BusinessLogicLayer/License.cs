using System;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics.Eventing.Reader;
using DLMS_DataAccessLayer;
using GlobalUtility;

namespace DLMS_BusinessLogicLayer
{
    public class License
    {

        private enStatusMode _mode;

        internal enum enIssueReason
        {
            NewLicense = 1,
            RenewLicense,
            DamagedLicense,
            LostLicense
        }
        public int ID { get; private set; }

        private int _applicationID;
        public int ApplicationID
        {

            get { return _applicationID; }

            set
            {

                if (_mode == enStatusMode.AddMode)
                {
                    _applicationID = value;

                    application = Application.Find(_applicationID, false);
                }
            }

        }

        public Application? application { get; private set; }

        private int _driverID;
        public int DriverID
        {

            get { return _driverID; }

            set
            {

                if (_mode == enStatusMode.AddMode)
                {
                    _driverID = value;

                    driver = Driver.Find(_driverID);
                }
            }
        }

        public Driver? driver { get; private set; }


        private int _licenseClassID;
        public int LicenseClassID
        {

            get { return _licenseClassID; }

            set
            {

                if (_mode == enStatusMode.AddMode)
                {
                    _licenseClassID = value;

                    licenseClass = LicenseClass.Find(_licenseClassID);
                }
            }
        }

        public LicenseClass? licenseClass { get; private set; }

        public string IssueReason { get; set; }



        public DateTime IssueDate { get; set; }

        private DateTime _ExpirationDate;

        public DateTime ExpirationDate
        {
            get
            {
                return _ExpirationDate;
            }
            set
            {
                if (_mode == enStatusMode.AddMode)
                    _ExpirationDate = value;
            }
        }

        public bool IsActive { get; set; }

        public bool IsReserved { get; set; }

        public string Notes { get; set; }

        public decimal PaidFess { get; private set; }


        private int _createdByUserID;
        public int CreatedByUserID
        {

            get { return _createdByUserID; }

            set
            {

                if (_mode == enStatusMode.AddMode)
                {
                    _createdByUserID = value;

                    createdByUser = User.Find(_createdByUserID)?.Username;
                }
            }
        }



        public string? createdByUser { get; private set; }

        public int? OldLicenseID { get; private set; }  


        private License(int iD, int applicationID, int driverID,
            int licenseClassID, string issueReason, DateTime issueDate, DateTime expirationDate,
            bool isActive, string notes, decimal paidFess, int createdByUserID)
        {

            //_mode = enStatusMode.AddMode;

            ID = iD;
            ApplicationID = applicationID;

            DriverID = driverID;

            LicenseClassID = licenseClassID;

            IssueReason = issueReason;
            IssueDate = issueDate;
            ExpirationDate = expirationDate;
            IsActive = isActive;
            Notes = notes;
            PaidFess = paidFess;
            CreatedByUserID = createdByUserID;

            IsReserved = IsDetained();

            OldLicenseID = GetPreviousLicenseID();

            _mode = enStatusMode.UpdateMode;
        }


        public License()
        {
            ID = -1;
            ApplicationID = -1;

            DriverID = -1;

            LicenseClassID = -1;


            IssueReason = null;
            IssueDate = DateTime.Now;
            ExpirationDate = IssueDate.AddYears(10);
            IsActive = false;
            Notes = null;
            PaidFess = 0;
            CreatedByUserID = -1;

            _mode = enStatusMode.AddMode;
        }



        public static License Find(int id)
        {
            int applicationID = -1;
            int driverID = -1;

            int licenseClassID = -1;
            int issueReasonID = -1;
            DateTime issueDate = DateTime.MinValue;
            DateTime expirationDate = DateTime.MinValue;
            bool isActive = false;
            string notes = null;
            decimal paidFess = 0;
            int createdByUserID = -1;

            if (LicenseDataAccess.FindByLicenseID(id, ref applicationID, ref driverID, ref licenseClassID,
                ref issueReasonID, ref issueDate, ref expirationDate, ref isActive,
                ref notes, ref paidFess, ref createdByUserID))
            {
                return new License(id, applicationID, driverID, licenseClassID, LicenseIssueReason.Find(issueReasonID)?.Name,
                    issueDate, expirationDate, isActive, notes, paidFess, createdByUserID);
            }

            return null;
        }


        //public static License FindByDriverID(int driverID)
        //{
        //    int applicationID = -1;
        //    int id = -1;

        //    int licenseClassID = -1;
        //    int issueReasonID = -1;
        //    DateTime issueDate = DateTime.MinValue;
        //    DateTime expirationDate = DateTime.MinValue;
        //    bool isActive = false;
        //    string notes = null;
        //    decimal paidFess = 0;
        //    int createdByUserID = -1;

        //    if (LicenseDataAccess.FindByDriverID(ref id, ref applicationID, driverID, ref licenseClassID,
        //        ref issueReasonID, ref issueDate, ref expirationDate, ref isActive,
        //        ref notes, ref paidFess, ref createdByUserID))
        //    {
        //        return new License(id, applicationID, driverID, licenseClassID, LicenseIssueReason.Find(issueReasonID)?.Name,
        //            issueDate, expirationDate, isActive, notes, paidFess, createdByUserID);
        //    }

        //    return null;
        //}


        public static License FindByApplicationID(int applicationID)
        {
            int driverID = -1;
            int id = -1;

            int licenseClassID = -1;
            int issueReasonID = -1;
            DateTime issueDate = DateTime.MinValue;
            DateTime expirationDate = DateTime.MinValue;
            bool isActive = false;
            string notes = null;
            decimal paidFess = 0;
            int createdByUserID = -1;

            if (LicenseDataAccess.FindByApplicationID(ref id, applicationID, ref driverID, ref licenseClassID,
                ref issueReasonID, ref issueDate, ref expirationDate, ref isActive,
                ref notes, ref paidFess, ref createdByUserID))
            {
                return new License(id, applicationID, driverID, licenseClassID, LicenseIssueReason.Find(issueReasonID)?.Name,
                    issueDate, expirationDate, isActive, notes, paidFess, createdByUserID);
            }

            return null;
        }


        private bool _AddNewLicense()
        {
            return (ID = LicenseDataAccess.AddNewLicense(ApplicationID, DriverID, LicenseClassID,
                LicenseIssueReason.Find(IssueReason)?.ID ?? 0, IssueDate, ExpirationDate,
                IsActive, Notes, PaidFess, CreatedByUserID)) > 0;
        }


        private bool _UpadateLicense()
        {
            return LicenseDataAccess.UpdateLicense(ID, IsActive);
        }


        private bool _HasLicenseLoweredByOneDegree(int driverID, int licenseClassID)
        {

            License license = License.Find(License.GetLicenseIdOfExistingLicenseByDriverID(driverID));

            if (license == null) return false;

            return (license.licenseClass.ID == (licenseClassID - 1)) ? true : false;
        }

        private void _SetPaymentFees()
        {
            if (_HasLicenseLoweredByOneDegree(DriverID, _licenseClassID))
            {
                PaidFess = 10;
            }
            else
            {
                PaidFess = licenseClass.Fees;
            }
        }
        internal bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:

                    _SetPaymentFees();

                    if (_AddNewLicense())
                    {
                        _mode = enStatusMode.UpdateMode;

                        return true;
                    }

                    return false;

                case enStatusMode.UpdateMode:
                    if (DateTime.Now < ExpirationDate && (IsAcitveLicenseExistByDriverID(DriverID) || IsDetained()))
                        return _UpadateLicense();

                    return false;

                default:
                    return true;
            }
        }
        public static DataTable GetAll(int id)
        {
            return LicenseDataAccess.GetAllLocalLicenseByDriverID(id);
        }


        public static bool IsExistByLicenseClassAndDriverID(int driverID, int licenseClassID)
        {
            return LicenseDataAccess.IsExistByLicenseClassAndDriverID(driverID, licenseClassID);
        }


        internal static bool IsLicenseWithDateThresholdByDriverAndClassIdExists(int driverID,
            int licenseClassID, int numberOfYears)
        {
            return LicenseDataAccess.IsLicenseWithDateThresholdByDriverAndClassIdExists(driverID, licenseClassID, numberOfYears);
        }

        public static bool IsLicenseWithDateThresholdByDriverAndClassIdExists(int driverID, int licenseClassID) 
        {
            return LicenseDataAccess.IsLicenseWithDateThresholdByDriverAndClassIdExists(driverID, licenseClassID, 2); 
        }

        public static bool IsAcitveLicenseExistByDriverID(int driverID)
        {
            return LicenseDataAccess.IsAcitveLicenseExistByDriverID(driverID);
        }

        public bool IsActiveLicense() 
        {
            return IsActiveByLicenseID(ID);
        }
        public static bool IsActiveByLicenseID(int licenseID) 
        {
            return LicenseDataAccess.IsActiveByLicenseID(licenseID);
        }

        public static bool IsExistByDriverID(int driverID)
        {
            return LicenseDataAccess.IsExistByDriverID(driverID);
        }

        public static int GetLicenseIdOfExistingLicenseByDriverID(int driverID)
        {
            return LicenseDataAccess.GetLicenseIdOfExistingLicenseByDriverID(driverID);
        }


        public bool Renewable()
        {
            if (DateTime.Now > ExpirationDate)
            {
                return true;
            }

            if (Util.GetDifferenceInYears(DateTime.Now, ExpirationDate) < 1)
                return true;

            return false;
        }

        private License _CreateLicense(Application application, string notes, enIssueReason reason)
        {
            License license = new License();

            license.ApplicationID = application.ID;

            license.DriverID = _driverID;

            //There's an issue here i will fix it later
            if (application.request != null)
            {
                application.UpdatedByUserID = application.CreatedByUserID;
            }
            license.CreatedByUserID = int.TryParse(application.UpdatedByUserID.ToString(), out int result) ? result : -1;

            license.IsActive = true;
            license.IssueReason = LicenseIssueReason.Find((int)reason).Name;

            license.ExpirationDate = license.IssueDate.AddYears(licenseClass.ValidatyLength);

            license.licenseClass = licenseClass;

            license.LicenseClassID = licenseClass.ID;

            license.Notes = notes;

            license.PaidFess = license.licenseClass.Fees;

            return license;
        }
        private License _RenewLicense(Application application, string notes)
        {
            if (application == null)
                return null;


            return _CreateLicense(application, notes, enIssueReason.RenewLicense);
        }
        public License Renew(Application application, string notes)
        {
            return Renewable() ? _RenewLicense(application, notes) : null;
        }

        public License ReplaceLicense(Application application, string notes)
        {
            if (application == null) return null;

            if (!IsActive) return null;

            return (application.Type == ApplicationType.Find((int)enApplicationTypes.LostLicenseApp).Type) ?
                _CreateLicense(application, notes, enIssueReason.LostLicense) :
                _CreateLicense(application, notes, enIssueReason.DamagedLicense);
        }


        public bool DetainLicense(int byUserId, decimal fineFees, string notes)
        {
            if (!IsActive) return false;

            DetainedLicense detainedLicense = new DetainedLicense();

            detainedLicense.LicenseID = ID;
            detainedLicense.CreatedByUserID = byUserId;

            detainedLicense.FineFees = fineFees;

            detainedLicense.Notes = notes;



            if (!detainedLicense.Save()) return false;


            IsActive = false;

            Save();

            return true;
        }


        public static bool IsDetained(int licenseID)
        {
            return DetainedLicense.IsExistByLicenseID(licenseID);
        }

        public bool IsDetained()
        {
            return IsDetained(ID);
        }

        //internal bool Release(int releasedByUserID, int releasedByAppID)
        //{
        //    if (IsActive || !IsDetained()) return false;

        //    if (DetainedLicense.Find(DetainedLicense.GetIdOfDetainedLicenseByLicenseID(ID))?.Reelease(releasedByUserID, releasedByAppID) ?? false)
        //    {
        //        IsActive = true;

        //        Save(); return true;
        //    }

        //    return false;
        //}

        internal bool Release(int releasedByUserID, int releasedByAppID)
        {
            if (IsActive || !IsDetained())
                return false;

            IsActive = true;
            if (!Save())
                return false;

            var detained = DetainedLicense.Find(DetainedLicense.GetIdOfDetainedLicenseByLicenseID(ID));
            if (!(detained?.Reelease(releasedByUserID, releasedByAppID) ?? false))
            {
              
                IsActive = false;
                Save();
                return false;
            }

            return true;
        }


        public int GetPreviousLicenseID() 
        {
            return GetPreviousLicenseID(DriverID, ID);
        }
        public static int GetPreviousLicenseID(int driverID, int licenseID) 
        {
            return LicenseDataAccess.GetPreviousLicenseID(driverID, licenseID);
        }

    }
}
