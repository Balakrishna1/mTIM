using System;
using System.Collections.Generic;
using System.Linq;
using Urho;
using Urho.SharpReality;

namespace mTIM.Models.D
{
    public class TimMesh
    {
        public Result proto { get; set; }
        public string ElementName { get; set; }
        public VertexBuffer vertexBuffer { get; set; }
        public IndexBuffer indexBuffer { get; set; }
        public List<TimSubMesh> subMeshes { get; set; }
        public List<Vertex> vertices { get; set; } 
        public List<Triangle> triangles { get; set; }
        public List<int> lineIndices { get; set; }
        public Urho.Model model { get; set; }

        public TimMesh()
        {
            this.proto = null;
            this.vertexBuffer = null;
            this.indexBuffer = null;
            vertices = new List<Vertex>();
            triangles = new List<Triangle>();
            lineIndices = new List<int>();
            subMeshes = new List<TimSubMesh>();
        }

        public void AddTriangle(int[] data)
        {
            if(triangles == null)
            {
                triangles = new List<Triangle>();
            }
            Console.WriteLine(string.Format("Triangle data: {0},{1},{2}", data[0], data[1], data[2]));
            triangles.Add(new Triangle() { A = (uint)data[0], B = (uint)data[1], C = (uint)data[2] });
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

        public int GetPositionIndex(Vector3 position)
        {
            var item = vertices?.Where(x => x.Equals(position)).FirstOrDefault();
            return vertices.IndexOf(item);
        }

        public VertexBuffer.PositionNormalColorTexcoord[] GetVertextData(TimSubMesh submesh)
        {
            var data = new Urho.VertexBuffer.PositionNormalColorTexcoord[submesh.triangleBatch.numVertices];
            int startIndex = submesh.triangleBatch.baseVertexIndex;
            int endIndex = startIndex + submesh.triangleBatch.numVertices;
            for (int i = startIndex; i < endIndex; i++)
            {
                var vd = vertices[i];

                var d = new Urho.VertexBuffer.PositionNormalColorTexcoord();
                d.Position = vd.position;
                d.Normal = vd.normal;
                d.TexCoord = getuvs(vd);
                d.Color = vd.color.ToUInt();
                data[i] = d;
            }

            return data;
        }

        public int AddVertex(Vertex vertex)
        {
            int newIndex = vertices.Count();
            //Todo: Getting error need to fix it later.
            var data = vertices.Where(x => x.Equals(vertex)).FirstOrDefault();
            if (data != null)
            {
                int index = vertices.IndexOf(data);

                if (index == -1)
                {
                    vertices.Add(vertex);
                    return newIndex;
                }
                else
                {
                    return index;
                }
            }
            else
            {
                vertices.Add(vertex);
            }
            return newIndex;
        }

        private Vector2 getuvs(Vertex vd)
        {
            Vector2 vector = new Vector2();
            if(vd!= null && vd.uvs?.Length > 0)
            {
                foreach(var uv in vd.uvs)
                {
                    vector.Add(uv);
                }
            }

            return vector;
        }

        public uint[] GetIndexData()
        {
            var data = new uint[3 * triangles.Count];

            for (int i = 0; i < triangles.Count; i++)
            {
                int idx = 3 * i;

                data[idx + 0] = triangles[i].A;
                data[idx + 1] = triangles[i].B;
                data[idx + 2] = triangles[i].C;
            }

            return data;
        }

        public uint[] GetIndexData(TimSubMesh subMesh)
        {
            var data = new uint[3 * subMesh.triangleBatch.primitiveCount];

            int startIndex = subMesh.triangleBatch.startIndex/3;
            int endIndex = startIndex + subMesh.triangleBatch.primitiveCount;
            int j = 0;
            for (int i = startIndex; i < endIndex; i++)
            {
                int idx = 3 * j;

                data[idx + 0] = triangles[i].A;
                data[idx + 1] = triangles[i].B;
                data[idx + 2] = triangles[i].C;
                j++;
            }

            return data;
        }

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

        public uint[] Add(uint[] lst, uint[] lst1)
        {
            
            var data = new uint[lst.Length + lst1.Length];
            int idx = lst.Count();
            for (int i = 0; i < lst1.Count(); i++)
            {
                data[idx] = (uint)lst1[i];
                idx++;
            }

            return data;
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
