namespace Backoffice.Application.Common.Interfaces;

/// <summary>
/// Service for encrypting and decrypting sensitive data
/// </summary>
public interface ICryptographyService
{
    /// <summary>
    /// Encrypts a string value
    /// </summary>
    /// <param name="plainText">The text to encrypt</param>
    /// <returns>The encrypted text</returns>
    string Encrypt(string plainText);
    
    /// <summary>
    /// Decrypts an encrypted string value
    /// </summary>
    /// <param name="cipherText">The encrypted text</param>
    /// <returns>The decrypted text</returns>
    string Decrypt(string cipherText);
    
    /// <summary>
    /// Generates a secure random string (for keys, etc.)
    /// </summary>
    /// <param name="length">Length of the string to generate</param>
    /// <returns>A random string</returns>
    string GenerateRandomKey(int length = 32);
    
    /// <summary>
    /// Computes a hash of the provided input
    /// </summary>
    /// <param name="input">The input to hash</param>
    /// <returns>The computed hash</returns>
    string ComputeHash(string input);
}