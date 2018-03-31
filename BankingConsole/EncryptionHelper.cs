using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace BankingConsole
{
    public static class EncryptionHelper
    {
        public static string EncryptString(string input)
        {
            // Convert the input string to a byte array and compute the hash.            
            // Return the hexadecimal string.            
            return GetMd5Hash(input);
            
        }

        public static string GetMd5Hash(string input)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sBuilder = new StringBuilder();
                // format each byte as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }
                return sBuilder.ToString();
            }
            
        }

        public static bool VerifyMd5Hash(string input, string hash)
        {
             // Hash the input.
            string hashOfInput = GetMd5Hash(input);
            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;
            if (0 == comparer.Compare(hashOfInput, hash))
            {
                return true;
            }
            else
            {
                return false;
            }            
        }
    }
}
