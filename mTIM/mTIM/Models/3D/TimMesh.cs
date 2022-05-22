using System;
using System.Collections.Generic;
using System.Linq;
using Urho;

namespace mTIM.Models.D
{
    public class TimMesh
    {
        public Result proto { get; set; }
        public string ElementName { get; set; }
        public VertexBuffer vertexBuffer { get; set; }
        public IndexBuffer indexBuffer { get; set; }
        public List<TimSubMesh> subMeshes { get; set; }
        public List<TimElementMesh> elementMeshes { get; set; }
        public List<Vertex> vertices { get; set; }
        public List<int> indeces { get; set; }
        public List<int> lineIndices { get; set; }
        public Urho.Model model { get; set; }

        public TimMesh()
        {
            this.proto = null;
            this.vertexBuffer = null;
            this.indexBuffer = null;
            vertices = new List<Vertex>();
            indeces = new List<int>();
            lineIndices = new List<int>();
            subMeshes = new List<TimSubMesh>();
            elementMeshes = new List<TimElementMesh>();
        }

        public void AddLine(int indexA, int indexB)
        {
            if (lineIndices == null)
            {
                lineIndices = new List<int>();
            }
            lineIndices.Add(indexA);
            lineIndices.Add(indexB);
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

        /// <summary>
        /// To get the vertex data from Mesh.
        /// </summary>
        /// <returns></returns>
        public VertexBuffer.PositionNormalColorTexcoord[] GetVertextData()
        {
            var data = new Urho.VertexBuffer.PositionNormalColorTexcoord[vertices.Count];

            for (int i = 0; i < vertices.Count; i++)
            {
                var vd = vertices[i];

                var d = new Urho.VertexBuffer.PositionNormalColorTexcoord();
                d.Position = vd.position;
                d.Normal = vd.normal;
                //d.TexCoord = getuvs(vd);
                d.Color = vd.color.ToUInt();
                data[i] = d;
            }

            return data;
        }

        /// <summary>
        /// To the Index data from mesh.
        /// </summary>
        /// <returns></returns>
        public uint[] GetIndexData()
        {
            var data = new uint[indeces.Count];

            for (int i = 0; i < indeces.Count; i++)
            {
                data[i] = (uint)indeces[i];
            }
            return data;
        }

        /// <summary>
        /// This is used the get the line indices data from mesh.
        /// </summary>
        /// <returns></returns>
        public uint[] GetLineIndexData()
        {
            //uint[] indData = GetIndexData();
            var data = new uint[lineIndices.Count];
            for (int i = 0; i < lineIndices.Count; i++)
            {
                data[i] = (uint)lineIndices[i];
            }

            return data;
        }

        /// <summary>
        /// To get the BoundingBox based vertices.
        /// </summary>
        /// <returns></returns>
        public Urho.BoundingBox GetBoundingBox()
        {
            float minx, miny, minz, maxx, maxy, maxz;

            minx = miny = minz = float.MaxValue;
            maxx = maxy = maxz = float.MinValue;

            foreach (var v in vertices)
            {
                minx = Math.Min(minx, v.position.X);
                miny = Math.Min(miny, v.position.Y);
                minz = Math.Min(minz, v.position.Z);
                maxx = Math.Max(maxx, v.position.X);
                maxy = Math.Max(maxy, v.position.Y);
                maxz = Math.Max(maxz, v.position.Z);
            }

            return new Urho.BoundingBox(
                new Urho.Vector3(minx, miny, minz),
                new Urho.Vector3(maxx, maxy, maxz));
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

    public struct TimElementMesh
    {
        public TimBatch triangleBatch;
        public TimBatch lineBatch;

        public AABB aabb;
        public int visualizableIndex;
        public bool visible;
        public bool opaque;
        public bool active;
        public Color color;
        public int listId;
        public int simplificationLevel;
    };

}
