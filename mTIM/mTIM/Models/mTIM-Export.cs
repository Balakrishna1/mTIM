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
}
