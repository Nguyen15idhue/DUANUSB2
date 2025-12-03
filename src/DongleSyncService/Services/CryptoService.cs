using System.Security.Cryptography;
using System.Text;
using Serilog;

namespace DongleSyncService.Services
{
    public class CryptoService
    {
        private const int KeySize = 256;
        private const int Iterations = 100000;

        /// <summary>
        /// Decrypt DLL file from USB dongle
        /// </summary>
        public byte[] DecryptDLL(string encryptedPath, string ivPath, string usbSerial)
        {
            try
            {
                Log.Information("Decrypting DLL from: {Path}", encryptedPath);

                // Read encrypted data
                var encryptedData = File.ReadAllBytes(encryptedPath);
                var iv = File.ReadAllBytes(ivPath);

                // Derive key from USB serial
                var key = DeriveKey(usbSerial);

                // Decrypt
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                var decryptedData = decryptor.TransformFinalBlock(encryptedData, 0, encryptedData.Length);

                Log.Information("DLL decrypted successfully. Size: {Size} bytes", decryptedData.Length);
                return decryptedData;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decrypt DLL");
                throw;
            }
        }

        /// <summary>
        /// Encrypt DLL file for USB dongle
        /// </summary>
        public (byte[] encrypted, byte[] iv) EncryptDLL(byte[] dllData, string usbSerial)
        {
            try
            {
                Log.Information("Encrypting DLL. Size: {Size} bytes", dllData.Length);

                // Derive key
                var key = DeriveKey(usbSerial);

                // Generate random IV
                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.GenerateIV();
                var iv = aes.IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Encrypt
                using var encryptor = aes.CreateEncryptor();
                var encrypted = encryptor.TransformFinalBlock(dllData, 0, dllData.Length);

                Log.Information("DLL encrypted successfully");
                return (encrypted, iv);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to encrypt DLL");
                throw;
            }
        }

        /// <summary>
        /// Derive encryption key from USB serial using PBKDF2
        /// </summary>
        private byte[] DeriveKey(string usbSerial)
        {
            // Master secret (hardcoded - would be obfuscated in production)
            const string masterSecret = "DongleSecretKey2025!@#$%^&*()";
            
            // Combine USB serial with master secret
            var password = $"{usbSerial}|{masterSecret}";
            
            // Use USB serial as salt
            var salt = Encoding.UTF8.GetBytes(usbSerial);
            
            // Derive key using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );
            
            return pbkdf2.GetBytes(KeySize / 8);
        }

        /// <summary>
        /// Encrypt bind key data
        /// </summary>
        public byte[] EncryptBindKey(string json, string machineId)
        {
            try
            {
                var data = Encoding.UTF8.GetBytes(json);
                var key = DeriveKey(machineId);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.GenerateIV();
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

                // Prepend IV to encrypted data
                var result = new byte[aes.IV.Length + encrypted.Length];
                Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
                Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

                return result;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to encrypt bind key");
                throw;
            }
        }

        /// <summary>
        /// Decrypt bind key data
        /// </summary>
        public string DecryptBindKey(byte[] encryptedData, string machineId)
        {
            try
            {
                var key = DeriveKey(machineId);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Extract IV from beginning
                var iv = new byte[aes.BlockSize / 8];
                Buffer.BlockCopy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;

                // Extract encrypted data
                var encrypted = new byte[encryptedData.Length - iv.Length];
                Buffer.BlockCopy(encryptedData, iv.Length, encrypted, 0, encrypted.Length);

                // Decrypt
                using var decryptor = aes.CreateDecryptor();
                var decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to decrypt bind key");
                throw;
            }
        }
    }
}