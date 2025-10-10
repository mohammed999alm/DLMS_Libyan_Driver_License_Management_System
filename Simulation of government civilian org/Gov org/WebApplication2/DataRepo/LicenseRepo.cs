using System.ComponentModel;
using WebApplication2.TestClasses;

namespace WebApplication2.DataRepo
{
    public class LicenseRepo
    {
        private static  List<TestClasses.License> licenses = new List<TestClasses.License> { 
            new TestClasses.License { ID = 1, Name = "Mohammed Almislaty", Type = "Class A(B)",
                IssueDate = new DateTime(2017, 10, 17) }
        ,  new TestClasses.License { ID = 2, Name = "Mohammed Ali", Type = "Class A(B)",
                IssueDate = new DateTime(2020, 10, 17) },

         new TestClasses.License { ID = 3, Name = "Alia Samir", Type = "Class A(B)",
                IssueDate = new DateTime(2018, 1, 19) }
        };


        public static TestClasses.License? Find(int licenseID)
        {
            foreach (var license in licenses)
            {
                if (license.ID == licenseID) return license;
            }

            return null;
        }
    }

}
