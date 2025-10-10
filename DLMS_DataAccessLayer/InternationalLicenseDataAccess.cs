using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using GlobalUtility;
using System.Linq.Expressions;

namespace DLMS_DataAccessLayer
{
    public class InternationalLicenseDataAccess
    {


        private static bool _ArchiveExpiredLicenses()
        {
            int rowsAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"UPDATE InternationalLicenses SET IsActive = 0 
                            WHERE GETDATE() >= InternationalLicenses.ExpirationDate";

            SqlCommand command = new SqlCommand(query, connection);

            try
            {
                connection.Open();

                rowsAffected = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception Message {ex.Message}  | Stack Trace {ex.StackTrace}");
            }
            finally { connection.Close(); }

            return rowsAffected > 0;
        }
        public static bool FindByInternationalLicenseID(int internationalLicenseID,
    ref int applicationID, ref int driverID, ref int issuedByLocalLicenseID,
    ref DateTime issueDate, ref DateTime expirationDate,
    ref bool isActive, ref int createdByUserID)
        {
            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Internationallicenses WHERE InternationalLicenseID = @id";



            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", internationalLicenseID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    applicationID = (int)reader["ApplicationID"];
                    driverID = (int)reader["DriverID"];
                    issuedByLocalLicenseID = (int)reader["IssuedByLocalLicenseID"];
                    issueDate = (DateTime)reader["IssueDate"];
                    expirationDate = (DateTime)reader["ExpirationDate"];
                    isActive = (bool)reader["IsActive"];
                    createdByUserID = (int)reader["CreatedByUserID"];

                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Message: {ex.Message} | Error Details: {ex.StackTrace}");
            }
            finally
            {
                connection.Close();
            }

            return isFound;
        }


        public static bool FindByApplicationID(int applicationID,
    ref int internationalLicenseID, ref int driverID, ref int issuedByLocalLicenseID,
    ref DateTime issueDate, ref DateTime expirationDate,
    ref bool isActive, ref int createdByUserID)
        {

            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = "SELECT * FROM Internationallicenses WHERE ApplicationID = @id";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", applicationID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    internationalLicenseID = (int)reader["InternationalLicenseID"];
                    driverID = (int)reader["DriverID"];
                    issuedByLocalLicenseID = (int)reader["IssuedByLocalLicenseID"];
                    issueDate = (DateTime)reader["IssueDate"];
                    expirationDate = (DateTime)reader["ExpirationDate"];
                    isActive = (bool)reader["IsActive"];
                    createdByUserID = (int)reader["CreatedByUserID"];

                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Message: {ex.Message} | Error Details: {ex.StackTrace}");
            }
            finally
            {
                connection.Close();
            }

            return isFound;
        }



        public static DataTable GetAllInternationalLicensesByDriverID(int driverID)
        {
            _ArchiveExpiredLicenses();

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = @"SELECT * FROM InternationalLicenses
                     WHERE DriverID = @id";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", driverID);

            DataTable dataTable = new DataTable();

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                dataTable.Columns.Add("الفهرس", typeof(int));
                dataTable.Columns.Add("معرف الطلب", typeof(int));
                dataTable.Columns.Add("رقم الرخصة المحلية", typeof(int));
                dataTable.Columns.Add("تاريخ الإصدار", typeof(DateTime));
                dataTable.Columns.Add("تاريخ انتهاء الصلاحية", typeof(DateTime));
                dataTable.Columns.Add("فعالة", typeof(bool));
                dataTable.Columns.Add("تم إنشائه من قبل المستخدم", typeof(int));

                while (reader.Read())
                {
                    DataRow row = dataTable.NewRow();

                    row["الفهرس"] = (int)reader["InternationalLicenseID"];
                    row["معرف الطلب"] = (int)reader["ApplicationID"];
                    row["رقم الرخصة المحلية"] = (int)reader["IssuedByLocalLicenseID"];
                    row["تاريخ الإصدار"] = (DateTime)reader["IssueDate"];
                    row["تاريخ انتهاء الصلاحية"] = (DateTime)reader["ExpirationDate"];
                    row["فعالة"] = (bool)reader["IsActive"];
                    row["تم إنشائه من قبل المستخدم"] = (int)reader["CreatedByUserID"];

                    dataTable.Rows.Add(row);
                }

                if (dataTable.Rows.Count <= 0)
                    dataTable = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error Message: {ex.Message} | Error Details: {ex.StackTrace}");
                dataTable = null;
            }
            finally
            {
                connection.Close();
            }

            return dataTable;
        }


        public static int AddNewInternationalLicense(
    int applicationID, int driverID, int issuedByLocalLicenseID,
    DateTime issueDate, DateTime expirationDate, bool isActive, int createdByUserID)
        {
            int id = -1;

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            {
                string query = @"
                                INSERT INTO Internationallicenses 
                                (ApplicationID, DriverID, IssuedByLocalLicenseID, IssueDate, 
                                ExpirationDate, IsActive, CreatedByUserID)
                                 VALUES 
                                 (@ApplicationID, @DriverID, @IssuedByLocalLicenseID, @IssueDate,
                                    @ExpirationDate, @IsActive, @CreatedByUserID);
                                SELECT SCOPE_IDENTITY();";

                try
                {
					connection.Open();

					using (SqlTransaction trx = connection.BeginTransaction())
                    {

                        try
                        {
                            SqlCommand command = new SqlCommand(query, connection, trx);

                            command.Parameters.AddWithValue("@ApplicationID", applicationID);
                            command.Parameters.AddWithValue("@DriverID", driverID);
                            command.Parameters.AddWithValue("@IssuedByLocalLicenseID", issuedByLocalLicenseID);
                            command.Parameters.AddWithValue("@IssueDate", issueDate);
                            command.Parameters.AddWithValue("@ExpirationDate", expirationDate);
                            command.Parameters.AddWithValue("@IsActive", isActive);
                            command.Parameters.AddWithValue("@CreatedByUserID", createdByUserID);


                            Update(GetIDOfLatestLicenseExistByDriverID(driverID), false, connection, trx);

                            object row = command.ExecuteScalar();

                            if (row != null && int.TryParse(row.ToString(), out int rowID))
                            {
                                id = rowID;
                            }

                            trx.Commit();
                        }
                        catch (Exception ex) 
                        {
                            trx.Rollback();
					        LoggerUtil.LogError(ex, nameof(InternationalLicenseDataAccess), nameof(AddNewInternationalLicense), 
                                new Dictionary<string, object> { { "DriverID", driverID}, { "ApplicationID", applicationID} });
						}
					}

                }
                catch (Exception ex) 
                {

					Debug.WriteLine($"Error adding new international license: {ex.Message} | StackTrace: {ex.StackTrace}");

					LoggerUtil.LogError(ex, nameof(InternationalLicenseDataAccess), nameof(AddNewInternationalLicense));
				}
            }

            return id;
        }


        public static bool UpdateInternationalLicense(int internationalLicenseID, bool isActive)
        {
            int rowsAffected = 0;

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            {
                string query = @"
                UPDATE Internationallicenses 
                 SET IsActive = @IsActive 
                 WHERE InternationalLicenseID = @InternationalLicenseID";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@InternationalLicenseID", internationalLicenseID);
                command.Parameters.AddWithValue("@IsActive", isActive);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error updating international license: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            }

            return rowsAffected > 0;
        }


        private static bool Update(int internationalLicenseID, bool isActive, SqlConnection connection, SqlTransaction trx)
        {
            int rowsAffected = 0;


            string query = @"
                UPDATE Internationallicenses 
                 SET IsActive = @IsActive 
                 WHERE InternationalLicenseID = @InternationalLicenseID";

            using (SqlCommand command = new SqlCommand(query, connection, trx))
            {

                command.Parameters.AddWithValue("@InternationalLicenseID", internationalLicenseID);
                command.Parameters.AddWithValue("@IsActive", isActive);

                rowsAffected = command.ExecuteNonQuery();   

				return rowsAffected > 0;

			}
		}



		public static bool IsExist(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select 1 From InternationalLicenses
                            WHERE InternationalLicenseID";

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




        public static bool IsExistByDriverID(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select 1 From InternationalLicenses
                            WHERE DriverID = @ID";

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
        public static bool IsActiveLicenseExistByDriverID(int id)
        {
            _ArchiveExpiredLicenses();

            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select 1 From InternationalLicenses
                            WHERE DriverID = @ID  AND IsActive = 1";

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


        public static int GetIDOfActiveLicenseExistByDriverID(int id)
        {
            _ArchiveExpiredLicenses();

            int licenseID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select InternationalLicenseID From InternationalLicenses
                            WHERE DriverID = @ID  AND IsActive = 1";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int result))
                {
                    licenseID = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return licenseID;
        }



		public static int GetIDOfLatestLicenseExistByLicenseID(int id)
		{
			_ArchiveExpiredLicenses();

			int licenseID = -1;

			SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

			string query = @"SELECT TOP 1 InternationalLicenseID From InternationalLicenses
                            WHERE IssuedByLocalLicenseID = @ID  AND IsActive = 1";

			SqlCommand command = new SqlCommand(query, connection);


			command.Parameters.AddWithValue("@ID", id);


			try
			{
				connection.Open();

				object row = command.ExecuteScalar();

				if (row != null && int.TryParse(row.ToString(), out int result))
				{
					licenseID = result;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"{ex.Message}  {ex}");
			}
			finally { connection.Close(); }


			return licenseID;
		}


		public static int GetIDOfLatestLicenseExistByDriverID(int id)
		{
			_ArchiveExpiredLicenses();

			int licenseID = -1;

			SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

			string query = @"SELECT TOP 1 InternationalLicenseID From InternationalLicenses
                            WHERE DriverID = @ID  AND IsActive = 1 ORDER BY InternationalLicenseID DESC";

			SqlCommand command = new SqlCommand(query, connection);


			command.Parameters.AddWithValue("@ID", id);


			try
			{
				connection.Open();

				object row = command.ExecuteScalar();

				if (row != null && int.TryParse(row.ToString(), out int result))
				{
					licenseID = result;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"{ex.Message}  {ex}");
			}
			finally { connection.Close(); }


			return licenseID;
		}
	}
}
