using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using mTIM.Models.D;
using Urho;
using Urho.Urho2D;

namespace mTIM.Components
{
    public class ObjectModel : StaticModel
    {
        protected UrhoApp App => Application.Current as UrhoApp;

        public override void OnAttachedToNode(Urho.Node node)
        {
            base.OnAttachedToNode(node); 
        }

        public void LoadModel(string modelPath, string materialPath)
        {
            var cache = Application.ResourceCache;

            this.Model = cache.GetModel(modelPath);

            if(string.IsNullOrEmpty(materialPath) == false)
            {
                this.SetMaterial(cache.GetMaterial(materialPath));

                App.Light.Brightness = 2f;
            }
            else
            {
                App.Light.Brightness = 1f;
            }

            //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
            var halfSize = this.WorldBoundingBox.HalfSize;
            var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));
            this.Node.ScaleNode(1f / scaleFactor);

            //position model so world bounding box is centered on origin
            this.Node.Position = this.WorldBoundingBox.Center * -1;

        }

        public bool LoadMesh(TimMesh mesh, bool isLineList = false)
        {
            Urho.Application.InvokeOnMain(() =>
            {
                var vb = mesh.vertexBuffer;
                var ib = mesh.indexBuffer;

                var geom = new Geometry();

                var vdata = mesh.GetVertextData();
                var idata = mesh.GetIndexData();
                var ildata = mesh.GetLineIndexData();

                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                vb.Shadowed = true;
                vb.SetSize((uint)vdata.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color | ElementMask.TexCoord1, true);
                vb.SetData(vdata.ToArray());
                vb.Unlock();

                ib.Shadowed = true;
                ib.SetSize((uint)(idata.Length), true);
                ib.SetData(idata);
                ib.Unlock();

                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(isLineList? PrimitiveType.LineList : PrimitiveType.TriangleList, 0 , (uint)idata.Length);
                //geom.SetDrawRange(PrimitiveType.LineList, (uint)idata.Length, (uint)ildata.Length);

                var model = new Model();
                model.NumGeometries = 1;
                model.SetGeometry(0, 0, geom);

                BoundingBox boundingBox = new BoundingBox();
                for (int i = 0; i < vdata.Length; i++)
                {
                    var position = vdata[i].Position;
                    boundingBox.Merge(position);

                    var normal = vdata[i].Normal;
                    vdata[i].Normal = normal;
                }
                model.BoundingBox = mesh.GetBoundingBox();

                Material GrayLineMaterial = Material.FromColor(Color.Gray);
                GrayLineMaterial.SetTechnique(1, CoreAssets.Techniques.NoTextureUnlit, 1, 1);
                GrayLineMaterial.CullMode = CullMode.Cw;
                GrayLineMaterial.FillMode = FillMode.Solid;
                GrayLineMaterial.LineAntiAlias = true;

                Material BlackLineMaterial = Material.FromColor(Color.Black);
                BlackLineMaterial.SetTechnique(0, CoreAssets.Techniques.DiffNormal, 0, 0);
                BlackLineMaterial.CullMode = CullMode.Cw;
                BlackLineMaterial.LineAntiAlias = true;

                Model = model;
                SetMaterial(isLineList ? BlackLineMaterial : GrayLineMaterial);
                CastShadows = true;

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
    }
}
