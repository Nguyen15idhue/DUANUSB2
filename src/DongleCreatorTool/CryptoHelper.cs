using System.Security.Cryptography;
using System.Text;

namespace DongleCreatorTool
{
    public class CryptoHelper
    {
        private const int KeySize = 256;
        private const int Iterations = 100000;

        public static (byte[] encrypted, byte[] iv) EncryptDLL(byte[] dllData, string usbSerial)
        {
            try
            {
                var key = DeriveKey(usbSerial);

                using var aes = Aes.Create();
                aes.KeySize = KeySize;
                aes.Key = key;
                aes.GenerateIV();
                var iv = aes.IV;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var encryptor = aes.CreateEncryptor();
                var encrypted = encryptor.TransformFinalBlock(dllData, 0, dllData.Length);

                return (encrypted, iv);
            }
            catch (Exception ex)
            {
                throw new Exception("Encryption failed", ex);
            }
        }

        private static byte[] DeriveKey(string usbSerial)
        {
            const string masterSecret = "DongleSecretKey2025!@#$%^&*()";
            var password = $"{usbSerial}|{masterSecret}";
            var salt = Encoding.UTF8.GetBytes(usbSerial);

            using var pbkdf2 = new Rfc2898DeriveBytes(
                password,
                salt,
                Iterations,
                HashAlgorithmName.SHA256
            );

            return pbkdf2.GetBytes(KeySize / 8);
        }
    }
}
