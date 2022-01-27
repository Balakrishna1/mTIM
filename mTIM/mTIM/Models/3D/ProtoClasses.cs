//using System;
//using System.Collections.Generic;
//using mTIM.Models.D;
//using ProtoBuf;
//using Urho;

//namespace mTIM.Models
//{
//	public class ProtoClasses
//	{
//		public ProtoClasses()
//		{
//		}
//	}

//	public class Result
//	{
//		ProtoLoader protoLoader;
//		public Result()
//		{
//		}

//		int fieldNumber;
//		int wireType;
//		public bool Read(ProtoLoader loader)
//		{
//			while (!loader.IsEof())
//			{
//				if (!loader.PeekField(ref fieldNumber, ref wireType))
//				{
//					return false;
//				}

//				switch (fieldNumber)
//				{
//					case 1:
//						{
//							Visualizable visualizeable = new Visualizable();
//							if (!loader.ReadType<IProtoType>(fieldNumber, visualizeable))
//							{
//								return false;
//							}
//							visualizables.Add(visualizeable);
//						}
//						break;
//					case 2:
//						{
//							GeometryData geometryData = new GeometryData();
//							if (!loader.ReadType<IProtoType>(fieldNumber, geometryData))
//							{
//								//PURPLE_DELETE(geometryData);
//								return false;
//							}
//							geometries.Add(geometryData);
//						}
//						break;
//					case 3:
//						{
//							XYZ xyz = new XYZ();
//							if (!loader.ReadType<IProtoType>(fieldNumber, xyz))
//							{
//								return false;
//							}
//							points.Add(new Vector3(xyz.X, xyz.Y, xyz.Z));
//						}
//						break;
//					default:
//						loader.SkipField(fieldNumber, wireType);
//						break;
//				};
//			}

//			return true;
//		}


//		public List<Visualizable> visualizables = new List<Visualizable>();
//		public List<GeometryData> geometries = new List<GeometryData>();
//		public List<Vector3> points = new List<Vector3>();
//	}

//	[ProtoContract]
//	[Serializable]
//	public class Guid : IProtoType
//	{
//		public UUID uuid = new UUID();
//		public bool Read(ProtoLoader loader)
//		{
//			char[] data = Arrays.InitializeWithDefaultInstances<char>(2);
//			if (!loader.ReadBit64(1, data[0]))
//			{
//				return false;
//			}
//			if (!loader.ReadBit64(2, data[1]))
//			{
//				return false;
//			}

//			uuid.Init(data, true);
//			//Log("UUID: %s\n", uuid.ToString().GetBuffer());

//			return true;
//		}
//	}

//	public class GeometryReference : IProtoType
//	{
//		public UInt32 geometryIndex = 0;
//		public Transform transform = new Transform();
//		public string colorTag = string.Empty;
//		public string layerTag = string.Empty;
//		public Guid guid = new Guid();
//		public UInt32 simplificationLevel = 0;

//		int fieldNumber;
//		int wireType;
//		public bool Read(ProtoLoader loader)
//		{
//			while (!loader.IsEof())
//			{
//				if (!loader.PeekField(ref fieldNumber, ref wireType))
//				{
//					return false;
//				}

//				switch (fieldNumber)
//				{
//					case 1:
//						loader.ReadU32(fieldNumber, geometryIndex);
//						break;
//					case 2:
//						{
//							if (!loader.ReadType<IProtoType>(fieldNumber, transform))
//							{
//								return false;
//							}
//						}
//						break;
//					case 3:
//						if (!loader.ReadUtf8String(fieldNumber,out colorTag))
//						{
//							return false;
//						}
//						break;
//					case 4:
//						if (!loader.ReadUtf8String(fieldNumber,out layerTag))
//						{
//							return false;
//						}
//						break;
//					case 5:
//						{
//							if (!loader.ReadType(fieldNumber, guid))
//							{
//								return false;
//							}
//						}
//						break;
//					case 6:
//						{
//							loader.ReadU32(fieldNumber, simplificationLevel);
//						};
//						break;
//					default:
//						{
//							loader.SkipField(fieldNumber, wireType);
//						}
//						break;
//				};
//			};

//			return true;
//		}

//	}

//	public class Transform : IProtoType
//	{
//		public Matrix4 matrix = Matrix4.Identity;

//		public bool Read(ProtoLoader loader)
//		{
//			int index = 0;
//			float[] val = new float[] { matrix.M11, matrix.M12, matrix.M13, matrix.M14, matrix.M21, matrix.M22, matrix.M23, matrix.M24, matrix.M31, matrix.M32, matrix.M33, matrix.M34, matrix.M41, matrix.M42, matrix.M43, matrix.M44 };

//			while (!loader.IsEof())
//			{
//				loader.ReadFloat(1, out val[index++]);
//			};


//			return true;
//		}

//	}


//	public class Visualizable: IProtoType
//	{
//		public int taskId;
//		public List<GeometryReference> references = new List<GeometryReference>();

//		int fieldNumber;
//		int wireType;
//		public bool Read(ProtoLoader loader)
//		{
//			while (!loader.IsEof())
//			{
//				if (!loader.PeekField(ref fieldNumber, ref wireType))
//				{
//					return false;
//				}

//				switch (fieldNumber)
//				{
//					case 1:
//						{
//							Guid guid = new Guid();
//							if (!loader.ReadType(fieldNumber, guid))
//							{
//								return false;
//							}
//							char[] data = guid.uuid.GetData();
//							int value = (int)data[0];
//							taskId = value;
//						}
//						break;
//					case 2:
//						{
//							GeometryReference geometryRef = new GeometryReference();
//							if (!loader.ReadType<IProtoType>(fieldNumber, geometryRef))
//							{
//								return false;
//							}
//							references.Add(geometryRef);
//						}
//						break;
//					default:
//						loader.SkipField(fieldNumber, wireType);
//						break;
//				}
//			};

//			return true;
//	}

//}

//	public class XYZ : IProtoType
//	{
//		public float X;
//		public float Y;
//		public float Z;

//		public bool Read(ProtoLoader loader)
//		{
//			if (!loader.ReadFloat(1, out X))
//			{
//				return false;
//			}
//			if (!loader.ReadFloat(2, out Y))
//			{
//				return false;
//			}
//			if (!loader.ReadFloat(3, out Z))
//			{
//				return false;
//			}

//			return true;
//		}
//	}

//	public class Triangle : IProtoType
//	{
//		public UInt32 A = 0;
//		public UInt32 B = 0;
//		public UInt32 C = 0;

//		public bool Read(ProtoLoader loader)
//		{
//			if (!loader.ReadU32(1, A))
//			{
//				return false;
//			}
//			if (!loader.ReadU32(2, B))
//			{
//				return false;
//			}
//			if (!loader.ReadU32(3, C))
//			{
//				return false;
//			}

//			return true;
//		}
//	}

//	public class Line: IProtoType
//	{
//		public UInt32 A = 0;
//		public UInt32 B = 0;

//		public bool Read(ProtoLoader loader)
//		{
//			if (!loader.ReadU32(1, A))
//			{
//				return false;
//			}
//			if (!loader.ReadU32(2, B))
//			{
//				return false;
//			}
//			return true;
//		}

//	}

//	public class TriangulatedGeometryData : IProtoType
//	{
//		public List<Triangle> triangles = new List<Triangle>();
//		public List<Line> lines = new List<Line>();

//		int fieldNumber;
//		int wireType;
//		public bool Read(ProtoLoader loader)
//		{
//			while (!loader.IsEof())
//			{
//				if (!loader.PeekField(ref fieldNumber, ref wireType))
//				{
//					return false;
//				}

//				switch (fieldNumber)
//				{
//					case 1:
//						{
//							Triangle triangle = new Triangle();
//							if (!loader.ReadType<IProtoType>(fieldNumber, triangle))
//							{
//								return false;
//							}

//							triangles.Add(triangle);
//						}
//						break;
//					case 2:
//						{
//							Line line = new Line();
//							if (!loader.ReadType<IProtoType>(fieldNumber, line))
//							{
//								return false;
//							}
//							lines.Add(line);
//						};
//						break;
//					default:
//						loader.SkipField(fieldNumber, wireType);
//						break;
//				};
//			};

//			return true;
//		}

//	}


//	public class GeometryData : IProtoType
//	{
//		public GeometryData()
//		{
//		}

//		public TriangulatedGeometryData triangulatedGeometryData = null;

//		public bool Read(ProtoLoader loader)
//		{
//			while (!loader.IsEof())
//			{
//				triangulatedGeometryData = new TriangulatedGeometryData();
//				if (!loader.ReadType<IProtoType>(1, triangulatedGeometryData))
//				{
//					triangulatedGeometryData = new TriangulatedGeometryData();
//					return false;
//				}
//			};

//			return true;
//		}

//	}

//	//public class Vector3 : IDisposable
//	//{
//	//	public Vector3 X_AXIS = new Vector3();
//	//	public Vector3 Y_AXIS = new Vector3();
//	//	public Vector3 Z_AXIS = new Vector3();
//	//	public Vector3 ONE = new Vector3();
//	//	public Vector3 ZERO = new Vector3();

//	//	public float X;
//	//	public float Y;
//	//	public float Z;

//	//	/// <summary>Creates a new empty vector.</summary>
//	//	public Vector3()
//	//	{
//	//		this.X = 0.0f;
//	//		this.Y = 0.0f;
//	//		this.Z = 0.0f;
//	//	}
//	//	/// <summary>Destroys the vector.</summary>
//	//	public void Dispose()
//	//	{
//	//	}

//	//	public static Vector3Data GetVector3Data(Vector3 ImpliedObject)
//	//	{
//	//		return new Vector3Data() { X = ImpliedObject.X, Y = ImpliedObject.Y, Z = ImpliedObject.Z };
//	//	}
//	//}
//}
