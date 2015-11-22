using System;
using System.Text;
using System.Security.Cryptography;

namespace DarkRift.LoginPlugin
{
	/// <summary>
	/// 	Security helper for the login plugin.
	/// </summary>
	public class SecurityHelper
	{
		static byte[] GetMD5Hash(string inputString)
		{
			HashAlgorithm algorithm = MD5.Create();
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		static byte[] GetSHA1Hash(string inputString)
		{
			HashAlgorithm algorithm = SHA1.Create();
			return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
		}

		/// <summary>
		/// 	Gets a hash of the input string through the specified algorithm.
		/// </summary>
		/// <returns>The hashed string.</returns>
		/// <param name="inputString">The input string.</param>
		/// <param name="hashType">The hash algorithm to use.</param>
		public static string GetHash(string inputString, HashType hashType)
		{
			byte[] hash = new byte[0];

			switch (hashType)
			{
			case HashType.MD5:
				hash = GetMD5Hash(inputString);
				break;

			case HashType.SHA1:
				hash = GetSHA1Hash(inputString);
				break;
			}

			StringBuilder sb = new StringBuilder();
			foreach (byte b in hash)
				sb.Append(b.ToString("X2"));
			
			return sb.ToString();
		}
	}
}

