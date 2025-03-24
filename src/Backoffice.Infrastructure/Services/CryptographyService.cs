using System.Security.Cryptography;
using System.Text;
using Backoffice.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Backoffice.Infrastructure.Services;

public class CryptographyService : ICryptographyService
{
    private readonly string _encryptionKey;
    private readonly string _encryptionIv;
    
    public CryptographyService(IConfiguration configuration)
    {
        // Get encryption key from configuration or generate one
        _encryptionKey = configuration["Encryption:Key"] ?? "a1B2c3D4e5F6g7H8i9J0k1L2m3N4o5P6";
        _encryptionIv = configuration["Encryption:IV"] ?? "q7R8s9T0u1V2w3X4";
        
        // Ensure key and IV are of correct length
        if (_encryptionKey.Length < 32)
        {
            _encryptionKey = _encryptionKey.PadRight(32, '!');
        }
        
        if (_encryptionIv.Length < 16)
        {
            _encryptionIv = _encryptionIv.PadRight(16, '!');
        }
    }
    
    public string Encrypt(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return plainText;
            
        byte[] encryptedBytes;
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        using (Aes aes = Aes.Create())
        {
            aes.Key = Encoding.UTF8.GetBytes(_encryptionKey)[..32]; // Using first 32 bytes
            aes.IV = Encoding.UTF8.GetBytes(_encryptionIv)[..16];   // Using first 16 bytes
            
            ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            
            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
            
            cs.Write(plainBytes, 0, plainBytes.Length);
            cs.FlushFinalBlock();
            
            encryptedBytes = ms.ToArray();
        }
        
        return Convert.ToBase64String(encryptedBytes);
    }
    
    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
            return cipherText;
            
        try
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] plainBytes;
            
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(_encryptionKey)[..32];
                aes.IV = Encoding.UTF8.GetBytes(_encryptionIv)[..16];
                
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                
                using MemoryStream ms = new(cipherBytes);
                using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
                using MemoryStream output = new();
                
                cs.CopyTo(output);
                plainBytes = output.ToArray();
            }
            
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch
        {
            // If decryption fails for any reason, return the original text
            return cipherText;
        }
    }
    
    public string GenerateRandomKey(int length = 32)
    {
        byte[] randomBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
    
    public string ComputeHash(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;
            
        using var sha256 = SHA256.Create();
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        byte[] hash = sha256.ComputeHash(bytes);
        
        return Convert.ToBase64String(hash);
    }
}