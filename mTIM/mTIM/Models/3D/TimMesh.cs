using System;
using System.Collections.Generic;
using Urho;

namespace mTIM.Models.D
{
    public class TimMesh
    {
        public Result proto;
        public VertexBuffer vertexBuffer;
        public IndexBuffer indexBuffer;
        public List<TimSubMesh> subMeshes { get; set; }

        public TimMesh()
        {
            this.proto = null;
            this.vertexBuffer = null;
            this.indexBuffer = null;
        }

        public static Vector3 Minus(Vector3 a, Vector3 b)
        {
            Vector3 result = new Vector3();
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
            return result;
        }

        public static Vector3 Plush(Vector3 a, Vector3 b)
        {
            Vector3 result = new Vector3();
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            return result;
        }

        public static Vector3 Into(Vector3 a, float val)
        {
            Vector3 result = new Vector3();
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            return result;
        }
    }

    public struct TimVertex
    {
       public Vector3 position;
       public Vector3 normal;


        public bool TimVertexData(TimVertex other)
        {
            return position.X == other.position.X && position.Y == other.position.Y && position.Z == other.position.Z && normal.X == other.normal.X && normal.Y == other.normal.Y && normal.Z == other.normal.Z;
        }

        public int CalcHash()
        {
            return 0;
                //SimpleHash.Calc32(this, sizeof(TimVertex));
        }

    };

    public struct TimBatch
    {
        public PrimitiveType primitiveType;
        public int baseVertexIndex;
        public int minIndex;
        public int numVertices;
        public int startIndex;
        public int primitiveCount;
    };

    public struct TimSubMesh
    {

        public TimBatch triangleBatch;
        public TimBatch lineBatch;

        public AABB aabb;
        public int visualizableIndex;
        public int refIndex;
        public bool visible;
        public bool opaque;
        public bool active;
        public Color color;
        public int listId;
        public int simplificationLevel;
    };

}
