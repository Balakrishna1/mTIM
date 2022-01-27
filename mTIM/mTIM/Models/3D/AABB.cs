using System;
using ProtoBuf;
using Urho;

namespace mTIM.Models.D
{
	public class AABB
	{
	    public Vector3 Minimum;
		public Vector3 Maximum;
		public AABB()
		{
		}
		public static AABB EMPTY = new AABB();

		public AABB(Vector3 position)
		{
			this.Minimum = position;
			this.Maximum = position;
		}
		public AABB(Vector3 _minimum, Vector3 _maximum)
		{
			this.Minimum = _minimum;
			this.Maximum = _maximum;
		}


		public AABB(Vector3[] positions, int positionCount)
		{
			Minimum = positions[0];
			Maximum = positions[0];
			for (int i = 1; i < positionCount; i++)
			{
				Grow(positions[i]);
			}
		}
		public AABB(AABB aabb)
		{
			this.Minimum = aabb.Minimum;
			this.Maximum = aabb.Maximum;
		}
		public bool IsEmpty()
		{
			return (float.IsNaN(Minimum.X));
		}
		public void GrowFast(Vector3 position)
		{
			if (position.X < Minimum.X)
			{
				Minimum.X = position.X;
			}
			if (position.Y < Minimum.Y)
			{
				Minimum.Y = position.Y;
			}
			if (position.Z < Minimum.Z)
			{
				Minimum.Z = position.Z;
			}

			if (position.X > Maximum.X)
			{
				Maximum.X = position.X;
			}
			if (position.Y > Maximum.Y)
			{
				Maximum.Y = position.Y;
			}
			if (position.Z > Maximum.Z)
			{
				Maximum.Z = position.Z;
			}
		}
		public void Grow(Vector3 position)
		{
			if (float.IsNaN(Minimum.X))
			{
				Minimum = position;
				Maximum = position;
			}
			else
			{
				GrowFast(position);
			}
		}
		public void Grow(AABB aabb)
		{
			if (aabb == null)
				return;
			if (float.IsNaN(Minimum.X))
			{
				Minimum = aabb.Minimum;
				Maximum = aabb.Maximum;
			}
			else
			{
				if (Smaller(aabb.Minimum.X, Minimum.X))
				{
					Minimum.X = aabb.Minimum.X;
				}
				if (Smaller(aabb.Minimum.Y, Minimum.Y))
				{
					Minimum.Y = aabb.Minimum.Y;
				}
				if (Smaller(aabb.Minimum.Z, Minimum.Z))
				{
					Minimum.Z = aabb.Minimum.Z;
				}

				if (Greater(aabb.Maximum.X, Maximum.X))
				{
					Maximum.X = aabb.Maximum.X;
				}
				if (Greater(aabb.Maximum.Y, Maximum.Y))
				{
					Maximum.Y = aabb.Maximum.Y;
				}
				if (Greater(aabb.Maximum.Z, Maximum.Z))
				{
					Maximum.Z = aabb.Maximum.Z;
				}
			}
		}
		public void Extend(Vector3 extendInAllDirections)
		{
			Minimum -= extendInAllDirections;
			Maximum += extendInAllDirections;
		}

		public float CalcVolume()
		{
			Vector3 dimension = GetDimension();
			return dimension.X * dimension.Y * dimension.Z;
		}

		private Vector3 GetDimension()
		{
			return Maximum - Minimum;
		}


	static bool Greater(float af, float bf, int maxDiff = 4)
		{
			//return (af > bf) && !Equals(af, bf, maxDiff);
			return Smaller(bf, af, maxDiff);
		}

		static bool Smaller(float af, float bf, int maxDiff = 4)
		{
			//return (af < bf) && !Equals(af, bf, maxDiff);
			int ai = Convert.ToInt32(af);
			int bi = Convert.ToInt32(bf);
			int testa = SIGNMASK(ai);
			int testb = SIGNMASK(bi);
			ai = (ai & 0x7fffffff) ^ testa;
			bi = (bi & 0x7fffffff) ^ testb;
			return ai + maxDiff < bi;
		}

		static int SIGNMASK(int i)
		{
			return ((int)(~(((int)(i))>> 31)-1));
		}
		public bool Intersects(AABB other)
		{
			if ((Minimum.X > other.Maximum.X) || (other.Minimum.X > Maximum.X))
			{
				return false;
			}

			if ((Minimum.Y > other.Maximum.Y) || (other.Minimum.Y > Maximum.Y))
			{
				return false;
			}

			if ((Minimum.Z > other.Maximum.Z) || (other.Minimum.Z > Maximum.Z))
			{
				return false;
			}

			return true;
		}
		public bool Contains(Vector3 p)
		{
			return (p.X >= Minimum.X && p.Y >= Minimum.Y && p.Z >= Minimum.Z && p.X <= Maximum.X && p.Y <= Maximum.Y && p.Z <= Maximum.Z);
		}
		public Vector3 CalcClosestPoint(Vector3 p)
		{
			float x = p.X;
			if (x < Minimum.X)
			{
				x = Minimum.X;
			}
			if (x > Maximum.X)
			{
				x = Maximum.X;
			}

			float y = p.Y;
			if (y < Minimum.Y)
			{
				y = Minimum.Y;
			}
			if (y > Maximum.Y)
			{
				y = Maximum.Y;
			}

			float z = p.Z;
			if (z < Minimum.Z)
			{
				z = Minimum.Z;
			}
			if (z > Maximum.Z)
			{
				z = Maximum.Z;
			}

			return new Vector3(x, y, z);
		}
	}
}
