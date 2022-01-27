using mTIM.Models.D;

namespace mTIM.Models
{
	public class UUID : System.IDisposable
	{
		public UUID()
        {
        }

		~UUID()
        {

        }
		public static int[] littleEndianByteOrder = { 3, 2, 1, 0, 5, 4, 7, 6, 8, 9, 10, 11, 12, 13, 14, 15 };

		public UUID EMPTY = new UUID();

		public class SimpleSerializationAnnotation
		{
		}

		public static UUID Create()
		{
			UUID uuid = new UUID();
			uuid.Generate();
			return uuid;
		}

		public void Generate()
		{
			Random.RandomSeed();
			char[] newData = Arrays.InitializeWithDefaultInstances<char>(16);
			int[] s32Data = InitializeWithDefaultInstances(newData);
			for (int i = 0; i < 4; i++)
			{
				s32Data[i] = Random.Int();
			}
			newData[6] = (char)(0x40 | (newData[6] & 0xf));
			newData[8] = (char)(0x80 | (newData[8] & 0x3f));
			Init(newData, false);
		}

		public static int[] InitializeWithDefaultInstances(char[] data)
		{
			int[] array = new int[data.Length];
			for (int i = 0; i < data.Length; i++)
			{
				array[i] = data[i];
			}
			return array;
		}

		public void Init(char[] _data, bool littleEndian)
		{
			if (data != _data)
			{
				if (littleEndian)
				{
					//Platform.MemCopy(data, _data, 16);
				}
				else
				{
					for (int i = 0; i < 16; i++)
					{
						data[i] = _data[littleEndianByteOrder[i]];
					}
				}

				hash = SimpleHash.Simple32(data, 16);
			}
			//CreateDebugString();
		}


		//ORIGINAL LINE: u32 CalcHash() const
		public int CalcHash()
		{
			return hash;
		}

		public char[] GetData()
		{
			return data;
		}

        public void Dispose()
        {
            
        }

        private char[] data = Arrays.InitializeWithDefaultInstances<char>(16);
		private int hash = default(int);
#if DEBUG
		private string debugString = new string(new char[40]);
#endif
	}

	public partial class Random
	{
		float RAND_MAX = 0x7fffffff;

		public void Seed(int seed)
		{
			RandomNumbers.Seed(seed);
		}
		public static void RandomSeed()
		{
			RandomNumbers.Seed((int)System.DateTime.Now.Millisecond);
		}
		public float Float()
		{
			return ((float)RandomNumbers.NextNumber()) / (float)RAND_MAX;
		}
		public float FloatSymmetrical()
		{
			return (Float() - 0.5f) * 2.0f;
		}
		public double Double()
		{
			return ((double)RandomNumbers.NextNumber()) / (double)RAND_MAX;
		}
		public double DoubleSymmetrical()
		{
			return (Double() - 0.5) * 2.0;
		}
		public static int Int()
		{
			return RandomNumbers.NextNumber();
		}
		public int Int(int max)
		{
			return (Int() % max);
		}
		public int Int(int min, int max)
		{
			return (Int() % (max - min)) + min;
		}
	}
}

internal static class RandomNumbers
{
	private static System.Random r;

	public static int NextNumber()
	{
		if (r == null)
			Seed();

		return r.Next();
	}

	public static int NextNumber(int ceiling)
	{
		if (r == null)
			Seed();

		return r.Next(ceiling);
	}

	public static void Seed()
	{
		r = new System.Random();
	}

	public static void Seed(int seed)
	{
		r = new System.Random(seed);
	}
}
