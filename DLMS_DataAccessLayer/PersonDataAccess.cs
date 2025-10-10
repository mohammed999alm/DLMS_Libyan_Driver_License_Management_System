
using DLMS_DataAccessLayer.TransactionUnits;
using GlobalUtility;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;



namespace DLMS_DataAccessLayer
{
    public class PersonDataAccess
    {
        public static bool FindByPersonID(int id, ref string nationalNumber, ref string firstName, ref string secondName,
                    ref string thirdName, ref string lastName, ref DateTime dateOfBirth, ref string gender, ref string address,
                    ref int nationalityID, ref int municipalityID, ref string imagePath)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM People WHERE PersonID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    nationalNumber = (string)reader["NationalNumber"];
                    firstName = (string)reader["FirstName"];
                    secondName = (string)reader["SecondName"];
                    thirdName = (string)reader["ThirdName"];
                    lastName = (string)reader["LastName"];
                    dateOfBirth = (DateTime)reader["DateOfBirth"];

                    gender = (!(bool)reader["Gender"]) ? "أنثى" : "ذكر";

                    address = (string)reader["Address"];
                    nationalityID = (int)reader["NationalityID"];
                    municipalityID = (int)reader["MunicipalityID"];
                    imagePath = reader["ImagePath"] != DBNull.Value ? (string)reader["ImagePath"] : null;

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


        public static bool FindByNationalNumber(ref int id, string nationalNumber, ref string firstName, ref string secondName,
                   ref string thirdName, ref string lastName, ref DateTime dateOfBirth, ref string gender, ref string address,
                   ref int nationalityID, ref int municipalityID, ref string imagePath)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM People WHERE NationalNumber = @nationalNumber";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["PersonID"];
                    firstName = (string)reader["FirstName"];
                    secondName = (string)reader["SecondName"];
                    thirdName = (string)reader["ThirdName"];
                    lastName = (string)reader["LastName"];
                    dateOfBirth = (DateTime)reader["DateOfBirth"];

                    gender = (!(bool)reader["Gender"]) ? "أنثى" : "ذكر";

                    address = (string)reader["Address"];
                    nationalityID = (int)reader["NationalityID"];
                    municipalityID = (int)reader["MunicipalityID"];
                    imagePath = reader["ImagePath"] != DBNull.Value ? (string)reader["ImagePath"] : null;

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



        public static DataTable GetPeopleList()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM PeopleView";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("الرقم الوطني", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("الإسم الأول", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("اللقب", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("تاريخ الميلاد", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("الجنس", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("العنوان", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("البلد", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("البلدية", typeof(string)));


                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["PersonID"];
                    row["الرقم الوطني"] = (string)reader["NationalNumber"];
                    row["الإسم الأول"] = (string)reader["firstName"];
                    row["اللقب"] = (string)reader["lastName"];
                    row["تاريخ الميلاد"] = (DateTime)reader["DateOfBirth"];

                    row["الجنس"] = (!(bool)reader["Gender"]) ? "أنثى" : "ذكر";
                    row["العنوان"] = (string)reader["Address"];
                    row["البلد"] = (string)reader["CountryName"];
                    row["البلدية"] = (string)reader["MunicipalityName"];

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




        public static int AddNewPerson(string nationalNumber, string firstName, string secondName, string thirdName,
            string lastName, DateTime dateOfBirth, string gender, string address,
            int nationalityID, int municipalityID, string imagePath)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"INSERT INTO  People (nationalNumber, firstName, secondName, thirdName, lastName, dateOfBirth,
                            gender, address, nationalityID, municipalityID, imagePath)

                            VALUES (@nationalNumber, @firstName, @secondName, @thirdName, @lastName, @dateOfBirth,
                            @gender, @address, @nationalityID, @municipalityID, @imagePath)

                            SELECT SCOPE_IDENTITY()";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@secondName", secondName);
            command.Parameters.AddWithValue("@thirdName", thirdName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@dateOfBirth", dateOfBirth);

            if (gender == "ذكر")
            {
                bool genderBit = true;

                command.Parameters.AddWithValue("@gender", genderBit);
            }
            else
            {
                bool genderBit = false;

                command.Parameters.AddWithValue("@gender", genderBit);
            }

            command.Parameters.AddWithValue("@address", address);
            command.Parameters.AddWithValue("@nationalityID", nationalityID);
            command.Parameters.AddWithValue("@municipalityID", municipalityID);

            if (imagePath != null)
                command.Parameters.AddWithValue("@imagePath", imagePath);
            else
                command.Parameters.AddWithValue("@imagePath", DBNull.Value);



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
                //LoggerUtil.SetTheLogMessage(ex, $"Error Occured while adding new user {firstName} {lastName}");
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return insertedRow;
        }


        //private static void LogError(Exception ex)
        //{
        //    //string filePath = "errorLog.txt"; // Path for the log file
        //    string errorMessage = $"{DateTime.Now}: {ex.Message}\n{ex.StackTrace}\n";

        //    try
        //    {
        //        // Append the error to the file (creates the file if it doesn't exist)
        //        //File.AppendAllText(ResourcePath.LogErrPath, errorMessage);
        //    }
        //    catch (IOException ioEx)
        //    {
        //        Debug.WriteLine($"Failed to write to log file: {ioEx.Message}");
        //    }
        //}


        public static bool UpdatePerson(int id, string nationalNumber, string firstName, string secondName, string thirdName,
            string lastName, DateTime dateOfBirth, string gender, string address,
            int nationalityID, int municipalityID, string imagePath)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Update People 
                            Set nationalNumber = @nationalNumber,
                            firstName = @firstName,
                            secondName = @secondName, 
                            thirdName = @thirdName,
                            lastName = @lastName,
                            dateOfBirth = @dateOfBirth,
                            gender = @gender,
                            address = @address,
                            nationalityID = @nationalityID, 
                            municipalityID = @municipalityID,
                            imagePath = @imagePath
                            
                            WHERE PersonID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);
            command.Parameters.AddWithValue("@firstName", firstName);
            command.Parameters.AddWithValue("@secondName", secondName);
            command.Parameters.AddWithValue("@thirdName", thirdName);
            command.Parameters.AddWithValue("@lastName", lastName);
            command.Parameters.AddWithValue("@dateOfBirth", dateOfBirth);

            if (gender == "ذكر")
            {
                bool genderBit = true;

                command.Parameters.AddWithValue("@gender", genderBit);
            }
            else
            {
                bool genderBit = false;

                command.Parameters.AddWithValue("@gender", genderBit);
            }

            command.Parameters.AddWithValue("@address", address);
            command.Parameters.AddWithValue("@nationalityID", nationalityID);
            command.Parameters.AddWithValue("@municipalityID", municipalityID);

            if (imagePath != null)
                command.Parameters.AddWithValue("@imagePath", imagePath);
            else
                command.Parameters.AddWithValue("@imagePath", DBNull.Value);



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


        public static bool DeletePerson(int id)
        {
            return PersonTransactions.DeletePerson(id);
        }


        //public static bool DeletePerson(int id)
        //{
        //    int rowAffected = -1;

        //    SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        //    string query = @"Delete People
        //                    WHERE PersonID = @ID";

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

        public static bool IsExist(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select PersonID From People
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

        public static bool IsExist(string nationalNumber)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select PersonID From People
                            WHERE NationalNumber = @nationalNumber";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@nationalNumber", nationalNumber);


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
