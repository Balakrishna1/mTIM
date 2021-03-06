
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using mTIM.ViewModels;
using Urho;

namespace mTIM.Models.D
{
    public class TimMeshLoader : System.IDisposable
    {
        public TimMeshLoader()
        {
        }

        public void Dispose()
        {
        }

        TimMesh mesh = new TimMesh();
        public TimMesh Load(Result result)
        {
            if (result.Elements.Count() == 0)
            {
                return null;
            }

            TimMeshBuilder<Vertex> builder = new TimMeshBuilder<Vertex>();
            {
                XYZ[] sourcePoints = result.PointsAndVectors;
                List<int> indexMap = new List<int>();
                indexMap.Resize(sourcePoints.Count());
#if DEBUG
                for (int i = 0; i < indexMap.Count; i++)
                {
                    indexMap[i] = -1;
                }
#endif

                int currentProjectId = -1;

                for (int iVisu = 0; iVisu < result.Elements.Count(); iVisu++)
                {
                    Visualizable visu = result.Elements[iVisu];
                    BaseViewModel ViewModel = App.Current.MainPage.BindingContext as BaseViewModel;
                    var taskData = ViewModel?.TotalListList.Where(x => x.ObjectId.Equals(visu.Id)).FirstOrDefault();
                    //Logic.Instance().GetTaskListData().GetTaskById(visu.taskId);
                    if (taskData != null && currentProjectId != taskData.Parent)
                    {
                        currentProjectId = taskData.Parent;
                        builder.StartProject();
                    }
                    if (visu.Geometries != null)
                    {
                        for (int iRef = 0; iRef < visu.Geometries.Count(); iRef++)
                        {
                            GeometryReference @ref = visu.Geometries[iRef];
                            uint geometryIndex = @ref.GeometryIndex;
                            float[] m = @ref.Transform.Matrix;

                            TriangulatedGeometryData triGeometryData = (TriangulatedGeometryData)result.Geometries[(int)geometryIndex];

                            builder.StartSubMesh();
                            for (int iTriangle = 0; iTriangle < triGeometryData.Triangles.Count(); iTriangle++)
                            {
                                Triangle triangle = triGeometryData.Triangles[iTriangle];

                                // premultiply points
                                Vector3 pa = MultiplyVector(sourcePoints[Convert.ToInt32(triangle.A)], m);
                                Vector3 pb = MultiplyVector(sourcePoints[Convert.ToInt32(triangle.B)], m);
                                Vector3 pc = MultiplyVector(sourcePoints[Convert.ToInt32(triangle.C)], m);

                                pa.Y = -pa.Y;
                                pb.Y = -pb.Y;
                                pc.Y = -pc.Y;
                                var value1 = pb - pa;
                                var value2 = pb - pc;
                                Vector3 normalVector = new Vector3();
                                Vector3.Cross(ref value1, ref value2, out normalVector);
                                if (normalVector.Length <= 0.00001f)
                                {
                                    normalVector = new Vector3(0.0f, 1.0f, 0.0f);
                                }
                                else
                                {
                                    normalVector.Normalize();
                                }

                                int[] triangleIndices = new int[3];
                                Vertex a = new Vertex();
                                a.position = pa;
                                a.normal = normalVector;
                                triangleIndices[0] = builder.AddVertex(a);
                                Vertex b = new Vertex();
                                b.position = pb;
                                b.normal = normalVector;
                                triangleIndices[1] = builder.AddVertex(b);
                                Vertex c = new Vertex();
                                c.position = pc;
                                c.normal = normalVector;
                                triangleIndices[2] = builder.AddVertex(c);
                                mesh.AddTriangle(triangleIndices);
                                builder.AddTriangle(triangleIndices);

                                indexMap[Convert.ToInt32(triangle.A)] = triangleIndices[0];
                                indexMap[Convert.ToInt32(triangle.B)] = triangleIndices[1];
                                indexMap[Convert.ToInt32(triangle.C)] = triangleIndices[2];

                                if (builder.vertexTable.Count() > builder.vertexTable.Values.Count() * 75 / 100)
                                {
                                    builder.vertexTable.Clear();
                                }
                            }

                            for (int iLine = 0; iLine < triGeometryData.Lines.Count(); iLine++)
                            {
                                Line line = triGeometryData.Lines[iLine];
                                mesh.AddLine(indexMap[Convert.ToInt32(line.A)], indexMap[Convert.ToInt32(line.B)]);
                                builder.AddLine(indexMap[Convert.ToInt32(line.A)], indexMap[Convert.ToInt32(line.B)]);
                            }
                            TimSubMesh sm = builder.EndSubMesh();
                            sm.visualizableIndex = iVisu;
                            sm.refIndex = iRef;
                            sm.simplificationLevel = 1;
                            sm.listId = taskData != null ? taskData.Id : -1;
                            builder.subMeshes[builder.subMeshes.Count - 1] = sm;
                        }
                    }
                }
               
            }
            int vertexCount = builder.vertices.Count();

            //mesh = new TimMesh();
            mesh.proto = result;
            mesh.vertices = builder.vertices.GetChunk(0);
            VertexBuffer vertexBuffer = new VertexBuffer(Application.CurrentContext);
            //vertexBuffer.SetSize((uint)builder.vertices.Count(), ElementMask.Position, true);

                
            mesh.vertexBuffer = vertexBuffer;

            IndexBuffer indexBuffer = new IndexBuffer(Application.CurrentContext);
              
            if (builder.lineIndices.Count() > 0)
            {
                for (int iChunk = 0; iChunk < builder.lineIndices.ChunkCount(); iChunk++)
                {
                    int current = builder.indices.Count() + iChunk * builder.lineIndices.SizePerChunk();
                    //indexBuffer.Fill(builder.lineIndices.GetChunk(iChunk).GetBuffer(), INDEXFORMAT_32, current, builder.lineIndices.GetChunk(iChunk).Count());
                }
            }
            mesh.indexBuffer = indexBuffer;


            mesh.subMeshes = builder.subMeshes;
            for (int i = 0; i < mesh.subMeshes.Count; i++)
            {
                var data = mesh.subMeshes[i];
                data.lineBatch.startIndex += builder.indices.Count();
                mesh.subMeshes[i] = data;
            }
            //mesh.CreateBoundBoxesForSubmeshes();
            return mesh;
        }

        public static float[] multiply(float[] x, float factor)
        {
            if (x == null) throw new ArgumentNullException();
            return x.Select(r => r * factor).ToArray();
        }

        public static Vector3 TransFormNormal(XYZ v, float[] m)
        {
            Matrix4 matrix = new Matrix4(m[0], m[1], m[2], m[3], m[4], m[5], m[6], m[7], m[8], m[9], m[10], m[11], m[12], m[13], m[14], m[15]);
            //Matrix4 matrix2 = Matrix4.CreateTranslation(new Vector3(v.X, v.Y, v.Z));
            //var mx = matrix * matrix2;
            
            return Vector3.TransformNormal(new Vector3(v.X, v.Y, v.Z), matrix);
        }

        public Vector3 MultiplyVector(XYZ v, float[] m)
        {
            Vector3 res = new Vector3();
            res.X = m[0] * v.X + m[1] * v.Y + m[2] * v.Z + m[3];
            res.Y = m[4] * v.X + m[5] * v.Y + m[6] * v.Z + m[7];
            res.Z = m[8] * v.X + m[9] * v.Y + m[10] * v.Z + m[11];
            return res;
        }
    }
}
