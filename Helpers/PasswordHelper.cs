using System;
using System.Security.Cryptography;
using System.Text;

namespace ParkMate2._0.Helpers
{
    public static class PasswordHelper
    {
        //Hashar lösenord med SHA256
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        //Verifierar lösenordet genom att jämföra med den hashade versionen
        public static bool VerifyPassword(string inputPassword, string storedPassword)
        {
            return HashPassword(inputPassword) == storedPassword;
        }
    }
}
