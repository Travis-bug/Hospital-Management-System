using System.Security.Cryptography;

namespace Hospital_Management_System.Utilities;


public static class SecureIdGenerator
{
    public static string GenerateID (int length = 12)
    {
        // Added your special characters here
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // removed 0, O, 1 and i to prevent human reading error

        return string.Create(length, chars, (buffer, alphabet) =>
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                // Using true entropy to pick from the expanded character set
                buffer[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];
            }
        });
    }
}