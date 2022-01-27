
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
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
            Timer t = new Timer();
            //t.Print("T1");
            //t.Print("T2");

            if (result.Elements.Count() == 0)
            {
                return null;
            }

            //static TimMeshBuilder<TimVertex> builder;
            //builder.Reset();
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

                //for (int iVisu=1118; iVisu<1119/*result.visualizables.Count()*/; iVisu++)
                for (int iVisu = 0; iVisu < result.Elements.Count(); iVisu++)
                {
                    //for (int iVisu=0; iVisu<5; iVisu++)
                    Visualizable visu = result.Elements[iVisu];
                    TimTaskModel taskData = new TimTaskModel();
                    //Logic.Instance().GetTaskListData().GetTaskById(visu.taskId);
                    if (taskData != null && currentProjectId != taskData.Parent)
                    {
                        currentProjectId = taskData.Parent;
                        // Log("ID: %d %d %d\n", taskData->id, taskData->projectRootId, builder.vertexTable.size);
                        builder.StartProject();
                    }

                    for (int iRef = 0; iRef < visu.Geometries.Count(); iRef++)
                    {
                        GeometryReference @ref = visu.Geometries[iRef];
                        //Log("Ref: %s %s %s\n", ref.colorTag.GetBuffer(), ref.layerTag.GetBuffer(), visu.uuid.ToString().GetBuffer());
                        uint geometryIndex = @ref.GeometryIndex;
                        float[] m = @ref.Transform.Matrix;

                        TriangulatedGeometryData triGeometryData = (TriangulatedGeometryData)result.Geometries[(int)geometryIndex];

                        builder.StartSubMesh();
                        for (int iTriangle = 0; iTriangle < triGeometryData.Triangles.Count(); iTriangle++)
                        {
                            Triangle triangle = triGeometryData.Triangles[iTriangle];

                            // premultiply points
                           XYZ pa = sourcePoints[Convert.ToInt32(triangle.A)]; //m* sourcePoints[triangle.A];
                            XYZ pb = sourcePoints[Convert.ToInt32(triangle.B)];//m * sourcePoints[triangle.B];
                            XYZ pc = sourcePoints[Convert.ToInt32(triangle.C)];//m * sourcePoints[triangle.C];
                            Vector3 vpa = new Vector3(pa.X, pa.Y, pa.Z);
                            Vector3 vpb = new Vector3(pb.X, pb.Y, pb.Z);
                            Vector3 vpc = new Vector3(pc.X, pc.Y, pc.Z);
                            vpa.Y = -vpa.Y;
                            vpb.Y = -vpb.Y;
                            vpc.Y = -vpc.Y;

                            //Log("Triangle: %d %d %d\n", triangle.A, triangle.B, triangle.C);

                            var value1 = vpb - vpa;
                            var value2 = vpb - vpc;
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
                            normalVector.X = (float)Math.Round(normalVector.X, 2);
                            normalVector.Y = (float)Math.Round(normalVector.Y, 2);
                            normalVector.Z = (float)Math.Round(normalVector.Z, 2);

                            int[] triangleIndices = new int[3];
                            Vertex a = new Vertex();
                            a.position = vpa;
                            a.normal = normalVector;
                            triangleIndices[0] = builder.AddVertex(a);
                            Vertex b = new Vertex();
                            b.position = vpb;
                            b.normal = normalVector;
                            triangleIndices[1] = builder.AddVertex(b);
                            Vertex c = new Vertex();
                            c.position = vpc;
                            c.normal = normalVector;
                            triangleIndices[2] = builder.AddVertex(c);
                            builder.AddTriangle(triangleIndices);

                            indexMap[Convert.ToInt32(triangle.A)] = triangleIndices[0];
                            indexMap[Convert.ToInt32(triangle.B)] = triangleIndices[1];
                            indexMap[Convert.ToInt32(triangle.C)] = triangleIndices[2];

                            if (builder.vertexTable.size > builder.vertexTable.tabSize * 75 / 100)
                            {
                                builder.vertexTable.Clear();
                            }
                        }
                        for (int iLine = 0; iLine < triGeometryData.Lines.Count(); iLine++)
                        {
                            Line line = triGeometryData.Lines[iLine];
                            //Log("Line: %d %d\n", line.A, line.B);
                            //ASSERT(indexMap[line.A] != -1, "Line A not filled!");
                            //ASSERT(indexMap[line.B] != -1, "Line B not filled!");
                            builder.AddLine(indexMap[Convert.ToInt32(line.A)], indexMap[Convert.ToInt32(line.B)]);
                        }
                        TimSubMesh sm = builder.EndSubMesh();
                        sm.visualizableIndex = iVisu;
                        sm.refIndex = iRef;
                        //C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
                        //ORIGINAL LINE: sm.simplificationLevel = ref.simplificationLevel;
                        sm.simplificationLevel = 1;
                        sm.listId = taskData != null ? taskData.Id : -1;
                    }
                }
                //t.Print("T3");
            }
            // builder..Dispose();
            // builder.DisposeTemp();
            //Log("Vertices: %d - Indices: %d\n", builder.vertices.Count(), builder.indices.Count());
            int vertexCount = builder.vertices.Count();

            mesh = new TimMesh();
            mesh.proto = result;

            VertexBuffer vertexBuffer = new VertexBuffer(Application.CurrentContext);
            vertexBuffer.SetSize((uint)builder.vertices.Count(), ElementMask.Position, true);

                //GraphicsManager.Instance().CreateVertexBuffer(builder.vertices.Count(), VertexDescription.PositionNormal, true);
            if (builder.vertices.Count() > 0)
            {
                //Log("VertexCount: %d\n", builder.vertices.Count());
                // vertexBuffer->Fill(&builder.vertices[0], VertexDescription::PositionNormal, 0, builder.vertices.Count());
                for (int iChunk = 0; iChunk < builder.vertices.ChunkCount(); iChunk++)
                {
                    int current = iChunk * builder.vertices.SizePerChunk();
                    var vData = GetVertextData(builder.vertices);
                    vertexBuffer.SetData(vData);
                    //vertexBuffer.Fill(builder.vertices.GetChunk(iChunk).GetBuffer(), VertexDescription.PositionNormal, current, builder.vertices.GetChunk(iChunk).Count());
                }
            }
            mesh.vertexBuffer = vertexBuffer;

            // builder.vertices.Destroy();

            IndexBuffer indexBuffer = new IndexBuffer(Application.CurrentContext);
            indexBuffer.SetSize((uint)(builder.indices.Count() + builder.lineIndices.Count()), vertexCount < 64 * 1024, true);
                //GraphicsManager.Instance().CreateIndexBuffer(builder.indices.Count() + builder.lineIndices.Count(), vertexCount < 64 * 1024 ? INDEXFORMAT_16 : INDEXFORMAT_32, true);
            if (builder.indices.Count() > 0)
            {
                for (int iChunk = 0; iChunk < builder.indices.ChunkCount(); iChunk++)
                {
                    int current = iChunk * builder.indices.SizePerChunk();
                    var vIData = GetIndexData(builder.trindices);
                    indexBuffer.SetData(vIData);
                    //indexBuffer.Fill(builder.indices.GetChunk(iChunk).GetBuffer(), INDEXFORMAT_32, current, builder.indices.GetChunk(iChunk).Count());
                }
            }
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
            //t.Print("T4");

            return mesh;

        }

        public VertexBuffer.PositionNormalColorTexcoord[] GetVertextData(ChunkedArray<Vertex> result)
        {
            var data = new Urho.VertexBuffer.PositionNormalColorTexcoord[result.Count()];

            for (int i = 0; i < result.Count() -1; i++)
            {
                var vd = result.GetChunk(i).FirstOrDefault();

                var d = new Urho.VertexBuffer.PositionNormalColorTexcoord();
                d.Position = vd.position;
                //new Urho.Vector3(v.X, v.Y, v.Z);
                d.Normal = vd.normal;
                d.TexCoord = vd.uvs[i];
                //new Urho.Vector3(n.X, n.Y, n.Z);
                d.Color = vd.color.ToUInt();

                data[i] = d;
            }

            return data;
        }

        public uint[] GetIndexData(List<Triangle> Triangles)
        {
            var data = new uint[3 * Triangles.Count];

            for (int i = 0; i < Triangles.Count; i++)
            {
                int idx = 3 * i;

                data[idx + 0] = Triangles[i].A;
                data[idx + 1] = Triangles[i].B;
                data[idx + 2] = Triangles[i].C;
            }

            return data;
        }
    }
}
