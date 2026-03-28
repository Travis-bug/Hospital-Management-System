using System.Security.Cryptography;
using System.Text;

namespace Hospital_Management_System.Utilities
{
    public static class SecureIdGenerator
    {
        // Removed vowels and look-alike characters (like 1, l, I, 0, O) to make IDs easy to read!
        private static readonly char[] chars = "ABCDEFGHJKMNPQUSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789".ToCharArray();

        public static string GenerateID(int length = 15, string prefix = "")
        {
            var data = new byte[length];
            using (var crypto = RandomNumberGenerator.Create())
            {
                crypto.GetBytes(data);
            }
            
            var result = new StringBuilder(length);
            foreach (var b in data)
            {
                result.Append(chars[b % chars.Length]);
            }

            // If they provided a prefix, attach it with an underscore!
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                return $"{prefix}_{result.ToString()}";
            }

            // Otherwise, just return the random string
            return result.ToString();
        }
    }
}