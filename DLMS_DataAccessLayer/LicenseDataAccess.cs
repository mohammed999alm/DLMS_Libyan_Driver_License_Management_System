using System;
using System.ComponentModel;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.Remoting;
using static System.Net.Mime.MediaTypeNames;
using GlobalUtility;
using System.Runtime.CompilerServices;


namespace DLMS_DataAccessLayer
{
    public class LicenseDataAccess
    {


        private static bool _ArchiveExpiredLicenses()
        {
            int rowsAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"UPDATE Licenses SET IsActive = 0 
                            WHERE GETDATE() >= Licenses.ExpirationDate";

            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                connection.Open();

                rowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID));
                //GlobalUtility.LoggerUtil.SetTheLogMessage(ex, "");
                //Debug.WriteLine($"Exception Message : {ex.Message}    |  Stack Trace {ex.StackTrace}");
            }
            finally { connection.Close(); }

            return rowsAffected > 0;
        }



        public static bool FindByLicenseID(int licenseID, ref int applicationID, ref int driverID,
            ref int licenseClassID, ref int issueReasonID, ref DateTime issueDate, ref DateTime expirationDate,
            ref bool isActive, ref string notes, ref decimal paidFees, ref int createdByUserID)
        {
            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Licenses WHERE LicenseID = @id";


            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", licenseID);


            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    applicationID = (int)reader["ApplicationID"];
                    driverID = (int)reader["DriverID"];
                    licenseClassID = (int)reader["LicenseClassID"];
                    issueReasonID = (int)reader["IssueReasonID"];
                    issueDate = (DateTime)reader["IssueDate"];
                    expirationDate = (DateTime)reader["ExpirationDate"];
                    isActive = (bool)reader["IsActive"];

                    notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null;

                    paidFees = (decimal)reader["paidFees"];
                    createdByUserID = (int)reader["CreatedByUserID"];

                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
new Dictionary<string, object> { { "LicenseID", licenseID } });

                //Debug.WriteLine($"Error Message : {ex.Message}  : Error Details {ex.StackTrace}");
            }
            finally { connection.Close(); }

            return isFound;
        }



        public static bool FindByDriverID(ref int licenseID, ref int applicationID, int driverID,
          ref int licenseClassID, ref int issueReasonID, ref DateTime issueDate, ref DateTime expirationDate,
          ref bool isActive, ref string notes, ref decimal paidFees, ref int createdByUserID)
        {
            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Licenses WHERE DriverID = @id";


            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", driverID);


            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    licenseID = (int)reader["LicenseID"];
                    applicationID = (int)reader["ApplicationID"];
                    licenseClassID = (int)reader["LicenseClassID"];
                    issueReasonID = (int)reader["IssueReasonID"];
                    issueDate = (DateTime)reader["IssueDate"];
                    expirationDate = (DateTime)reader["ExpirationDate"];
                    isActive = (bool)reader["IsActive"];

                    notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null;

                    paidFees = (decimal)reader["paidFees"];
                    createdByUserID = (int)reader["CreatedByUserID"];

                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Message : {ex.Message}  : Error Details {ex.StackTrace}");
            }
            finally { connection.Close(); }

            return isFound;
        }



        public static bool FindByApplicationID(ref int licenseID, int applicationID, ref int driverID,
          ref int licenseClassID, ref int issueReasonID, ref DateTime issueDate, ref DateTime expirationDate,
          ref bool isActive, ref string notes, ref decimal paidFees, ref int createdByUserID)
        {

            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Licenses WHERE ApplicationID = @id";


            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", applicationID);


            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    licenseID = (int)reader["LicenseID"];
                    driverID = (int)reader["DriverID"];
                    licenseClassID = (int)reader["LicenseClassID"];
                    issueReasonID = (int)reader["IssueReasonID"];
                    issueDate = (DateTime)reader["IssueDate"];
                    expirationDate = (DateTime)reader["ExpirationDate"];
                    isActive = (bool)reader["IsActive"];

                    notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null;

                    paidFees = (decimal)reader["paidFees"];
                    createdByUserID = (int)reader["CreatedByUserID"];

                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
         new Dictionary<string, object> { { "applicationID", applicationID } });

                //Debug.WriteLine($"Error Message : {ex.Message}  : Error Details {ex.StackTrace}");
            }
            finally { connection.Close(); }

            return isFound;
        }


        private static DataColumn _CreateDataColumn(string columnName, Type dataType)
        {
            return new DataColumn(columnName, dataType);
        }


        public static DataTable GetAllLocalLicenseByDriverID(int driverID)
        {
            _ArchiveExpiredLicenses();

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT LicenseID, ApplicationID, ClassName, 
                                IssueDate, ExpirationDate, IsActive
                            FROM Licenses

                            INNER JOIN LicenseClasses ON LicenseClasses.LicenseClassID = Licenses.LicenseClassID
                            WHERE DriverID = @id";


            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", driverID);


            DataTable dataTable;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dataTable = new DataTable();

                dataTable.Columns.Add("رقم الرخصة", typeof(int));
                dataTable.Columns.Add("رقم الطلب", typeof(int));
                dataTable.Columns.Add("فئة الرخصة", typeof(string));
                dataTable.Columns.Add("تاريخ تسجيل الرخصة", typeof(DateTime));
                dataTable.Columns.Add("تاريخ الإنتهاء", typeof(DateTime));
                dataTable.Columns.Add("فعالة", typeof(bool));


                while (reader.Read())
                {
                    DataRow row = dataTable.NewRow();

                    row["رقم الرخصة"] = (int)reader["LicenseID"];
                    row["رقم الطلب"] = (int)reader["ApplicationID"];
                    row["فئة الرخصة"] = (string)reader["ClassName"];
                    row["تاريخ تسجيل الرخصة"] = (DateTime)reader["IssueDate"];
                    row["تاريخ الإنتهاء"] = (DateTime)reader["ExpirationDate"];
                    row["فعالة"] = (bool)reader["IsActive"];

                    dataTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                dataTable = null;

                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
               new Dictionary<string, object> { { "driverID", driverID } });

                //Debug.WriteLine($"Error Message : {ex.Message}  : Error Details {ex.StackTrace}");
            }
            finally { connection.Close(); }

            return dataTable;
        }


        public static int GetPreviousLicenseID(int driverID, int licenseID) 
        {
            string query = @"SELECT TOP 1 LicenseID  FROM Licenses WHERE DriverID = @driverID AND LicenseID < @licenseID ORDER BY LicenseID DESC";

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            using (SqlCommand command = new SqlCommand(query, connection)) 
            {
                command.Parameters.AddWithValue("@driverID", driverID);
                command.Parameters.AddWithValue("@licenseID", licenseID);

                try
                {
                    connection.Open();

                    object rowID = command.ExecuteScalar();

                    return int.TryParse(rowID?.ToString(), out int id) ? id : -1;
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
                        new Dictionary<string, object> { { "driverID", driverID }, { "licenseID", licenseID } });

                    return -1;
                }
            }
        }

       
        public static int AddNewLicense(int applicationID, int driverID, int licenseClassID, int issueReasonID,
                                 DateTime issueDate, DateTime expirationDate, bool isActive,
                                 string notes, decimal paidFees, int createdByUserID)
        {


            var parameters = new Dictionary<string, object>
            {
                 { "ApplicationID", applicationID },
                 { "DriverID", driverID },
                 { "LicenseClassID", licenseClassID },
                 { "IssueReasonID", issueReasonID },
                 { "IssueDate", issueDate },
                 { "ExpirationDate", expirationDate },
                 { "IsActive", isActive },
                 { "Notes", string.IsNullOrEmpty(notes) ? "null" : notes },
                 { "PaidFees", paidFees },
                 { "CreatedByUserID", createdByUserID }
            };

            int id = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO Licenses (ApplicationID, DriverID, LicenseClassID, IssueReasonID, IssueDate, 
                      ExpirationDate, IsActive, Notes, PaidFees, CreatedByUserID) 
                      VALUES (@ApplicationID, @DriverID, @LicenseClassID, @IssueReasonID, @IssueDate, @ExpirationDate, 
                      @IsActive, @Notes, @PaidFees, @CreatedByUserID)

                       SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ApplicationID", applicationID);
            command.Parameters.AddWithValue("@DriverID", driverID);
            command.Parameters.AddWithValue("@LicenseClassID", licenseClassID);
            command.Parameters.AddWithValue("@IssueReasonID", issueReasonID);
            command.Parameters.AddWithValue("@IssueDate", issueDate);
            command.Parameters.AddWithValue("@ExpirationDate", expirationDate);
            command.Parameters.AddWithValue("@IsActive", isActive);
            command.Parameters.AddWithValue("@Notes", string.IsNullOrEmpty(notes) ? DBNull.Value : (object)notes);
            command.Parameters.AddWithValue("@PaidFees", paidFees);
            command.Parameters.AddWithValue("@CreatedByUserID", createdByUserID);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int rowID))
                {
                    id = rowID;
                }

            }
            catch (Exception ex)
            {

                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID), parameters);
                Debug.WriteLine($"Error inserting new license: {ex.Message} | StackTrace: {ex.StackTrace}");
            }

            finally { connection.Close(); }



            return id;
        }



        public static bool UpdateLicense(int licenseID, bool isActive)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"UPDATE Licenses 
                             SET IsActive = @isActive

                            WHERE LicenseID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", licenseID);
            command.Parameters.AddWithValue("@isActive", isActive);


            try
            {
                connection.Open();

                rowAffected = command.ExecuteNonQuery();


            }
            catch (Exception ex)
            {

                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
            new Dictionary<string, object> { { "licenseID", licenseID }, { "IsActive", isActive} });
                //Debug.WriteLine($"Error inserting new license: {ex.Message} | StackTrace: {ex.StackTrace}");
            }

            finally { connection.Close(); }



            return rowAffected > 0;
        }



        public static bool IsExistByID(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT 1 FROM Liceneses WHERE LicenseID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null)
                    isFound = true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
           new Dictionary<string, object> { { "licenseID", id } });

                //Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return isFound;

        }


        public static bool IsExistByDriverID(int id)
        {

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT 1 FROM Liceneses WHERE DriverID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null)
                    isFound = true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
        new Dictionary<string, object> { { "DriverID", id } });
                //Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return isFound;
        }


        public static int GetLicenseIdOfExistingLicenseByDriverID(int id)
        {
            int licenseID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT TOP 1 LicenseID FROM Licenses
                             WHERE DriverID = @id
                             ORDER BY LicenseID DESC";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null && int.TryParse(row.ToString(), out int result))
                    licenseID = result;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
        new Dictionary<string, object> { { "DriverID", id } });
                Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return licenseID;
        }



        public static bool IsAcitveLicenseExistByDriverID(int id)
        {
            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT 1 FROM Licenses WHERE DriverID = @id AND IsActive = 1";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null)
                    isFound = true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
        new Dictionary<string, object> { { "DriverID", id } });
                Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return isFound;
        }


        public static bool IsActiveByLicenseID(int id)
        {
            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT 1 FROM Licenses WHERE LicenseID = @id AND IsActive = 1";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null)
                    isFound = true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(IsActiveByLicenseID),
        new Dictionary<string, object> { { "LicenseID", id } });
                Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return isFound;
        }


        public static bool IsExistByLicenseClassAndDriverID(int id, int licenseClassID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT 1 FROM Licenses WHERE DriverID = @id AND LicenseClassID = @licenseClassID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@licenseClassID", licenseClassID);

            //command.Parameters.Add("@id", SqlDbType.Int).Value = id;
            //command.Parameters.Add("@licenseClassID", SqlDbType.Int).Value = licenseClassID;


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null)
                    isFound = true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
        new Dictionary<string, object> { { "driverID", id }, { "LicenseClassID", licenseClassID} });
                Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return isFound;
        }


        public static bool IsLicenseWithDateThresholdByDriverAndClassIdExists(int driverID,
            int licenseClassID, int numberOfYears)
        {

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT 1  FROM Licenses
                              WHERE driverID = @driverID
                              AND LicenseClassID = @licenseClassID
                              AND GetDate() >= DateAdd(year, @numberOfYears, IssueDate)";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@driverID", driverID);
            command.Parameters.AddWithValue("@licenseClassID", licenseClassID);
            command.Parameters.AddWithValue("@numberOfYears", numberOfYears);



            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null)
                    isFound = true;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
        new Dictionary<string, object> { { "driverID", driverID }, { "licenseClassID", licenseClassID }, { "numberOfYears", numberOfYears } });
                Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return isFound;
        }



        public static int GetIDOfActiveLicenseByDriverID(int id)
        {
            _ArchiveExpiredLicenses();

            int licenseID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT LicenseID FROM Liceneses WHERE DriverID = @id AND IsActive = 1";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();


                if (row != null && int.TryParse(row.ToString(), out int rowID))
                    licenseID = rowID;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(GetPreviousLicenseID),
new Dictionary<string, object> { { "DriverID", id } });
                Debug.WriteLine($"Message  :  {ex.Message}  \nDetails : {ex.StackTrace}");
            }


            finally { connection.Close(); }

            return licenseID;
        }



    }
}
