using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;

namespace DLMS_DataAccessLayer
{
    public class PhoneDataAccess
    {

        public static bool FindByPhoneID(int id, ref string phone, ref int personId)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Phones WHERE PhoneID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    personId = (int)reader["OwnerPersonID"];
                    phone = (string)reader["Phone"];

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


        public static bool FindByPhone(ref int id, string phone, ref int personId)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Phones WHERE Phone = @phone";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@phone", phone);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["PhoneID"];
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



        public static DataTable FindPhonesByPersonID(int personID)
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM Phones 
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
                dt.Columns.Add(_CreateDataColumn("رقم الهاتف", typeof(string)));


                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["PhoneID"];
                    row["معرف الشخص"] = (int)reader["OwnerPersonID"];
                    row["رقم الهاتف"] = (string)reader["Phone"];


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


        public static DataTable GetAllPhones()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Phones";

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


        public static int AddPhone(int personID, string phone)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  Phones (OwnerPersonID, Phone)
                            VALUES (@personID, @phone)
                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@personID", personID);
            command.Parameters.AddWithValue("@phone", phone);


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


        public static bool UpdatePhone(int id, int personID, string phone)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"UPDATE Phones
                            SET OwnerPersonID = @PersonID,
                            Phone = @Phone
                      
                            WHERE PhoneID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@personID", personID);
            command.Parameters.AddWithValue("@phone", phone);


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

        public static bool DeletePhone(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete Phones
                            WHERE PhoneID = @ID";

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

        public static bool DeletePhone(string phone)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete Phones
                            WHERE Phone = @phone";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@phone", phone);


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


        public static bool DeletePhoneByPersonID(int personID)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete Phones
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


        public static bool IsExist(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select PhoneID From Phones
                            WHERE PhoneID = @ID";

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

            string query = @"Select PhoneID From Phones
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
        public static bool IsExist(string phone)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select PhoneID From Phones
                            WHERE Phone = @phone";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@phone", phone);


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
