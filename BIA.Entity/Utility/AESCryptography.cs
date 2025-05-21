using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace BIA.Entity.Utility
{
    public class AESCryptography
    {
        public static string Encrypt(string plainText)
        {
            byte[] encrypted;
            const string aes_key = "Lh98YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=";
            const string aes_iv = "vFdnWolsAyO7kCfWuyrnqg==";

            using (Aes aes = Aes.Create())
            {
                aes.Key = Convert.FromBase64String(aes_key);
                aes.IV = Convert.FromBase64String(aes_iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform enc = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, enc, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        encrypted = ms.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public static string Decrypt(string encryptedText)
        {
            string decrypted = null;
            byte[] cipher = Convert.FromBase64String(encryptedText);
            const string aes_key = "Lh98YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=";
            const string aes_iv = "vFdnWolsAyO7kCfWuyrnqg==";
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(aes_key);
                    aes.IV = Convert.FromBase64String(aes_iv);
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(cipher))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                decrypted = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                decrypted = "InvalidSessionToken";
                return decrypted;
            }

            return decrypted;
        }


        public static byte[] Decrypts(byte[] cipherBytes)
        {
            byte[] decrypted;
            const string aes_key = "Lh98YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=";
            const string aes_iv = "vFdnWolsAyO7kCfWuyrnqg==";

            using (Aes aes = Aes.Create())
            {
                // Set AES Key and IV
                aes.Key = Convert.FromBase64String(aes_key);
                aes.IV = Convert.FromBase64String(aes_iv);
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                // Create AES Decryptor
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (MemoryStream plainTextStream = new MemoryStream())
                        {
                            // Read the decrypted bytes into the plainTextStream
                            cs.CopyTo(plainTextStream);
                            decrypted = plainTextStream.ToArray();
                        }
                    }
                }
            }

            // Return the decrypted byte array
            return decrypted;
        }

        public static string DecryptAES_FP(byte[] cipher)
        {
            string decrypted = null;
            //byte[] cipher = Convert.FromBase64String(encryptedText);
            const string aes_key = "Lh98YwuIn1zxt3FPWTZFlAa14EHdPAdN9FaZ9RQWihc=";
            const string aes_iv = "vFdnWolsAyO7kCfWuyrnqg==";
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Convert.FromBase64String(aes_key);
                    aes.IV = Convert.FromBase64String(aes_iv);
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform dec = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream(cipher))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, dec, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                decrypted = sr.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                decrypted = "InvalidSessionToken";
                return decrypted;
            }

            return decrypted;
        }       

        public static bool Verify(string encryptedString, string plainText)
        {
            // Check arguments.
            if (encryptedString == null || encryptedString.Length <= 0)
                throw new ArgumentNullException("encryptedString");

            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            var a = Decrypt(encryptedString);
            return string.Equals(Decrypt(encryptedString), plainText);
        }

        static byte[] EncryptStringToBytes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (AesManaged rijAlg = new AesManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = rijAlg.CreateEncryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an RijndaelManaged object
            // with the specified key and IV.
            using (AesManaged rijAlg = new AesManaged())
            {
                rijAlg.Key = Key;
                rijAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;

        }
    }
}
