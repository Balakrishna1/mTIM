using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace mTIM.Models
{
    internal static class Ex
    {
        internal static System.Guid ToGuid(this byte[] bytes)
        {
            return new System.Guid(bytes);
        }
    }

    public class PolyDebugInfo
    {
        public byte[] blob { get; set; }
        public Lazy<byte[]> hash { get; set; }
    }

    public class PolyMatrix
    {
        public string Polyeder { get; set; }
        public string Matrix { get; set; }
        public GraphicsType GraphicType { get; set; }
        public Guid? ObjectID { get; set; }
        public string Color { get; set; }
        public PolyDebugInfo DebugInfo { get; set; }
    }

    public class ThingToSerialize
    {
        public IEnumerable<PolyMatrix> Polyeders { get; set; }
        public Guid Id { get; set; }
    }

    public enum GraphicsType { Full, Simple }


    public class CElementPolyeder
    {

    }

    public class CMatrix
    {

    }

    //public static class mTIM_Export
    //{
    //    public static Result ShufflePoints(ThingToSerialize[] input, string dumpDirectory = null, Action<string> logLine = null)
    //    {
    //        // use one global point list to deduplicate the points
    //        int nextIndex = 0;
    //        var xyzList = new Dictionary<XYZ, int>();
    //        uint countTakenFirstBranch = 0;
    //        uint countTakenSecondBranch = 0;
    //        Func<XYZ, int> insertOrGet = p =>
    //        {
    //            int indexToReturn;
    //            if (xyzList.TryGetValue(p, out indexToReturn))
    //            {
    //                countTakenFirstBranch += 1;
    //                return indexToReturn;
    //            }
    //            else
    //            {
    //                countTakenSecondBranch += 1;
    //                xyzList.Add(p, nextIndex);
    //                checked { nextIndex += 1; } // check for overflow
    //                return nextIndex - 1;
    //            }
    //        };

    //        var geometries = new List<GeometryData>();
    //        var vizzes = new List<Visualizable>();

    //        foreach (var x in input)
    //        {
    //            var geometryReferences = new List<GeometryReference>();

    //            foreach (var pm in x.Polyeders)
    //            {
    //                // triangles
    //                var triangles = new List<Triangle>();
    //                var polyeder = pm.Polyeder;
    //                if (pm.GraphicType == GraphicsType.Simple)
    //                {
    //                    try
    //                    {
    //                        polyeder = Geometry_Functions.CalculateSimplifiedPolyederForMTim(polyeder);
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        logLine?.Invoke($"Error: could not simplify polyeder, using full. (ID = {x.Id}, hash={(object)pm.DebugInfo?.hash?.Value?.ToGuid() ?? "not defined"}): {ex}");
    //                        if (dumpDirectory != null && pm.DebugInfo != null)
    //                        {
    //                            var id = new Guid(pm.DebugInfo.hash.Value);
    //                            var filePath = Path.Combine(dumpDirectory, id.ToString());
    //                            if (false == File.Exists(filePath))
    //                            {
    //                                File.WriteAllBytes(filePath, pm.DebugInfo.blob);
    //                            }
    //                        }
    //                    }
    //                }
    //                var vertexIndex = polyeder.GetVertexIndex();
    //                Func<CustomVertex.PositionColored, XYZ> vertexToXYZ = v => new XYZ { X = v.Position.X, Y = v.Position.Y, Z = v.Position.Z };
    //                var realIndizes = Enumerable.Range(0, vertexIndex.vertex.Length)
    //                                      .Select(i => new { oldIndex = i, newIndex = insertOrGet(vertexToXYZ(vertexIndex.vertex[i])) })
    //                                      .ToDictionary(kv => kv.oldIndex, kv => kv.newIndex);

    //                for (int i = 0; i < vertexIndex.index.Length / 3; i++)
    //                {
    //                    var ia = realIndizes[(vertexIndex.index[i * 3])];
    //                    var ib = realIndizes[(vertexIndex.index[i * 3 + 1])];
    //                    var ic = realIndizes[(vertexIndex.index[i * 3 + 2])];
    //                    triangles.Add(new Triangle { A = (uint)ia, B = (uint)ib, C = (uint)ic });
    //                }

    //                // lines
    //                var lines = new List<Line>();
    //                var edges = polyeder.edge.Select(edge => new { begin = polyeder.point[Abs(edge.begin) - 1], end = polyeder.point[Abs(edge.end) - 1] }).ToArray();
    //                Func<CElementPoint, XYZ> celementPointToXYZ = cPoint => new XYZ { X = (float)cPoint.X, Y = (float)cPoint.Y, Z = (float)cPoint.Z };
    //                foreach (var e in edges)
    //                {
    //                    var a = celementPointToXYZ(e.begin);
    //                    var b = celementPointToXYZ(e.end);
    //                    lines.Add(new Line { A = (uint)insertOrGet(a), B = (uint)insertOrGet(b) });
    //                }

    //                // matrix
    //                Transform matrix = null;
    //                var matrixAsFloat = pm.Matrix.Values.Select(d => (float)d).ToArray();
    //                if (false == matrixAsFloat.SequenceEqual(identityMatrix))
    //                {
    //                    matrix = new Transform { Matrix = matrixAsFloat };
    //                }
    //                var triangulatedGeometry = new TriangulatedGeometryData { Triangles = triangles.ToArray(), Lines = lines.ToArray() };
    //                // also try to deduplicate the geometries
    //                var indexOfGeo = geometries.IndexOf(triangulatedGeometry);
    //                if (-1 == indexOfGeo)
    //                {
    //                    geometries.Add(triangulatedGeometry);
    //                    indexOfGeo = geometries.Count - 1;
    //                }
    //                geometryReferences.Add(new GeometryReference
    //                {
    //                    ColorTag = pm.Color,
    //                    LayerTag = "PRECAST",
    //                    Transform = matrix,
    //                    GeometryIndex = (uint)indexOfGeo,
    //                    ObjectID = pm.ObjectID,
    //                    GraphicsType = pm.GraphicType
    //                });
    //            }
    //            var viz = new Visualizable
    //            {
    //                Id = x.Id,
    //                Geometries = geometryReferences.ToArray()
    //            };
    //            vizzes.Add(viz);
    //        }

    //        //Console.WriteLine("points written: {0}, points deduplicated: {1}", countTakenSecondBranch, countTakenFirstBranch);
    //        //Console.WriteLine("elements written: {0}, null-pos geometries written: {1}", vizzes.Count, geometries.Count);

    //        // now combine the values
    //        var resultSet = new Result();
    //        resultSet.Elements = vizzes.ToArray();
    //        resultSet.Geometries = geometries.ToArray();
    //        var xyzarray = new XYZ[xyzList.Count];
    //        foreach (var x in xyzList)
    //            xyzarray[x.Value] = x.Key;
    //        resultSet.PointsAndVectors = xyzarray;

    //        return resultSet;
    //    }

    //    // use row major notation
    //    static float[] identityMatrix = new float[]{1,0,0,0,
    //                                                0,1,0,0,
    //                                                0,0,1,0,
    //                                                0,0,0,1};
    //}
}
