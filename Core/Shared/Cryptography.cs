using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Core.Shared {
    public class Cryptography {
        public class Hash {
            public static string getHash(string rawData) {
                using (SHA256 sha256Hash = SHA256.Create()) {
                    // ComputeHash - returns byte array  
                    byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
                    // Convert byte array to a string   
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++) {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
            public static string MD5Checksum(string str) {
                string hash;
                using (MD5 md5 = MD5.Create()) {
                    hash = BitConverter.ToString(
                      md5.ComputeHash(Encoding.UTF8.GetBytes(str))
                    ).Replace("-", String.Empty);
                }
                return hash;
            }
        }
        public class AES : IDisposable {
            private readonly CipherMode mode;
            private readonly PaddingMode padding;
            private readonly int keySize;
            private readonly int blockSize;
            private Aes? AesObj;
            public AES(int blockSize = 128, int keySize = 128, CipherMode mode = CipherMode.CBC, PaddingMode padding = PaddingMode.PKCS7) {
                this.blockSize = blockSize;
                this.keySize = keySize;
                this.padding = padding;
                this.mode = mode;
            }
            
            private void initAes(byte[] keyBytes, byte[] ivbytes) {
                Dispose();
                AesObj = Aes.Create();
                AesObj.KeySize = keySize;
                AesObj.BlockSize = blockSize;
                AesObj.IV = ivbytes;
                AesObj.Key = keyBytes;
                AesObj.Mode = mode;
                AesObj.Padding = padding;
            }
            public void setKeys(string key, string? IV = null) {
                IV = string.IsNullOrEmpty(IV) ? key : IV;
                int keyByteSize = this.keySize / 8;
                var keyBytes = new byte[keyByteSize];
                var IVBytes = new byte[keyByteSize];
                var secretKeyBytes = Encoding.UTF8.GetBytes(key);
                var IVBkeyytes = Encoding.UTF8.GetBytes(IV);
                Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
                Array.Copy(IVBkeyytes, keyBytes, Math.Min(IVBkeyytes.Length, IVBytes.Length));
                initAes(secretKeyBytes, IVBkeyytes);
            }
            public void setKeys(byte[] key, byte[] IV = null) {
                int keyByteSize = this.keySize / 8;
                if (IV == null) {
                    IV = new byte[keyByteSize];
                }
                initAes(key, IV);
            }
            private byte[] encrypt(byte[] data) {
                if (AesObj is null)
                    throw new Exception("Key is missing");
                byte[]? encrypted = null;
                ICryptoTransform encryptor = AesObj.CreateEncryptor(AesObj.Key, AesObj.IV);
                using (MemoryStream mstream = new MemoryStream()) {
                    using (CryptoStream csstream = new CryptoStream(mstream, encryptor, CryptoStreamMode.Write)) {
                        using (StreamWriter swriter = new StreamWriter(csstream)) {
                            var str = Encoding.Default.GetString(data);
                            swriter.Write(str);
                        }
                        encrypted = mstream.ToArray();
                    }
                }
                return encrypted;
            }
            private string decrypt(byte[] data) {
                if (AesObj is null)
                    throw new Exception("Key is missing");
                string? decrypted = null;
                ICryptoTransform decryptor = AesObj.CreateDecryptor(AesObj.Key, AesObj.IV);
                using (MemoryStream mstream = new MemoryStream(data)) {
                    using (CryptoStream csstream = new CryptoStream(mstream, decryptor, CryptoStreamMode.Read)) {
                        using (StreamReader sreader = new StreamReader(csstream)) {
                            decrypted = sreader.ReadToEnd();
                        }
                    }
                }
                return decrypted;
            }
            public string? encrypt(string plainText) {
                try {
                    var plainBytes = Encoding.UTF8.GetBytes(plainText);
                    var t = encrypt(plainBytes);
                    return Convert.ToBase64String(t);
                } catch {
                    return null;
                }
            }

            public string? encrypt<T>(T obj) where T : class {
                try {
                    string plainText = JObject.FromObject(obj).ToString();
                    return encrypt(plainText);
                } catch {
                    return null;
                }
            }
            public string? decrypt(string encryptedText) {
                try {
                    var encryptedBytes = Convert.FromBase64String(encryptedText);
                    var result = decrypt(encryptedBytes);
                    return result;
                } catch {
                    return null;
                }
            }
            public void Dispose() {
                try {
                    AesObj?.Dispose();
                } catch { }
            }
        }
        public class CharGenerator {
            private static readonly List<string> templates = new List<string> { "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ", "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", "1234567890", "1234567890abcdef", "ABCDEFGHIJKLMNOPQRSTUVWXYZ", "abcdefghijklmnopqrstuvwxyz" };
            public static string genID(int counts, characterSet type = 0) {
                char[] generated = new char[counts];
                char[] characters = templates[(int)type].ToCharArray();
                var random = new Random();
                int sampleLength = characters.Length - 1;
                for (int i = 0; i < counts; i++) {
                    int index = random.Next(0, sampleLength);
                    generated[i] = characters[index];
                }
                return new string(generated);
            }

            public static string genID() {
                Guid id = Guid.NewGuid();
                return id.ToString();
            }

            public enum characterSet {
                NUMERIC = 2,
                ALPHA_NUMERIC_NON_CASE = 0,
                ALPHA_NUMERIC_CASE = 1,
                HEX_STRING = 3,
                GUID = 6,
                UPPER_ALPHABETS_ONLY = 4,
                LOWER_ALPHABETS_ONLY = 5
            }
        }
    }
}
