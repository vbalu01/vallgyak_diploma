using System.Security.Cryptography;
using System.Text;

namespace AutoPortal.Libs
{
    public static class PasswordManager
    {
        public static string CreateSalt(int size)
        {

            var rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[size];
            rng.GetBytes(buff);
            return Convert.ToBase64String(buff);
        }
        public static string GenerateHash(string input, string salt = "")
        {
            if (String.IsNullOrEmpty(salt))
            {
                Random rnd = new();

                int saltSize = rnd.Next(10, 15);
                salt = CreateSalt(saltSize);
            }

            byte[] bytes = Encoding.UTF8.GetBytes(input + salt);
            var sHA256ManagedString = new SHA256Managed();
            byte[] hash = sHA256ManagedString.ComputeHash(bytes);
            return Convert.ToBase64String(hash) + "$" + salt;
        }
        public static bool AreEqual(string plainTextInput, string hashedInput)
        {
            string[] tomb = hashedInput.Split("$");
            string newHashedPin = GenerateHash(plainTextInput, tomb[1]);


            return newHashedPin.Equals(hashedInput);

        }
    }
}
