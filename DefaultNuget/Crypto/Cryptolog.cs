using System.Text;
using System.Security.Cryptography;

namespace OneNugetToRuleThemAll.Crypto
{
    public class Cryptolog : ICryptolog
    {
        private readonly string protectorWord = "MyCryptoWord";

        public string Encrypt(string data)
        {
            byte[] inputArray = Encoding.UTF8.GetBytes(data);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(protectorWord);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = aes.CreateEncryptor();
                byte[] resultArray = transform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                aes.Clear();
                return Convert.ToBase64String(resultArray, 0, resultArray.Length);
            }
        }

        public string Decrypt(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception(token);
            }

            string decryptedToken = string.Empty;
            byte[] inputArray = Convert.FromBase64String(token);
            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(protectorWord);
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.PKCS7;
                ICryptoTransform transform = aes.CreateDecryptor();
                byte[] resultArray = transform.TransformFinalBlock(inputArray, 0, inputArray.Length);
                aes.Clear();
                decryptedToken = Encoding.UTF8.GetString(resultArray);
            }

            if (string.IsNullOrEmpty(decryptedToken))
            {
                throw new Exception(decryptedToken);
            }

            return decryptedToken;
        }
    }
}