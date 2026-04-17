using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace Hospital_Management_System.Utilities;

public static class IdentityTokenCodec
{
    public static string Encode(string token)
    {
        return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
    }

    public static string Decode(string encodedToken)
    {
        try
        {
            var bytes = WebEncoders.Base64UrlDecode(encodedToken);
            return Encoding.UTF8.GetString(bytes);
        }
        catch (Exception ex)
        {
            throw new ArgumentException("The onboarding token is invalid or malformed.", ex);
        }
    }
}
