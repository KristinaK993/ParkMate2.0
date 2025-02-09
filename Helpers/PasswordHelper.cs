using System;
using System.Security.Cryptography;
using System.Text;

namespace ParkMate2._0.Helpers
{
    public static class PasswordHelper
    {
        //Hash password with SHA256
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        //Verifiy password by comparing it with the hashed version
        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            string hashedInput = HashPassword(inputPassword);  // Hash inputpassword
            return hashedInput == storedPassword;  // Comparing the hashed password with the stored hashvalue
        }

    }
}
