using GlobalUtility;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace DLMS_DataAccessLayer.TransactionUnits;

internal static class PersonTransactions
{


    private static bool _DeleteTestsByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @"DELETE Tests  FROM Tests  test

                            INNER JOIN TestAppointments appointment ON appointment.TestAppointmentID = test.TestAppointmentID
                            INNER JOIN LocalLicenseApplications lApp  ON lApp.LocalLicenseApplicationID = appointment.LocalLicenseApplicationID
                            INNER JOIN Applications app ON app.ApplicationID = lApp.ApplicationID

                            WHERE app.ApplicantPersonID = @personID";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);
            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }

    private static bool _DeleteTestAppointmentsByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @" DELETE TestAppointments FROM TestAppointments appointment 
                              INNER JOIN LocalLicenseApplications lApp  ON lApp.LocalLicenseApplicationID = appointment.LocalLicenseApplicationID
                              INNER JOIN Applications app ON app.ApplicationID = lApp.ApplicationID
                              WHERE app.ApplicantPersonID = @personID";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);

            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }


    private static bool _DeleteLocalLicenseAppByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @" DELETE LocalLicenseApplications  FROM LocalLicenseApplications lApp

                             INNER JOIN Applications app ON lApp.ApplicationID = app.ApplicationID
                             WHERE app.ApplicantPersonID = @personId";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);

            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }

    private static bool _DeleteApplicationByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @" DELETE Applications WHERE ApplicantPersonID = @personID";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);

            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }


    private static bool _DeleteRequestsByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @" DELETE Requests FROM Requests 
                              INNER JOIN People ON People.NationalNumber = Requests.NationalNumber
                              WHERE People.PersonID = @personID";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);

            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }



    private static bool _DeletePhonesByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @"DELETE Phones WHERE OwnerPersonID = @personID";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);

            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }

    private static bool _DeleteEmailsByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @"DELETE EmailAddresses WHERE OwnerPersonID = @personID";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);

            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }
    private static bool _DeletePersonByPersonID(int personID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @"Delete People
                            WHERE PersonID = @personID";


        using (var cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@personID", personID);

            int affectedRows = cmd.ExecuteNonQuery();

            return affectedRows > 0;
        }
    }

    internal static bool DeletePerson(int id)
    {
        bool isRowAffected = false;

        try
        {
            using (SqlConnection connection = new SqlConnection(DataAccessSettings.stringConnection))
            {

                connection.Open();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        _DeleteTestsByPersonID(id, connection, transaction);
                        _DeleteTestAppointmentsByPersonID(id, connection, transaction);
                        _DeleteLocalLicenseAppByPersonID(id, connection, transaction);
                        _DeleteApplicationByPersonID(id, connection, transaction);
                        _DeleteRequestsByPersonID(id, connection, transaction);
                        _DeletePhonesByPersonID(id, connection, transaction);
                        _DeleteEmailsByPersonID(id, connection, transaction);
                        isRowAffected = _DeletePersonByPersonID(id, connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        LoggerUtil.LogError(ex, nameof(PersonDataAccess), nameof(DeletePerson), new Dictionary<string, object> { { "PersonID", id } });
                        isRowAffected = false;
                    }

                    //return isRowAffected;
                }
            }

        }
        catch (Exception ex) 
        {
			LoggerUtil.LogError(ex, nameof(PersonDataAccess), nameof(DeletePerson), new Dictionary<string, object> { { "PersonID", id } });
			isRowAffected = false;
		}
		return isRowAffected;
	}
}
