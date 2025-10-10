using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DLMS_DataAccessLayer;


namespace DLMS_BusinessLogicLayer
{
    public class ApplicationType
    {

        private int _id;
        public int ID { get { return _id; } }

        public decimal Fee { get; set; }

        public string Type { get; set; }



        private enStatusMode _mode;

        private ApplicationType(int id, decimal fee, string ApplicationType)
        {
            _id = id;
            Fee = fee;
            Type = ApplicationType;
            _mode = enStatusMode.UpdateMode;
        }

        public ApplicationType()
        {
            _id = -1;
            Fee = -1;
            Type = "";


            _mode = enStatusMode.AddMode;
        }

        public static ApplicationType Find(int id)
        {

            decimal applicationFee = -1;
            string ApplicationType = "";

            if (ApplicationTypeDataAccess.FindByApplicationTypeID(id, ref ApplicationType, ref applicationFee))
                return new ApplicationType(id, applicationFee, ApplicationType);

            return null;
        }

        public static ApplicationType Find(string ApplicationType)
        {
            int id = -1;
            decimal applicationFee = -1;

            if (ApplicationTypeDataAccess.FindByApplicationType(ref id, ApplicationType, ref applicationFee))
                return new ApplicationType(id, applicationFee, ApplicationType);

            return null;
        }



        public static DataTable GetAllApplicationTypes()
        {
            return ApplicationTypeDataAccess.GetAll();
        }

        //private bool _AddNewApplicationType()
        //{
        //    return (_id = ApplicationTypeDataAccess.AddApplicationType(Type, Fee)) > 0;
        //}

        private bool _UpdateApplicationType()
        {
            if (ID > 7 || ID < 1) return false;

            return ApplicationTypeDataAccess.UpdateApplicationType(_id, Type, Fee);
        }

        public bool Save()
        {
            switch (_mode)
            {
                //case enStatusMode.AddMode:
                //    return _AddNewApplicationType();

                case enStatusMode.UpdateMode:
                    return _UpdateApplicationType();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return ApplicationTypeDataAccess.IsExist(id);
        }

        public static bool IsExist(string ApplicationType)
        {
            return ApplicationTypeDataAccess.IsExist(ApplicationType);
        }


        public static bool DeleteApplicationType(int id)
        {
            return ApplicationTypeDataAccess.DeleteApplicationType(id);
        }

        public static bool DeleteApplicationType(string ApplicationType)
        {
            return ApplicationTypeDataAccess.DeleteApplicationType(ApplicationType);
        }

    }
}
