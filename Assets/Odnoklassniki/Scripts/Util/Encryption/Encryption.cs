using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

internal static class Encryption
{
	const int KeySize = 16;
	const string JsonTag = "{";

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
		using (RijndaelManaged rijAlg = new RijndaelManaged())
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
		{
			return null;
		}

		if (cipherText.Length % 16 != 0)
		{
			throw new FormatException("Encrypted length is not divided to 16 (" + cipherText.Length + ")");
		}

		if (Key == null || Key.Length <= 0)
			throw new ArgumentNullException("Key");
		if (IV == null || IV.Length <= 0)
			throw new ArgumentNullException("Key");

		try
		{
			// Declare the string used to hold
			// the decrypted text.
			string plaintext = null;

			// Create an RijndaelManaged object
			// with the specified key and IV.
			using (RijndaelManaged rijAlg = new RijndaelManaged())
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
		catch (Exception e)
		{
			Debug.LogError(e.ToString());
			return null;
		}
	}

	/// <summary>
	/// SHA1: Returns 20 bytes
	/// </summary>
	public static byte[] Hash(string strToEncrypt)
	{
		UTF8Encoding ue = new UTF8Encoding();

		byte[] bytes = ue.GetBytes(strToEncrypt);

		// encrypt bytes

		HashAlgorithm hash = new SHA1CryptoServiceProvider();

		byte[] hashBytes = hash.ComputeHash(bytes);

		return hashBytes;
	}

	public static int HashInt(string strToEncrypt)
	{
		var bytes = Hash(strToEncrypt);
		return bytes[0] | (bytes[1] << 8) | (bytes[2] << 16) | (bytes[3] << 24);
	}

	public static string HashString(string strToEncrypt)
	{
		byte[] hash = Hash(strToEncrypt);

		return Convert.ToBase64String(hash);
	}

	public static string Encrypt(string plain)
	{
		byte[] key = EncryptionKey();
		string result = Convert.ToBase64String(EncryptStringToBytes(plain, key, iv));

		ClearKey(key);
		return result;
	}

	static void ClearKey(byte[] key)
	{
		for (int i = 0; i < key.Length; i++)
		{
			key[i] = 0;
		}
	}

	public static string DecryptAuto(string encrypted)
	{
		if (encrypted.StartsWith("\"") && encrypted.EndsWith("\""))
		{
			encrypted = encrypted.Substring(1, encrypted.Length - 2);
		}

		byte[] key = EncryptionKey();
		byte[] fromBase64String = Convert.FromBase64String(encrypted);

		if (IsDecrypted(fromBase64String))
		{
			//Debug.Log("AutoDecryptJson base64->plain(" + encrypted + ")");
			return Encoding.UTF8.GetString(fromBase64String, 0, fromBase64String.Length);
		}

		//Debug.Log("DecryptStringFromBytes (" + encrypted + ")");
		string result = DecryptStringFromBytes(fromBase64String, key, iv);
		ClearKey(key);
		return result;
	}

	public static string DecryptPlain(byte[] bytearray)
	{
		byte[] key = EncryptionKey();
		string result = DecryptStringFromBytes(bytearray, key, iv);
		ClearKey(key);
		return result;
	}

	public static string AutoDecryptJson(string data)
	{
		//if (String.IsNullOrEmpty(data))
		//{
		//	return data;
		//}

//		if (!IsDecrypted(data))
//		{
//			data = DecryptAuto(data);
//		}
		return data;
	}

	public static string AutoDecryptJson(byte[] data)
	{
		return AutoDecryptJson(Encoding.UTF8.GetString(data, 0, data.Length));
	}

	public static bool IsDecrypted(string s)
	{
		return s.StartsWith(JsonTag);
	}

	public static bool IsDecrypted(byte[] s)
	{
		return s.Length == 0 ||
			   s.Length > 8 &&
			   s[0] == '/' &&
			   s[1] == '*' &&
			   s[2] == 'j' &&
			   s[3] == 's' &&
			   s[4] == 'o' &&
			   s[5] == 'n' &&
			   s[6] == '*' &&
			   s[7] == '/';
	}

	internal static Func<byte[]> EncryptionKey = GetAiRandom; // EncryptionKey
	internal static byte[] iv; // Encryption initial vector
	internal static byte[] DeviceIdHash;

	internal static byte[] GetAiRandom()
	{
		byte[] result = new byte[16];

		byte[] hash = DeviceIdHash;

		int state = 0;

		while (state != -16)
		{
			switch (state)
			{
				case 3: result[0] = (byte)((hash[17] ^ 0x5d) ^ state); state = 12; break;
				case 15: result[6] = (byte)((hash[1] ^ 0x39) ^ state); state = 1333; break;
				case 140: result[13] = (byte)((hash[14] - 0xb8) ^ state); state = 4; break;
				case 10: result[5] = (byte)((hash[10 - state] + 0x2B) ^ state); state = 3; break;
				case 4: result[9] = (byte)((hash[state] + 0x6e) ^ state); state = 5; break;
				case 0: result[7] = (byte)((hash[4] ^ 0x2a) ^ state); state = 123; break;
				case 2: result[2] = (byte)((hash[2] - 0x4c) ^ state); state = 11; break;
				case 13: result[10] = (byte)((hash[15] + 0xa4) ^ state); state = 13; break;
				case 1333: result[10] = (byte)((hash[15] + 0xa4) ^ state); state = 140; break;
				case 123: result[15] = (byte)((hash[12] + 0x9B) ^ state); state = 9; break;
				case 9: result[11] = (byte)((hash[8] ^ 0x21) ^ state); state = 8; break;
				case 11: result[12] = (byte)((hash[19] - 0xe2) ^ state); state = 5; break;
				case 1: result[4] = (byte)((hash[13] + 0x3B) ^ state); state = 10; break;
				case 6: result[14] = (byte)((hash[18] ^ 0x8a) ^ state); state = 1; break;
				case 8: result[1] = (byte)((hash[9] - 0x2c) ^ state); state = 2; break;
				case 5: result[8] = (byte)((hash[5 + state] - 0x7f) ^ state); state = 6; break;
				case 12: result[3] = (byte)((hash[7] ^ 0x23) ^ state); state = -16; break;
			}
		}

		return result;
	}

	static Encryption()
	{
		iv = new byte[] { 100, 221, 56, 37, 249, 255, 60, 199, 57, 0/*DeviceIdHash[8]*/, 233, 122, 123, 95, 95, 5 }; // Encryption initial vector
		DeviceIdHash = Hash(SystemInfo.deviceUniqueIdentifier);
	}

	/// <summary>
	/// Returns 16 bytes
	/// </summary>
	public static byte[] Md5(string plain)
	{
		UTF8Encoding ue = new UTF8Encoding();

		byte[] bytes = ue.GetBytes(plain);

		// encrypt bytes
		HashAlgorithm hash = new MD5CryptoServiceProvider();
		byte[] hashBytes = hash.ComputeHash(bytes);
		return hashBytes;
	}

	public static string Md5string(string plain)
	{
		byte[] hash = Md5(plain);

		StringBuilder sb = new StringBuilder();
		foreach (byte b in hash)
			sb.Append(b.ToString("x2"));
		return sb.ToString();
	}
}
