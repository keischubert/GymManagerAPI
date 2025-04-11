using System.Security.Cryptography;

namespace GymManagerAPI.Services
{
    public class PasswordHasher
    {
        // Configurable parameters for PBKDF2
        private const int SaltSize = 16; // 128-bit salt
        private const int HashSize = 32; // 256-bit hash
        private const int Iterations = 100000; // The higher, the more secure but slower
        // Creates a PBKDF2 hash and salt for the given password.
        // Parameters:
        // - password: The plaintext password provided by the user.
        // - passwordHash: An output parameter that will hold the PBKDF2 hash of the password.
        // - passwordSalt: An output parameter that will hold the randomly generated salt.
        public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            // Initialize a byte array to hold the salt with the specified SaltSize
            passwordSalt = new byte[SaltSize];
            // Create an instance of a cryptographically secure random number generator
            // RandomNumberGenerator.Create() provides a way to generate secure random bytes
            using (var rng = RandomNumberGenerator.Create())
            {
                // Fill the passwordSalt array with cryptographically strong random bytes
                rng.GetBytes(passwordSalt);
                // At this point, passwordSalt contains a unique salt for this password
            }
            // Create an instance of Rfc2898DeriveBytes to perform the PBKDF2 hashing
            // Parameters:
            // - password: The plaintext password to hash
            // - passwordSalt: The unique salt generated above
            // - Iterations: The number of iterations to perform (increases computational difficulty)
            // - HashAlgorithmName.SHA256: Specifies that SHA256 is used as the underlying hash algorithm
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, passwordSalt, Iterations, HashAlgorithmName.SHA256))
            {
                // Derive the hash bytes from the password and salt
                // The GetBytes method returns a byte array with the length specified by HashSize
                passwordHash = pbkdf2.GetBytes(HashSize);
                // At this point, passwordHash contains the derived key (hash) for the password
            }
            // After execution:
            // - passwordSalt contains the unique salt used for hashing
            // - passwordHash contains the derived hash of the password
            // These should be stored securely in the database for future password verifications
        }
        // Verifies the given password against the stored hash and salt.
        // Parameters:
        // - password: The plaintext password to be verified.
        // - storedHash: The hash previously generated and stored in the database.
        // - storedSalt: The salt used to generate the original hash.
        // Returns:
        // - A boolean indicating whether the password matches the stored hash and salt.
        public static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            // Derive a hash from the provided password and stored salt
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, storedSalt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] computedHash = pbkdf2.GetBytes(HashSize);
                // Compare the computed hash with the stored hash byte-by-byte
                return CryptographicOperations.FixedTimeEquals(computedHash, storedHash);
            }
        }
    }
}
