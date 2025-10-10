using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using GlobalUtility;

namespace DLMS_DataAccessLayer;

public class ApplicationDataAccess
{
    public static bool FindByApplicationID(int id, ref int applicantID, ref int applicationTypeID,
ref DateTime applicationDate, ref int applicationStatusID, ref DateTime? lastStatusDate, ref decimal paidFees,
ref int createdByUserID, ref int? updatedByUserID, ref int? requestID)
    {
        bool isFound = false;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = "SELECT * FROM Applications WHERE ApplicationID = @id";

        SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@id", id);

        try
        {
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                applicantID = (int)reader["ApplicantPersonID"];
                applicationTypeID = (int)reader["ApplicationTypeID"];
                applicationDate = (DateTime)reader["ApplicationDate"];
                applicationStatusID = (int)reader["ApplicationStatusID"];

                //lastStatusDate = ((DateTime)reader["lastStatusDate"] != null ) ? lastStatusDate : null;
                if (reader["lastStatusDate"] != DBNull.Value)
                    lastStatusDate = (DateTime)reader["LastStatusDate"];
                else
                    lastStatusDate = null;


                paidFees = (decimal)reader["paidFees"];

                createdByUserID = (int)reader["createdByUserID"];

                updatedByUserID = (reader["updatedByUserID"] != DBNull.Value) ? (int?)reader["UpdatedByUserID"] : null;

                requestID = (reader["RequestID"] != DBNull.Value) ? (int?)reader["RequestID"] : null;

                isFound = true;
            }

            reader.Close();
        }

        catch (Exception ex)
        {
            Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());

            //isFound = false;
        }
        finally { connection.Close(); }

        return isFound;
    }




    public static bool FindByRequestID(ref int id, ref int applicantID, ref int applicationTypeID,
ref DateTime applicationDate, ref int applicationStatusID, ref DateTime? lastStatusDate, ref decimal paidFees,
ref int createdByUserID, ref int? updatedByUserID,  int? requestID)
    {
        bool isFound = false;

        if (requestID is null && !requestID.HasValue) return false;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = "SELECT * FROM Applications WHERE RequestID = @RequestID";

        SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@RequestID", requestID);

        try
        {
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            if (reader.Read())
            {
                applicantID = (int)reader["ApplicantPersonID"];
                applicationTypeID = (int)reader["ApplicationTypeID"];
                applicationDate = (DateTime)reader["ApplicationDate"];
                applicationStatusID = (int)reader["ApplicationStatusID"];

                //lastStatusDate = ((DateTime)reader["lastStatusDate"] != null ) ? lastStatusDate : null;
                if (reader["lastStatusDate"] != DBNull.Value)
                    lastStatusDate = (DateTime)reader["LastStatusDate"];
                else
                    lastStatusDate = null;


                paidFees = (decimal)reader["paidFees"];

                createdByUserID = (int)reader["createdByUserID"];

                updatedByUserID = (reader["updatedByUserID"] != DBNull.Value) ? (int?)reader["UpdatedByUserID"] : null;

                id =  (int)reader["ApplicationID"];

                isFound = true;
            }

            reader.Close();
        }

        catch (Exception ex)
        {
            LoggerUtil.LogError(ex, nameof(ApplicationDataAccess), nameof(FindByRequestID),
       new Dictionary<string, object>
                                             {
                                                { "ID", id },
                                                { "ApplicantPersonID", applicantID },
                                                { "ApplicationTypeID", applicationTypeID },
                                                { "ApplicationDate", applicationDate },
                                                { "ApplicationStatusID", applicationStatusID },
                                                { "LastStatusDate", lastStatusDate },
                                                { "PaidFees", paidFees },
                                                { "CreatedByUserID", createdByUserID },
                                                { "UpdatedByUserID", updatedByUserID },
                                                { "RequestID", requestID },
                                                { "IsFound", isFound }
                                             } );

            //isFound = false;

        }
        finally { connection.Close(); }

        return isFound;
    }





    private static DataColumn _CreateDataColumn(string name, Type type)
    {
        DataColumn column = new DataColumn(name);
        column.DataType = type;

        column.AllowDBNull = true;

        return column;
    }



    public static DataTable GetAllApplications(string? appType = null)
    {
        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"SELECT * FROM ApplicationsView
                             WHERE (@appType IS NULL OR ApplicationTypeTitle = @appType)";

        SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@appType", string.IsNullOrEmpty(appType) ? DBNull.Value : appType);

        //object? appyType2 = appType;
        //command.Parameters.AddWithValue("@appType", appyType2 ?? DBNull.Value);


        DataTable? dt = null;

        try
        {
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            dt = new DataTable();

            dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
            dt.Columns.Add(_CreateDataColumn("الشخص المفهرس برقم", typeof(int)));
            dt.Columns.Add(_CreateDataColumn("الرقم الوطني", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("الإسم", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("اللقب", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("حالة الطلب", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("نوع الطلب", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("تاريخ إنشاء الطلب", typeof(DateTime)));
            dt.Columns.Add(_CreateDataColumn("تاريخ إنتهاء الطلب", typeof(DateTime)));
            dt.Columns.Add(_CreateDataColumn("الرسوم المدفوعة", typeof(decimal)));
            dt.Columns.Add(_CreateDataColumn("تم الإنشاء بواسطة المستخدم المفهرس", typeof(int)));
            dt.Columns.Add(_CreateDataColumn("تم التعديل بواسطة المستخدم المفهرس", typeof(int)));

            while (reader.Read())
            {
                DataRow row = dt.NewRow();

                row["الفهرس"] = (int)reader["ApplicationID"];
                row["الشخص المفهرس برقم"] = (int)reader["PersonID"];
                row["الرقم الوطني"] = (string)reader["NationalNumber"];
                row["الإسم"] = (string)reader["FirstName"];
                row["اللقب"] = (string)reader["LastName"];
                row["حالة الطلب"] = (string)reader["StatusTypeTitle"];
                row["نوع الطلب"] = (string)reader["ApplicationTypeTitle"];
                row["تاريخ إنشاء الطلب"] = (DateTime)reader["ApplicationDate"];


                if (reader["LastStatusDate"] != DBNull.Value)
                    row["تاريخ إنتهاء الطلب"] = (DateTime)reader["LastStatusDate"];



                row["الرسوم المدفوعة"] = (decimal)reader["PaidFees"];
                row["تم الإنشاء بواسطة المستخدم المفهرس"] = (int)reader["CreatedByUserID"];

                if (reader["UpdatedByUserID"] != DBNull.Value)
                    row["تم التعديل بواسطة المستخدم المفهرس"] = (int)reader["UpdatedByUserID"];

                Debug.WriteLine("Paid Fees   : {0}",
                row["الرسوم المدفوعة"]);


                Debug.WriteLine("Created By User ID :    {0}",
                 row["تم الإنشاء بواسطة المستخدم المفهرس"]);

                Debug.WriteLine("Updated By User ID :  {0}",
                row["تم التعديل بواسطة المستخدم المفهرس"]);
                dt.Rows.Add(row);
            }

            reader.Close();
        }
        catch (Exception ex)
        {
            //Debug.WriteLine("{0}   : {1}", ex.Message, ex.ToString());

            //GlobalUtility.LoggerUtil.SetTheLogMessage(ex, ex.Message);

            LoggerUtil.LogError(ex, nameof(ApplicationDataAccess), nameof(GetAllApplications), new Dictionary<string, object> { { "AppType", appType } });
            
            dt = null;
        }
        finally
        {
            connection.Close();
        }

        return dt;
    }



    public static DataTable GetApplicationsWithNoHistory()
    {
        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"SELECT * FROM ApplicationsWithNoHistoryOfTranactions where RoleTag <> 'مدير النظام'";

        SqlCommand command = new SqlCommand(query, connection);

        DataTable dt = null;

        try
        {
            connection.Open();

            SqlDataReader reader = command.ExecuteReader();

            dt = new DataTable();

            dt.Columns.Add(_CreateDataColumn("الفهرس", typeof(int)));
            dt.Columns.Add(_CreateDataColumn("إسم المستخدم", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("الدور الوظيفي", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("الإسم", typeof(string)));
            dt.Columns.Add(_CreateDataColumn("حالة المستخدم", typeof(string)));



            while (reader.Read())
            {
                DataRow row = dt.NewRow();

                row["الفهرس"] = (int)reader["ApplicationID"];
                row["إسم المستخدم"] = (string)reader["Applicationname"];
                row["الدور الوظيفي"] = (string)reader["roleTag"];
                row["الإسم"] = (string)reader["Fullname"];
                row["حالة المستخدم"] = ((bool)reader["IsActive"]) ? "نشط" : "غير نشط";

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




    public static int AddNewApplication(int applicantPersonID, int applicationTypeID, DateTime applicationDate, int applicationStatusID,
        DateTime? lastStatusDate, decimal paidFees, int createdByUserID, int? updatedByUserID, int? requestID, int? referencedToMainAppID)
    {
        int insertedRow = -1;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"
        INSERT INTO Applications (ApplicantPersonID, ApplicationTypeID, ApplicationDate, ApplicationStatusID, LastStatusDate, PaidFees, CreatedByUserID, UpdatedByUserID, RequestID, ReferencedToMainAppID)
        VALUES (@ApplicantPersonID, @ApplicationTypeID, @ApplicationDate, @ApplicationStatusID, @LastStatusDate, @PaidFees, @CreatedByUserID, @UpdatedByUserID, @RequestID, @ReferencedToMainAppID);
        SELECT SCOPE_IDENTITY()";

        SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ApplicantPersonID", applicantPersonID);
        command.Parameters.AddWithValue("@ApplicationTypeID", applicationTypeID);
        command.Parameters.AddWithValue("@ApplicationDate", applicationDate);
        command.Parameters.AddWithValue("@ApplicationStatusID", applicationStatusID);
        command.Parameters.AddWithValue("@LastStatusDate", lastStatusDate.HasValue ? (object)lastStatusDate.Value : DBNull.Value);
        command.Parameters.AddWithValue("@PaidFees", paidFees);
        command.Parameters.AddWithValue("@CreatedByUserID", createdByUserID);
        command.Parameters.AddWithValue("@UpdatedByUserID", updatedByUserID.HasValue ? (object)updatedByUserID.Value : DBNull.Value);
        command.Parameters.AddWithValue("@RequestID", requestID.HasValue ? (object)requestID.Value : DBNull.Value);
        command.Parameters.AddWithValue("@ReferencedToMainAppID", referencedToMainAppID.HasValue ? (object)referencedToMainAppID.Value : DBNull.Value);



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


    public static bool UpdateApplication(int id, int statusID, DateTime? lastStatusDate, int? updatedByUserID)
    {
        int rowAffected = -1;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Update Applications 
                            Set ApplicationStatusID = @statusID,
                            LastStatusDate = @lastStatusDate,
                            UpdatedByUserID = @updatedByUserID
                            
                            WHERE ApplicationID = @id";

        SqlCommand command = new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@statusID", statusID);
        command.Parameters.AddWithValue("@lastStatusDate", lastStatusDate);
        command.Parameters.AddWithValue("@updatedByUserID", updatedByUserID);


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

    public static bool DeleteApplication(int id)
    {
        if (!CouldBeDeletedByAppID(id))
            return false;

        int rowAffected = -1;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Delete Applications
                            WHERE ApplicationID = @ID";

        SqlCommand command = new SqlCommand(query, connection);


        command.Parameters.AddWithValue("@ID", id);


        try
        {
            connection.Open();

            rowAffected = command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            //Debug.WriteLine($"{ex.Message}  {ex}");

            LoggerUtil.LogError(ex, nameof(ApplicationDataAccess), nameof(DeleteApplication), new Dictionary<string, object> { { "ApplicationID", id } });
        }
        finally { connection.Close(); }


        return rowAffected > 0;
    }


    public static bool DeleteApplicationByPersonID(int id)
    {
        int rowAffected = -1;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Delete Applications
                            WHERE ApplicantPersonID = @ID";

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

        string query = @"Select ApplicationID From Applications
                            WHERE ApplicationID = @ID";

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


    public static bool CouldBeDeletedByAppID(int id) 
    {
        string query = @"SELECT 1 FROM Applications WHERE ApplicationID = @ID AND ApplicationStatusID <> 3";

        using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
        using (SqlCommand command = new SqlCommand(query, connection))
        {


            command.Parameters.AddWithValue("@ID", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                return row != null;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(ApplicationDataAccess), nameof(IsActiveApplicationExistByApplicationID),
                    new Dictionary<string, object> { { "ApplicationID", id } });

                return false;
            }
        }
    }



    public static bool CouldBeDeletedByRequestID(int id)
    {
        string query = @"SELECT 1 FROM Applications WHERE RequestID = @ID AND ApplicationStatusID <> 3";

        using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
        using (SqlCommand command = new SqlCommand(query, connection))
        {


            command.Parameters.AddWithValue("@ID", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                return row != null;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(ApplicationDataAccess), nameof(IsActiveApplicationExistByApplicationID),
                    new Dictionary<string, object> { { "ApplicationID", id } });

                return false;
            }
        }
    }


    public static bool IsExistByRequestID(int id)
    {
        bool isFound = false;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Select 1 From Applications
                            WHERE RequestID = @ID";

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
            //Debug.WriteLine($"{ex.Message}  {ex}");
            LoggerUtil.LogError(ex, nameof(ApplicationDataAccess), nameof(IsExistByRequestID), new Dictionary<string, object> { { "RequestID", id } });
        }
        finally { connection.Close(); }


        return isFound;
    }


    public static bool IsActiveApplicationExistByApplicationID(int id)
    {

        string query = @"SELECT 1 FROM ApplicationsView WHERE ApplicationID = @ID AND StatusTypeTitle = 'جديد'";

        using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
        using (SqlCommand command = new SqlCommand(query, connection)) 
        {


            command.Parameters.AddWithValue("@ID", id);

            try
            {
                connection.Open();

                object row = command.ExecuteScalar();

                return row != null;
            }
            catch (Exception ex)
            {
                LoggerUtil.LogError(ex, nameof(ApplicationDataAccess), nameof(IsActiveApplicationExistByApplicationID),
                    new Dictionary<string, object> { { "ApplicationID", id } });

                return false;
            }
        }
    }


    public static bool IsExist(string Applicationname)
    {
        bool isFound = false;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Select ApplicationID From Applications
                            WHERE ApplicationID = @ApplicationID";

        SqlCommand command = new SqlCommand(query, connection);


        command.Parameters.AddWithValue("@ApplicationID", Applicationname);


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


    public static bool IsActiveApplicationExistByPersonID(int id)
    {
        bool isFound = false;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Select ApplicationID From ApplicationsView
                            WHERE PersonID = @ID  AND StatusTypeTitle = 'جديد'";

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

    public static bool IsApplicationExistByPersonID(int id)
    {
        bool isFound = false;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Select ApplicationID From ApplicationsView
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


    public static bool IsActiveRetakeApplicationExistByPersonIDAndApplicationType(int id, string applicationType)
    {
        bool isFound = false;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Select ApplicationID From ApplicationsView
                            WHERE PersonID = @ID  AND StatusTypeTitle = 'جديد' AND ApplicationTypeTitle = @type";

        SqlCommand command = new SqlCommand(query, connection);


        command.Parameters.AddWithValue("@ID", id);
        command.Parameters.AddWithValue("@type", applicationType);


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


    public static int GetActiveApplicationIDByPersonID(int id)
    {

        int applicationID = -1;

        SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection);

        string query = @"Select ApplicationID From ApplicationsView
                            WHERE PersonID = @ID  AND StatusTypeTitle = 'جديد'";

        SqlCommand command = new SqlCommand(query, connection);


        command.Parameters.AddWithValue("@ID", id);


        try
        {
            connection.Open();

            object row = command.ExecuteScalar();

            if (row != null && int.TryParse(row.ToString(), out int temp))
            {
                applicationID = temp;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"{ex.Message}  {ex}");
        }
        finally { connection.Close(); }


        return applicationID;
    }
}
