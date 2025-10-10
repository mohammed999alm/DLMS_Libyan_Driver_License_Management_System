using System;
using System.ComponentModel;
using System.Data;
using DLMS_DataAccessLayer;

namespace DLMS_BusinessLogicLayer
{
    public class InternationalLicense
    {
        private enStatusMode _mode;


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


        private int _localLicenseID;
        public int LocalLicenseID
        {

            get { return _localLicenseID; }

            set
            {

                if (_mode == enStatusMode.AddMode)
                {
                    _localLicenseID = value;

                    license = License.Find(_localLicenseID);
                }
            }
        }


        public License? license { get; private set; }

        private DateTime _issueDate;
        public DateTime IssueDate
        {
            get { return _issueDate; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _issueDate = value;
                    ExpirationDate = value.AddYears(1);
                }

            }
        }

        public DateTime ExpirationDate { get; private set; }

        public bool IsActive { get; set; }

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


        private InternationalLicense(int iD, int applicationID, int driverID,
            int localLicenseID, DateTime issueDate, DateTime expirationDate,
            bool isActive, int createdByUserID)
        {

            //_mode = enStatusMode.AddMode;

            ID = iD;
            ApplicationID = applicationID;

            DriverID = driverID;

            LocalLicenseID = localLicenseID;


            IssueDate = issueDate;
            ExpirationDate = expirationDate;
            IsActive = isActive;
            CreatedByUserID = createdByUserID;

            _mode = enStatusMode.UpdateMode;


			if (license is not null && !license.IsActiveLicense())
			{
                IsActive = false;
                _UpadateLicense();
			}
		}


        public InternationalLicense()
        {
            ID = -1;
            ApplicationID = -1;

            DriverID = -1;

            LocalLicenseID = -1;

            IssueDate = DateTime.Now;
            //ExpirationDate = IssueDate.AddYears(10);
            IsActive = false;

            CreatedByUserID = -1;

            _mode = enStatusMode.AddMode;
        }



        public static InternationalLicense Find(int id)
        {
            int applicationID = -1;
            int driverID = -1;

            int localLicenseID = -1;

            DateTime issueDate = DateTime.MinValue;
            DateTime expirationDate = DateTime.MinValue;
            bool isActive = false;

            int createdByUserID = -1;

           

            if (InternationalLicenseDataAccess.FindByInternationalLicenseID(id, ref applicationID, ref driverID,
                ref localLicenseID, ref issueDate, ref expirationDate, ref isActive,
                ref createdByUserID))
            {
                return new InternationalLicense(id, applicationID, driverID, localLicenseID,
                    issueDate, expirationDate, isActive, createdByUserID);
            }

            return null;
        }


        public static InternationalLicense FindByApplicatonID(int applicationID)
        {
            int driverID = -1;
            int id = -1;

            int localLicenseID = -1;

            DateTime issueDate = DateTime.MinValue;
            DateTime expirationDate = DateTime.MinValue;
            bool isActive = false;

            int createdByUserID = -1;

            if (InternationalLicenseDataAccess.FindByApplicationID(applicationID, ref id, ref driverID,
                 ref localLicenseID,
                 ref issueDate, ref expirationDate, ref isActive,
                 ref createdByUserID))
            {
                return new InternationalLicense(id, applicationID, driverID,
                    localLicenseID, issueDate, expirationDate, isActive, createdByUserID);
            }

            return null;
        }




        private bool _AddNewLicense()
        {
            return (ID = InternationalLicenseDataAccess.AddNewInternationalLicense(ApplicationID, DriverID,
                LocalLicenseID,
                IssueDate, ExpirationDate,
                IsActive, CreatedByUserID)) > 0;
        }


        private bool _UpadateLicense()
        {
            return InternationalLicenseDataAccess.UpdateInternationalLicense(ID, IsActive);
        }


        internal bool Save()
        {
            if (license == null) return false;

            if (DateTime.Now >= ExpirationDate) return false;

            switch (_mode)
            {
                case enStatusMode.AddMode:

                    if (_AddNewLicense())
                    {
                        _mode = enStatusMode.UpdateMode;

                        return true;
                    }

                    return false;

                case enStatusMode.UpdateMode:
                    return _UpadateLicense();


                default:
                    return true;
            }
        }
        public static DataTable GetAll(int id)
        {
            return InternationalLicenseDataAccess.GetAllInternationalLicensesByDriverID(id);
        }


        public static bool IsExist(int id)
        {
            return InternationalLicenseDataAccess.IsExist(id);
        }

        public static bool IsExistByDriverID(int id)
        {
            return InternationalLicenseDataAccess.IsExistByDriverID(id);
        }




        public static bool IsAcitveLicenseExistByDriverID(int driverID)
        {
            return InternationalLicenseDataAccess.IsActiveLicenseExistByDriverID(driverID);
        }


        public static int GetLicenseIdOfExistingLicenseByDriverID(int driverID)
        {
            return InternationalLicenseDataAccess.GetIDOfActiveLicenseExistByDriverID(driverID);
        }
    }
}
