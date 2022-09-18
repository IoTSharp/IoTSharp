using System.Security.Cryptography;
using System.Text;

namespace Microsoft.AspNetCore.Identity
{
    public static class IdentityExtension
    {

        /// Hashes an email with MD5.  Suitable for use with Gravatar profile
        /// image urls
        public static string Gravatar(this IdentityUser user)
        {
            string email = user.Email;
            // Create a new instance of the MD5CryptoServiceProvider object.  
            var _SHA512 =  SHA512.Create();

            // Convert the input string to a byte array and compute the hash.  
            byte[] data = _SHA512.ComputeHash(Encoding.Default.GetBytes(email));

            // Create a new Stringbuilder to collect the bytes  
            // and create a string.  
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string.  
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return string.Format("https://www.gravatar.com/avatar/{0}", sBuilder.ToString()); ;  // Return the hexadecimal string. 
        }
    }
}