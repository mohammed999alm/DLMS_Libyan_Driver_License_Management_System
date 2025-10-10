using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalUtility;
using DLMS_DTO;
using DLMS_DataAccessLayer.TransactionUnits;

namespace DLMS_DataAccessLayer
{
    public class RequestDataAccess
    {
        public static bool FindByRequestID(int id, ref string nationalNumber, ref int? licenseClassID, ref int? licenseID,
                    ref int requestTypeID, ref byte requestStatusID,
                    ref decimal? FeesAmount, ref bool isPaid, ref DateTime? paymentTimeStamps,
                    ref DateTime CreatedDate, ref DateTime? UpdatedDate
                  )
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Requests WHERE RequestID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    nationalNumber = (string)reader["NationalNumber"];
                    licenseClassID = reader["LicenseClassID"] != DBNull.Value ? (int?)reader["licenseClassID"] : null;
                    licenseID = reader["LicenseID"] != DBNull.Value ? (int?)reader["licenseID"] : null;

                    requestTypeID = (int)reader["RequestTypeID"];
                    requestStatusID = (byte)reader["RequestStatusID"];


                    isPaid = (bool)reader["IsPaid"];


                    FeesAmount = (reader["FeesAmount"] != DBNull.Value) ?
                        (decimal?)reader["FeesAmount"] : null;

                    if (reader["PaymentTimeStamp"] != DBNull.Value)
                        paymentTimeStamps = (DateTime)reader["PaymentTimeStamp"];
                    else
                        paymentTimeStamps = null;



                    CreatedDate = (DateTime)reader["CreatedDate"];

                    UpdatedDate = (reader["UpdatedDate"] != DBNull.Value) ? (DateTime?)reader["UpdatedDate"] : null;

                    isFound = true;
                }

                reader.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());

                //isFound = false;
            }
            finally { connection.Close(); }

            return isFound;
        }







        private static DataColumn _CreateDataColumn(string name, Type type)
        {
            DataColumn column = new DataColumn(name);
            column.DataType = type;

            column.AllowDBNull = true;

            return column;
        }



        public static DataTable GetAllRequestsDetails()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM RequestsView";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
   
                dt.Columns.Add(_CreateDataColumn("الرقم الوطني", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الإسم", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("نوع الطلب", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("نوع الرخصة", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("رقم الرخصة", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("حالة الطلب", typeof(string)));

                dt.Columns.Add(_CreateDataColumn("تاريخ إنشاء الطلب", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("تاريخ الدفع", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("تاريخ إنتهاء الطلب", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("رسوم الخدمة", typeof(decimal)));
                dt.Columns.Add(_CreateDataColumn("هل تم الدفع", typeof(bool)));

      

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["RequestID"];

                    if (reader["LicenseID"] != DBNull.Value)
                       row["رقم الرخصة"] = (int)reader["LicenseID"];

                    row["الرقم الوطني"] = (string)reader["NationalNumber"];
                    row["الإسم"] = (string)reader["ApplicantName"];

                    if (reader["ClassName"] != DBNull.Value)
                        row["نوع الرخصة"] = (string)reader["ClassName"];


                    row["حالة الطلب"] = (string)reader["StatusType"];
                    row["نوع الطلب"] = (string)reader["ApplicationTypeTitle"];
                    row["تاريخ إنشاء الطلب"] = (DateTime)reader["CreatedDate"];


                    if (reader["UpdatedDate"] != DBNull.Value)
                        row["تاريخ إنتهاء الطلب"] = (DateTime)reader["UpdatedDate"];


                    if (reader["PaymentTimeStamp"] != DBNull.Value)
                        row["تاريخ إنتهاء الطلب"] = (DateTime)reader["PaymentTimeStamp"];

                    if (reader["FeesAmount"] != DBNull.Value)
                        row["رسوم الخدمة"] = (decimal)reader["FeesAmount"];
             
                    if (reader["IsPaid"] != DBNull.Value)
                        row["هل تم الدفع"] = (bool)reader["IsPaid"];

                  
                    dt.Rows.Add(row);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());

                LoggerUtil.LogError(ex, nameof(RequestDataAccess), nameof(GetAllRequestsDetails));
                dt = null;
            }
            finally
            {
                connection.Close();
            }

            return dt;
        }



        public static List<RequestDetailedDto> GetAllRequestsDetailsList(string nationalNumber)
        {
            var requests = new List<RequestDetailedDto>();

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            {
                string query = "SELECT * FROM RequestsView WHERE NationalNumber = @nationalNumber";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nationalNumber", nationalNumber);

                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var dto = new RequestDetailedDto
                                {
                                    RequestID = (int)reader["RequestID"],
                                    NationalNumber = (string)reader["NationalNumber"],
                                    ApplicantName = (string)reader["ApplicantName"],
                                    ApplicationTypeTitle = (string)reader["ApplicationTypeTitle"],
                                    StatusType = (string)reader["StatusType"],
                                    CreatedDate = (DateTime)reader["CreatedDate"],

                                    LicenseID = reader["LicenseID"] != DBNull.Value ? (int?)reader["LicenseID"] : null,
                                    ClassName = reader["ClassName"] != DBNull.Value ? (string?)reader["ClassName"] : null,
                                    UpdatedDate = reader["UpdatedDate"] != DBNull.Value ? (DateTime?)reader["UpdatedDate"] : null,
                                    PaymentTimeStamp = reader["PaymentTimeStamp"] != DBNull.Value ? (DateTime?)reader["PaymentTimeStamp"] : null,
                                    FeesAmount = reader["FeesAmount"] != DBNull.Value ? (decimal?)reader["FeesAmount"] : null,
                                    IsPaid = reader["IsPaid"] != DBNull.Value ? (bool?)reader["IsPaid"] : null
                                };

                                requests.Add(dto);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());

                        LoggerUtil.LogError(ex, nameof(RequestDataAccess), nameof(GetAllRequestsDetailsList),
                            new Dictionary<string, object> { { "NationalNumber", nationalNumber } });
                    }
                }
            }

            return requests;
        }


        public static DataTable GetAllRequests()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Requests";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();


                if (reader.HasRows)
                {
                    dt.Load(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());
                dt = null;
            }
            finally
            {
                connection.Close();
            }

            return dt;
        }




        public static int AddNewRequest(string nationalNumber, int? licenseClassID, int? licenseID,
     int? requestTypeID, int requestStatusID, decimal? FeesAmount, bool isPaid,
      DateTime? paymentTimeStamps, DateTime CreatedDate, DateTime? UpdatedDate)
        {
            if (licenseClassID == null && licenseID == null) return -1;

            int idInserted = -1;

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            {
                string query = @"INSERT INTO Requests (NationalNumber, LicenseClassID, LicenseID, RequestTypeID, RequestStatusID, 
                          FeesAmount, IsPaid, PaymentTimeStamp, CreatedDate, UpdatedDate)
                          VALUES (@nationalNumber, @licenseClassID, @licenseID, @requestTypeID, @requestStatusID, 
                          @FeesAmount, @isPaid, @paymentTimeStamps, @CreatedDate, @UpdatedDate)

                          SELECT SCOPE_IDENTITY()";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@nationalNumber", nationalNumber);
                    command.Parameters.AddWithValue("@CreatedDate", CreatedDate);
                    command.Parameters.AddWithValue("@requestTypeID", requestTypeID);
                    command.Parameters.AddWithValue("@requestStatusID", requestStatusID);


                    command.Parameters.AddWithValue("@licenseClassID", licenseClassID ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@licenseID", licenseID ?? (object)DBNull.Value);


                    command.Parameters.AddWithValue("@FeesAmount", FeesAmount ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@isPaid", isPaid);
                    command.Parameters.AddWithValue("@paymentTimeStamps", paymentTimeStamps ??
                        (object)DBNull.Value);
                    command.Parameters.AddWithValue("@UpdatedDate", UpdatedDate ?? (object)DBNull.Value);

                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int resultID))
                        {
                            idInserted = resultID;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("{0} : {1}", ex.Message, ex.ToString());
                    }
                }
            }

            return idInserted;
        }


        public static bool UpdateRequest(int id, int statusID, DateTime? paymentTimeStamps, DateTime? UpdatedDate,
            decimal? FeesAmount, bool isPaid)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Update Requests 
                            Set RequestStatusID = @requestStatusID,
                            FeesAmount = @FeesAmount,
                            IsPaid = @isPaid,
                            PaymentTimeStamp = @paymentTimeStamps,
                            UpdatedDate = @UpdatedDate
                            WHERE RequestID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@requestStatusID", statusID);

            command.Parameters.AddWithValue("@FeesAmount", FeesAmount ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@isPaid", isPaid);
            command.Parameters.AddWithValue("@paymentTimeStamps", paymentTimeStamps ??
                (object)DBNull.Value);
            command.Parameters.AddWithValue("@UpdatedDate", UpdatedDate ?? (object)DBNull.Value);
            try
            {
                connection.Open();

                rowAffected = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return rowAffected > 0;
        }

        public static bool DeleteRequest(int id)
        {
            if (!ApplicationDataAccess.IsExistByRequestID(id) || ApplicationDataAccess.CouldBeDeletedByRequestID(id))
                return RequestTransactions.DeleteRequest(id);

            return false;
        }


        public static bool DeleteRequestByNationalNumber(string nationalNumber)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete Requests
                            WHERE NationalNumber = @nationalNumber";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);


            try
            {
                connection.Open();

                rowAffected = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return rowAffected > 0;
        }

        public static bool IsExist(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select RequestID From Requests
                            WHERE RequestID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null)
                {
                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return isFound;
        }

        //public static bool IsExist(string Requestname)
        //{
        //    bool isFound = false;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Select RequestID From Requests
        //                    WHERE RequestID = @RequestID";

        //    SqlCommand command = new SqlCommand(query, connection);


        //    command.Parameters.AddWithValue("@RequestID", Requestname);


        //    try
        //    {
        //        connection.Open();

        //        object row = command.ExecuteScalar();

        //        if (row != null)
        //        {
        //            isFound = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"{ex.Message}  {ex}");
        //    }
        //    finally { connection.Close(); }


        //    return isFound;
        //}


        public static bool IsActiveRequestExistByNationalNumber(string nationalNumber)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM RequestsView
                             WHERE NationalNumber = @nationalNumber AND (StatusType = 'موافق عليه' or StatusType = 'قيد الإنتظار')";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null)
                {
                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return isFound;
        }

        public static bool IsRequestExistByNationalNumber(string nationalNumber)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select RequestID From RequestsView
                            WHERE NationalNumber = @nationalNumber";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null)
                {
                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return isFound;
        }


        public static int GetRequestIDFromTheLatestRequestByNationalNumber(string nationalNumber)
        {

            int requestID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT TOP 1 RequestID FROM RequestsView 
                            WHERE NationalNumber = @nationalNumber  ORDER BY CreatedDate DESC";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int rowID))
                {
                    requestID = rowID;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return requestID;
        }


        public static int GetLocalLicenseIDBy_RequestID(int id)
        {
            int appID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT lApp.LocalLicenseApplicationID FROM Requests req 
                             INNER JOIN Applications app ON app.RequestID = req.RequestID 
                             INNER JOIN LocalLicenseApplications lApp ON lApp.ApplicationID = app.ApplicationID 
                             WHERE req.RequestID = @reqID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@reqID", id);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int result))
                {
                    appID = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return appID;
        }



        //public static bool IsActiveRetakeRequestExistByPersonIDAndRequestType(int id, string RequestType)
        //{
        //    bool isFound = false;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Select RequestID From RequestsView
        //                    WHERE PersonID = @ID  AND StatusTypeTitle = 'جديد' AND RequestTypeTitle = @type";

        //    SqlCommand command = new SqlCommand(query, connection);


        //    command.Parameters.AddWithValue("@ID", id);
        //    command.Parameters.AddWithValue("@type", RequestType);


        //    try
        //    {
        //        connection.Open();

        //        object row = command.ExecuteScalar();

        //        if (row != null)
        //        {
        //            isFound = true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"{ex.Message}  {ex}");
        //    }
        //    finally { connection.Close(); }


        //    return isFound;
        //}


        //public static int GetActiveRequestIDByPersonID(int id)
        //{

        //    int RequestID = -1;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Select RequestID From RequestsView
        //                    WHERE PersonID = @ID  AND StatusTypeTitle = 'جديد'";

        //    SqlCommand command = new SqlCommand(query, connection);


        //    command.Parameters.AddWithValue("@ID", id);


        //    try
        //    {
        //        connection.Open();

        //        object row = command.ExecuteScalar();

        //        if (row != null && int.TryParse(row.ToString(), out int temp))
        //        {
        //            RequestID = temp;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"{ex.Message}  {ex}");
        //    }
        //    finally { connection.Close(); }


        //    return RequestID;
        //}
    }
}
