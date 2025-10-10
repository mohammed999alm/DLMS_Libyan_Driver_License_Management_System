using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class Municipality
    {

        public int ID { get; }
        public string Name { get; }

        private Municipality(int id, string name)
        {
            ID = id;
            Name = name;
        }


        public static Municipality Find(int id)
        {
            string name = null;

            if (MunicipalityDataAccess.FindByID(id, ref name))
                return new Municipality(id, name);

            return null;
        }


        public static Municipality Find(string name)
        {
            int id = -1;

            if (MunicipalityDataAccess.FindByName(ref id, name))
                return new Municipality(id, name);

            return null;
        }


        public static DataTable GetAll()
        {
            return MunicipalityDataAccess.GetAllMunicipalities();
        }
    }
}
