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
    public class ApplicationStatusTypeDataAccess
    {

        public static bool FindByApplicationStatusTypeID(int id, ref string ApplicationStatusTypeTag)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM ApplicationStatusTypes WHERE StatusID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    ApplicationStatusTypeTag = (string)reader["StatusTypeTitle"];

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


        public static bool FindByApplicationStatusTypeTag(ref int id, string ApplicationStatusTypeTag)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM ApplicationStatusTypes WHERE StatusTypeTitle = @name";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", ApplicationStatusTypeTag);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["StatusID"];

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







        public static DataTable GetAll()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM ApplicationStatusTypes ORDER BY StatusID";

            SqlCommand command = new SqlCommand(query, connection);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();



                if (reader.HasRows)
                {
                    dt = new DataTable();

                    dt.Load(reader);
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


        public static List<string>? GetAllStatus()
        {
            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT StatusTypeTitle FROM ApplicationStatusTypes ORDER BY StatusID";

            SqlCommand command = new SqlCommand(query, connection);

            List<string>? list = new List<string>(); 

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();



                while (reader.Read())
                {
                    list.Add((string)reader["StatusTypeTitle"]);
                }

                reader.Close();
            }

            catch (Exception ex)
            {
                Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());

                list = null;
            }
            finally { connection.Close(); }

            return list;
        }
    }
}
