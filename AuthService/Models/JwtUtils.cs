using System.Text;
using System.Security.Cryptography;

namespace AuthService.Models;

public class JwtUtils
{
    public static string Base64UrlEncode(byte[] input)
    {
        var base64 = Convert.ToBase64String(input);
        base64 = base64.Split('=')[0];
        base64 = base64.Replace('+', '-');
        base64 = base64.Replace('/', '_');
        return base64;
    }

    public static string GenerateSignature(string header, string payload, string secret)
    {
        var key = Encoding.UTF8.GetBytes(secret);
        var data = Encoding.UTF8.GetBytes(header + "." + payload);
        
        using (var hmac = new HMACSHA256(key))
        {
            var hash = hmac.ComputeHash(data);
            return Base64UrlEncode(hash);
        }
    }

    public static bool VerifySignature(string token, string secret)
    {
        var parts = token.Split('.');
        if (parts.Length != 3) return false;

        var header = parts[0];
        var payload = parts[1];
        var signature = parts[2];

        var expectedSignature = GenerateSignature(header, payload, secret);
        return expectedSignature == signature;
    }
}
