using System.Linq;
using mTIM.Models.D;
using Urho;

namespace mTIM.Components
{
    public class ObjectModel : StaticModel
    {
        protected UrhoApp App => Application.Current as UrhoApp;

        public static string NormalColorCode = "#757474";//Gray color
        public static string TransparentColorCode = "#26757474"; //15% transpent gray color.
        public static string SelectionColorCode = "#6495ED"; //Blue color.

        public override void OnAttachedToNode(Urho.Node node)
        {
            base.OnAttachedToNode(node);
        }

        /// <summary>
        /// Load the lines
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="isLineList"></param>
        /// <returns></returns>
        public bool LoadLinesMesh(TimMesh mesh, bool isLineList = false)
        {
            Urho.Application.InvokeOnMain(() =>
            {
                var vb = mesh.vertexBuffer ?? new VertexBuffer(Application.CurrentContext);
                var ib = mesh.indexBuffer ?? new IndexBuffer(Application.CurrentContext);

                var geom = new Geometry();

                var vdata = mesh.GetVertextData();
                var idata = mesh.GetIndexData();
                var ildata = mesh.GetLineIndexData();
                vb.Release();
                ib.Release();
                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                if (vb != null)
                {
                    vb.Shadowed = true;
                    vb.SetSize((uint)vdata.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color | ElementMask.TexCoord1, true);
                    vb.SetData(vdata.ToArray());
                }

                if (ib != null)
                {
                    ib.Shadowed = true;
                    ib.SetSize(isLineList ? (uint)ildata.Length : (uint)(idata.Length), true);
                    ib.SetData(isLineList ? ildata : idata);
                }
                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(isLineList ? PrimitiveType.LineList : PrimitiveType.TriangleList, 0, isLineList ? (uint)ildata.Length : (uint)idata.Length);

                var model = new Model();
                model.NumGeometries = 1;
                model.SetGeometry(0, 0, geom);

                model.BoundingBox = mesh.GetBoundingBox();

                Material GrayLineMaterial = Material.FromColor(Color.FromHex(TransparentColorCode), true);
                GrayLineMaterial.SetTechnique(1, CoreAssets.Techniques.DiffAddAlpha);
                GrayLineMaterial.CullMode = CullMode.MaxCullmodes;
                GrayLineMaterial.FillMode = FillMode.Solid;
                GrayLineMaterial.LineAntiAlias = true;

                Material BlackLineMaterial = Material.FromColor(Color.Black, true);
                GrayLineMaterial.SetTechnique(1, CoreAssets.Techniques.DiffNormal);
                GrayLineMaterial.CullMode = CullMode.MaxCullmodes;
                GrayLineMaterial.FillMode = FillMode.Solid;
                BlackLineMaterial.LineAntiAlias = true;
                Model = model;
                SetMaterial(isLineList ? BlackLineMaterial : GrayLineMaterial);
                CastShadows = false;

                //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
                var halfSize = WorldBoundingBox.HalfSize;
                var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));

                //Node.Rotation = new Quaternion(60, 0, 30);
                Node.ScaleNode(2.5f / scaleFactor);
                //position model so world bounding box is centered on origin
                Node.Position = WorldBoundingBox.Center * -1;
            });
            return true;
        }

        /// <summary>
        /// Update the meterial to element
        /// </summary>
        /// <param name="isActive"></param>
        public void UpdateMaterial(bool isActive)
        {
            Urho.Application.InvokeOnMain(() =>
            {
                Material activeMaterial = Material.FromColor(Color.FromHex(NormalColorCode), false);
                activeMaterial.SetTechnique(1, CoreAssets.Techniques.Diff);
                activeMaterial.CullMode = CullMode.MaxCullmodes;
                activeMaterial.FillMode = FillMode.Solid;
                activeMaterial.LineAntiAlias = true;

                Material inActiveMaterial = Material.FromColor(Color.FromHex(TransparentColorCode), false);
                inActiveMaterial.SetTechnique(1, CoreAssets.Techniques.Diff);
                inActiveMaterial.CullMode = CullMode.MaxCullmodes;
                inActiveMaterial.FillMode = FillMode.Solid;
                inActiveMaterial.LineAntiAlias = true;
                SetMaterial(isActive ? activeMaterial : inActiveMaterial);
            });
        }

        /// <summary>
        /// Apply Color to model.
        /// </summary>
        /// <param name="color"></param>
        public void ApplyColor(Color color)
        {
            Urho.Application.InvokeOnMain(() =>
            {
                Material material = Material.FromColor(color, true);
                material.SetTechnique(1, CoreAssets.Techniques.Diff);
                material.CullMode = CullMode.MaxCullmodes;
                material.FillMode = FillMode.Solid;
                material.LineAntiAlias = true;
                SetMaterial(material);
            });
        }

        /// <summary>
        /// To update the selection.
        /// </summary>
        public void UpdateSelection()
        {
            Urho.Application.InvokeOnMain(() =>
            {
                Material selectMaterial = Material.FromColor(Color.FromHex(SelectionColorCode), false);
                selectMaterial.SetTechnique(1, CoreAssets.Techniques.Diff);
                selectMaterial.CullMode = CullMode.MaxCullmodes;
                selectMaterial.FillMode = FillMode.Solid;
                selectMaterial.LineAntiAlias = true;
                SetMaterial(selectMaterial);
            });
        }

        /// <summary>
        /// To load the 3D drawing based on index.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        /// <returns></returns>
        public bool LoadMesh(TimMesh mesh, int fromIndex, int toIndex)
        {
            Urho.Application.InvokeOnMain(() =>
            {
                var vb = mesh.vertexBuffer ?? new VertexBuffer(Application.CurrentContext);
                var ib = mesh.indexBuffer ?? new IndexBuffer(Application.CurrentContext);

                var geom = new Geometry();

                var vdata = mesh.GetVertextData();
                var idata = mesh.GetIndexData();
                var ildata = mesh.GetLineIndexData();
                vb.Release();
                ib.Release();
                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                if (vb != null)
                {
                    vb.Shadowed = true;
                    vb.SetSize((uint)vdata.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color | ElementMask.TexCoord1, true);
                    vb.SetData(vdata.ToArray());
                }

                if (ib != null)
                {
                    ib.Shadowed = true;
                    ib.SetSize((uint)(idata.Length), true);
                    ib.SetData(idata);
                }
                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(PrimitiveType.TriangleList, (uint)fromIndex, (uint)toIndex);

                //geom.SetDrawRange(PrimitiveType.LineList, (uint)idata.Length, (uint)ildata.Length);

                var model = new Model();
                model.NumGeometries = 1;
                model.SetGeometry(0, 0, geom);
                model.BoundingBox = mesh.GetBoundingBox();

                Material GrayLineMaterial = Material.FromColor(Color.FromHex(NormalColorCode), false);
                GrayLineMaterial.SetTechnique(5, CoreAssets.Techniques.Diff);
                GrayLineMaterial.CullMode = CullMode.MaxCullmodes;
                GrayLineMaterial.FillMode = FillMode.Solid;
                GrayLineMaterial.LineAntiAlias = true;

                Model = model;
                SetMaterial(GrayLineMaterial);
                CastShadows = false;

                //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
                var halfSize = WorldBoundingBox.HalfSize;
                var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));

                //Node.Rotation = new Quaternion(60, 0, 30);
                Node.ScaleNode(2.5f / scaleFactor);
                //position model so world bounding box is centered on origin
                Node.Position = WorldBoundingBox.Center * -1;
            });
            return true;
        }

        /// <summary>
        /// To load the element mesh.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="timElement"></param>
        /// <param name="isActiveList"></param>
        public void LoadElementMesh(TimMesh mesh, TimElementMesh timElement, bool isActiveList)
        {
            Urho.Application.InvokeOnMain(() =>
            {
                var vb = mesh.vertexBuffer ?? new VertexBuffer(Application.CurrentContext);
                var ib = mesh.indexBuffer ?? new IndexBuffer(Application.CurrentContext);

                var geom = new Geometry();

                var vdata = mesh.GetVertextData();
                var idata = mesh.GetIndexData();
                var ildata = mesh.GetLineIndexData();
                vb.Release();
                ib.Release();
                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                if (vb != null)
                {
                    vb.Shadowed = true;
                    vb.SetSize((uint)vdata.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color | ElementMask.TexCoord1, true);
                    vb.SetData(vdata.ToArray());
                }

                if (ib != null)
                {
                    ib.Shadowed = true;
                    ib.SetSize((uint)(idata.Length), true);
                    ib.SetData(idata);
                }
                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(PrimitiveType.TriangleList, (uint)timElement.triangleBatch.startIndex, (uint)timElement.triangleBatch.primitiveCount);

                //geom.SetDrawRange(PrimitiveType.LineList, (uint)idata.Length, (uint)ildata.Length);

                var model = new Model();
                model.NumGeometries = 1;
                model.SetGeometry(0, 0, geom);
                model.BoundingBox = mesh.GetBoundingBox();

                Material GrayLineMaterial = Material.FromColor(Color.FromHex(isActiveList ? NormalColorCode : TransparentColorCode), false);
                GrayLineMaterial.SetTechnique(5, CoreAssets.Techniques.Diff);
                GrayLineMaterial.CullMode = CullMode.MaxCullmodes;
                GrayLineMaterial.FillMode = FillMode.Solid;
                GrayLineMaterial.LineAntiAlias = true;

                Model = model;
                SetMaterial(GrayLineMaterial);
                CastShadows = false;

                //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
                var halfSize = WorldBoundingBox.HalfSize;
                var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));

                //Node.Rotation = new Quaternion(60, 0, 30);
                Node.ScaleNode(2.5f / scaleFactor);
                //position model so world bounding box is centered on origin
                Node.Position = WorldBoundingBox.Center * -1;
            });
        }
    }
}
