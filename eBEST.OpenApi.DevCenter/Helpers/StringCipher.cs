using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace App.Helpers;

// https://stackoverflow.com/questions/10168240/encrypting-decrypting-a-string-in-c-sharp
public static class StringCipher
{
    // This constant is used to determine the keysize of the encryption algorithm in bits.
    // We divide this by 8 within the code below to get the equivalent number of bytes.
    private const int Keysize = 256;
    private const int SaltCount = (Keysize / 8);
    private const int IvCount = (SaltCount / 2);


    // This constant determines the number of iterations for the password bytes generation function.
    private const int DerivationIterations = 1000;

    public static string Encrypt(string plainText, string passPhrase = "passPhrase")
    {
        try
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = RandomNumberGenerator.GetBytes(32);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA256);
            var keyBytes = password.GetBytes(SaltCount);
            var ivStringBytes = password.GetBytes(IvCount);
            using var symmetricKey = Aes.Create();
            using var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
            cryptoStream.FlushFinalBlock();
            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
            var cipherTextBytes = saltStringBytes;
            cipherTextBytes = [.. cipherTextBytes, .. ivStringBytes];
            cipherTextBytes = [.. cipherTextBytes, .. memoryStream.ToArray()];
            memoryStream.Close();
            cryptoStream.Close();
            return Convert.ToBase64String(cipherTextBytes);
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }

    public static string Decrypt(string cipherText, string passPhrase = "passPhrase")
    {
        try
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv[..SaltCount];
            // Get the IV bytes by extracting the next 16 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv[SaltCount..(SaltCount + IvCount)];
            // Get the actual cipher text bytes by removing the first 48 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv[(SaltCount + IvCount)..];

            using var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations, HashAlgorithmName.SHA256);
            var keyBytes = password.GetBytes(SaltCount);
            using var symmetricKey = Aes.Create();
            using var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes);
            using var memoryStream = new MemoryStream(cipherTextBytes);
            using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            using var streamReader = new StreamReader(cryptoStream, Encoding.UTF8);
            return streamReader.ReadToEnd();
        }
        catch (Exception)
        {
            return string.Empty;
        }
    }
}
