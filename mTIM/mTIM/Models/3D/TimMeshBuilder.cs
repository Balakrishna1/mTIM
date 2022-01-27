﻿
using System;
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
            chunks[chunks.Count-1].Add(t);
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
        public RobinTable<Key, T, RobinObjectAdapter<Key>> vertexTable = new RobinTable<Key,T, RobinObjectAdapter<Key>>();
        public ChunkedArray<int> indices = new ChunkedArray<int>();
        public List<TimSubMesh> subMeshes = new List<TimSubMesh>();
        public int lastLineIndexCount;
        public ChunkedArray<int> lineIndices = new ChunkedArray<int>();

        public TimMeshBuilder()
        {
            vertexTable.Resize(256 * 1024);
            subMeshes = new List<TimSubMesh>(1024);
            lastIndexCount = 0;
            lastLineIndexCount = 0;
        }

        public void Dispose()
        {
            for (int i = 0; i < subMeshes.Count; i++)
                subMeshes.RemoveAt(i);
        }

        public void Reset()
        {
            vertices.Clear();
            vertexTable.Clear();
            subMeshes.Clear();
            indices.Clear();
            lastIndexCount = 0;
            lineIndices.Clear();
            lastLineIndexCount = 0;
            aabb = new AABB();
        }

        public void DisposeTemp()
        {
           vertexTable.Dispose();
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
            vertices.Add(vertex);
            return newIndex;
            //Todo: Getting error need to fix it later.
            //int index = vertexTable.FindOrAdd((Key)(object)vertex, (T)(object)newIndex);
            //if (index == -1)
            //{
            //    vertices.Add(vertex);
            //    return newIndex;
            //}
            //else
            //{
            //    return Convert.ToInt32(vertexTable.GetValueByIndex(index));
            //}
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

        public void StartSubMesh()
        {
            subMeshes.Add(new TimSubMesh());

            lastIndexCount = indices.Count();
            lastLineIndexCount = lineIndices.Count();
        }

        public TimSubMesh EndSubMesh()
        {
            TimSubMesh sm = subMeshes[subMeshes.Count - 1];

            {
                TimBatch batch = sm.triangleBatch;
                batch.primitiveType = PrimitiveType.TriangleList;
                batch.baseVertexIndex = 0;
                batch.minIndex = 0;
                batch.numVertices = vertices.Count();
                batch.startIndex = lastIndexCount;
                batch.primitiveCount = (indices.Count() - lastIndexCount) / 3;
            }
            {
                TimBatch batch = sm.lineBatch;
                batch.primitiveType = PrimitiveType.LineList;
                batch.baseVertexIndex = 0;
                batch.minIndex = 0;
                batch.numVertices = vertices.Count();
                batch.startIndex = lastLineIndexCount;
                batch.primitiveCount = (lineIndices.Count() - lastLineIndexCount) / 2;
            }

            sm.aabb = aabb;
            sm.visible = true;
            sm.opaque = false;
            return sm;
        }
    }
}