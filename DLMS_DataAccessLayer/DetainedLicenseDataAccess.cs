using System;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Runtime.Remoting;

namespace DLMS_DataAccessLayer
{
    public class DetainedLicenseDataAccess
    {
        public static bool FindByDetainID(int detainID,
            ref int licenseID, ref DateTime detainDate, ref decimal fineFees,
            ref int createdByUserID, ref DateTime? releaseDate, ref int? releasedByUserID,
            ref int? releasedApplicationID, ref string notes, ref bool isDetained)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = "SELECT * FROM DetainedLicenses WHERE DetainID = @id";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", detainID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    licenseID = (int)reader["LicenseID"];
                    detainDate = (DateTime)reader["DetainDate"];
                    fineFees = (decimal)reader["FineFees"];
                    createdByUserID = (int)reader["CreatedByUserID"];
                    releaseDate = reader["ReleaseDate"] != DBNull.Value ? (DateTime?)reader["ReleaseDate"] : null;
                    releasedByUserID = reader["ReleasedByUserID"] != DBNull.Value ? (int?)reader["ReleasedByUserID"] : null;
                    releasedApplicationID = reader["ReleasedApplicationID"] != DBNull.Value ?
                        (int?)reader["ReleasedApplicationID"] : null;
                    notes = reader["Notes"].ToString();

                    isDetained = (bool)reader["IsDetained"];

                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message} | Details: {ex.StackTrace}");
            }
            finally
            {
                connection.Close();
            }

            return isFound;
        }


        public static bool FindByLicenseID(ref int detainID,
           int licenseID, ref DateTime detainDate, ref decimal fineFees,
           ref int createdByUserID, ref DateTime? releaseDate, ref int? releasedByUserID,
           ref int? releasedApplicationID, ref string notes, ref bool isDetained)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = "SELECT * FROM DetainedLicenses WHERE LicenseID = @id And isDetained = 1";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", licenseID);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    detainID = (int)reader["DetainID"];
                    detainDate = (DateTime)reader["DetainDate"];
                    fineFees = (decimal)reader["FineFees"];
                    createdByUserID = (int)reader["CreatedByUserID"];
                    releaseDate = reader["ReleaseDate"] != DBNull.Value ? (DateTime?)reader["ReleaseDate"] : null;
                    releasedByUserID = reader["ReleasedByUserID"] != DBNull.Value ? (int?)reader["ReleasedByUserID"] : null;
                    releasedApplicationID = reader["ReleasedApplicationID"] != DBNull.Value ?
                        (int?)reader["ReleasedApplicationID"] : null;

                    notes = reader["Notes"].ToString();

                    isDetained = (bool)reader["IsDetained"];


                    isFound = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message} | Details: {ex.StackTrace}");
            }
            finally
            {
                connection.Close();
            }

            return isFound;
        }

        public static int AddNewDetainedLicense(int licenseID, DateTime detainDate, decimal fineFees,
            int createdByUserID, string notes)
        {
            int id = -1;

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            {
                string query = @"
                    INSERT INTO DetainedLicenses 
                    (LicenseID, DetainDate, FineFees, CreatedByUserID, Notes, IsDetained)
                    VALUES 
                    (@LicenseID, @DetainDate, @FineFees, @CreatedByUserID, @Notes, 1);
                    SELECT SCOPE_IDENTITY();";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@LicenseID", licenseID);
                command.Parameters.AddWithValue("@DetainDate", detainDate);
                command.Parameters.AddWithValue("@FineFees", fineFees);
                command.Parameters.AddWithValue("@CreatedByUserID", createdByUserID);
                command.Parameters.AddWithValue("@Notes", notes);

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
                    Debug.WriteLine($"Error adding detained license: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            }

            return id;
        }


        public static bool UpdateDetainedLicense(int detainedLicense, DateTime? releaseDate,
           int? releasedByUserID, int? applicationID)
        {
            int rowAffected = -1;

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            {
                string query = @"
                    UPDATE DetainedLicenses 
                    SET ReleaseDate = @ReleaseDate,
                    ReleasedByUserID = @ReleasedByUserID,
                    ReleasedApplicationID = @ReleasedByApplicationID,
                    IsDetained = 0
                    WHERE DetainID = @ID;";

                SqlCommand command = new SqlCommand(query, connection);

                command.Parameters.AddWithValue("@ID", detainedLicense);

                command.Parameters.AddWithValue("@ReleaseDate", (releaseDate
                    != null) ? (object)releaseDate : DBNull.Value);

                command.Parameters.AddWithValue("@ReleasedByUserID", (releasedByUserID
                    != null) ? (object)releasedByUserID : DBNull.Value);

                command.Parameters.AddWithValue("@ReleasedByApplicationID",
                    (applicationID != null) ? (object)applicationID : DBNull.Value);

                try
                {
                    connection.Open();
                    rowAffected = command.ExecuteNonQuery();


                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error adding detained license: {ex.Message} | StackTrace: {ex.StackTrace}");
                }
            }

            return rowAffected > 0;
        }

        public static bool IsDetainedLicenseExist(int detainID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = "SELECT 1 FROM DetainedLicenses WHERE DetainID = @id AND ReleaseDate IS NULL";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", detainID);

            try
            {
                connection.Open();
                object row = command.ExecuteScalar();
                isFound = row != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message} | Details: {ex}");
            }
            finally
            {
                connection.Close();
            }

            return isFound;
        }


        public static bool IsDetainedLicenseExistByLicenseID(int licenseID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = "SELECT 1 FROM DetainedLicenses WHERE LicenseID = @id  AND ReleaseDate IS NULL";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", licenseID);

            try
            {
                connection.Open();
                object row = command.ExecuteScalar();
                isFound = row != null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message} | Details: {ex}");
            }
            finally
            {
                connection.Close();
            }

            return isFound;
        }



        public static int GetIdOfDetainedLicenseExistByLicenseID(int licenseID)
        {
            int detainedID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = "SELECT DetainID FROM DetainedLicenses WHERE LicenseID = @id  AND ReleaseDate IS NULL";

            SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", licenseID);

            try
            {
                connection.Open();
                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int result))
                {
                    detainedID = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message} | Details: {ex}");
            }
            finally
            {
                connection.Close();
            }

            return detainedID;
        }



        public static DataTable GetAllDetainedLicenses()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);
            string query = "SELECT * FROM DetainedLicenses";

            SqlCommand command = new SqlCommand(query, connection);
            DataTable dataTable = new DataTable();

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                dataTable.Load(reader);

                if (dataTable.Rows.Count == 0)
                    dataTable = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message} | Details: {ex.StackTrace}");
                dataTable = null;
            }
            finally
            {
                connection.Close();
            }

            return dataTable;
        }
    }
}
