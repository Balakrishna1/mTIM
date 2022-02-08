﻿using System;
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
                var totalLines = mesh.Add(idata , ildata);

                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                vb.Shadowed = true;
                vb.SetSize((uint)vdata.Length, ElementMask.Position | ElementMask.Normal | ElementMask.Color | ElementMask.TexCoord1, true);
                vb.SetData(vdata.ToArray());

                ib.Shadowed = true;
                ib.SetSize((uint)idata.Length, true);
                ib.SetData(isLineList ? ildata : idata);

                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(isLineList? PrimitiveType.LineList : PrimitiveType.TriangleList, 0 , (uint)idata.Length);
                //geom.SetDrawRange(PrimitiveType.LineList, (uint)idata.Length, (uint)ildata.Length);

                //CustomGeometry cgeom = App.RootNode.CreateComponent<CustomGeometry>();
                //cgeom.BeginGeometry(0, PrimitiveType.LineList);
                //var material = new Material();
                //material.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlitVCol, 1, 1);
                //cgeom.SetMaterial(material);

                //float size = 1;

                ////x
                //cgeom.DefineVertex(Vector3.Zero);
                //cgeom.DefineColor(Color.Red);
                //cgeom.DefineVertex(Vector3.UnitX * size);
                //cgeom.DefineColor(Color.Red);
                ////y
                //cgeom.DefineVertex(Vector3.Zero);
                //cgeom.DefineColor(Color.Green);
                //cgeom.DefineVertex(Vector3.UnitY * size);
                //cgeom.DefineColor(Color.Green);
                ////z
                //cgeom.DefineVertex(Vector3.Zero);
                //cgeom.DefineColor(Color.Blue);
                //cgeom.DefineVertex(Vector3.UnitZ * size);
                //cgeom.DefineColor(Color.Blue);

                //cgeom.Commit();

                var model = new Model();
                model.NumGeometries = 1;
                model.SetGeometry(0, 0, geom);

                BoundingBox boundingBox = new BoundingBox();
                for (int i = 0; i < vdata.Length; i++)
                {
                    var position =  vdata[i].Position;
                    vdata[i].Position = position;
                    boundingBox.Merge(position);

                    var normal = vdata[i].Normal;
                    vdata[i].Normal = normal;
                }
                model.BoundingBox = boundingBox;

                Material GrayLineMaterial = Material.FromColor(Color.Gray);
                GrayLineMaterial.SetTechnique(0, CoreAssets.Techniques.NoTextureUnlit, 1, 1);
                GrayLineMaterial.CullMode = CullMode.Cw;
                GrayLineMaterial.LineAntiAlias = true;

                Material BlackLineMaterial = Material.FromColor(Color.Black);
                BlackLineMaterial.SetTechnique(0, CoreAssets.Techniques.DiffNormal, 0, 0);
                BlackLineMaterial.CullMode = CullMode.Cw;
                BlackLineMaterial.LineAntiAlias = true;

                Model = model;
                //SetMaterial(BlackLineMaterial);
                SetMaterial(isLineList ? BlackLineMaterial : GrayLineMaterial);
                
                //SetMaterial(material.Clone());
                CastShadows = true;

                //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
                var halfSize = WorldBoundingBox.HalfSize;
                var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));
                Node.ScaleNode(1f / scaleFactor);

                //position model so world bounding box is centered on origin
                Node.Position = WorldBoundingBox.Center * -1;
            });
            return true;
        }

        public Urho.BoundingBox GetBoundingBox(Result result)
        {
            float minx, miny, minz, maxx, maxy, maxz;

            minx = miny = minz = float.MaxValue;
            maxx = maxy = maxz = float.MinValue;

            foreach (var v in result.PointsAndVectors)
            {
                minx = Math.Min(minx, v.X);
                miny = Math.Min(miny, v.Y);
                minz = Math.Min(minz, v.Z);
                maxx = Math.Max(maxx, v.X);
                maxy = Math.Max(maxy, v.Y);
                maxz = Math.Max(maxz, v.Z);
            }

            return new Urho.BoundingBox(
                new Urho.Vector3(minx, miny, minz),
                new Urho.Vector3(maxx, maxy, maxz));
        }

        public async Task<bool> LoadMesh(string modelPath, bool fromResource)
        {
            var mesh = new Mesh();

            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();

            // do the mesh loading in a thread because it takes a while
            await Task.Run(async () =>
            {
                var result = await mesh.Load(modelPath, fromResource);
                completion.SetResult(result);
            });

            await completion.Task;

            if (!completion.Task.Result)
            {
                Debug.WriteLine($"Failed to load mesh at {modelPath}");
                return false;
            }
            Urho.Application.InvokeOnMain(() =>
            {
                var vb = new VertexBuffer(Context, false);
                var ib = new IndexBuffer(Context, false);

                var geom = new Geometry();

                var vdata = mesh.GetVertextData();
                var idata = mesh.GetIndexData();

                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                vb.Shadowed = true;
                vb.SetSize((uint)mesh.Vertices.Count, ElementMask.Position | ElementMask.Normal | ElementMask.Color, false);
                vb.SetData(vdata);

                ib.Shadowed = true;
                ib.SetSize((uint)mesh.Triangles.Count * 3, true);
                ib.SetData(idata);

                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(PrimitiveType.TriangleList, 0, (uint)mesh.Triangles.Count * 3, true);

                var model = new Model();
                model.NumGeometries = 1;
                model.SetGeometry(0, 0, geom);
                model.BoundingBox = mesh.GetBoundingBox();

                var material = new Material();

                // full no alpha
                //material.SetTechnique(0, CoreAssets.Techniques.NoTextureVCol, 1, 1);

                // with alpha
                material.SetTechnique(0, CoreAssets.Techniques.NoTextureVColAddAlpha, 1, 1);
                material.SetShaderParameter("MatDiffColor", Color.Green);

                Model = model;
                SetMaterial(material.Clone());
                CastShadows = true;

                //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
                var halfSize = WorldBoundingBox.HalfSize;
                var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));
                Node.ScaleNode(1f / scaleFactor);

                //position model so world bounding box is centered on origin
                Node.Position = WorldBoundingBox.Center * -1;
            });
            return true;

        }

        public async Task<bool> LoadMesh(byte[] modelData)
        {
            var mesh = new Mesh();

            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();

            // do the mesh loading in a thread because it takes a while
            await Task.Run(() =>
            {
                var result = mesh.Load(modelData);
                completion.SetResult(result);
            });

            await completion.Task;

            if(!completion.Task.Result)
            {
                Debug.WriteLine($"Failed to load mesh at {modelData}");
                return false;
            }                

            var vb = new VertexBuffer(Context, false);
            var ib = new IndexBuffer(Context, false);
            var geom = new Geometry();

            var vdata = mesh.GetVertextData();
            var idata = mesh.GetIndexData();

            // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
            vb.Shadowed = true;
            vb.SetSize((uint)mesh.Vertices.Count, ElementMask.Position | ElementMask.Normal | ElementMask.Color, false);
            vb.SetData(vdata);

            ib.Shadowed = true;
            ib.SetSize((uint)mesh.Triangles.Count * 3, true);
            ib.SetData(idata);

            geom.SetVertexBuffer(0, vb);
            geom.IndexBuffer = ib;
            geom.SetDrawRange(PrimitiveType.TriangleList, 0, (uint)mesh.Triangles.Count * 3, true);

            var model = new Model();
            model.NumGeometries = 1;
            model.SetGeometry(0, 0, geom);
            model.BoundingBox = mesh.GetBoundingBox();

            var material = new Material();

            // full no alpha
            material.SetTechnique(0, CoreAssets.Techniques.NoTextureVCol, 1, 1);

            // with alpha
            //material.SetTechnique(0, CoreAssets.Techniques.NoTextureVColAddAlpha, 1, 1);
            //material.SetShaderParameter("MatDiffColor", new Urho.Color(1f, 1f, 1f, 0.3f));

            Model = model;
            SetMaterial(material.Clone());
            CastShadows = true;

            //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
            var halfSize = WorldBoundingBox.HalfSize;
            var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));
            Node.ScaleNode(1f / scaleFactor);

            //position model so world bounding box is centered on origin
            Node.Position = WorldBoundingBox.Center * -1;

            return true;

        }

        public async Task<bool> LoadTexturedMesh(string modelPath, string texturePath, bool fromResource)
        {
            var mesh = new Mesh();

            Texture2D texture = null;

            TaskCompletionSource<bool> completion = new TaskCompletionSource<bool>();

            // do the mesh loading in a thread because it takes a while
            Task.Run(async () =>
            {
                var result = await mesh.Load(modelPath, fromResource);
                if (!result)
                {
                    completion.SetResult(result);
                    return;
                }

                texture = mesh.LoadTexture(texturePath, true);
                completion.SetResult(result);
            });

            await completion.Task;

            if (!completion.Task.Result)
            {
                Debug.WriteLine($"Failed to load mesh at {modelPath}");
                return false;
            }

            if(texture == null)
            {
                Debug.WriteLine($"Failed to load texture at {texturePath}");
                return false;
            }
            Urho.Application.InvokeOnMain(() =>
            {
                var vb = new VertexBuffer(Context, false);
                var ib = new IndexBuffer(Context, false);
                var geom = new Geometry();

                var vdata = mesh.GetTexturedVertextData();
                var idata = mesh.GetIndexData();

                // Shadowed buffer needed for raycasts to work, and so that data can be automatically restored on device loss
                vb.Shadowed = true;
                vb.SetSize((uint)mesh.Vertices.Count, ElementMask.Position | ElementMask.Normal | ElementMask.Color | ElementMask.TexCoord1, false);
                vb.SetData(vdata);

                ib.Shadowed = true;
                ib.SetSize((uint)mesh.Triangles.Count * 3, true);
                ib.SetData(idata);

                geom.SetVertexBuffer(0, vb);
                geom.IndexBuffer = ib;
                geom.SetDrawRange(PrimitiveType.TriangleList, 0, (uint)mesh.Triangles.Count * 3, true);

                var model = new Model();
                model.NumGeometries = 1;
                model.SetGeometry(0, 0, geom);
                model.BoundingBox = mesh.GetBoundingBox();

                var material = new Material();
                material.SetTexture(TextureUnit.Diffuse, texture);
                material.SetTechnique(0, CoreAssets.Techniques.DiffUnlit, 0, 0);

                Model = model;
                SetMaterial(material.Clone());
                CastShadows = true;

                //scaling model node to fit in a 2x2x2 space (not taking orientation of model into account)
                var halfSize = WorldBoundingBox.HalfSize;
                var scaleFactor = System.Math.Max(halfSize.X, System.Math.Max(halfSize.Y, halfSize.Z));
                Node.ScaleNode(1f / scaleFactor);

                //position model so world bounding box is centered on origin
                Node.Position = WorldBoundingBox.Center * -1;
            });
            return true;
        }
    }
}
