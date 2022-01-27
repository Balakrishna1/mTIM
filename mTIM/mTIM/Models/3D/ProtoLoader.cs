using System;
namespace mTIM.Models
{
    public class ProtoLoader 
	{
		char data;
        int end;
		public ProtoLoader(char _data, int _length)
		{
			this.data = _data;
			this.end = _data + _length;
		}

		public bool IsEof()
		{
			return data >= end;
		}

		public bool ReadType<T>(int expectedFieldNumber, T protoType) where T : IProtoType
		{

			if (!ExpectField(expectedFieldNumber, Type.LengthDelimited))
			{
				return false;
			}

			StringPart sp = new StringPart();
			if (!ReadStringPart(ref sp))
			{
				return false;
			}

			ProtoLoader newLoader = new ProtoLoader(Convert.ToChar(sp.GetBuffer()), (int)sp.Length());
			return protoType.Read(newLoader);
		}

		int fieldNumber;
		int wireType;
		public bool ExpectField(int expectedFieldNumber, Type expectedWireType)
        {
            if (!ReadField(ref fieldNumber, ref wireType))
            {
                return false;
            }

            //ASSERT(wireType == expectedWireType, "Invalid proto type!");
            //ASSERT(fieldNumber == expectedFieldNumber, "Invalid proto field number!");
            return (Type)wireType == expectedWireType && fieldNumber == expectedFieldNumber;
        }

		public bool ReadBit64(int expectedFieldNumber, long outData)
		{

			if (!ExpectField(expectedFieldNumber, Type.Bit64))
			{
				return false;
			}

			char @out = (char)outData;
			@out += data++;
		    @out += data++;
			@out += data++;
			@out += data++;
			@out += data++;
			@out += data++;
			@out += data++;
			@out += data++;

			return true;
		}


		public bool ReadStringPart(ref StringPart stringPart)
        {
            UInt64 val = new UInt64();
            if (!ReadVarint64(ref val))
            {
                return false;
            }
            stringPart = new StringPart(data.ToString(), (int)val);
            data += (char)val;
            return true;
        }

		public bool ReadVarint64(ref UInt64 outValue)
		{
			outValue = 0;
			uint b = (uint)data++;
			outValue = (b & 0x7F);
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += (b & 0x7F) << 7;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += (b & 0x7F) << 14;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += (b & 0x7F) << 21;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += ((UInt64)(b & 0x7F)) << 28;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += ((UInt64)(b & 0x7F)) << 35;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += ((UInt64)(b & 0x7F)) << 42;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += ((UInt64)(b & 0x7F)) << 49;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += ((UInt64)(b & 0x7F)) << 56;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += ((UInt64)(b & 0x7F)) << 63;
			if ((b & 0x80) == null)
			{
				return true;
			}
			return true;
		}

		public bool ReadVarint32(ref UInt32 outValue)
		{
			outValue = 0;
			uint b = (uint)data++;
			outValue = (b & 0x7F);
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += (b & 0x7F) << 7;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += (b & 0x7F) << 14;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += (b & 0x7F) << 21;
			if ((b & 0x80) == null)
			{
				return true;
			}
			b = (uint)data++;
			outValue += (b & 0x7F) << 28;
			if ((b & 0x80) == null)
			{
				return true;
			}
			return true;
		}


		public bool ReadField(ref int fieldNumber, ref int wireType)
		{
			if (PeekField(ref fieldNumber, ref wireType))
			{
				data++;
				return true;
			}
			return false;
		}

		public bool PeekField(ref int fieldNumber, ref int wireType)
		{
			if (data < end)
			{
				int key = data;
				wireType = key & 0x07;
				fieldNumber = key >> 3;
				return true;
			}
			//ASSERT(false, "Couldn't peek field!");
			return false;
		}

		public bool ReadFloat(int expectedFieldNumber, out float outValue)
		{
			if (!ExpectField(expectedFieldNumber, Type.Bit32))
			{
				outValue = 0;
				return false;
			}


			UInt32 u32Data = (UInt32)data; // bus error on Android
			outValue = (float)u32Data;
			data += char.MaxValue;
			return true;
		}

		public bool ReadDouble(int expectedFieldNumber, out double outValue)
		{
			if (!ExpectField(expectedFieldNumber, Type.Bit64))
			{
				outValue = 0;
				return false;
			}

			UInt64 u64Data = (UInt64)data; // bus error on Android
			outValue = (double)u64Data;
			data += Convert.ToChar(sizeof(char));
			return true;
		}

		public bool ReadU32(int expectedFieldNumber, UInt32 outValue)
		{
			if (!ExpectField(expectedFieldNumber, Type.Varint))
			{
				return false;
			}
			return ReadVarint32(ref outValue);
		}

		public void SkipField(int expectedFieldNumber, int wireType)
		{
			switch ((Type)wireType)
			{
				case Type.Varint:
					{
						UInt64 val = new UInt64();
						ReadVarint64(ref val);
						break;
					}
				case Type.Bit64:
					{
						double val;
						ReadDouble(expectedFieldNumber,out val);
						break;
					};
				case Type.LengthDelimited:
					{
						string str;
						ReadUtf8String(expectedFieldNumber, out str);
						break;
					};
				case Type.StartGroup:
					{
						//ASSERT(false, "Not implemented so far!");
						break;
					};
				case Type.EndGroup:
					{
						//ASSERT(false, "Not implemented so far!");
						break;
					};
				case Type.Bit32:
					{
						float val;
						ReadFloat(expectedFieldNumber, out val);
					}
					break;
				default:
					//ASSERT(false, "Not implemented so far!");

					break;
			}
		}

		public bool ReadUtf8String(int expectedFieldNumber, out string outString)
		{
			if (!ExpectField(expectedFieldNumber, Type.LengthDelimited))
			{
				outString = string.Empty;
				return false;
			}

			StringPart sp = new StringPart();
			if (!ReadStringPart(ref sp))
			{
				outString = string.Empty;
				return false;
			}

			outString = sp.GetBuffer();
			return true;
		}

        ~ProtoLoader()
        {

        }
    }

    public enum Type
    {
        Varint,
        Bit64,
        LengthDelimited,
        StartGroup,
        EndGroup,
        Bit32,
        Count
    }

	public interface IProtoType
	{
		bool Read(ProtoLoader loader);
	}

}
