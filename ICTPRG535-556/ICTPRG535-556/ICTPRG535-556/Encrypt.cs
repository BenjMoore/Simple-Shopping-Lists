using BCrypt.Net;
namespace ICTPRG535_556.Encrypt
{
    

    public static class PasswordUtility
    {
        /// <summary>
        /// Hashes a password using bcrypt.
        /// </summary>
        /// <param name="password">The plaintext password to hash.</param>
        /// <returns>The hashed password.</returns>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("Password cannot be null or empty.");

            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Verifies a plaintext password against a hashed password.
        /// </summary>
        /// <param name="password">The plaintext password to verify.</param>
        /// <param name="hashedPassword">The hashed password to compare against.</param>
        /// <returns>True if the password matches; otherwise, false.</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hashedPassword))
                throw new ArgumentException("Password and hashed password cannot be null or empty.");

            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }

}



