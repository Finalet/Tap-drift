#if !NETFX_CORE

using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace MoodkieSecurity
{
    public class AESEncryptor
    {
        private string fPassword;
        private AESBits fEncryptionBits;
        private byte[] fSalt = new byte[] { 0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4 };

        /// <summary>
        /// Initialize new AESEncryptor.
        /// </summary>
        /// <param name="password">The password to use for encryption/decryption.</param>
        /// <param name="encryptionBits">Encryption bits (128,192,256).</param>
        public AESEncryptor(string password, AESBits encryptionBits)
        {
            fPassword = password;
            fEncryptionBits = encryptionBits;
        }
        
        /// <summary>
        /// Initialize new AESEncryptor.
        /// </summary>
        /// <param name="password">The password to use for encryption/decryption.</param>
        /// <param name="encryptionBits">Encryption bits (128,192,256).</param>
        /// <param name="salt">Salt bytes. Bytes length must be 15.</param>
        public AESEncryptor(string password, AESBits encryptionBits, byte[] salt)
        {
            fPassword = password;
            fEncryptionBits = encryptionBits;
            fSalt = salt;
        }

        private byte[] Encrypt(byte[] data, byte[] key, byte[] iV)
        {
#if DISABLE_ENCRYPTION
			return data;
#else
            MemoryStream ms = new MemoryStream();

            RijndaelManaged alg = new RijndaelManaged();
            alg.Key = key;
            alg.IV = iV;
            CryptoStream cs = new CryptoStream(ms, alg.CreateEncryptor(), CryptoStreamMode.Write);

            cs.Write(data, 0, data.Length);
            cs.Dispose();
            byte[] encryptedData = ms.ToArray();
            return encryptedData;
#endif
        }

        /// <summary>
        /// Encrypt byte array with AES algorithm.
        /// </summary>
        /// <param name="data">Bytes to encrypt.</param>
        public byte[] Encrypt(byte[] data)
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(fPassword,  fSalt, 50);
            switch (fEncryptionBits)
            {
                case AESBits.BITS128:
                    return Encrypt(data, pdb.GetBytes(16), pdb.GetBytes(16));

                case AESBits.BITS192:
                    return Encrypt(data, pdb.GetBytes(24), pdb.GetBytes(16));

                case AESBits.BITS256:
                    return Encrypt(data, pdb.GetBytes(32), pdb.GetBytes(16));
            }
            return null;
        }

        private byte[] Decrypt(byte[] data, byte[] key, byte[] iv)
        {
#if DISABLE_ENCRYPTION
			return data;
#else
            MemoryStream ms = new MemoryStream();
            //Aes alg = Aes.Create();
            RijndaelManaged alg = new RijndaelManaged();
            alg.Key = key;
            alg.IV = iv;
            CryptoStream cs = new CryptoStream(ms, alg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Dispose();
            byte[] decryptedData = ms.ToArray();
            return decryptedData;
#endif
        }

        /// <summary>
        /// Decrypt byte array with AES algorithm.
        /// </summary>
        /// <param name="data">Encrypted byte array.</param>
        public byte[] Decrypt(byte[] data)
        {
            Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(fPassword, fSalt, 50);

            switch (fEncryptionBits)
            {
                case AESBits.BITS128:
                    return Decrypt(data, pdb.GetBytes(16), pdb.GetBytes(16));

                case AESBits.BITS192:
                    return Decrypt(data, pdb.GetBytes(24), pdb.GetBytes(16));

                case AESBits.BITS256:
                    return Decrypt(data, pdb.GetBytes(32), pdb.GetBytes(16));
            }
            return null;
        }

        /// <summary>
        /// Encryption/Decryption password.
        /// </summary>
        public string Password
        {
            get { return fPassword; }
            set { fPassword = value; }
        }

        /// <summary>
        /// Encryption/Decryption bits.
        /// </summary>
        public AESBits EncryptionBits
        {
            get { return fEncryptionBits; }
            set { fEncryptionBits = value; }
        }

        /// <summary>
        /// Salt bytes (bytes length must be 15).
        /// </summary>
        public byte[] Salt
        {
            get { return fSalt; }
            set { fSalt = value; }
        }
    }

}

#else

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;

namespace MoodkieSecurity
{
    public class AESEncryptor
    {
        private string fPassword;
        private AESBits fEncryptionBits;
        private byte[] fSalt = new byte[] { 0x00, 0x01, 0x02, 0x1C, 0x1D, 0x1E, 0x03, 0x04, 0x05, 0x0F, 0x20, 0x21, 0xAD, 0xAF, 0xA4 };

        /// <summary>
        /// Initialize new AESEncryptor.
        /// </summary>
        /// <param name="password">The password to use for encryption/decryption.</param>
        /// <param name="encryptionBits">Encryption bits (128,192,256).</param>
        public AESEncryptor(string password, AESBits encryptionBits)
        {
            fPassword = password;
            fEncryptionBits = encryptionBits;
        }

        /// <summary>
        /// Initialize new AESEncryptor.
        /// </summary>
        /// <param name="password">The password to use for encryption/decryption.</param>
        /// <param name="encryptionBits">Encryption bits (128,192,256).</param>
        /// <param name="salt">Salt bytes. Bytes length must be 15.</param>
        public AESEncryptor(string password, AESBits encryptionBits, byte[] salt)
        {
            fPassword = password;
            fEncryptionBits = encryptionBits;
            fSalt = salt;
        }

        /// <summary>
        /// Encrypt byte array with AES algorithm.
        /// </summary>
        /// <param name="data">Bytes to encrypt.</param>
        public byte[] Encrypt(byte[] data)
        {
            return iEncrypt(data);
        }

        /// <summary>
        /// Decrypt byte array with AES algorithm.
        /// </summary>
        /// <param name="data">Encrypted byte array.</param>
        public byte[] Decrypt(byte[] data)
        {
            return iDecrypt(data);
        }

        public byte[] iEncrypt(byte[] data)
        {
#if DISABLE_ENCRYPTION
			return data;
#else
            IBuffer pwBuffer = CryptographicBuffer.ConvertStringToBinary(Password, BinaryStringEncoding.Utf8);
            IBuffer saltBuffer = CryptographicBuffer.CreateFromByteArray(Salt);
            IBuffer plainBuffer = CryptographicBuffer.CreateFromByteArray(data);

            // Derive key material for password size 32 bytes for AES256 algorithm
            KeyDerivationAlgorithmProvider keyDerivationProvider = Windows.Security.Cryptography.Core.KeyDerivationAlgorithmProvider.OpenAlgorithm("PBKDF2_SHA1");
            // using salt and 1000 iterations
            KeyDerivationParameters pbkdf2Parms = KeyDerivationParameters.BuildForPbkdf2(saltBuffer, 50);

            // create a key based on original key and derivation parmaters
            CryptographicKey keyOriginal = keyDerivationProvider.CreateKey(pwBuffer);
            IBuffer keyMaterial;
            if (EncryptionBits == AESBits.BITS128)
                keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 16);
            else if (EncryptionBits == AESBits.BITS192)
                keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 24);
            else // EncryptionBits == AESBits.BITS256
                keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 32);
            CryptographicKey derivedPwKey = keyDerivationProvider.CreateKey(pwBuffer);

            // derive buffer to be used for encryption salt from derived password key 
            IBuffer saltMaterial = CryptographicEngine.DeriveKeyMaterial(derivedPwKey, pbkdf2Parms, 16);

            // display the buffers – because KeyDerivationProvider always gets cleared after each use, they are very similar unforunately
            string keyMaterialString = CryptographicBuffer.EncodeToBase64String(keyMaterial);
            string saltMaterialString = CryptographicBuffer.EncodeToBase64String(saltMaterial);

            SymmetricKeyAlgorithmProvider symProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7");
            // create symmetric key from derived password key
            CryptographicKey symmKey = symProvider.CreateSymmetricKey(keyMaterial);

            // encrypt data buffer using symmetric key and derived salt material
            IBuffer resultBuffer = CryptographicEngine.Encrypt(symmKey, plainBuffer, saltMaterial);
            byte[] result;
            CryptographicBuffer.CopyToByteArray(resultBuffer, out result);

            return result;
#endif
        }

        public byte[] iDecrypt(byte[] encryptedData)
        {
#if DISABLE_ENCRYPTION
			return encryptedData;
#else
            IBuffer pwBuffer = CryptographicBuffer.ConvertStringToBinary(Password, BinaryStringEncoding.Utf8);
            IBuffer saltBuffer = CryptographicBuffer.CreateFromByteArray(Salt);
            IBuffer cipherBuffer = CryptographicBuffer.CreateFromByteArray(encryptedData);

            // Derive key material for password size 32 bytes for AES256 algorithm
            KeyDerivationAlgorithmProvider keyDerivationProvider = Windows.Security.Cryptography.Core.KeyDerivationAlgorithmProvider.OpenAlgorithm("PBKDF2_SHA1");
            // using salt and 1000 iterations
            KeyDerivationParameters pbkdf2Parms = KeyDerivationParameters.BuildForPbkdf2(saltBuffer, 50);

            // create a key based on original key and derivation parmaters
            CryptographicKey keyOriginal = keyDerivationProvider.CreateKey(pwBuffer);
            IBuffer keyMaterial;
            if (EncryptionBits == AESBits.BITS128)
                keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 16);
            else if (EncryptionBits == AESBits.BITS192)
                keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 24);
            else // EncryptionBits == AESBits.BITS256
                keyMaterial = CryptographicEngine.DeriveKeyMaterial(keyOriginal, pbkdf2Parms, 32);
            CryptographicKey derivedPwKey = keyDerivationProvider.CreateKey(pwBuffer);

            // derive buffer to be used for encryption salt from derived password key 
            IBuffer saltMaterial = CryptographicEngine.DeriveKeyMaterial(derivedPwKey, pbkdf2Parms, 16);

            // display the keys – because KeyDerivationProvider always gets cleared after each use, they are very similar unforunately
            string keyMaterialString = CryptographicBuffer.EncodeToBase64String(keyMaterial);
            string saltMaterialString = CryptographicBuffer.EncodeToBase64String(saltMaterial);

            SymmetricKeyAlgorithmProvider symProvider = SymmetricKeyAlgorithmProvider.OpenAlgorithm("AES_CBC_PKCS7");
            // create symmetric key from derived password material
            CryptographicKey symmKey = symProvider.CreateSymmetricKey(keyMaterial);

            // encrypt data buffer using symmetric key and derived salt material
            IBuffer resultBuffer = CryptographicEngine.Decrypt(symmKey, cipherBuffer, saltMaterial);
            byte[] result;
            CryptographicBuffer.CopyToByteArray(resultBuffer, out result);

            return result;
	#endif
        }

        /// <summary>
        /// Encryption/Decryption password.
        /// </summary>
        public string Password
        {
            get { return fPassword; }
            set { fPassword = value; }
        }

        /// <summary>
        /// Encryption/Decryption bits.
        /// </summary>
        public AESBits EncryptionBits
        {
            get { return fEncryptionBits; }
            set { fEncryptionBits = value; }
        }

        /// <summary>
        /// Salt bytes (bytes length must be 15).
        /// </summary>
        public byte[] Salt
        {
            get { return fSalt; }
            set { fSalt = value; }
        }
    }

}
#endif