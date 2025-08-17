using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace StackOverflowService.Helpers
{
    public class PasswordHelper
    {
        // SHA algoritam za hesovanje lozinki 
        // koriscen salt

        public static string HashPassword(string password, string salt)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // dodajemo salt pre hashovanja
                var combined = password + salt;
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(combined));
                return Convert.ToBase64String(bytes);
            }
        }

        public static bool VerifyPassword(string enteredPassword, string storedHash, string salt)
        {
            string hashOfEntered = HashPassword(enteredPassword, salt);
            return storedHash == hashOfEntered;
        }
    }
}