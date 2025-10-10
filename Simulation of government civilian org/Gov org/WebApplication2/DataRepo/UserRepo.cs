using System.Security.Cryptography.X509Certificates;
using WebApplication2.TestClasses;

namespace WebApplication2.DataRepo
{
    public class UserRepo
    {

        static List<User> users = new List<User>{ new User { Username = "user1", Password = "123", Role = "مدير النظام"} ,
            new User { Username = "user2", Password = "1234", Role = "مستخدم النظام"},
            new User { Username = "user3", Password = "123", Role = "شرطي مرور"} };


        public static User? Login(string username, string password) 
        {
            foreach (var user in users) 
            {
                if (user.Username == username && user.Password == password) 
                {
                    return user;    
                }
            }

            return null;
        }
    }
}
