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
    public class DriverDataAccess
    {

        public static bool FindByDriverID(int id, ref int personID, ref int createdByUserID, ref DateTime creationDate)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Drivers WHERE DriverID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    personID = (int)reader["PersonID"];

                    createdByUserID = (int)reader["CreatedByUserID"];

                    creationDate = (DateTime)reader["CreationDate"];

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


        public static bool FindByPersonID(ref int id, int personID, ref int createdByUserID, ref DateTime creationDate)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM Drivers WHERE PersonID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", personID);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["DriverID"];

                    createdByUserID = (int)reader["CreatedByUserID"];

                    creationDate = (DateTime)reader["CreationDate"];

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

            string query = @"SELECT * FROM DriversView";

            SqlCommand command = new SqlCommand(query, connection);


            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("معرف الشخص", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("الرقم الوطني", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("إسم السائق", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("تاريخ التسجيل", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("عدد الرخص الفعالة", typeof(int)));




                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["DriverID"];
                    row["معرف الشخص"] = (int)reader["PersonID"];
                    row["الرقم الوطني"] = (string)reader["NationalNumber"];
                    row["إسم السائق"] = (string)reader["FullName"];
                    row["تاريخ التسجيل"] = (DateTime)reader["CreationDate"];
                    row["عدد الرخص الفعالة"] = (int)reader["NumberOfActiveLicenses"];

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



        public static int AddDriver(int personID, int createdByUserID, DateTime creationDate)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  Drivers (PersonID, CreatedByUserID, CreationDate)
                            VALUES (@personID, @userID, @creationDate)
                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@personID", personID);
            command.Parameters.AddWithValue("@userID", createdByUserID);
            command.Parameters.AddWithValue("@creationDate", creationDate);


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



        //public static bool DeleteDriver(int id)
        //{
        //    int rowAffected = -1;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Delete Drivers
        //                    WHERE DriverID = @ID";

        //    SqlCommand command = new SqlCommand(query, connection);


        //    command.Parameters.AddWithValue("@ID", id);


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

        //public static bool DeleteDriver(string Driver)
        //{
        //    int rowAffected = -1;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Delete Drivers
        //                    WHERE Driver = @Driver";

        //    SqlCommand command = new SqlCommand(query, connection);


        //    command.Parameters.AddWithValue("@Driver", Driver);


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


        //public static bool DeleteDriverByPersonID(int personID)
        //{
        //    int rowAffected = -1;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Delete Drivers
        //                    WHERE OwnerPersonID = @personID";

        //    SqlCommand command = new SqlCommand(query, connection);


        //    command.Parameters.AddWithValue("@personID", personID);


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

            string query = @"Select DriverID From Drivers
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



        public static bool IsExistByPersonID(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select DriverID From Drivers
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


        public static int GetDriverIDByPersonID(int id)
        {
            int rowID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select DriverID From Drivers
                            WHERE PersonID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int result))
                {
                    rowID = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return rowID;
        }


        public static int GetNumberOfActiveLicensesByDriverID(int id)
        {
            int numberOfActiveLicenses = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT NumberOfActiveLicenses FROM DriversView WHERE DriverID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int result))
                {
                    numberOfActiveLicenses = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return numberOfActiveLicenses;
        }

    }
}
