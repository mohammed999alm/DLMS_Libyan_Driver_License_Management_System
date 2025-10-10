
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;


namespace DLMS_DataAccessLayer
{
    public class EmailAddressDataAccess
    {
        public static bool FindByEmailAddressID(int id, ref string EmailAddress, ref int personId)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM EmailAddresses WHERE EmailAddressID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    personId = (int)reader["OwnerPersonID"];
                    EmailAddress = (string)reader["EmailAddress"];

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


        public static bool FindByEmailAddress(ref int id, string EmailAddress, ref int personId)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM EmailAddresses WHERE EmailAddress = @EmailAddress";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@EmailAddress", EmailAddress);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["EmailAddressID"];
                    personId = (int)reader["OwnerPersonID"];


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



        public static DataTable FindEmailAddresssByPersonID(int personID)
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM EmailAddresses 
                            WHERE OwnerPersonID = @personID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@personID", personID);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("معرف الشخص", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("البريد الإلكتروني", typeof(string)));


                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["EmailAddressID"];
                    row["معرف الشخص"] = (int)reader["OwnerPersonID"];
                    row["البريد الإلكتروني"] = (string)reader["EmailAddress"];


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


        public static DataTable GetAllEmailAddresss()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM EmailAddresses";

            DataTable table = null;

            SqlCommand command = new SqlCommand(query, connection);


            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                table = new DataTable();

                if (reader.HasRows)
                {
                    table.Load(reader);
                }

                reader.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }

            return table;
        }


        public static int AddEmailAddress(int personID, string EmailAddress)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  EmailAddresses (OwnerPersonID, EmailAddress)
                            VALUES (@personID, @EmailAddress)
                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@personID", personID);
            command.Parameters.AddWithValue("@EmailAddress", EmailAddress);


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


        public static bool UpdateEmailAddress(int id, int personID, string EmailAddress)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"UPDATE EmailAddresses
                            SET OwnerPersonID = @PersonID,
                            EmailAddress = @EmailAddress
                      
                            WHERE EmailAddressID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@personID", personID);
            command.Parameters.AddWithValue("@EmailAddress", EmailAddress);


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

        public static bool DeleteEmailAddress(string email)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete EmailAddresses
                            WHERE EmailAddress = @email";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@email", email);


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


        public static bool DeleteEmailAddress(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete EmailAddresses
                            WHERE EmailAddressID = @ID";

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


        public static bool DeleteEmailAddressByPersonID(int personID)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete EmailAddresses
                            WHERE OwnerPersonID = @personID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@personID", personID);


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



        public static bool IsExist(string email)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select EmailAddressID From EmailAddresses
                            WHERE EmailAddress = @email";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@email", email);


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
        public static bool IsExist(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select EmailAddressID From EmailAddresses
                            WHERE EmailAddressID = @ID";

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


        public static bool IsExistByPersonID(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select EmailAddressID From EmailAddresses
                            WHERE OwnerPersonID = @ID";

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
