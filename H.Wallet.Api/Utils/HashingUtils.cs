using System.Security.Cryptography;
using System.Text;

namespace H.Wallet.Api.Utils;

public class HashingUtils
{
    public static void CreateHashAndSaltFor(string input, out byte[] hash, out byte[] salt)
    {
        using (HMACSHA512 hmac = new HMACSHA512())
        {
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        }
    }

    public static bool VerifyHashFor(string input, byte[] hash, byte[] salt)
    {
        using (HMACSHA512 hmac = new HMACSHA512(salt))
        {
            byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
            
            for(int i = 0; i < computedHash.Length; i++)
            {
                if(computedHash[i] != hash[i]) return false;
            }

            return true;
        }
    }
}