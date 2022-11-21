using System.Collections.Generic;
using Urho;

namespace mTIM.Models.D
{
    public class ChunkedArray<T> : System.IDisposable
    {
        private int S = 128 * 1024;

        public ChunkedArray()
        {
            this.size = 0;
        }
        public void Dispose()
        {
        }

        public int Count()
        {
            return size;
        }

        public int ChunkCount()
        {
            return chunks.Count;
        }

        public int SizeOfChunk(int index)
        {
            return chunks[index].Count;
        }

        public int SizePerChunk()
        {
            return S;
        }

        public List<T> GetChunk(int index)
        {
            return chunks[index];
        }

        public void Clear()
        {
            size = 0;
            chunks.Clear();
        }

        public void Add(T t)
        {
            if (chunks.Count == 0 || chunks.Count == S)
            {
                List<T> T = new List<T>(S);
                chunks.Add(T);
            }
            chunks[chunks.Count - 1].Add(t);
            size++;
        }
        private int size;

        private List<List<T>> chunks = new List<List<T>>();
    }


    public class Vertex
    {
        public Vertex()
        {

        }
        public enum AnonymousEnum
        {
            UV_COUNT = 2
        }

        public Vector3 position = new Vector3();
        public Vector3 normal = new Vector3();
        public Color color = new Color();
        public Vector2[] uvs = Arrays.InitializeWithDefaultInstances<Vector2>((int)AnonymousEnum.UV_COUNT);
        public char[] blendIndices = Arrays.InitializeWithDefaultInstances<char>(4);
        public char[] blendWeights = Arrays.InitializeWithDefaultInstances<char>(4);
    }

    internal static class Arrays
    {
        public static T[] InitializeWithDefaultInstances<T>(int length) where T : new()
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = new T();
            }
            return array;
        }

        public static string[] InitializeStringArrayWithDefaultInstances(int length)
        {
            string[] array = new string[length];
            for (int i = 0; i < length; i++)
            {
                array[i] = "";
            }
            return array;
        }

        public static void DeleteArray<T>(T[] array) where T : System.IDisposable
        {
            foreach (T element in array)
            {
                if (element != null)
                    element.Dispose();
            }
        }
    }

    public class TimMeshBuilder<T> : System.IDisposable
    {
        public int lastIndexCount = 0;
        const int Size = 128 * 1024;
        public AABB aabb = new AABB();
        public ChunkedArray<Vertex> vertices = new ChunkedArray<Vertex>();
        //public RobinTable<Vertex, int, RobinObjectAdapter<Vertex>> vertexTable = new RobinTable<Vertex,int, RobinObjectAdapter<Vertex>>();
        public Dictionary<Vertex, int> vertexTable = new Dictionary<Vertex, int>();
        public ChunkedArray<int> indices = new ChunkedArray<int>();
        public IList<TimElementMesh> elementMeshes = new List<TimElementMesh>();
        public int lastLineIndexCount;
        public ChunkedArray<int> lineIndices = new ChunkedArray<int>();

        public TimMeshBuilder()
        {
            lastIndexCount = 0;
            lastLineIndexCount = 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < elementMeshes.Count; i++)
                elementMeshes.RemoveAt(i);
        }

        public void Reset()
        {
            vertices.Clear();
            vertexTable.Clear();
            indices.Clear();
            lastIndexCount = 0;
            lineIndices.Clear();
            elementMeshes.Clear();
            lastLineIndexCount = 0;
            aabb = new AABB();
        }

        public void DisposeTemp()
        {
            //vertexTable.Dispose();
        }

        public int AddVertex(Vertex vertex)
        {
            if (indices.Count() == lastIndexCount)
            {
                aabb.Minimum = vertex.position;
                aabb.Maximum = vertex.position;
            }
            else
            {
                aabb.GrowFast(vertex.position);
            }

            int newIndex = vertices.Count();
            int index = -1;
            if (!vertexTable.TryGetValue(vertex, out index))
            {
                //Console.WriteLine(string.Format("Vertex position: {0},{1},{2}", vertex.position.X, vertex.position.Y, vertex.position.Z));
                vertexTable.TryAdd(vertex, newIndex);
                vertices.Add(vertex);
                return newIndex;
            }
            else
            {
                return index;
            }
        }

        public void AddTriangle(int[] triangleIndices)
        {
            // indices.Add(triangleIndices, 3);      
            indices.Add(triangleIndices[0]);
            indices.Add(triangleIndices[1]);
            indices.Add(triangleIndices[2]);
        }

        public void AddLine(int indexA, int indexB)
        {
            lineIndices.Add(indexA);
            lineIndices.Add(indexB);
        }

        public void StartProject()
        {
            //Log("VertexTAbleSize: %d %d\n", vertexTable.GetSize(), vertexTable.GetCapacity());
            vertexTable.Clear();
        }

        public void StartElementMesh()
        {
            elementMeshes.Add(new TimElementMesh());
            aabb = new AABB();
            lastIndexCount = indices.Count();
            lastLineIndexCount = lineIndices.Count();
        }

        public TimElementMesh EndElementMesh()
        {
            TimBatch trianglebatch;
            TimBatch linebatch;
            TimElementMesh sm = elementMeshes[elementMeshes.Count - 1];
            {
                trianglebatch = sm.triangleBatch;
                trianglebatch.primitiveType = PrimitiveType.TriangleList;
                trianglebatch.baseVertexIndex = 0;
                trianglebatch.minIndex = 0;
                trianglebatch.numVertices = vertices.GetChunk(0).Count;
                trianglebatch.startIndex = lastIndexCount;
                trianglebatch.primitiveCount = indices.GetChunk(0).Count - lastIndexCount;
            }
            {
                linebatch = sm.lineBatch;
                linebatch.primitiveType = PrimitiveType.LineList;
                linebatch.baseVertexIndex = 0;
                linebatch.minIndex = 0;
                linebatch.numVertices = vertices.GetChunk(0).Count;
                linebatch.startIndex = lastLineIndexCount;
                linebatch.primitiveCount = lineIndices.GetChunk(0).Count - lastLineIndexCount;
            }
            sm.triangleBatch = trianglebatch;
            sm.lineBatch = linebatch;
            sm.visible = true;
            sm.opaque = false;
            return sm;
        }
    }
}
