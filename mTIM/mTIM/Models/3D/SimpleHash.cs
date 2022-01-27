using System;
namespace mTIM.Models
{
    public class SimpleHash
    {
		public static int Calc32(char memory, int length, int seed = 0)
		{
			return xHash.Calc32(memory, length, seed);
		}

		public static int Simple32(char[] memory, int length, int seed = 0)
		{
			int code = 0;
			int scramble = seed;
			for (int i = 0; i < length; i++)
			{
				scramble = ((scramble) + (code >> 8));
				code = (code << 8) + memory[i];
				code = (code ^ scramble);
			};
			return code;
		}

	}

	public class xHash
	{
		public static int Calc32(object memory, int len, int seed)
		{
			return 0;
				//new XXH32(memory, len, seed);
		}

	}
}
