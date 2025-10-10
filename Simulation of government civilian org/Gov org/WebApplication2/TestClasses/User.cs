using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json.Serialization;

namespace WebApplication2.TestClasses
{
    public class User
    {
        public string Username { get; set; }    
        public string Password { get; set; }


        
        public string? Role { get; set; }    

    }
}
