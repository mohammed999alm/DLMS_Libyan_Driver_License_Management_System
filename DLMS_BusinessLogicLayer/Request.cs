using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Net.Http;
using System.Threading.Tasks;
using DLMS_DataAccessLayer;
using GlobalUtility;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Serialization;
using DLMS_DTO;

namespace DLMS_BusinessLogicLayer
{
    enum enRequestStatus { Approved = 1, Pending, Declined, Completed }
    public class Request
    {
        private enStatusMode _mode;

        internal enRequestStatus _enStatus { get; private set; } = enRequestStatus.Pending;

        private enApplicationTypes enAppType;
        public int ID { get; private set; }

        private string nationalNumber;

  
        public string NationalNumber
        {
            get { return nationalNumber; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    nationalNumber = value;

                    person = Person.Find(value);
                }
            }
        }


        [JsonIgnore]
        [BindNever]
        public Person? person { get; private set; }

        private int _requestTypeID;
        public int RequestTypeID
        {
            get { return _requestTypeID; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _requestTypeID = value;
                    requestType = ApplicationType.Find(_requestTypeID);
                    if (requestType != null)
                    {
                        TheTypeOfRequest = requestType.Type;
                        enAppType = (enApplicationTypes)requestType.ID;
                    }
                }
            }
        }


        private int? _licenseClassID;
        public int? LicenseClassID
        {
            get { return _licenseClassID; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _licenseClassID = value;

                    if (_licenseClassID != null) licenseClass = LicenseClass.Find((int)_licenseClassID);
                }
            }
        }

        private int? _licenseID;
        public int? LicenseID
        {
            get { return _licenseID; }
            set
            {
                if (_mode == enStatusMode.AddMode)
                {
                    _licenseID = value;

                    if (_licenseID != null) license = License.Find((int)_licenseID);
                }
            }
        }


        //[JsonIgnore]
        //[BindNever]
        public string? PhoneNumber { get; set; }

        //[JsonIgnore]
        //[BindNever]
        public string? Email { get; set; }

        [JsonIgnore]
        [BindNever]
        public License? license { get; private set; }

        private byte? _requestStatusID;
        public byte? RequestStatusID
        {
            get { return _requestStatusID; }

            set
            {

                if (_enStatus != enRequestStatus.Completed || _enStatus != enRequestStatus.Declined)
                {
                    if (value == null) return;

                    _requestStatusID = value;

                    _enStatus = (enRequestStatus)value;

                    status = RequestStatus.Find((int)_requestStatusID);
                }
            }
        }



        public RequestStatus? status { get; private set; }

        [JsonIgnore]
        [BindNever]
        public LicenseClass? licenseClass { get; private set; }


        [System.Text.Json.Serialization.JsonIgnore]
        [BindNever]
        public ApplicationType? requestType { get; private set; }


        public decimal? FeeAmount { get; internal set; }

        public bool IsPaid { get; private set; }

        public DateTime? PaymentTimeStamps { get; private set; }


        public string TheTypeOfRequest { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }


        internal Application? application { get;  set; }

        private Request(int iD, string nationalNumber, int requestTypeID, int? licenseClassID, int? licenseID, byte statusID, decimal? feeAmount,
            bool isPaid, DateTime? paymentTimeStamps,
            DateTime createDate, DateTime? updateDate)
        {

            ID = iD;
            NationalNumber = nationalNumber;

            RequestTypeID = requestTypeID;

            LicenseClassID = licenseClassID;
            LicenseID = licenseID;

            FeeAmount = feeAmount;
            IsPaid = isPaid;
            PaymentTimeStamps = paymentTimeStamps;

            CreateDate = createDate;
            RequestStatusID = statusID;
            UpdateDate = updateDate;

            application = Application.Find((int?)ID, false);

            _mode = enStatusMode.UpdateMode;
        }

        public Request()
        {
            _mode = enStatusMode.AddMode;
            ID = -1;
            NationalNumber = null;

            RequestTypeID = -1;

            LicenseClassID = null;
            LicenseID = null;

            Email = null;
            PhoneNumber = null;

            //That Below Fields Not  Public For Modification

            FeeAmount = null;
            IsPaid = false;
            PaymentTimeStamps = null;

            CreateDate = DateTime.Now;
            RequestStatusID = (byte)enRequestStatus.Pending;
            UpdateDate = null;
        }



        public static Request? Find(int id)
        {
            string nationalNumber = null;
            int requestedTypeID = -1;
            byte requestedStatusID = (byte)enRequestStatus.Pending;
            int? licenseClassID = null;
            int? licenseID = null;
            decimal? feeAmount = null;
            DateTime? paymentTimeStamps = null;
            DateTime createDate = DateTime.Now;
            DateTime? updateDate = null;
            bool isPaid = false;

            if (RequestDataAccess.FindByRequestID(id, ref nationalNumber, ref licenseClassID, ref licenseID, ref requestedTypeID, ref requestedStatusID,
                ref feeAmount, ref isPaid, ref paymentTimeStamps, ref createDate, ref updateDate))
            {
                return new Request(id, nationalNumber, requestedTypeID, licenseClassID, licenseID, requestedStatusID, feeAmount, isPaid, paymentTimeStamps, createDate, updateDate);
            }

            return null;
        }


        private bool _AddNewRequest()
        {
            int statusID = int.TryParse(RequestStatusID.ToString(), out int num2) ? num2 : 0;

            return (ID = RequestDataAccess.AddNewRequest(NationalNumber, LicenseClassID, LicenseID, RequestTypeID, statusID, FeeAmount, IsPaid, PaymentTimeStamps, CreateDate, UpdateDate)) > 0;
        }

        private bool _UpdateRequest()
        {
            int statusID = int.TryParse(RequestStatusID.ToString(), out int num2) ? num2 : 0;


            return RequestDataAccess.UpdateRequest(ID, statusID, PaymentTimeStamps, UpdateDate, FeeAmount, IsPaid);
        }



        public static async Task<string> GetPersonByIdAsync(string url)
        {
            HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }
                return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
            }
            catch (Exception ex)
            {
                return $"Exception occurred: {ex.Message}";
            }
        }

        public static async Task<DLMS_DTO.ServerJsonCustomObject> GetPersonByIdAsynchronously(string url)
        {
            ServerJsonCustomObject obj = new ServerJsonCustomObject();

            HttpClient client = new HttpClient();
            try
            {
                var response = await client.GetAsync(url);

                obj.Status = response.IsSuccessStatusCode;

                obj.Data = await response.Content.ReadAsStringAsync();  
              
            }
            catch (Exception ex)
            {
                obj.Status = false;
                obj.Data = null;
                obj.ExceptionMessage = ex.Message;
            }

            return obj;
        }
        internal async Task<bool> GovCitizenIsExist()
        {
            string url = $"https://localhost:7256/api/People/NationalNumber/{NationalNumber}";

            string jsonText = await GetPersonByIdAsync(url);

            if (string.IsNullOrEmpty(jsonText)) { return false; }

            Person? person = Newtonsoft.Json.JsonConvert.DeserializeObject<Person>(jsonText);

            if (person == null) { return false; }


            return person.Save() && person.CreateNewContact(PhoneNumber, Email);
        }
        internal bool _IsPersonExist()
        {
            if (Person.IsExist(NationalNumber))
                return true;


            else
            {
                try
                {
                    return GovCitizenIsExist().GetAwaiter().GetResult();
                }
                catch { return false; }
            }
        }

        public static bool HasActiveApplication(string nataionalNumber) 
        {
            Person person  = Person.Find(nataionalNumber);

            if (person == null) { return false; }

            return person != null && (IsActiveRequestExistByNationalNumber(nataionalNumber) || Person.HasActiveApplication(person.ID));
        }

        public bool Save()
        {
            if (LicenseID is null && LicenseClassID is null) return false;


            switch (_mode)
            {
                case enStatusMode.AddMode:

                    if (!_IsPersonExist()) return false;

                    
                    if (HasActiveApplication(NationalNumber)) return false;


                    if (LicenseID != null && license == null) return false;

                    if (license is not null)
                    {
                        if (enAppType == enApplicationTypes.RenewLicenseApp && !license.Renewable()) return false;
                    }
                    if (_AddNewRequest())
                    {
                        _mode = enStatusMode.UpdateMode;
                        return true;
                    }
                    return false;

                case enStatusMode.UpdateMode:
                    return _UpdateRequest();

                default:
                    return false;
            }
        }

        //public static DataTable GetAll()
        //{
        //    return RequestDataAccess.GetAll();
        //}

        //public static bool IsRequestExistByApplicationID(int applicationID)
        //{
        //    return RequestDataAccess.IsRequestExistByApplicationID(applicationID);
        //}

        //public static bool Delete(int id)
        //{
        //    return RequestDataAccess.Delete(id);
        //}


        private bool _CreateLocalLicenseApplication(int createdByUserID, int? licenseClassID)
        {

            if (licenseClassID is null) return false;



            LocalLicenseApplication app = new LocalLicenseApplication
            {
                PersonID = person.ID,
                TypeID = RequestTypeID,
                CreatedByUserID = createdByUserID,
                RequestID = ID,
                CreatedDate = DateTime.Now,
                LicenseClassID = licenseClassID.Value
            };



            return app.Save();
        }

        private bool _CreateOtherApplicationTypes(int createdByUserID)
        {

            Application app = new Application
            {
                PersonID = person.ID,
                TypeID = RequestTypeID,
                CreatedByUserID = createdByUserID,
                RequestID = ID,
                CreatedDate = DateTime.Now,
            };


            return app.Save();
        }


        private bool _IsPersonLoadedInSystem() 
        {

            var temp = Person.Find(NationalNumber);

            if (person is null) person = temp;

            if (temp is null) return false;

            return true;
        }
        private bool _CreateApplication(int createdByUserID)
        {
            _IsPersonLoadedInSystem();     

            switch (enAppType)
            {
                case enApplicationTypes.NewLicenseApp:
                    return _CreateLocalLicenseApplication(createdByUserID, LicenseClassID);
                case enApplicationTypes.RenewLicenseApp:
                    return _CreateLocalLicenseApplication(createdByUserID, license?.LicenseClassID);

                default: return _CreateOtherApplicationTypes(createdByUserID);
            }
        }
        public bool Approve(int approvedByUserID)
        {
            if (person == null || _enStatus != enRequestStatus.Pending) return false;

            if (_CreateApplication(approvedByUserID))
            {
                RequestStatusID = (int)enRequestStatus.Approved;
                UpdateDate = DateTime.Now;
                FeeAmount = requestType?.Fee;

                return Save();
            }

            return false;
        }


        public bool Decline()
        {
            if (_enStatus == enRequestStatus.Completed || _enStatus == enRequestStatus.Declined) { return false; }



            UpdateDate = DateTime.Now;
            RequestStatusID = (byte)enRequestStatus.Declined;
            //PaymentTimeStamps = DateTime.Now;

            //IsPaid = true;

            application = Application.Find(ID);

            if (application != null) 
            {
                if (application.IsActive())
                    return false;
            }

            return Save();
        }

        public bool Completed()
        {
            if (_enStatus != enRequestStatus.Approved) { return false; }

            UpdateDate = DateTime.Now;
            PaymentTimeStamps = DateTime.Now;
            RequestStatusID = (byte)enRequestStatus.Completed;
            IsPaid = true;

            return Save();
        }

        public static DataTable GetAll()
        {
            return RequestDataAccess.GetAllRequestsDetails();
        }


    
        public static List<RequestDetailedDto> GetAllRequestsByNationalNumber(string nationalNumber) 
        {
            return RequestDataAccess.GetAllRequestsDetailsList(nationalNumber);
        }


        public bool IsActive() 
        {
            return IsActiveRequestExistByNationalNumber(NationalNumber);
        }
        public static bool IsActiveRequestExistByNationalNumber(string nationalNumber)
        {
            return RequestDataAccess.IsActiveRequestExistByNationalNumber(nationalNumber);
        }

        public static bool IsRequestExistByNationalNumber(string nationalNumber)
        {
            return RequestDataAccess.IsRequestExistByNationalNumber(nationalNumber);
        }


        public static int GetRequestIDFromTheLatestRequestByNationalNumber(string nationalNumber) 
        {
            return RequestDataAccess.GetRequestIDFromTheLatestRequestByNationalNumber(nationalNumber);
        }


        public static bool IsExist(int id) 
        {

            return RequestDataAccess.IsExist(id);
        }
        public static bool DeleteByRequestID(int id) 
        {
            
            if (!IsExist(id))
                return false;

            if (Application.IsExistByRequestID(id)) 
            {
                if (!Application.CouldBeDeletedByRequestID(id)) 
                {
                    return false;
                }
            }

            return RequestDataAccess.DeleteRequest(id);
        }


    }
}
