using System;
using System.Collections.Generic;
using ProtoBuf;

namespace mTIM.Models.D
{
    [ProtoContract]
    [Serializable]
    [ProtoInclude(1, typeof(TriangulatedGeometryData))]
    public abstract class GeometryData : IEquatable<GeometryData>, IComparable<GeometryData>
    {
        public abstract bool Equals(GeometryData other);
        public abstract int CompareTo(GeometryData other);
    }

    [ProtoContract]
    [Serializable]
    public class Transform : IEquatable<Transform>, IComparable<Transform>
    {
        [ProtoMember(1, IsRequired = true)]
        public float[] Matrix { get; set; }

        //static IEqualityComparer _equalityComparer = ComparerExtensions.KeyEqualityComparer<Transform>.Using(_ => _.Matrix, ArrayComparer<float>.Instance);
        //static IComparer _comparer = ComparerExtensions.KeyComparer<Transform>.OrderBy(_ => _.Matrix);

        public override bool Equals(object obj)
        {
            return true;
                //_equalityComparer.Equals(this, obj);
        }

        bool IEquatable<Transform>.Equals(Transform other)
        {
            return true;
                //_equalityComparer.Equals(this, other);
        }

        public int CompareTo(Transform other)
        {
            return 0;
                //_comparer.Compare(this, other);
        }

        public override int GetHashCode()
        {
            return 0;
                //_equalityComparer.GetHashCode(this);
        }
    }

    [ProtoContract]
    [Serializable]
    public struct XYZ : IEquatable<XYZ>, IComparable<XYZ>
    {
        [ProtoMember(1, IsRequired = true)]
        public float X { get; set; }
        [ProtoMember(2, IsRequired = true)]
        public float Y { get; set; }
        [ProtoMember(3, IsRequired = true)]
        public float Z { get; set; }

        //static IEqualityComparer _equalityComparer = ComparerExtensions.KeyEqualityComparer<XYZ>.Using(_ => _.X).And(_ => _.Y).And(_ => _.Z);
        //static IComparer _comparer = ComparerExtensions.KeyComparer<XYZ>.OrderBy(_ => _.X).ThenBy(_ => _.Y).ThenBy(_ => _.Z).Untyped();

        public override bool Equals(object obj)
        {
            return true;
            //_equalityComparer.Equals(this, obj);
        }

        bool IEquatable<XYZ>.Equals(XYZ other)
        {
            return true;
            //_equalityComparer.Equals(this, other);
        }

        int IComparable<XYZ>.CompareTo(XYZ other)
        {
            return 0;
            //_comparer.Compare(this, other);
        }

        public override int GetHashCode()
        {
            return 0;
                //_equalityComparer.GetHashCode(this);
        }
    }

    [ProtoContract]
    [Serializable]
    public class Triangle : IEquatable<Triangle>, IComparable<Triangle>
    {
        [ProtoMember(1, IsRequired = true)]
        public uint A { get; set; }
        [ProtoMember(2, IsRequired = true)]
        public uint B { get; set; }
        [ProtoMember(3, IsRequired = true)]
        public uint C { get; set; }

        //static IEqualityComparer _equalityComparer = ComparerExtensions.KeyEqualityComparer<Triangle>.Using(_ => _.A).And(_ => _.B).And(_ => _.C);
        //static IComparer _comparer = ComparerExtensions.KeyComparer<Triangle>.OrderBy(_ => _.A).ThenBy(_ => _.B).ThenBy(_ => _.C).Untyped();

        public override bool Equals(object obj)
        {
            return true;
            //_equalityComparer.Equals(this, obj);
        }

        bool IEquatable<Triangle>.Equals(Triangle other)
        {
            return true;
            //_equalityComparer.Equals(this, other);
        }

        int IComparable<Triangle>.CompareTo(Triangle other)
        {
            return 0;
                //_comparer.Compare(this, other);
        }

        public override int GetHashCode()
        {
            return 0;
                //_equalityComparer.GetHashCode(this);
        }
    }

    [ProtoContract]
    [Serializable]
    public class Line : IEquatable<Line>, IComparable<Line>
    {
        [ProtoMember(1, IsRequired = true)]
        public uint A { get; set; }
        [ProtoMember(2, IsRequired = true)]
        public uint B { get; set; }

        //static IEqualityComparer _equalityComparer = ComparerExtensions.KeyEqualityComparer<Line>.Using(_ => _.A).And(_ => _.B);
        //static IComparer _comparer = ComparerExtensions.KeyComparer<Line>.OrderBy(_ => _.A).ThenBy(_ => _.B).Untyped();

        public override bool Equals(object obj)
        {
            return true;
                //_equalityComparer.Equals(this, obj);
        }

        bool IEquatable<Line>.Equals(Line other)
        {
            return true;
            //_equalityComparer.Equals(this, other);
        }

        int IComparable<Line>.CompareTo(Line other)
        {
            return 0;
                //_comparer.Compare(this, other);
        }

        public override int GetHashCode()
        {
            return 0;
                //_equalityComparer.GetHashCode(this);
        }
    }

    [ProtoContract]
    [Serializable]
    public class TriangulatedGeometryData : GeometryData, IEquatable<TriangulatedGeometryData>, IComparable<TriangulatedGeometryData>
    {
        [ProtoMember(1, IsRequired = true)]
        public Triangle[] Triangles { get; set; }
        [ProtoMember(2, IsRequired = false)]
        public Line[] Lines { get; set; }

        //static IEqualityComparer _equalityComparer = ComparerExtensions.KeyEqualityComparer<TriangulatedGeometryData>.Using(_ => _.Triangles, ArrayComparer<Triangle>.Instance).And(_ => _.Lines, ArrayComparer<Line>.Instance);
        //static IComparer _comparer = ComparerExtensions.KeyComparer<TriangulatedGeometryData>.OrderBy(_ => _.Triangles, ArrayComparer<Triangle>.Instance).ThenBy(_ => _.Lines, ArrayComparer<Line>.Instance).Untyped();

        public override bool Equals(object obj)
        {
            return true;
            //_equalityComparer.Equals(this, obj);
        }

        bool IEquatable<TriangulatedGeometryData>.Equals(TriangulatedGeometryData other)
        {
            return true;
            //_equalityComparer.Equals(this, other);
        }

        int IComparable<TriangulatedGeometryData>.CompareTo(TriangulatedGeometryData other)
        {
            return 0;
                //_comparer.Compare(this, other);
        }

        public override int GetHashCode()
        {
            return 0;
                //_equalityComparer.GetHashCode(this);
        }

        public override bool Equals(GeometryData other)
        {
            return true;
            //_equalityComparer.Equals(this, other);
        }

        public override int CompareTo(GeometryData other)
        {
            return 0;
                //_comparer.Compare(this, other);
        }
    }

    [ProtoContract]
    [Serializable]
    public class GeometryReference
    {
        /// <summary>
        /// Index into Result.Geometries
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public uint GeometryIndex { get; set; }
        /// <summary>
        /// A (optional) transform applied to the geometry.
        /// </summary>
        [ProtoMember(2)]
        public Transform Transform { get; set; }
        /// <summary>
        /// Defines how the geometry should be painted.
        /// </summary>
        [ProtoMember(3, IsRequired = true)]
        public string ColorTag { get; set; }
        /// <summary>
        /// In which Layer this Geometry is.
        /// (Concrete, Reinforcement, ...)
        /// </summary>
        [ProtoMember(4, IsRequired = true)]
        public string LayerTag { get; set; }

        /// <summary>
        /// An optional ObjectID, relevant for status-colors, etc...
        /// </summary>
        [ProtoMember(5, IsRequired = false)]
        public System.Guid? ObjectID { get; set; }

        /// <summary>
        /// An optional ObjectID, relevant for status-colors, etc...
        /// </summary>
        [ProtoMember(6, IsRequired = false)]
        public GraphicsType GraphicsType { get; set; }
    }

    public enum GraphicsType { Full, Simple }

    [ProtoContract]
    [Serializable]
    public class Visualizable
    {
        /// <summary>
        /// The ID of the Element, referenced from externally
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public System.Guid Id { get; set; }
        /// <summary>
        /// The Geometries which form this Visualizable.
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public GeometryReference[] Geometries { get; set; }
    }

    [ProtoContract]
    [Serializable]
    public class Result
    {
        /// <summary>
        /// All Precast Elements.
        /// </summary>
        [ProtoMember(1, IsRequired = true)]
        public Visualizable[] Elements { get; set; }
        /// <summary>
        /// The geometries of the Elements.
        /// </summary>
        [ProtoMember(2, IsRequired = true)]
        public GeometryData[] Geometries { get; set; }
        /// <summary>
        /// A Vertex List shared by all Geometries.
        /// </summary>
        [ProtoMember(3, IsRequired = true)]
        public XYZ[] PointsAndVectors { get; set; }


        public static string GetProtoDefinition()
        {
            return ProtoBuf.Serializer.GetProto<Result>();
        }
    }


    //####################################
    //## Just examples, not implemented ##
    //####################################


    public abstract class Node
    {
        /// <summary>
        /// optional
        /// </summary>
        public Node Parent { get; set; }

        /// <summary>
        /// optional
        /// </summary>
        public IList<Node> Children { get; set; }
    }
    [Serializable]

    public class Leaf : Node
    {
        public IList<PrecastElementRef> Elements { get; set; }
    }
    [Serializable]

    public class PrecastElementRef
    {
        public System.Guid PrecastElementId { get; set; }
        public Transform Transform { get; set; }
    }

    [Serializable]

    public class ReinforcementGeometryData : GeometryData
    {
        public XYZ[] PolyLine3D { get; set; }
        public float Diameter { get; set; }
        public float BendingRadius { get; set; }

        public override bool Equals(GeometryData other)
        {
            throw new NotImplementedException();
        }

        public override int CompareTo(GeometryData other)
        {
            throw new NotImplementedException();
        }
    }
}
