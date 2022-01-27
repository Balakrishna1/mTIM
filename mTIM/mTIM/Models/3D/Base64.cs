using System;
using System.Collections.Generic;
using System.Text;

namespace mTIM.Models
{
    public class Base64
    {
        private static string base64_chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" + "abcdefghijklmnopqrstuvwxyz" + "0123456789+/";

        private static bool is_base64(char c)
        {
            return (char.IsLetterOrDigit(c) || (c == '+') || (c == '/'));
        }

		public static void Encode(string bytesToEncode, int length, ref StringBuilder stringBuilder)
		{
			int i = 0;
			int j = 0;
			byte[] char_array_3 = new byte[3];
			byte[] char_array_4 = new byte[4];

			while ((length--) != 0)
			{
				i++;
				//char_array_3[i++] = bytesToEncode++;
				if (i == 3)
				{
					char_array_4[0] = (byte)((char_array_3[0] & 0xfc) >> 2);
					char_array_4[1] = (byte)(((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4));
					char_array_4[2] = (byte)(((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6));
					char_array_4[3] = (byte)(char_array_3[2] & 0x3f);

					for (i = 0; (i < 4); i++)
					{
						stringBuilder.Append(base64_chars[char_array_4[i]]);
					}
					i = 0;
				}
			}

			if (i != 0)
			{
				for (j = i; j < 3; j++)
				{
					char_array_3[j] = (byte)'\0';
				}

				char_array_4[0] = (byte)((char_array_3[0] & 0xfc) >> 2);
				char_array_4[1] = (byte)(((char_array_3[0] & 0x03) << 4) + ((char_array_3[1] & 0xf0) >> 4));
				char_array_4[2] = (byte)(((char_array_3[1] & 0x0f) << 2) + ((char_array_3[2] & 0xc0) >> 6));
				char_array_4[3] = (byte)(char_array_3[2] & 0x3f);

				for (j = 0; (j < i + 1); j++)
				{
					stringBuilder.Append(base64_chars[char_array_4[j]]);
				}

				while ((i++ < 3))
				{
					stringBuilder.Append('=');
				}

			}
		}

		public static String Encode(String str)
		{
			StringBuilder builder = new StringBuilder();
			Encode(str, str.Length,ref builder);
			return builder.ToString();
		}

		public static String Encode(string bytesToEncode, int length)
		{
			StringBuilder builder = new StringBuilder();
			Encode(bytesToEncode, length,ref builder);
			return builder.ToString();
		}

		static int GetBaseValue(char base64Char)
        {
            switch (base64Char)
            {
                case 'A': return 0;
                case 'B': return 1;
                case 'C': return 2;
                case 'D': return 3;
                case 'E': return 4;
                case 'F': return 5;
                case 'G': return 6;
                case 'H': return 7;
                case 'I': return 8;
                case 'J': return 9;
                case 'K': return 10;
                case 'L': return 11;
                case 'M': return 12;
                case 'N': return 13;
                case 'O': return 14;
                case 'P': return 15;
                case 'Q': return 16;
                case 'R': return 17;
                case 'S': return 18;
                case 'T': return 19;
                case 'U': return 20;
                case 'V': return 21;
                case 'W': return 22;
                case 'X': return 23;
                case 'Y': return 24;
                case 'Z': return 25;
                case 'a': return 26;
                case 'b': return 27;
                case 'c': return 28;
                case 'd': return 29;
                case 'e': return 30;
                case 'f': return 31;
                case 'g': return 32;
                case 'h': return 33;
                case 'i': return 34;
                case 'j': return 35;
                case 'k': return 36;
                case 'l': return 37;
                case 'm': return 38;
                case 'n': return 39;
                case 'o': return 40;
                case 'p': return 41;
                case 'q': return 42;
                case 'r': return 43;
                case 's': return 44;
                case 't': return 45;
                case 'u': return 46;
                case 'v': return 47;
                case 'w': return 48;
                case 'x': return 49;
                case 'y': return 50;
                case 'z': return 51;
                case '0': return 52;
                case '1': return 53;
                case '2': return 54;
                case '3': return 55;
                case '4': return 56;
                case '5': return 57;
                case '6': return 58;
                case '7': return 59;
                case '8': return 60;
                case '9': return 61;
                case '+': return 62;
                case '/': return 63;
                default:
                    return -1;
            }
            return -1;
        }

		public static List<byte> Decode(string encodedString)
		{
			List<byte> outData = new List<byte>();
			//var stringChar = encodedString.ToCharArray();
			int in_len = encodedString.Length;
			int i = 0;
			int j = 0;
			int in_ = 0;
			char[] char_array_4 = new char[4];
			char[] char_array_3 = new char[3];

			while ((in_len--) != 0 && (encodedString[in_] != '=') && is_base64(encodedString[in_]))
			{
				char_array_4[i++] = encodedString[in_];
				in_++;
				if (i == 4)
				{
					for (i = 0; i < 4; i++)
					{
						char_array_4[i] = (char)GetBaseValue(char_array_4[i]);
					}

					char_array_3[0] = (char)((char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4));
					char_array_3[1] = (char)(((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2));
					char_array_3[2] = (char)(((char_array_4[2] & 0x3) << 6) + char_array_4[3]);

					for (i = 0; (i < 3); i++)
					{
						outData.Add((byte)char_array_3[i]);
					}
					i = 0;
				}
			}

			if (i != 0)
			{
				for (j = i; j < 4; j++)
				{
					char_array_4[j] = (char)0;
				}

				for (j = 0; j < i; j++)
				{
					char_array_4[j] = (char)GetBaseValue(char_array_4[j]);
				}

				char_array_3[0] = (char)((char_array_4[0] << 2) + ((char_array_4[1] & 0x30) >> 4));
				char_array_3[1] = (char)(((char_array_4[1] & 0xf) << 4) + ((char_array_4[2] & 0x3c) >> 2));
				char_array_3[2] = (char)(((char_array_4[2] & 0x3) << 6) + char_array_4[3]);

				for (j = 0; (j < i - 1); j++)
				{
					outData.Add((byte)char_array_3[j]);
				}
			}

			return outData;
		}

	}
}
