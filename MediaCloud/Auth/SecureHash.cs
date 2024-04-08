using System.Security.Cryptography;
using System.Text;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NLog;

namespace MediaCloud.WebApp
{
    /// <summary>
    /// Hashing and hash verifying for authentification.
    /// </summary>
    public static class SecureHash
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;

        /// <summary>
        /// Creates MD5 hash from string.
        /// </summary>
        /// <param name="data">The data string.</param>
        /// <returns>The MD5 hash.</returns>
        public static string HashMD5(string data)
        {
            byte[] hash = Encoding.ASCII.GetBytes(data);
            MD5 md5 = MD5.Create();
            byte[] hashenc = md5.ComputeHash(hash);
            string result = "";
            foreach (var b in hashenc)
            {
                result += b.ToString("x2");
            }

            return result.ToUpper();
        }

        /// <summary>
        /// Creates a hash from a password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="iterations">Number of iterations.</param>
        /// <returns>The hash.</returns>
        public static string Hash(string password, int iterations)
        {
            byte[] salt;
            string refreshToken;

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt = new byte[SaltSize]);
                refreshToken = Convert.ToBase64String(salt = new byte[SaltSize]);
            }

            //new RNGCryptoServiceProvider().GetBytes(salt = new byte[SaltSize]);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            var hash = pbkdf2.GetBytes(HashSize);

            var hashBytes = new byte[SaltSize + HashSize];
            Array.Copy(salt, 0, hashBytes, 0, SaltSize);
            Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

            return $"h5KPDjrv89{iterations}${Convert.ToBase64String(hashBytes)}";
        }

        /// <summary>
        /// Creates a hash from a password with 10000 iterations.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The hash.</returns>
        public static string Hash(string password)
        {
            return Hash(password, 10000);
        }

        /// <summary>
        /// Checks if hash is supported.
        /// </summary>
        /// <param name="hashString">The hash.</param>
        /// <returns>Is supported?</returns>
        public static bool IsHashSupported(string hashString)
        {
            return hashString.Contains("h5KPDjrv89");
        }

        /// <summary>
        /// Verifies a password against a hash.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="hashedPassword">The hash.</param>
        /// <returns>Could be verified?</returns>
        public static bool Verify(string password, string hashedPassword)
        {
            try 
            {
                return InternalVerify(password, hashedPassword);
            } 
            catch (Exception ex)
            {
                LogManager.GetLogger("SecureHash").Error(ex, "Could not verify password");
                return false;
            }
        }

        private static bool InternalVerify(string password, string hashedPassword)
        {   
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new NullReferenceException("Password cannot be null or empty");
            }

            if (!IsHashSupported(hashedPassword))
            {
                throw new NotSupportedException("The hashtype is not supported");
            }

            var splittedHashString = hashedPassword.Replace("h5KPDjrv89", "").Split('$');
            var iterations = int.Parse(splittedHashString[0]);
            var base64Hash = splittedHashString[1];

            var hashBytes = Convert.FromBase64String(base64Hash);

            var salt = new byte[SaltSize];
            Array.Copy(hashBytes, 0, salt, 0, SaltSize);

            var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations);
            byte[] hash = pbkdf2.GetBytes(HashSize);

            for (var i = 0; i < HashSize; i++)
            {
                if (hashBytes[i + SaltSize] != hash[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
