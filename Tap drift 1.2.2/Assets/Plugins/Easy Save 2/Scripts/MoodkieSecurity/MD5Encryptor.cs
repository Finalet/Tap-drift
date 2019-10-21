using System;
using System.Text;
using System.Security.Cryptography;

namespace MoodkieSecurity
{
    public class MD5Encryptor
    {
        public MD5Encryptor()
        {
        }
    
        public string GetMD5(byte[] data)
        {
			return MD5Core.GetHashString(data);
        }

        public string GetMD5(string data)
        {
            return MD5Core.GetHashString(data);
        }
    }
}