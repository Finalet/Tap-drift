using System;
using System.Text;

namespace MoodkieSecurity
{
	public class Obfuscator
	{
		// Obfuscates or Unobfuscates a byte array using XOR obfuscation.
		public static byte[] Obfuscate(byte[] data, string key)
		{
			byte[] keyBytes = GetBytes(key);
			
			for (int i = 0; i < data.Length; i++)
				data[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
			
			return data;
		}
		
		static byte[] GetBytes(string str)
		{
			byte[] bytes = new byte[str.Length * sizeof(char)];
			System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
			return bytes;
		}
	}
}
