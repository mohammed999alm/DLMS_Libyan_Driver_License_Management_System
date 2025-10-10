using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GlobalUtility;

namespace DLMS_DataAccessLayer
{
    public class TestTypeDataAccess
    {


        public static bool FindByTestTypeID(int id, ref string name, ref string description,
           ref decimal fees)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM TestTypes WHERE TestTypeID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    name = (string)reader["TestTypeTitle"];
                    description = reader["TestTypeDescription"]?.ToString();
                    fees = (decimal)reader["TestTypeFees"];
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


        public static bool FindByTestTypeName(ref int id, string name, ref string description,
            ref decimal fees)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM TestTypes WHERE TestTypeTitle = @name";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", name);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["TestTypeID"];
                    description = (string)reader["TestTypeDescription"];
                    fees = (decimal)reader["TestTypeFees"];
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



        public static List<string>? GetAllTypes()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT  TestTypeTitle FROM TestTypes ORDER BY  TestTypeID ASC";

            SqlCommand command = new SqlCommand(query, connection);

            List<string>? types = new List<string>();    

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {      
                   types.Add((string)reader["TestTypeTitle"]);
                }

                reader.Close();
            }

            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(TestTypeDataAccess), nameof(GetAllTypes));
                types = null;
            }
            finally { connection.Close(); }

            return types;
        }






        private static DataColumn _CreateDataColumn(string name, Type type)
        {
            DataColumn column = new DataColumn(name);
            column.DataType = type;

            return column;
        }



        public static DataTable GetAllTestTypes()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM TestTypes";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("الإختبار", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الوصف", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الرسوم المالية للإختبار", typeof(decimal)));





                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["TestTypeID"];
                    row["الإختبار"] = (string)reader["TestTypeTitle"];
                    row["الوصف"] = (string)reader["TestTypeDescription"];
                    row["الرسوم المالية للإختبار"] = (decimal)reader["TestTypeFees"];



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



        public static int AddNewTestType(string name, string description, decimal fees)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  TestTypes (TestTypeTitle, TestTypeDescription, TestTypeFees)

                            VALUES (@name, @desc, @fees)

                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", name);

            if (string.IsNullOrEmpty(description) || string.IsNullOrWhiteSpace(description))
                command.Parameters.AddWithValue("@desc", DBNull.Value);
            else
                command.Parameters.AddWithValue("@description", description);
            //command.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(description) ? null : description);
            command.Parameters.AddWithValue("@fees", fees);


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


        public static bool UpdateTestType(int id, string desc, decimal fees)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Update TestTypes 
                            Set TestTypeDescription = @desc,
                            TestTypeFees = @fees
                            
                            WHERE TestTypeID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID", id);

            if (string.IsNullOrEmpty(desc) || string.IsNullOrWhiteSpace(desc))
                command.Parameters.AddWithValue("@desc", DBNull.Value);
            else
                command.Parameters.AddWithValue("@desc", desc);

            command.Parameters.AddWithValue("@fees", fees);


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

        public static bool DeleteTestType(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete TestTypes
                            WHERE TestTypeID = @ID";

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

            string query = @"Select TestTypeID From TestTypes
                            WHERE TestTypeID = @ID";

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

        public static bool IsExist(string TestTypename)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select TestTypeID From TestTypes
                            WHERE TestTypeTitle = @name";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@name", TestTypename);


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
