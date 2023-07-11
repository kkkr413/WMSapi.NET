using System.Text;
using System.Security.Cryptography;

namespace WMSapi.Service
{
    public class STSHA512
    {
        public string HashPassword(string pw)
        {
            using (SHA512 sha512 = SHA512.Create())
            {
                byte[] hashedBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(pw));
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < hashedBytes.Length; i++)
                {
                    sb.Append(hashedBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

    }
}
