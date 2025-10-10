using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Data.SqlTypes;
using System.Security.Cryptography;
using NLog;
using GlobalUtility;

namespace DLMS_DataAccessLayer
{
    public class LicenseClassDataAccess
    {
        public static bool FindByLicenseClassID(int id, ref string name, ref string description,
          ref byte age, ref byte validatyRange, ref decimal fees)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM LicenseClasses WHERE LicenseClassID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    name = (string)reader["Classname"];
                    description = (string)reader["ClassDescription"];
                    age = (byte)reader["MinimumAllowedAge"];
                    validatyRange = (byte)reader["ValidatyLength"];
                    fees = (decimal)reader["ClassFees"];
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


        public static bool FindByLicenseClassName(ref int id, string name, ref string description,
           ref byte age, ref byte validatyRange, ref decimal fees)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM LicenseClasses WHERE ClassName = @name";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", name);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["LicenseClassID"];
                    description = (string)reader["ClassDescription"];
                    age = (byte)reader["MinimumAllowedAge"];
                    validatyRange = (byte)reader["ValidatyLength"];
                    fees = (decimal)reader["ClassFees"];
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



        public static DataTable GetAllLicenseClasses()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM LicenseClasses";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("إسم الفئة", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الوصف", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الحد الأدنى للعمر", typeof(byte)));
                dt.Columns.Add(_CreateDataColumn("مدة الصلاحية", typeof(byte)));
                dt.Columns.Add(_CreateDataColumn("الرسوم المالية للفئة", typeof(decimal)));





                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["LicenseClassID"];
                    row["إسم الفئة"] = (string)reader["Classname"];
                    row["الوصف"] = (string)reader["ClassDescription"];
                    row["مدة الصلاحية"] = (byte)reader["ValidatyLength"];
                    row["الحد الأدنى للعمر"] = (byte)reader["MinimumAllowedAge"];
                    row["الرسوم المالية للفئة"] = (decimal)reader["ClassFees"];



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


        public static List<string> GetAll() 
        {
            List<string> list = new List<string>();

            string query = "SELECT ClassName FROM LicenseClasses";

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                try
                {
                    list = new List<string>();

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {

                            list.Add((string)reader["ClassName"]);
                        }
                    }
                }
                catch (Exception ex) 
                {
                    LoggerUtil.LogError(ex, nameof(LicenseClassDataAccess), nameof(GetAll));
                    return null;
                }
            }

            return list;
        }


        public static List<DLMS_DTO.LicenseClassDto> GetAllWithDetails()
        {
            List<DLMS_DTO.LicenseClassDto> list = new List<DLMS_DTO.LicenseClassDto>();

            string query = "SELECT * FROM LicenseClasses";

            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            using (SqlCommand cmd = new SqlCommand(query, connection))
            {

                try
                {

                    connection.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {

                        while (reader.Read())
                        {
                            DLMS_DTO.LicenseClassDto licenseClass = new DLMS_DTO.LicenseClassDto();

                            licenseClass.ID = (int)reader["LicenseClassID"];
                            licenseClass.Name = (string)reader["Classname"];
                            licenseClass.Description = (string)reader["ClassDescription"];
                            licenseClass.Age = (byte)reader["MinimumAllowedAge"];
                            licenseClass.ValidatyLength = (byte)reader["ValidatyLength"];
                            licenseClass.Fees = (decimal)reader["ClassFees"];
                            
                            list.Add(licenseClass);
                        }
                    }
                }
                catch (Exception ex)
                {
                    LoggerUtil.LogError(ex, nameof(LicenseClassDataAccess), nameof(GetAllWithDetails));


                    return null;
                }
            }

            return list;
        }


        public static int AddNewLicenseClass(string name, string description, byte age, byte validatyLength, decimal fees)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  LicenseClasses (Classname, ClassDescription, MinimumAllowedAge, ValidatyLength, ClassFees)

                            VALUES (@name, @desc, @age, @validaty, @fees)

                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", name);

            if (string.IsNullOrEmpty(description) || string.IsNullOrWhiteSpace(description))
                command.Parameters.AddWithValue("@desc", DBNull.Value);
            else
                command.Parameters.AddWithValue("@description", description);
            //command.Parameters.AddWithValue("@desc", string.IsNullOrEmpty(description) ? null : description);
            command.Parameters.AddWithValue("@age", age);
            command.Parameters.AddWithValue("@validaty", validatyLength);
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


        public static bool UpdateLicenseClass(int id, string desc, byte age, byte validatyLength, decimal fees)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Update LicenseClasses 
                            Set ClassDescription = @desc,
                            ValidatyLength = @validaty,
                            MinimumAllowedAge = @age,
                            ClassFees = @fees
                            
                            WHERE LicenseClassID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID", id);

            if (string.IsNullOrEmpty(desc) || string.IsNullOrWhiteSpace(desc))
                command.Parameters.AddWithValue("@desc", DBNull.Value);
            else
                command.Parameters.AddWithValue("@desc", desc);

            command.Parameters.AddWithValue("@validaty", validatyLength);
            command.Parameters.AddWithValue("@age", age);
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

        public static bool DeleteLicenseClass(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete LicenseClasses
                            WHERE LicenseClassID = @ID";

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

            string query = @"Select LicenseClassID From LicenseClasses
                            WHERE LicenseClassID = @ID";

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

        public static bool IsExist(string LicenseClassname)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select LicenseClassID From LicenseClasses
                            WHERE ClassName = @name";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@name", LicenseClassname);


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
