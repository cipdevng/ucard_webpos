using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Libraries.Banks {
    public class TripleDES {
        public static byte[] encrypt(byte[] toEncryptArray, byte[] keyArray) {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.None;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray =
              cTransform.TransformFinalBlock(toEncryptArray, 0,
              toEncryptArray.Length);
            tdes.Clear();
            return resultArray;
        }

        public static byte[] decrypt(byte[] toDecryptArray, byte[] keyArray) {
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.ECB;
            tdes.Padding = PaddingMode.None;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(
                                 toDecryptArray, 0, toDecryptArray.Length);
            tdes.Clear();
            return resultArray;
        }

        public static byte[] encryptDes(byte[] data, byte[] key) {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = key;
            des.Mode = CipherMode.ECB;
            ICryptoTransform transform = des.CreateEncryptor();
            byte[] resultArray =
              transform.TransformFinalBlock(data, 0,
              data.Length);
            des.Clear();
            return resultArray;

        }
    }
}
