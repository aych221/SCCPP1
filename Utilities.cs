using System.Security.Cryptography;
using System.Text;

namespace SCCPP1
{
    public static class Utilities
    {

        public static string ToSHA256Hash(string input)
        {
            StringBuilder s = new StringBuilder();

            using (SHA256 hash = SHA256.Create())
            {
                byte[] result = hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                foreach (byte b in result)
                    s.Append(b.ToString("x2"));
            }
            return s.ToString();
        }

    }
}
