using GlobalUtility;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_DataAccessLayer.TransactionUnits;

internal static class RequestTransactions
{

    private static bool _DeleteRequest(int reqID, SqlConnection conn, SqlTransaction trx)
    {
        string query = @"DELETE Requests WHERE RequestID = @reqID";

        using (SqlCommand cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@reqID", reqID);

            return cmd.ExecuteNonQuery() > 0;
        }
    }


    private static bool _DeleteApplicationByRequestID(int reqID, SqlConnection conn, SqlTransaction trx) 
    {
        string query = @"DELETE Applications WHERE RequestID = @reqID";

        using (SqlCommand cmd = new SqlCommand(query, conn, trx))
        {
            cmd.Parameters.AddWithValue("@reqID", reqID);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
    public static bool DeleteRequest(int reqID)
    {
        bool isDeleted = false;

        int lAppID = RequestDataAccess.GetLocalLicenseIDBy_RequestID(reqID);

        SqlTransaction trx;

        try
        {
            using (SqlConnection conn = new SqlConnection(DataAccessSettings.stringConnection))
            {
                conn.Open();

                trx = conn.BeginTransaction();
                {
                    try
                    {
                        if (lAppID > 0)
                            LocalLicenseAppTransactions.DeleteRequestDepedentActions(lAppID, conn, trx);
                        else
                            _DeleteApplicationByRequestID(reqID, conn, trx);

                        isDeleted = _DeleteRequest(reqID, conn, trx);

                        trx.Commit();
                    }
                    catch (Exception ex)
                    {
                        LoggerUtil.LogError(ex, nameof(RequestTransactions), nameof(DeleteRequest), new Dictionary<string, object> { { "RequestID", reqID } });


                        isDeleted = false;
                        try
                        {
                            trx.Rollback();
                        }
                        catch (Exception ex2)
                        {

                            LoggerUtil.LogError(ex, nameof(RequestTransactions), nameof(DeleteRequest), new Dictionary<string, object> { { "RequestID", reqID } });

                        }
                    }

                }

            }
        }
        catch (Exception ex) 
        {
            LoggerUtil.LogError(ex, nameof(RequestTransactions), nameof(DeleteRequest), new Dictionary<string, object> { { "RequestID", reqID } });
		}

		return isDeleted;
	}
}
  
