using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DLMS_DataAccessLayer
{
    public class TestDataAccess
    {
        public static bool FindByTestID(int id, ref string notes, ref bool testResult,
        ref int testAppoinment, ref int createdByUserID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Tests WHERE TestID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null;
                    testResult = (bool)reader["TestResults"];
                    testAppoinment = (int)reader["TestAppointmentID"];
                    createdByUserID = (int)reader["CreatedByUserID"];
                    isFound = true;
                }

                reader.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());
            }
            finally { connection.Close(); }

            return isFound;
        }


        public static bool FindByTestAppointmentID(ref int id, ref string notes, ref bool testResult,
            int testAppoinment, ref int createdByUserID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Tests WHERE TestAppointmentID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", testAppoinment);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["TestID"];
                    notes = reader["Notes"] != DBNull.Value ? (string)reader["Notes"] : null;
                    testResult = (bool)reader["TestResults"];
                    createdByUserID = (int)reader["CreatedByUserID"];
                    isFound = true;
                }

                reader.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());
            }
            finally { connection.Close(); }

            return isFound;
        }







        private static DataColumn _CreateDataColumn(string name, Type type)
        {
            DataColumn column = new DataColumn(name);
            column.DataType = type;

            return column;
        }





        public static int AddNewTest(bool testResults, string notes, int testAppointmentID, int createdByUserID)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  Tests (Notes, TestResults, TestAppointmentID, CreatedByUserID)

                            VALUES (@notes, @testResults, @testAppointmentID, @createdByUserID)

                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@testResults", testResults);

            if (string.IsNullOrEmpty(notes) || string.IsNullOrWhiteSpace(notes))
                command.Parameters.AddWithValue("@notes", DBNull.Value);
            else
                command.Parameters.AddWithValue("@notes", notes);

            command.Parameters.AddWithValue("@testAppointmentID", testAppointmentID);
            command.Parameters.AddWithValue("@createdByUserID", createdByUserID);

            try
            {
                connection.Open();
                object inserted = command.ExecuteScalar();

                if (inserted != null && int.TryParse(inserted.ToString(), out int rowID))
                {
                    if (rowID > insertedRow)
                    {
                        insertedRow = rowID;
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return insertedRow;
        }


        public static bool UpdateTest(int id, string notes, bool testResult)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Update Tests 
                            Set Notes = @notes,
                            TestResults = @result
                   
                            WHERE TestID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID", id);

            if (string.IsNullOrEmpty(notes) || string.IsNullOrWhiteSpace(notes))
                command.Parameters.AddWithValue("@desc", DBNull.Value);
            else
                command.Parameters.AddWithValue("@desc", notes);

            command.Parameters.AddWithValue("@age", testResult);


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

        public static bool DeleteTestByAppointmentID(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete Tests
                            WHERE TestAppointmentID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


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


        public static bool DeleteTestByLocalLicenseAppID(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete Tests
                            FROM Tests 
                            
                            INNER JOIN TestAppointments
                            ON TestAppointments.TestAppointmentID = Tests.TestAppointmentID

                            WHERE TestAppointments.LocalLicenseApplicationID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


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

            string query = @"Select TestID From Tests
                            WHERE TestID = @ID";

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

        public static bool IsPassedTestExist(int localLicenseApplicationID, int testTypeID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select 1 From TestTypes

                            INNER JOIN TestAppointments 

                            ON 

                            TestAppointments.TestAppointmentID = Tests.TestAppointmentID

                            WHERE 

                            LocalLicenseApplicationID = @localLicenseAppID AND

                            Tests.TestResults = 1

                            AND TestAppointments.TestTypeID = @testTypeID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("localLicenseAppID", localLicenseApplicationID);
            command.Parameters.AddWithValue("@testTypeID", testTypeID);


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


        public static bool IsFailedTestExist(int localLicenseApplicationID, int testTypeID)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select 1 From Tests

                            INNER JOIN TestAppointments 

                            ON 

                            TestAppointments.TestAppointmentID = Tests.TestAppointmentID

                            WHERE 

                            LocalLicenseApplicationID = @localLicenseAppID AND

                            Tests.TestResults = 0

                            AND TestAppointments.TestTypeID = @testTypeID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("localLicenseAppID", localLicenseApplicationID);
            command.Parameters.AddWithValue("@testTypeID", testTypeID);


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

    }
}
