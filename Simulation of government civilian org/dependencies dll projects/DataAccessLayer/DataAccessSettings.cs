using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer
{
    public static class DataAccessSettings
    {
        //internal static string connectionString = "Server=LAPTOP-8KU24VTD\\MSSQLSERVER22;Database=MYDLVD;User Id=sa;Password=123456;\"Server=LAPTOP-8KU24VTD\\\\MSSQLSERVER22;Database=MYDLVD;User Id=sa;Password=123456;TrustServerCertificate=True;";

        internal static string connectionString = "";

        public static void Initialize(string connectionStringParam) 
        {
            connectionString = connectionStringParam;
        }

        //internal static SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
        //{
        //    DataSource = "LAPTOP-8KU24VTD\\MSSQLSERVER22",
        //    InitialCatalog = "MYDLVD",
        //    UserID = "sa",
        //    Password = "123456",
        //    TrustServerCertificate = true // Ensure to trust the server certificate
        //};

        //JsonForm
    }
}
