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
    public class ApplicationTypeDataAccess
    {

        public static bool FindByApplicationTypeID(int id, ref string applicationType, ref decimal ApplicationFee)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM ApplicationTypes WHERE ApplicationTypeID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    ApplicationFee = (decimal)reader["ApplicationFee"];
                    applicationType = (string)reader["ApplicationTypeTitle"];

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


        public static bool FindByApplicationType(ref int id, string ApplicationType, ref decimal ApplicationFee)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM ApplicationTypes WHERE ApplicationTypeTitle = @ApplicationType";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ApplicationType", ApplicationType);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["ApplicationTypeID"];
                    ApplicationFee = (decimal)reader["ApplicationFee"];


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

            string query = @"SELECT * FROM ApplicationTypes 
                            WHERE ApplicationTypeTitle <> 'migrate'
                            ";

            SqlCommand command = new SqlCommand(query, connection);



            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("نوع الطلب", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("رسوم الطلب", typeof(double)));


                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["ApplicationTypeID"];
                    row["رسوم الطلب"] = (decimal)reader["ApplicationFee"];
                    row["نوع الطلب"] = (string)reader["ApplicationTypeTitle"];


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


        //public static DataTable GetAllApplicationTypes()
        //{
        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = "SELECT * FROM ApplicationTypes";

        //    DataTable table = null;

        //    SqlCommand command = new SqlCommand(query, connection);


        //    try
        //    {
        //        connection.Open();

        //        SqlDataReader reader = command.ExecuteReader();

        //        table = new DataTable();

        //        if (reader.HasRows)
        //        {
        //            table.Load(reader);
        //        }

        //        reader.Close();
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine($"{ex.Message}  {ex}");
        //    }
        //    finally { connection.Close(); }

        //    return table;
        //}


        public static int AddApplicationType(string applicationType, decimal applicationFee)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  ApplicationTypes (ApplicationTypeTitle, ApplicationFee)
                            VALUES (@ApplicationType, @ApplicationFee)
                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ApplicationType", applicationType);
            command.Parameters.AddWithValue("@ApplicationFee", applicationFee);


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


        public static bool UpdateApplicationType(int id, string ApplicationType, decimal applicationFee)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"UPDATE ApplicationTypes
                            SET ApplicationFee = @applicationFee,
                            ApplicationTypeTitle = @ApplicationType
                      
                            WHERE ApplicationTypeID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@applicationFee", applicationFee);
            command.Parameters.AddWithValue("@ApplicationType", ApplicationType);


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

        public static bool DeleteApplicationType(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete ApplicationTypes
                            WHERE ApplicationTypeID = @ID";

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

        public static bool DeleteApplicationType(string ApplicationType)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete ApplicationTypes
                            WHERE ApplicationType = @ApplicationType";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ApplicationType", ApplicationType);


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

            string query = @"Select ApplicationTypeID From ApplicationTypes
                            WHERE ApplicationTypeID = @ID";

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




        public static bool IsExist(string ApplicationType)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select ApplicationTypeID From ApplicationTypes
                            WHERE ApplicationTypeTitle = @ApplicationType";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ApplicationType", ApplicationType);


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
