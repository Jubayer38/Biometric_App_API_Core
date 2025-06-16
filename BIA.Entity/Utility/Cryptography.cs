using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Security.Cryptography;
using System.Text;

namespace BIA.Entity.Utility
{
    /// <summary>
    /// User password cryptography
    /// </summary>
    public class Cryptography
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Encrypto(string value)
        {
            StringBuilder hash = new StringBuilder();
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                for (int i = 0; i < bytes.Length; i++)
                {
                    hash.Append(bytes[i].ToString("x2"));
                }
            }
            return hash.ToString();
        }

        public static string Encrypt(string toEncrypt, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toEncrypt);

            if (useHashing)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    keyArray = sha256.ComputeHash(UTF8Encoding.UTF8.GetBytes("bl_smart_pos"));
                }
            }
            else
                keyArray = UTF8Encoding.UTF8.GetBytes("bl_smart_pos");

            using (Aes tdes = Aes.Create())
            {
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.CBC;
                tdes.Padding = PaddingMode.PKCS7;
                tdes.GenerateIV();

                ICryptoTransform cTransform = tdes.CreateEncryptor();
                byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                // Prepend IV to ciphertext
                byte[] resultWithIv = new byte[tdes.IV.Length + resultArray.Length];
                Buffer.BlockCopy(tdes.IV, 0, resultWithIv, 0, tdes.IV.Length);
                Buffer.BlockCopy(resultArray, 0, resultWithIv, tdes.IV.Length, resultArray.Length);

                return Convert.ToBase64String(resultWithIv);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cipherString"></param>
        /// <param name="useHashing"></param>
        /// <returns></returns>
        public static string Decrypt(string cipherString, bool useHashing)
        {
            byte[] keyArray;
            byte[] toEncryptArray;
            try
            {
                toEncryptArray = Convert.FromBase64String(cipherString);
            }
            catch (Exception)
            {
                throw new Exception("Invalid security token");
            }

            if (useHashing)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    keyArray = sha256.ComputeHash(UTF8Encoding.UTF8.GetBytes("bl_smart_pos"));
                }
            }
            else
            {
                keyArray = UTF8Encoding.UTF8.GetBytes("bl_smart_pos");
            }

            using (Aes tdes = Aes.Create())
            {
                tdes.Key = keyArray;
                tdes.Mode = CipherMode.CBC;
                tdes.Padding = PaddingMode.PKCS7;

                // Extract IV from the beginning of the cipher text
                byte[] iv = new byte[tdes.BlockSize / 8];
                byte[] actualCipher = new byte[toEncryptArray.Length - iv.Length];
                Buffer.BlockCopy(toEncryptArray, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(toEncryptArray, iv.Length, actualCipher, 0, actualCipher.Length);

                tdes.IV = iv;

                byte[] resultArray;
                try
                {
                    ICryptoTransform cTransform = tdes.CreateDecryptor();
                    resultArray = cTransform.TransformFinalBlock(actualCipher, 0, actualCipher.Length);
                }
                catch (Exception)
                {
                    throw new Exception("Invalid security token");
                }

                return UTF8Encoding.UTF8.GetString(resultArray);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="encryptedValue"></param>
        /// <returns></returns>
        public static bool Verify(string stringValue, string encryptedValue)
        {
            // Hash the input.
            string hashOfInput = Encrypto(stringValue);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (0 == comparer.Compare(hashOfInput, encryptedValue))
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class clsCrypto
    {
        private string _KEY = string.Empty;
        protected internal string KEY
        {
            get
            {
                return _KEY;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _KEY = value;
                }
            }
        }

        private string _IV = string.Empty;
        protected internal string IV
        {
            get
            {
                return _IV;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _IV = value;
                }
            }
        }

        private string CalcMD5(string strInput)
        {
            string strOutput = string.Empty;
            if (!string.IsNullOrEmpty(strInput))
            {
                try
                {
                    StringBuilder strHex = new StringBuilder();
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] bytArText = Encoding.Default.GetBytes(strInput);
                        byte[] bytArHash = sha256.ComputeHash(bytArText);
                        for (int i = 0; i < bytArHash.Length; i++)
                        {
                            strHex.Append(bytArHash[i].ToString("X2"));
                        }
                        strOutput = strHex.ToString();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return strOutput;
        }

        private byte[] GetBytesFromHexString(string strInput)
        {
            byte[] bytArOutput = new byte[] { };
            if ((!string.IsNullOrEmpty(strInput)) && strInput.Length % 2 == 0)
            {
                SoapHexBinary hexBinary = null;
                try
                {
                    hexBinary = SoapHexBinary.Parse(strInput);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                bytArOutput = hexBinary.Value;
            }
            return bytArOutput;
        }

        private byte[] GenerateIV()
        {
            byte[] bytArOutput = new byte[] { };
            try
            {
                string strIV = CalcMD5(IV);
                bytArOutput = GetBytesFromHexString(strIV);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bytArOutput;
        }

        private byte[] GenerateKey()
        {
            byte[] bytArOutput = new byte[] { };
            try
            {
                string strKey = CalcMD5(KEY);
                bytArOutput = GetBytesFromHexString(strKey);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bytArOutput;
        }

        protected internal string Encrypt(string strInput, CipherMode cipherMode)
        {
            string strOutput = string.Empty;
            if (!string.IsNullOrEmpty(strInput))
            {
                try
                {
                    byte[] bytePlainText = Encoding.Default.GetBytes(strInput);
                    using (RijndaelManaged rijManaged = new RijndaelManaged())
                    {
                        rijManaged.Mode = cipherMode;
                        rijManaged.BlockSize = 128;
                        rijManaged.KeySize = 128;
                        rijManaged.IV = GenerateIV();
                        rijManaged.Key = GenerateKey();
                        rijManaged.Padding = PaddingMode.Zeros;
                        ICryptoTransform icpoTransform = rijManaged.CreateEncryptor(rijManaged.Key, rijManaged.IV);
                        using (MemoryStream memStream = new MemoryStream())
                        {
                            using (CryptoStream cpoStream = new CryptoStream(memStream, icpoTransform, CryptoStreamMode.Write))
                            {
                                cpoStream.Write(bytePlainText, 0, bytePlainText.Length);
                                cpoStream.FlushFinalBlock();
                            }
                            strOutput = Encoding.Default.GetString(memStream.ToArray());
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return strOutput;
        }

        protected internal string Decrypt(string strInput, CipherMode cipherMode)
        {
            string strOutput = string.Empty;
            if (!string.IsNullOrEmpty(strInput))
            {
                try
                {
                    byte[] byteCipherText = Encoding.Default.GetBytes(strInput);
                    byte[] byteBuffer = new byte[strInput.Length];
                    using (RijndaelManaged rijManaged = new RijndaelManaged())
                    {
                        rijManaged.Mode = cipherMode;
                        rijManaged.BlockSize = 128;
                        rijManaged.KeySize = 128;
                        rijManaged.IV = GenerateIV();
                        rijManaged.Key = GenerateKey();
                        rijManaged.Padding = PaddingMode.Zeros;
                        ICryptoTransform icpoTransform = rijManaged.CreateDecryptor(rijManaged.Key, rijManaged.IV);
                        using (MemoryStream memStream = new MemoryStream(byteCipherText))
                        {
                            using (CryptoStream cpoStream = new CryptoStream(memStream, icpoTransform, CryptoStreamMode.Read))
                            {
                                cpoStream.Read(byteBuffer, 0, byteBuffer.Length);
                            }
                            strOutput = Encoding.Default.GetString(byteBuffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return strOutput;
        }

    }
}
