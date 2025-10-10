using Microsoft.SqlServer.Server;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlTypes;
using DTO_Layer;


namespace DataAccessLayer
{
    public class PeopleDataAccess
    {


        private static DataTable dataTable;
        private static DataColumn _GenerateDataColumn(string name, Type type)
        {
            return new DataColumn(name, type);
        }



        private static void _GenerateColumnsDataTable(string name, Type type)
        {
            dataTable.Columns.Add(_GenerateDataColumn(name, type));
        }


        public static bool FindPeople(int id, ref string nationalNumber, ref string firstName, ref string secondName,
            ref string thirdName, ref string lastName, ref string gender, ref string address,
            ref string phone, ref string email, ref int nationalID, ref string country, ref int municipalityID,
            ref string municipalityName, ref string ImagePath, ref DateTime dateOfBirth)
        {

            bool found = false;

            SqlConnection conn = new SqlConnection(DataAccessSettings.connectionString);

            string query = "Select * from PeopleDetails where ID = @Id";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@id", id);

            try
            {
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {

                    nationalNumber = (string)reader["NationalNumber"];
                    firstName = (string)reader["FirstName"];
                    secondName = (string)reader["SecondName"];
                    thirdName = (string)reader["ThirdName"];
                    lastName = (string)reader["LastName"];

                    if ((bool)reader["Gender"])
                        gender = "Male";
                    else
                        gender = "Female";

                    address = (string)reader["Address"];

                    if (reader["PhoneNumber"] != DBNull.Value)
                        phone = (string)reader["PhoneNumber"];
                    else
                        phone = "";



                    if (reader["EmailAddress"] != DBNull.Value)
                        email = (string)reader["EmailAddress"];
                    else
                        email = "";

                    nationalID = (int)reader["NationalID"];
                    country = (string)reader["CountryName"];


                    municipalityID = (int)reader["MunicipalityID"];
                    municipalityName = (string)reader["MunicipalityName"];

                    dateOfBirth = (DateTime)reader["DateOfBirth"];

                    if (reader["ImagePath"] != DBNull.Value)
                        ImagePath = (string)reader["ImagePath"];
                    else
                        ImagePath = "";

                    found = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                found = false;
            }
            finally
            {
                conn.Close();
            }
            return found;
        }


        public static  bool FindPeopleByNationalNumber(ref int id, string nationalNumber, ref string firstName, ref string secondName,
         ref string thirdName, ref string lastName, ref string gender, ref string address,
         ref string phone, ref string email, ref int nationalID, ref string country, ref int municipalityID, 
         ref string municipalityName, ref string ImagePath, ref DateTime dateOfBirth)
        {

            bool found = false;

            SqlConnection conn = new SqlConnection(DataAccessSettings.connectionString);

            string query = "Select * from PeopleDetails where NationalNumber = @nationalNumber";

            SqlCommand cmd = new SqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@nationalNumber", nationalNumber);

            try
            {
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {

                    id = (int)reader["ID"];
                    firstName = (string)reader["FirstName"];
                    secondName = (string)reader["SecondName"];
                    thirdName = (string)reader["ThirdName"];
                    lastName = (string)reader["LastName"];

                    if ((bool)reader["Gender"])
                        gender = "ذكر";
                    else
                        gender = "انثى";

                    address = (string)reader["Address"];

                    if (reader["PhoneNumber"] != DBNull.Value)
                        phone = (string)reader["PhoneNumber"];
                    else
                        phone = "";



                    if (reader["EmailAddress"] != DBNull.Value)
                        email = (string)reader["EmailAddress"];
                    else
                        email = "";

                    nationalID = (int)reader["NationalID"];
                    country = (string)reader["CountryName"];


                    municipalityID = (int)reader["MunicipalityID"];
                    municipalityName = (string)reader["MunicipalityName"];

                    dateOfBirth = (DateTime)reader["DateOfBirth"];

                    if (reader["ImagePath"] != DBNull.Value)
                        ImagePath = (string)reader["ImagePath"];
                    else
                        ImagePath = "";

                    found = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                found = false;
            }
            finally
            {
                conn.Close();
            }
            return found;
        }

        public static DataTable GetAllPeople()
        {
            SqlConnection conn = new SqlConnection(DataAccessSettings.connectionString);

            string query = "Select * from PeopleDetails;";

            SqlCommand cmd = new SqlCommand(query, conn);

            dataTable = new DataTable();

            _GenerateColumnsDataTable("ID", typeof(int));
            _GenerateColumnsDataTable("NationalNumber", typeof(string));
            _GenerateColumnsDataTable("FirstName", typeof(string));
            _GenerateColumnsDataTable("SecondName", typeof(string));
            _GenerateColumnsDataTable("ThirdName", typeof(string));
            _GenerateColumnsDataTable("LastName", typeof(string));
            _GenerateColumnsDataTable("Gender", typeof(string));
            _GenerateColumnsDataTable("Address", typeof(string));
            _GenerateColumnsDataTable("PhoneNumber", typeof(string));
            _GenerateColumnsDataTable("EmailAddress", typeof(string));
            _GenerateColumnsDataTable("NationalID", typeof(int));
            _GenerateColumnsDataTable("Country", typeof(string));

            _GenerateColumnsDataTable("MunicipalityID", typeof(int));
            _GenerateColumnsDataTable("MunicipalityName", typeof(string));
            _GenerateColumnsDataTable("ImagePath", typeof(string));



            List<string> genders = new List<string>();

            try
            {
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();



                DataRow row = null;


                //row = dataTable.NewRow();

                //dataTable.Rows.Add(row);

                while (reader.Read())
                {
                    row = dataTable.NewRow();

                    row["ID"] = (int)reader["ID"];
                    row["NationalNumber"] = (string)reader["NationalNumber"];
                    row["FirstName"] = (string)reader["FirstName"];
                    row["SecondName"] = (string)reader["SecondName"];
                    row["ThirdName"] = (string)reader["ThirdName"];
                    row["LastName"] = (string)reader["LastName"];

                    if (!(bool)reader["Gender"])
                        row["Gender"] = "Male";
                    else
                        row["Gender"] = "Female";

                    row["Address"] = (string)reader["Address"];

                    if (reader["PhoneNumber"] != DBNull.Value)
                        row["PhoneNumber"] = (string)reader["PhoneNumber"];
                    else
                        row["PhoneNumber"] = "";



                    if (reader["EmailAddress"] != DBNull.Value)
                        row["EmailAddress"] = (string)reader["EmailAddress"];
                    else
                        row["EmailAddress"] = "";

                    row["NationalID"] = (int)reader["NationalID"];
                    row["Country"] = (string)reader["CountryName"];

                    row["municipalityID"] = (int)reader["MunicipalityID"];
                    row["municipalityName"] = (string)reader["MunicipalityName"];

                    if (reader["ImagePath"] != DBNull.Value)
                        row["ImagePath"] = (string)reader["ImagePath"];
                    else
                        row["ImagePath"] = "";


                    dataTable.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally { conn.Close(); }

            return dataTable;
        }


        public static  List<PersonDTO> GetAllPeopleDto()
        {
            SqlConnection conn = new SqlConnection(DataAccessSettings.connectionString);

            string query = "Select * from PeopleDetails;";

            SqlCommand cmd = new SqlCommand(query, conn);

            dataTable = new DataTable();




            List<string> genders = new List<string>();

            List<PersonDTO> people;

            try
            {
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();



               people = new List<PersonDTO>();  


                //row = dataTable.NewRow();

                //dataTable.Rows.Add(row);

                while (reader.Read())
                {

                    PersonDTO person = new PersonDTO();

                    person.ID = (int)reader["ID"];
                    person.NationalNumber = (string)reader["NationalNumber"];
                    person.FirstName = (string)reader["FirstName"];
                    person.SecondName = (string)reader["SecondName"];
                    person.ThirdName = (string)reader["ThirdName"];
                    person.LastName = (string)reader["LastName"];

                    if (!(bool)reader["Gender"])
                        person.Gender = "Male";
                    else
                        person.Gender = "Female";

                    person.Address = (string)reader["Address"];

                    if (reader["PhoneNumber"] != DBNull.Value)
                        person.PhoneNumber = (string)reader["PhoneNumber"];
                    else
                        person.PhoneNumber = "";



                    if (reader["EmailAddress"] != DBNull.Value)
                        person.email = (string)reader["EmailAddress"];
                    else
                        person.email = "";

                    person.NationalID = (int)reader["NationalID"];
                    person.Country = (string)reader["CountryName"];

                    if (reader["ImagePath"] != DBNull.Value)
                        person.ImagePath = (string)reader["ImagePath"];
                    else
                        person.ImagePath = "";


                    people.Add(person);
                }

                if (people.Count <= 0)
                    people = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                people = null;
            }
            finally { conn.Close(); }

            return people;
        }
    }
}
