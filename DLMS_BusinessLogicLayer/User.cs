using DLMS_DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DLMS_BusinessLogicLayer
{
    public class User
    {

        private int _id;
        public int ID { get { return _id; } }

        public int PersonID { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        private string _role;
        public string UserRole { get { return _role; } set { _role = value; } }

        public string ActiveStatus { get; set; }

        internal int FailAttempts { get; set; }


        private enStatusMode _mode;

        private User(int id, int personID, string username, string password, string role, string activeStatus, int failAttempts = 0)
        {
            _id = id;
            PersonID = personID;
            Username = username;
            Password = password;
            _role = role;
            _mode = enStatusMode.UpdateMode;
            ActiveStatus = activeStatus;
            FailAttempts = failAttempts;    
        }

        public User()
        {
            _id = -1;
            PersonID = -1;
            Username = "";
            Password = "";
            _role = "";
            ActiveStatus = "";
            _mode = enStatusMode.AddMode;
        }

        //public static User Find(int id)
        //{

        //    int personID = -1;
        //    string User = "";

        //    if (UserDataAccess.FindByUserID(id, ref User, ref personID))
        //        return new User(id, personID, User);

        //    return null;
        //}

        public static User Find(string username, string password)
        {
            int id = -1;
            int personID = -1;
            string activeStatus = null;
            int roleID = -1;


            if (UserDataAccess.FindByUsernameAndPassword(ref id, username, password, ref roleID, ref personID, ref activeStatus))
            {
                return new User(id, personID, username, password, Role.Find(roleID).Name, activeStatus);
            }

            return null;
        }


        public static User Find(int id)
        {

            int personID = -1;
            string activeStatus = null;
            int roleID = -1;
            string username = null;
            string password = null;


            if (UserDataAccess.FindByUserID(id, ref username, ref password, ref roleID, ref personID, ref activeStatus))
            {
                return new User(id, personID, username, password, Role.Find(roleID).Name, activeStatus);
            }

            return null;
        }


        internal static User Find(string username)
        {

            int personID = -1;
            string activeStatus = null;
            int roleID = -1;
            int id = -1;
            string password = null;
            int attemptsFail = -1;

            if (UserDataAccess.FindByUsername(ref id,  username, ref password, ref roleID, ref personID, ref activeStatus, ref attemptsFail))
            {
                return new User(id, personID, username, password, Role.Find(roleID).Name, activeStatus, attemptsFail);
            }

            return null;
        }

        public static User FindByPersonID(int personID) 
        {
            int userID = -1;
            string activeStatus = null;
            int roleID = -1;
            string username = null;
            string password = null;


            if (UserDataAccess.FindByPerson(ref userID, ref username, ref password, ref roleID,  personID, ref activeStatus))
            {
                return new User(userID, personID, username, password, Role.Find(roleID).Name, activeStatus);
            }

            return null;
        }


        internal bool IsActive() 
        {
            return IsActive(Username);
        }
        internal static bool IsActive(string username) 
        {
            return UserDataAccess.IsActiveUserFoundByUserID(username);
        }

        public static DataTable GetAllUsers()
        {
            return UserDataAccess.GetAllUsers();
        }

        public static DataTable GetUsersWithNoHistory()
        {
            return UserDataAccess.GetUsersWithNoHistory();
        }


        public static bool IsExistWithNoHistoryUser(int id) 
        {
            return UserDataAccess.IsExistWithNoHistoryUser(id);
        }

        private bool _AddNewUser()
        {
            return (_id = UserDataAccess.AddNewUser(Username, Password, Role.Find(UserRole).ID, PersonID,
                ActiveStatus == "نشط" ? true : false)) > 0;
        }

        private bool _UpdateUser()
        {
            return UserDataAccess.UpdateUser(ID, Password, Role.Find(UserRole).ID,
                 ActiveStatus == "نشط" ? true : false, FailAttempts);
        }

        public bool Save()
        {
            switch (_mode)
            {
                case enStatusMode.AddMode:
                    if (_AddNewUser())
                    {
                        _mode = enStatusMode.UpdateMode;
                        return true;
                    }
                    return false;

                case enStatusMode.UpdateMode:
                    
                    if (!IsActive() && FailAttempts == 0)
                        ActiveStatus = "نشط";

                    if (FailAttempts == 3)
                        ActiveStatus = "غير نشط";
                    
                    return _UpdateUser();

                default:
                    return false;
            }
        }


        public static bool IsExist(int id)
        {
            return UserDataAccess.IsExist(id);
        }

        public static bool IsExist(string User)
        {
            return UserDataAccess.IsExist(User);
        }

        public static bool IsExistByPersonID(int id)
        {
            return UserDataAccess.IsExistByPersonID(id);
        }



        public static DataTable GetUsers()
        {
            return UserDataAccess.GetAllUsers();
        }


        public static bool DeleteUser(int id)
        {
            return UserDataAccess.DeleteUser(id);
        }

        //public static bool DeleteUser(string User)
        //{
        //    return UserDataAccess.DeleteUser(User);
        //}

        //public static bool DeleteUserByPersonID(int personID)
        //{
        //    return UserDataAccess.DeleteUserByPersonID(personID);
        //}
    }
}
