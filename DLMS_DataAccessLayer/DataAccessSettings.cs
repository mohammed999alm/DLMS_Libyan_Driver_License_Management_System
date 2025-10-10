using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DLMS_BusinessLogicLayer")]

namespace DLMS_DataAccessLayer
{

    public static class DataAccessSettings
    {

        internal static string stringConnection;


        public static void InjectConntection(string connetion)
        {
            stringConnection = connetion;
        }
    }
}
