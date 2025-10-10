using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;
using DLMS_DataAccessLayer;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DLMS_BusinessLogicLayer
{
    public class DetainedLicense
    {
        private enStatusMode _mode;

        public int DetainID { get; private set; }

        private int _licenseID;
        public int LicenseID
        {
            get { return _licenseID; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _licenseID = value;
                    license = License.Find(_licenseID);
                }
            }
        }


        [JsonIgnore]
        [BindNever]
        public License? license { get; private set; }

        public DateTime DetainDate { get; set; }
        public decimal FineFees { get; set; }

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


        public bool IsLicenseDetained { get; private set; }

        public string? createdByUser { get; private set; }

        public DateTime? ReleaseDate { get; set; }

        private int? _releasedByUserID;
        public int? ReleasedByUserID
        {
            get { return _releasedByUserID; }
            set
            {
                if (_mode == enStatusMode.UpdateMode)
                {
                    _releasedByUserID = value;

                    if (_releasedByUserID != null)
                        releasedByUser = User.Find((int)_releasedByUserID)?.Username;
                }
            }
        }

        public string? releasedByUser { get; private set; }

        private int? _releasedApplicationID;
        public int? ReleasedApplicationID
        {
            get { return _releasedApplicationID; }
            set
            {
                if (_mode == enStatusMode.UpdateMode)
                {
                    _releasedApplicationID = value;

                    if (_releasedApplicationID != null)
                        releasedApplication = Application.Find((int)_releasedApplicationID, false);
                }
            }
        }

        [JsonIgnore]
        [BindNever]
        public Application? releasedApplication { get; private set; }

        public string Notes { get; set; }

        private DetainedLicense(int detainID, int licenseID, DateTime detainDate, decimal fineFees,
            int createdByUserID, DateTime? releaseDate, int? releasedByUserID,
            int? releasedApplicationID, string notes, bool isLicenseDetained)
        {
            DetainID = detainID;
            LicenseID = licenseID;
            DetainDate = detainDate;
            FineFees = fineFees;
            CreatedByUserID = createdByUserID;
            ReleaseDate = releaseDate;
            ReleasedByUserID = releasedByUserID;
            ReleasedApplicationID = releasedApplicationID;
            Notes = notes;

            _mode = enStatusMode.UpdateMode;
            IsLicenseDetained = isLicenseDetained;
        }

        public DetainedLicense()
        {
            DetainID = -1;
            LicenseID = -1;
            DetainDate = DateTime.Now;
            FineFees = 0;
            CreatedByUserID = -1;
            ReleaseDate = null;
            ReleasedByUserID = -1;
            ReleasedApplicationID = -1;
            Notes = "";
            IsLicenseDetained = false;

            _mode = enStatusMode.AddMode;
        }

        public static DetainedLicense Find(int detainID)
        {
            int licenseID = -1;
            DateTime detainDate = DateTime.MinValue;
            decimal fineFees = 0;
            int createdByUserID = -1;
            DateTime? releaseDate = null;
            int? releasedByUserID = -1;
            int? releasedApplicationID = -1;
            string notes = "";
            bool isLicenseDetained = false;

            if (DetainedLicenseDataAccess.FindByDetainID(detainID, ref licenseID, ref detainDate,
                ref fineFees, ref createdByUserID, ref releaseDate,
                ref releasedByUserID, ref releasedApplicationID, ref notes, ref isLicenseDetained))
            {
                return new DetainedLicense(detainID, licenseID, detainDate, fineFees, createdByUserID,
                    releaseDate, releasedByUserID, releasedApplicationID, notes, isLicenseDetained);
            }

            return null;
        }


        public static DetainedLicense FindByLicenseID(int licenseID)
        {
            int detainID = -1;
            DateTime detainDate = DateTime.MinValue;
            decimal fineFees = 0;
            int createdByUserID = -1;
            DateTime? releaseDate = null;
            int? releasedByUserID = -1;
            int? releasedApplicationID = -1;
            string notes = "";
            bool isLicenseDetained = false;


            if (DetainedLicenseDataAccess.FindByLicenseID(ref detainID, licenseID, ref detainDate,
                ref fineFees, ref createdByUserID, ref releaseDate,
                ref releasedByUserID, ref releasedApplicationID, ref notes, ref isLicenseDetained))
            {
                return new DetainedLicense(detainID, licenseID, detainDate, fineFees, createdByUserID,
                    releaseDate, releasedByUserID, releasedApplicationID, notes, isLicenseDetained);
            }

            return null;
        }

        private bool _Add()
        {
            return (DetainID = DetainedLicenseDataAccess.AddNewDetainedLicense(LicenseID, DetainDate,
                FineFees, CreatedByUserID, Notes)) > 0;
        }

        public bool _Update()
        {
            return DetainedLicenseDataAccess.UpdateDetainedLicense(DetainID, ReleaseDate,
                ReleasedByUserID, ReleasedApplicationID);
        }
        internal bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    if (_Add())
                    {
                        _mode = enStatusMode.UpdateMode;
                        return true;
                    }

                    return false;

                case enStatusMode.UpdateMode:
                    if (IsLicenseDetained)
                        if (_Update())
                        {
                            IsLicenseDetained = false;

                            return true;
                        }
                    return false;

                default: return false;
            }
        }

        public static bool IsExist(int detainID)
        {
            return DetainedLicenseDataAccess.IsDetainedLicenseExist(detainID);
        }

        public static bool IsExistByLicenseID(int licenseID)
        {
            return DetainedLicenseDataAccess.IsDetainedLicenseExistByLicenseID(licenseID);
        }

        public static int GetIdOfDetainedLicenseByLicenseID(int licenseID)
        {
            return DetainedLicenseDataAccess.GetIdOfDetainedLicenseExistByLicenseID(licenseID);
        }

        public static DataTable GetAll()
        {
            return DetainedLicenseDataAccess.GetAllDetainedLicenses();
        }


        internal bool Reelease(int releasedByUserID, int releasedByApplicationID)
        {

            ReleasedByUserID = releasedByUserID;
            ReleasedApplicationID = releasedByApplicationID;
            ReleaseDate = DateTime.Now;

            return Save();
        }
    }
}
