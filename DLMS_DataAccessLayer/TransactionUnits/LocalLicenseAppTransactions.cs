using GlobalUtility;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_DataAccessLayer.TransactionUnits;

internal static class LocalLicenseAppTransactions
{

    private static bool _DeleteTestByLocalLicenseAppID(int id, SqlConnection connection, SqlTransaction trx)
    {
        int rowAffected = -1;


        string query = @"Delete Tests
                            FROM Tests 
                            
                            INNER JOIN TestAppointments
                            ON TestAppointments.TestAppointmentID = Tests.TestAppointmentID

                            WHERE TestAppointments.LocalLicenseApplicationID = @ID";


        using (SqlCommand command = new SqlCommand(query, connection, trx))
        {
            command.Parameters.AddWithValue("@ID", id);

            rowAffected = command.ExecuteNonQuery();

            return rowAffected > 0;
        }
    }


    private static bool _DeleteTestAppointmentsByLocalLicenseAppID(int id, SqlConnection connection, SqlTransaction trx)
    {
        int rowAffected = -1;


        string query = @"Delete TestAppointments
                            WHERE LocalLicenseApplicationID = @ID";


        using (SqlCommand command = new SqlCommand(query, connection, trx))
        {
            command.Parameters.AddWithValue("@ID", id);

            rowAffected = command.ExecuteNonQuery();

            return rowAffected > 0;
        }
    }


    //Obsolete version
    //private static bool _DeleteRetakeTest(SqlConnection connection, SqlTransaction trx)
    //{
    //    int rowAffected = -1;


    //    string query = @"DELETE Applications
    //                     FROM Applications app
    //                     LEFT JOIN TestAppointments apt ON app.ApplicationID = apt.RetakeTestApplicationID
    //                     WHERE ApplicationTypeID = 7 AND LocalLicenseApplicationID IS NULL";


    //    using (SqlCommand command = new SqlCommand(query, connection, trx))
    //    {
           
    //        rowAffected = command.ExecuteNonQuery();

    //        return rowAffected > 0;
    //    }
    //}

    private static bool _DeleteRetakeTestApps(int id, SqlConnection connection, SqlTransaction trx)
    {
        int rowAffected = -1;


        string query = @"Delete Applications
                            WHERE ReferencedToMainAppID = @ID";


        using (SqlCommand command = new SqlCommand(query, connection, trx))
        {
            command.Parameters.AddWithValue("@ID", id);

            rowAffected = command.ExecuteNonQuery();

            return rowAffected > 0;
        }
    }

    private static bool _DeleteLocalLicenseAppByLocalAppID(int id, SqlConnection connection, SqlTransaction trx)
    {
        int rowAffected = -1;


        string query = @"Delete LocalLicenseApplications
                            WHERE LocalLicenseApplicationID = @ID";


        using (SqlCommand command = new SqlCommand(query, connection, trx))
        {
            command.Parameters.AddWithValue("@ID", id);

            rowAffected = command.ExecuteNonQuery();

            return rowAffected > 0;
        }
    }



    private static bool _DeleteApplicaiton(int id, SqlConnection connection, SqlTransaction trx)
    {
        int rowAffected = -1;


        string query = @"DELETE Applications
                            WHERE ApplicationID = @ID";


        using (SqlCommand command = new SqlCommand(query, connection, trx))
        {
            command.Parameters.AddWithValue("@ID", id);

            rowAffected = command.ExecuteNonQuery();

            return rowAffected > 0;
        }
    }


    private static int ValidateAndFetchAppID(int localAppID, bool requestTransaction = false)
    {
        int appID = LocalLicenseApplicationDataAccess.GetAppID_LocalAppID(localAppID);

        if (appID == -1)
            throw new InvalidOperationException("Invalid appID — no valid Application found for LocalApplicationID.");

        if (!requestTransaction)
        {
            if (LocalLicenseApplicationDataAccess.GetRequestIDBy_LocalAppID(localAppID) != -1)
                throw new InvalidOperationException("Cannot delete LocalLicenseApplication — it has a linked Request. Delete the Request first.");
        }

        return appID;
    }
    public static bool DeleteLocalLicenseApplication(int localAppID)
    {
        bool isDeleted = false;



        using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
        {

            connection.Open();


            using (SqlTransaction trx = connection.BeginTransaction())
            {
                try
                {

                    int appID = ValidateAndFetchAppID(localAppID);


                    _DeleteTestByLocalLicenseAppID(localAppID, connection, trx);
                    _DeleteTestAppointmentsByLocalLicenseAppID(localAppID, connection, trx);
                    _DeleteRetakeTestApps(appID, connection, trx);
                    _DeleteLocalLicenseAppByLocalAppID(localAppID, connection, trx);

                    isDeleted = _DeleteApplicaiton(appID, connection, trx);

                    trx.Commit();
                }
                catch (Exception ex)
                {
                    trx.Rollback();

                    LoggerUtil.LogError(ex, nameof(LocalLicenseApplicationDataAccess), nameof(DeleteLocalLicenseApplication),
                        new Dictionary<string, object> { { "LocalAppID", localAppID } });

                    isDeleted = false;
                }

                return isDeleted;
            }
        }
    }


    internal static bool DeleteRequestDepedentActions(int localAppID, SqlConnection connection, SqlTransaction trx)
    {

        int appID = ValidateAndFetchAppID(localAppID, true);


        _DeleteTestByLocalLicenseAppID(localAppID, connection, trx);
        _DeleteTestAppointmentsByLocalLicenseAppID(localAppID, connection, trx);
        _DeleteRetakeTestApps(appID, connection, trx);

        _DeleteLocalLicenseAppByLocalAppID(localAppID, connection, trx);

        _DeleteApplicaiton(appID, connection, trx);

        return true;
    }
}
