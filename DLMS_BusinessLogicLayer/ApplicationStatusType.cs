using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class ApplicationStatusType
    {

        public int ID { get; }
        public string Type { get; }

        private ApplicationStatusType(int id, string type)
        {
            ID = id;
            Type = type;
        }


        public static ApplicationStatusType Find(int id)
        {
            string type = null;

            if (ApplicationStatusTypeDataAccess.FindByApplicationStatusTypeID(id, ref type))
                return new ApplicationStatusType(id, type);

            return null;
        }


        
        public static ApplicationStatusType? Find(string type)
        {
            int id = -1;

            if (ApplicationStatusTypeDataAccess.FindByApplicationStatusTypeTag(ref id, type))
                return new ApplicationStatusType(id, type);

            return null;
        }


        public static DataTable GetAll()
        {
            return ApplicationStatusTypeDataAccess.GetAll();
        }

        public static List<string>? GetAllStatus() 
        {
            return ApplicationStatusTypeDataAccess.GetAllStatus();
        }
    }
}
