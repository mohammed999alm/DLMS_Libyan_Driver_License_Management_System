using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GlobalUtility;
using System.Transactions;
using DLMS_DataAccessLayer.TransactionUnits;

namespace DLMS_DataAccessLayer
{
    public class LocalLicenseApplicationDataAccess
    {
        public static bool FindByLocalLicenseApplicationID(int id, ref int applicationID, ref int licenseClassID, ref byte passedTests)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM LocalLicenseAppView WHERE LocalLicenseApplicationID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    applicationID = (int)reader["ApplicationID"];
                    licenseClassID = (int)reader["LicenseClassID"];
                    //passedTests = byte.TryParse(reader["PassedTests"].ToString(), out passedTests) ? passedTests : (byte)0;

                    int temp = (int)reader["PassedTests"];

                    Debug.WriteLine(reader[passedTests].ToString());
                    Debug.WriteLine(temp);


                    passedTests = Convert.ToByte(temp);



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


        public static bool FindByApplicationID(ref int id, int applicationID, ref int licenseClassID, ref byte passedTests)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM LocalLicenseAppView WHERE ApplicationID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", applicationID);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["LocalLicenseApplicationID"];
                    licenseClassID = (int)reader["LicenseClassID"];
                    int temp = (int)reader["PassedTests"];

                    passedTests = Convert.ToByte(temp);


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



        public static DataTable GetAll()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM LocalLicenseApplicationsView ";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("معرف الشخص", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("إسم المتقدم", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("نوع الطلب", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("رسوم الطلب", typeof(decimal)));
                dt.Columns.Add(_CreateDataColumn("حالة الطلب", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("فئة الرخصة", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("اجتاز الإختبارات", typeof(int)));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["LocalLicenseApplicationID"];
                    row["معرف الشخص"] = (int)reader["ApplicantPersonID"];
                    row["إسم المتقدم"] = (string)reader["FullName"];
                    row["نوع الطلب"] = (string)reader["ApplicationTypeTitle"];
                    row["رسوم الطلب"] = (decimal)reader["ApplicationFee"];
                    row["حالة الطلب"] = (string)reader["StatusTypeTitle"];
                    row["فئة الرخصة"] = (string)reader["ClassName"];
                    row["اجتاز الإختبارات"] = (int)reader["PassedTests"];

                    dt.Rows.Add(row);
                }


                dt = (dt.Rows.Count > 0) ? dt : null;

                reader.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());
                dt = null;
            }
            finally { connection.Close(); }

            return dt;
        }


        public static int GetTestAppointmentAttemptsBasedOnLocalLicenseAppIdAndTestType(int localLicenseAppID, string testType) 
        {
            int numberOfAttempts = 0;

            string query = @"SELECT Attempts FROM AppointmentTestAttempts
                            WHERE LocalLicenseApplicationID = @localLicenseAppID AND TestTypeTitle = @testType";

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            using (SqlCommand command = new SqlCommand(query, connection)) 
            {
                try
                {
                    connection.Open();

                    command.Parameters.AddWithValue("@localLicenseAppID", localLicenseAppID);
                    command.Parameters.AddWithValue("@testType", testType);

                    object result = command.ExecuteScalar();

                    numberOfAttempts = int.TryParse(result?.ToString(), out numberOfAttempts) ? numberOfAttempts : 0;
                }
                catch (Exception ex) 
                {
                    LoggerUtil.LogError(ex, nameof(LocalLicenseApplicationDataAccess), nameof(GetTestAppointmentAttemptsBasedOnLocalLicenseAppIdAndTestType),
                        new Dictionary<string, object> { { "localLicenseAppID", localLicenseAppID }, { "testType" , testType} });

                    numberOfAttempts = -1;
                }
            }

            return numberOfAttempts;    
        }

        public static int AddLocalLicenseApplication(int applicationID, int licenseClassID)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  LocalLicenseApplications (ApplicationID, LicenseClassID)
                            VALUES (@appID, @licenseClassID)
                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@appID", applicationID);
            command.Parameters.AddWithValue("@licenseClassID", licenseClassID);



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


     
        public static bool DeleteLocalLicenseApplication(int localAppID)
        {
            if (ApplicationDataAccess.CouldBeDeletedByAppID(GetAppID_LocalAppID(localAppID)))
                return LocalLicenseAppTransactions.DeleteLocalLicenseApplication(localAppID);

            return false;
        }

        //public static bool DeleteLocalLicenseApplicationByApplicationID(int appID)
        //{
        //    int rowAffected = -1;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Delete LocalLicenseApplications
        //                    WHERE ApplicationID = @appID";

        //    SqlCommand command = new SqlCommand(query, connection);


        //    command.Parameters.AddWithValue("@appId", appID);


        //    try
        //    {
        //        connection.Open();

        //        rowAffected = command.ExecuteNonQuery();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"{ex.Message}  {ex}");
        //    }
        //    finally { connection.Close(); }


        //    return rowAffected > 0;
        //}




        public static bool IsExist(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select LocalLicenseApplicationID From LocalLicenseApplications
                            WHERE LocalLicenseApplicationID = @ID";

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


        public static int GetPassedTests(int id)
        {
            int passedTests = 0;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select  PassedTests From LocalLicenseAppView
                            WHERE LocalLicenseApplicationID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int result))
                {
                    passedTests = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return passedTests;
        }



        public static int GetAppID_LocalAppID(int id)
        {
            int appID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select  ApplicationID From LocalLicenseApplications
                            WHERE LocalLicenseApplicationID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


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


        public static int GetRequestIDBy_LocalAppID(int id)
        {
            int appID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT RequestID FROM LocalLicenseApplications lApp 
                             INNER JOIN Applications app ON app.ApplicationID = lApp.ApplicationID 
                             WHERE lApp.LocalLicenseApplicationID = @localAppID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@localAppID", id);


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




        public static bool IsExistByApplicationID(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select LocalLicenseApplicationID From LocalLicenseApplications
                            WHERE  applicationID = @ID";

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

    }
}
