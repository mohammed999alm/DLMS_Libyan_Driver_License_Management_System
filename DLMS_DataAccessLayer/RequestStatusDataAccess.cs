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
    public class RequestStatusDataAccess
    {

        public static bool FindByRequestStatusID(int id, ref string RequestStatusTag)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM RequestStatus WHERE RequestStatusID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    RequestStatusTag = (string)reader["StatusType"];

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


        public static bool FindByRequestStatusTag(ref int id, string RequestStatusTag)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM RequestStatus WHERE  StatusType = @name";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@name", RequestStatusTag);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (byte)reader["RequestStatusID"];

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

            string query = "SELECT * FROM RequestStatus";

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
    }
}
