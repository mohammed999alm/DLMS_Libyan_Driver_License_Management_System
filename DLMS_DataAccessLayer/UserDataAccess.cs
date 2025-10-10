using GlobalUtility;
using IronPdf.Logging;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Runtime.CompilerServices;



[assembly: InternalsVisibleTo("DLMS_BusinessLogicLayer")]
namespace DLMS_DataAccessLayer
{

    internal class UserDataAccess
    {

        public static bool FindByUserID(int id, ref string userName, ref string password,
            ref int userRoldID, ref int personID, ref string activateStatus)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Users WHERE UserID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    userName = (string)reader["Username"];
                    password = (string)reader["Password"];
                    userRoldID = (int)reader["userRole"];
                    personID = (int)reader["PersonID"];
                    activateStatus = ((bool)reader["IsActive"]) ? "نشط" : "غير نشط";
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


        internal static bool FindByPerson(ref int id, ref string userName, ref string password,
          ref int userRoldID,  int personID, ref string activateStatus)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Users WHERE PersonID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", personID);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    userName = (string)reader["Username"];
                    password = (string)reader["Password"];
                    userRoldID = (int)reader["userRole"];
                    id = (int)reader["userID"];
                    activateStatus = ((bool)reader["IsActive"]) ? "نشط" : "غير نشط";
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

        public static bool FindByUsername(ref int id, string userName, ref string password,
            ref int userRoldID, ref int personID, ref string activateStatus, ref int failedAttempts)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Users WHERE username = @username";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@username", userName);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["UserID"];
                    password = (string)reader["Password"];
                    userRoldID = (int)reader["userRole"];
                    personID = (int)reader["PersonID"];
                    activateStatus = ((bool)reader["IsActive"]) ? "نشط" : "غير نشط";
                    failedAttempts = (int)reader["FailedAttemps"];
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


        public static bool FindByUsernameAndPassword(ref int id, string userName, string password,
           ref int userRoldID, ref int personID, ref string userStatus)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            //Really intersting how that leads to sql injection :) just type username'-- and it will igonre password field
            //string query = $"SELECT * FROM Users WHERE username = '{userName}' AND password = @password";

            //So that's why we use scalar variable then binding the parameter to avoid the sql injection and prevents it
            string query = "SELECT * FROM Users WHERE username = @username AND password = @password";


            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@username", userName);
            command.Parameters.AddWithValue("Password", password);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["UserID"];
                    userRoldID = (int)reader["userRole"];
                    personID = (int)reader["PersonID"];
                    userStatus = ((bool)reader["IsActive"]) ? "نشط" : "غير نشط";
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



        public static DataTable GetAllUsers()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM UsersView where RoleTag <> 'مدير النظام'";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("إسم المستخدم", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الدور الوظيفي", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الإسم", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("حالة المستخدم", typeof(string)));



                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["UserID"];
                    row["إسم المستخدم"] = (string)reader["username"];
                    row["الدور الوظيفي"] = (string)reader["roleTag"];
                    row["الإسم"] = (string)reader["Fullname"];
                    row["حالة المستخدم"] = ((bool)reader["IsActive"]) ? "نشط" : "غير نشط";

                    dt.Rows.Add(row);
                }

                reader.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());
            }
            finally { connection.Close(); }

            return dt;
        }


        public static DataTable GetUsersWithNoHistory()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM UsersWithNoHistoryOfTranactions where RoleTag <> 'مدير النظام'";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("إسم المستخدم", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الدور الوظيفي", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الإسم", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("حالة المستخدم", typeof(string)));



                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["UserID"];
                    row["إسم المستخدم"] = (string)reader["username"];
                    row["الدور الوظيفي"] = (string)reader["roleTag"];
                    row["الإسم"] = (string)reader["Fullname"];
                    row["حالة المستخدم"] = ((bool)reader["IsActive"]) ? "نشط" : "غير نشط";

                    dt.Rows.Add(row);
                }

                reader.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());
            }
            finally { connection.Close(); }

            return dt;
        }




        public static int AddNewUser(string username, string password, int userRole, int personID, bool isActive)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  Users (username, password, userRole, personID, IsActive)

                            VALUES (@username, @password, @userRole, @personID, @IsActive)

                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@userRole", userRole);
            command.Parameters.AddWithValue("@personID", personID);
            command.Parameters.AddWithValue("@IsActive", isActive);


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
                LogError(ex);
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return insertedRow;
        }


        private static void LogError(Exception ex)
        {
            string filePath = "errorLog.txt"; // Path for the log file
            string errorMessage = $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n";

            try
            {
                // Append the error to the file (creates the file if it doesn't exist)
                File.AppendAllText(filePath, errorMessage);
            }
            catch (IOException ioEx)
            {
                Debug.WriteLine($"Failed to write to log file: {ioEx.Message}");
            }
        }


        public static bool UpdateUser(int id, string password, int userRole, bool isActive, int failAttempts)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Update Users 
                            Set password = @password,
                            userRole = @userRole,
                            isActive = @isActive,
                            FailedAttemps = @failAttempts
                            
                            WHERE UserID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@password", password);
            command.Parameters.AddWithValue("@userRole", userRole);
            command.Parameters.AddWithValue("@isActive", isActive);
            command.Parameters.AddWithValue("@failAttempts", failAttempts);



            try
            {
                connection.Open();

                rowAffected = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");

                LoggerUtil.LogError(ex, nameof(UserDataAccess), nameof(UpdateUser),
                    new Dictionary<string, object> { { "UserID", id }, { "Password", password },
                        { "UserRole", userRole}, { "IsActive", isActive}, { "FailAttempts", failAttempts} });


            }
            finally { connection.Close(); }


            return rowAffected > 0;
        }

        public static bool DeleteUser(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete Users
                            WHERE UserID = @ID";

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

            string query = @"Select UserID From Users
                            WHERE UserID = @ID";

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


        public static bool IsExistWithNoHistoryUser(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select UserID From UsersWithNoHistoryOfTranactions
                            WHERE UserID = @ID";

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

        public static bool IsExist(string username)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select UserID From Users
                            WHERE username = @username";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@username", username);


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


        public static bool IsActiveUserFoundByUserID(string username)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select 1 From Users
                            WHERE username = @username AND IsActive = 1";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@username", username);


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


        public static bool IsExistByPersonID(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select UserID From Users
                            WHERE PersonID = @ID";

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
