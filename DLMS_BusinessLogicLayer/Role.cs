using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    internal class Role
    {


        public int ID { get; }
        public string Name { get; }

        private Role(int id, string name)
        {
            ID = id;
            Name = name;
        }


        public static Role Find(int id)
        {
            string name = null;

            if (RoleDataAccess.FindByRoleID(id, ref name))
                return new Role(id, name);

            return null;
        }


        public static Role Find(string name)
        {
            int id = -1;

            if (RoleDataAccess.FindByRoleTag(ref id, name))
                return new Role(id, name);

            return null;
        }


        public static DataTable GetAll()
        {
            return RoleDataAccess.GetAll();
        }
    }
}