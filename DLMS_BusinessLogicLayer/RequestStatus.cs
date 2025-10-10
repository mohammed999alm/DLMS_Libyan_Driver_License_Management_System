using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class RequestStatus
    {


        public int ID { get; }
        public string Name { get; }

        private RequestStatus(int id, string name)
        {
            ID = id;
            Name = name;
        }


        public static RequestStatus Find(int id)
        {
            string name = null;

            if (RequestStatusDataAccess.FindByRequestStatusID(id, ref name))
                return new RequestStatus(id, name);

            return null;
        }


        public static RequestStatus Find(string name)
        {
            int id = -1;

            if (RequestStatusDataAccess.FindByRequestStatusTag(ref id, name))
                return new RequestStatus(id, name);

            return null;
        }


        public static DataTable GetAll()
        {
            return RequestStatusDataAccess.GetAll();
        }
    }
}
