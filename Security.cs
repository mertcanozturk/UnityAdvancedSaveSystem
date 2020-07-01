using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Security : MonoBehaviour
{
    private const int Keysize = 256;

    private const int DerivationIterations = 1000;

    private static string password = "xqpsowlckf";

    static public void SaveString(string key, string value)
    {
        string encryptedValue = Encrypt(value);
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public string LoadString(string key)
    {
        string decryptedValue = Decrypt(PlayerPrefs.GetString(key));
        return decryptedValue;
    }

    static public void SaveBool(string key, bool value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public bool LoadBool(string key)
    {
        bool decryptedValue = Convert.ToBoolean(Decrypt(PlayerPrefs.GetString(key)));
        return decryptedValue;
    }

    static public void SaveFloat(string key, float value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public float LoadFloat(string key)
    {
        float decryptedValue = Convert.ToSingle(Decrypt(PlayerPrefs.GetString(key)));

        return decryptedValue;
    }

    static public void SaveInteger(string key, int value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public int LoadInteger(string key)
    {
        int decryptedValue = Convert.ToInt32(Decrypt(PlayerPrefs.GetString(key)));
        return decryptedValue;
    }

    static public void SaveJson(T obj, string fileName, string filePath = "")
    {
        if (filePath == "")
            filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        string json = JsonUtility.ToJson(obj);

        string encryptedJson = Encrypt(json);

        System.IO.File.WriteAllText(filePath, encryptedJson);
    }

    static public T LoadJson(string fileName, string filePath = "")
    {
        if (filePath == "")
            filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        else
            filePath = System.IO.Path.Combine(filePath, fileName);
        T obj = JsonUtility.FromJson(Decrypt(File.ReadAllText(filePath)));

        return obj;
    }

    static private string Encrypt(string plainText, string passPhrase = "")
    {
        if (passPhrase == "")
            passPhrase = password;

        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        // so that the same Salt and IV values can be used when decrypting.  
        var saltStringBytes = Generate256BitsOfRandomEntropy();
        var ivStringBytes = Generate256BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }
    }

    static private string Decrypt(string cipherText, string passPhrase = "")
    {
        if (passPhrase == "")
            passPhrase = password;

        // Get the complete stream of bytes that represent:
        // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }
    }

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }
}using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Security : MonoBehaviour
{
    private const int Keysize = 256;

    private const int DerivationIterations = 1000;

    private static string password = "xqpsowlckf";

    static public void SaveString(string key, string value)
    {
        string encryptedValue = Encrypt(value);
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public string LoadString(string key)
    {
        string decryptedValue = Decrypt(PlayerPrefs.GetString(key));
        return decryptedValue;
    }

    static public void SaveBool(string key, bool value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public bool LoadBool(string key)
    {
        bool decryptedValue = Convert.ToBoolean(Decrypt(PlayerPrefs.GetString(key)));
        return decryptedValue;
    }

    static public void SaveFloat(string key, float value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public float LoadFloat(string key)
    {
        float decryptedValue = Convert.ToSingle(Decrypt(PlayerPrefs.GetString(key)));

        return decryptedValue;
    }

    static public void SaveInteger(string key, int value)
    {
        string encryptedValue = Encrypt(value.ToString());
        PlayerPrefs.SetString(key, encryptedValue);
    }

    static public int LoadInteger(string key)
    {
        int decryptedValue = Convert.ToInt32(Decrypt(PlayerPrefs.GetString(key)));
        return decryptedValue;
    }

    static public void SaveJson(T obj, string fileName, string filePath = "")
    {
        if (filePath == "")
            filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        string json = JsonUtility.ToJson(obj);

        string encryptedJson = Encrypt(json);

        System.IO.File.WriteAllText(filePath, encryptedJson);
    }

    static public T LoadJson(string fileName, string filePath = "")
    {
        if (filePath == "")
            filePath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);
        else
            filePath = System.IO.Path.Combine(filePath, fileName);
        T obj = JsonUtility.FromJson(Decrypt(File.ReadAllText(filePath)));

        return obj;
    }

    static private string Encrypt(string plainText, string passPhrase = "")
    {
        if (passPhrase == "")
            passPhrase = password;

        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        // so that the same Salt and IV values can be used when decrypting.  
        var saltStringBytes = Generate256BitsOfRandomEntropy();
        var ivStringBytes = Generate256BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }
    }

    static private string Decrypt(string cipherText, string passPhrase = "")
    {
        if (passPhrase == "")
            passPhrase = password;

        // Get the complete stream of bytes that represent:
        // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 256;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();
                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }
    }

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }
}