using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLMS_DataAccessLayer;
using System.Data;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DLMS_SERVER")]


namespace DLMS_BusinessLogicLayer
{
    public class Country
    {



        public int ID { get; }
        public string Name { get; }

        private Country(int id, string name)
        {
            ID = id;
            Name = name;
        }


        public static Country Find(int id)
        {
            string name = null;

            if (CountryDataAccess.FindByCountryID(id, ref name))
                return new Country(id, name);

            return null;
        }


        public static Country Find(string name)
        {
            int id = -1;

            if (CountryDataAccess.FindByCountryName(ref id, name))
                return new Country(id, name);

            return null;
        }


        public static DataTable GetAll()
        {
            return CountryDataAccess.GetAllCoutnries();
        }

    }
}
