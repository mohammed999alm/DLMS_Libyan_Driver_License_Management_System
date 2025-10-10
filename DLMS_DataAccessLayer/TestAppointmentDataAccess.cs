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
    public class TestAppointmentDataAccess
    {

		private static bool _ArchiveExpiredAppointments()
		{
			int rowsAffected = -1;

			SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

			string query = @"UPDATE TestAppointments SET IsLocked = 1 
                            WHERE CAST(GETDATE() AS DATE) > CAST(AppointmentDate AS DATE)
                            ";

			SqlCommand command = new SqlCommand(query, connection);

			try
			{
				connection.Open();

				rowsAffected = command.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				LoggerUtil.LogError(ex, nameof(LicenseDataAccess), nameof(_ArchiveExpiredAppointments));
				//GlobalUtility.LoggerUtil.SetTheLogMessage(ex, "");
				//Debug.WriteLine($"Exception Message : {ex.Message}    |  Stack Trace {ex.StackTrace}");
			}
			finally { connection.Close(); }

			return rowsAffected > 0;
		}


		public static bool FindByTestAppointmentID(int id, ref int testTypeID, ref int localLicenseApplicationID,
        ref DateTime appointmentDate, ref bool isLocked, ref decimal fees, ref int
            createByUserID, ref int? retakeTestAppID)
        {
            _ArchiveExpiredAppointments();

			bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM TestAppointments WHERE TestAppointmentID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@id", id);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    testTypeID = (int)reader["TestTypeID"];
                    localLicenseApplicationID = (int)reader["LocalLicenseApplicationID"];
                    appointmentDate = (DateTime)reader["AppointmentDate"];
                    isLocked = (bool)reader["IsLocked"];
                    fees = (decimal)reader["AppointmentFee"];
                    createByUserID = (int)reader["CreatedByUserID"];


                    retakeTestAppID = (reader["RetakeTestApplicationID"] != DBNull.Value) ?
                        (int?)reader["RetakeTestApplicationID"] : null;

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



        public static bool FindByRetakeTestAppointmentID(ref int id, ref int testTypeID, ref int localLicenseApplicationID,
       ref DateTime appointmentDate, ref bool isLocked, ref decimal fees, ref int
           createByUserID, int? retakeTestAppID)
        {
            _ArchiveExpiredAppointments();

			bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = "SELECT * FROM TestAppointments WHERE RetakeTestApplicationID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            if (retakeTestAppID != null)
                command.Parameters.AddWithValue("@id", retakeTestAppID);

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    id = (int)reader["TestAppointmentID"];

                    testTypeID = (int)reader["TestTypeID"];
                    localLicenseApplicationID = (int)reader["LocalLicenseApplicationID"];
                    appointmentDate = (DateTime)reader["AppointmentDate"];
                    isLocked = (bool)reader["IsLocked"];
                    fees = (decimal)reader["AppointmentFee"];
                    createByUserID = (int)reader["CreatedByUserID"];

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



        public static DataTable GetAllTestAppointmentesByPersonID(int personID)
        {
			_ArchiveExpiredAppointments();

			SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT * FROM TestAppointmentsView
                                WHERE ApplicantPersonID = @personID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@personID", personID);

            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("الإختبار", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("رقم المتقدم بالطلب", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("إسم مقدم الطلب", typeof(string)));

                dt.Columns.Add(_CreateDataColumn("فهرس طلب الرخصة المحلية", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("الرخصة القائم عليها الطلب", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("تاريخ الموعد", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("الرسوم المالية للموعد", typeof(decimal)));
                dt.Columns.Add(_CreateDataColumn("فهرس طلب إعادة الإختبار", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("تم إنشاء الموعد بواسطة المستخدم", typeof(int)));






                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["TestAppointmentID"];
                    row["الإختبار"] = (string)reader["TestTypeTitle"];
                    row["رقم المتقدم بالطلب"] = (int)reader["ApplicantPersonId"];
                    row["إسم مقدم الطلب"] = (string)reader["ApplicantName"];
                    row["فهرس طلب الرخصة المحلية"] = (int)reader["LocalLicenseApplicationID"];
                    row["الرخصة القائم عليها الطلب"] = (string)reader["ClassName"];
                    row["تاريخ الموعد"] = (DateTime)reader["AppointmentDate"];
                    row["الرسوم المالية للموعد"] = (decimal)reader["AppointmentFee"];

                    if (reader["RetakeTestApplicationID"] != DBNull.Value)
                        row["فهرس طلب إعادة الإختبار"] = (int)reader["RetakeTestApplicationID"];

                    row["تم إنشاء الموعد بواسطة المستخدم"] = (int)reader["CreatedByUserID"];

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


        public static DataTable GetAllTestAppointmentesByLocalLicenseAppIdAndTestTypeID(int localLicenseAppID, int testTypeID)
        {
			_ArchiveExpiredAppointments();

			SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT TestAppointmentID, AppointmentDate, AppointmentFee, IsLocked 
                                FROM TestAppointments   
                                WHERE LocalLicenseApplicationID = @localLicenseAppID 
                                AND TestTypeID = @testTypeID";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@localLicenseAppID", localLicenseAppID);
            command.Parameters.AddWithValue("@testTypeID", testTypeID);


            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
               
                dt.Columns.Add(_CreateDataColumn("موعد الإختبار", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("الرسوم المدفوعة", typeof(decimal)));
                dt.Columns.Add(_CreateDataColumn("الموعد مغلق", typeof(bool)));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["TestAppointmentID"];
                   
                    row["موعد الإختبار"] = (DateTime)reader["AppointmentDate"];
                    row["الرسوم المدفوعة"] = (decimal)reader["AppointmentFee"];
                    row["الموعد مغلق"] = (bool)reader["IsLocked"];

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


        public static DataTable GetAllTestAppointmentesByLocalLicenseAppIdAndTestType(int localLicenseAppID, string testType)
        {
			_ArchiveExpiredAppointments();

			SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"SELECT TestAppointmentID , TestTypeTitle, AppointmentDate, AppointmentFee, IsLocked 
                                FROM TestAppointmentsView   
                                WHERE LocalLicenseApplicationID = @localLicenseAppID 
                                AND TestTypeTitle = @testType";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@localLicenseAppID", localLicenseAppID);
            command.Parameters.AddWithValue("@testType", testType);


            DataTable dt = null;

            try
            {
                connection.Open();

                SqlDataReader reader = command.ExecuteReader();

                dt = new DataTable();

                dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
                dt.Columns.Add(_CreateDataColumn("نوع الإختبار", typeof(string)));
                dt.Columns.Add(_CreateDataColumn("موعد الإختبار", typeof(DateTime)));
                dt.Columns.Add(_CreateDataColumn("الرسوم المدفوعة", typeof(decimal)));
                dt.Columns.Add(_CreateDataColumn("الموعد مغلق", typeof(bool)));

                while (reader.Read())
                {
                    DataRow row = dt.NewRow();

                    row["الفهرس"] = (int)reader["TestAppointmentID"];
                    row["نوع الإختبار"] = (string)reader["TestTypeTitle"];

                    row["موعد الإختبار"] = (DateTime)reader["AppointmentDate"];
                    row["الرسوم المدفوعة"] = (decimal)reader["AppointmentFee"];
                    row["الموعد مغلق"] = (bool)reader["IsLocked"];

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




        public static int AddNewTestAppointment(int testTypeID, int localLicenseApplicationID,
            DateTime appointmentDate, bool isLocked, decimal fees, int createByUserID, int? retakeTestAppID)
        {
            int insertedRow = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"
                         INSERT INTO TestAppointments (TestTypeID, LocalLicenseApplicationID, AppointmentDate, 
                                       IsLocked, AppointmentFee, CreatedByUserID, RetakeTestApplicationID)

                                       VALUES (@testTypeID, @localLicenseApplicationID, @appointmentDate, 
                                          @isLocked, @fees, @createByUserID, @retakeTestAppID);

                               SELECT SCOPE_IDENTITY(); ";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@testTypeID", testTypeID);
            command.Parameters.AddWithValue("@localLicenseApplicationID", localLicenseApplicationID);
            command.Parameters.AddWithValue("@appointmentDate", appointmentDate);
            command.Parameters.AddWithValue("@isLocked", isLocked);
            command.Parameters.AddWithValue("@fees", fees);
            command.Parameters.AddWithValue("@createByUserID", createByUserID);


            command.Parameters.AddWithValue("retakeTestAppID", retakeTestAppID.HasValue ? (object)retakeTestAppID : DBNull.Value);

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
            finally
            {
                connection.Close();
            }

            return insertedRow;
        }



        public static bool UpdateTestAppointment(int id, int createdByUserID ,DateTime appointmentDate, bool isLocked = false)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Update TestAppointments 
                            Set IsLocked = @isLocked,
                            AppointmentDate = @date,
                            CreatedByUserID = @userID
                 
                            WHERE TestAppointmentID = @id";

            SqlCommand command = new SqlCommand(query, connection);

            command.Parameters.AddWithValue("@ID", id);
            command.Parameters.AddWithValue("@isLocked", isLocked);
            command.Parameters.AddWithValue("@date", appointmentDate);
            command.Parameters.AddWithValue("@userID", createdByUserID);

            try
            {
                connection.Open();

                rowAffected = command.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                //Debug.WriteLine($"{ex.Message}  {ex}");

                LoggerUtil.LogError(ex, nameof(TestAppointmentDataAccess), nameof(UpdateTestAppointment),
                    new Dictionary<string, object> { { "ID", id }, { "userID", createdByUserID }, { "date", appointmentDate }, { "isLocked", isLocked } });
            }
            finally { connection.Close(); }


            return rowAffected > 0;
        }

        public static bool DeleteTestAppointmentByID(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete TestAppointments
                            WHERE TestAppointmentID = @ID";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@ID", id);


            try
            {
                connection.Open();

                rowAffected = command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(TestAppointmentDataAccess), nameof(DeleteTestAppointmentByID), 
                    new Dictionary<string, object> { { "ID", id} });
            }
            finally { connection.Close(); }


            return rowAffected > 0;
        }


        public static bool DeleteTestAppointmentByLocalLicenseApplicationID(int id)
        {
            int rowAffected = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Delete TestAppointmentes
                            WHERE LocalLicenseApplicationID = @ID";

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

            string query = @"Select TestAppointmentID From TestAppointments
                            WHERE TestAppointmentID = @ID";

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

        public static bool IsExistByLocalLicneseID(int id)
        {
            bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select TestAppointmentID From TestAppointments
                            WHERE LocalLicenseApplicationID = @id";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@id", id);


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


        public static bool IsActiveAppointmentByLocalLicneseIDExist(int id)
        {
			_ArchiveExpiredAppointments();

			bool isFound = false;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select TestAppointmentID From TestAppointments
                            WHERE LocalLicenseApplicationID = @id AND IsLocked = 0";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@id", id);


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


        public static int GetID_OfActiveAppointmentByLocalLicneseIDExist(int id)
        {
			_ArchiveExpiredAppointments();

			int appointmentID = -1;

            SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

            string query = @"Select TestAppointmentID From TestAppointments
                            WHERE LocalLicenseApplicationID = @id AND IsLocked = 0";

            SqlCommand command = new SqlCommand(query, connection);


            command.Parameters.AddWithValue("@id", id);


            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                if (row != null && int.TryParse(row.ToString(), out int result))
                {
                    appointmentID = result;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"{ex.Message}  {ex}");
            }
            finally { connection.Close(); }


            return appointmentID;
        }

    }
}
