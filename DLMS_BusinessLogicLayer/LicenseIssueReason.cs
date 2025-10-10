using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class LicenseIssueReason
    {
        public int ID { get; }
        public string Name { get; }

        private LicenseIssueReason(int id, string name)
        {
            ID = id;
            Name = name;
        }


        public static LicenseIssueReason Find(int id)
        {
            string name = null;

            if (LicenseIssueReasonDataAccess.FindByID(id, ref name))
                return new LicenseIssueReason(id, name);

            return null;
        }


        public static LicenseIssueReason Find(string name)
        {
            int id = -1;

            if (LicenseIssueReasonDataAccess.FindByName(ref id, name))
                return new LicenseIssueReason(id, name);

            return null;
        }


        public static List<string> GetAll()
        {
            return LicenseIssueReasonDataAccess.GetAll();
        }
    }
}
