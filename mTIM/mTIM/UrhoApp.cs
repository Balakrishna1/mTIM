using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using mTIM.Components;
using mTIM.Helpers;
using mTIM.Models.D;
using mTIM.ViewModels;
using Urho;


namespace mTIM
{
    public class UrhoApp : Application
    {
        public Scene Scene => _scene;
        public Octree Octree => _octree;
        public Urho.Node RootNode => _rootNode;
        public Camera Camera => _camera;
        public Light Light => _light;
        public bool IsInitialized => _scene != null;
        public Urho.Node TouchedNode;

        private Scene _scene;
        private Octree _octree;
        private Urho.Node _rootNode;
        private Camera _camera;
        private Light _light;

        private ObjectModel model;

        public Vector3 CameraPosition => new Vector3(0, 0, 6);

        public UrhoApp(ApplicationOptions options = null) : base(options)
        {
            _scene = null;
        }

        public void Reset()
        {
            if (!IsInitialized)
                return;

            TouchedNode = null;
            _rootNode.RemoveAllChildren();
            _rootNode.SetWorldRotation(Quaternion.Identity);
            _camera.Zoom = 1f;
        }

        protected override void Start()
        {
            base.Start();

            InitScene();

            AddCameraAndLight();

            SetupViewport();

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                AddStuff();

            Input.TouchBegin += Input_TouchBegin;
            Input.TouchEnd += Input_TouchEnd;
        }

        #region Initialisation

        private void UrhoViewApp_UnhandledException(object sender, Urho.UnhandledExceptionEventArgs e)
        {
            e.Handled = true;
        }

        private void Engine_PostRenderUpdate(PostRenderUpdateEventArgs obj)
        {
            // If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
            // bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
            // bones properly
            Renderer.DrawDebugGeometry(false);
        }

        private void InitScene()
        {
            _scene = new Scene();
            _octree = _scene.CreateComponent<Octree>();
            //_scene.CreateComponent<DebugRenderer>();
            _rootNode = _scene.CreateChild("rootNode");
        }



        private async void AddStuff()
        {
            this.AddChild<WorldInputHandler>("inputs");
            if (!FileHelper.IsFileExists(GlobalConstants.GraphicsBlob_FILE))
            {
                return;
            }
            model = this.AddChild<ObjectModel>("model");
            //await model.LoadMesh("mTIM.Meshes.Model.obj", true);
            var compressedData = await FileHelper.ReadAllBytesAsync(GlobalConstants.GraphicsBlob_FILE);
            if (compressedData != null && compressedData.Length > 0)
            {
                var result = GZipHelper.DeserializeResult(compressedData);
                Debug.WriteLine(result.Geometries.Count());
                var mesh = CreateModel(result);
                model.LoadMesh(mesh);
            }
            //model.LoadModel("HoverBike.mdl", null);
            //}else
            //{
            //    var data = await FileHelper.ReadAllBytesAsync(GlobalConstants.GraphicsBlob_FILE);
            //    UpdateModel(data);
            //}
        }

        private T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }
        //public void LoadModelProtoClasses(byte[] data, int length, ref Result result)
        //{
        //    //var value = GZipHelper.Unzip(data);
        //    var chrValue = Convert.ToChar(data[0]);
        //    ProtoLoader loader = new ProtoLoader(chrValue, length) ;
        //    result.Read(loader);
        //}

        public TimMesh CreateModel(Result result)
        {
            TimMesh mesh = null;

            TimMeshLoader meshLoader = new TimMeshLoader();
            mesh = meshLoader.Load(result);
            if (mesh != null)
            {
                BaseViewModel VM = App.Current.MainPage.BindingContext as BaseViewModel;
                if (VM != null)
                {
                    int tc = VM.TotalListList.Count;
                    //taskListData.GetTaskCount();
                    for (int i = 0; i < tc; i++)
                    {
                        TimTaskModel td = VM.TotalListList[i];
                        td.aabb = AABB.EMPTY;
                        td.subMeshes.Clear();
                    }

                    for (int i = 0; i < mesh.subMeshes.Count(); i++)
                    {
                        TimSubMesh sm = mesh.subMeshes[i];
                        TimTaskModel current = VM.TotalListList.Where(x => x.Id.Equals(sm.listId)).FirstOrDefault();
                            //Logic.Instance().GetTaskListData().GetTaskById(sm.listId);
                        if (current != null)
                        {
                            current.aabb.Grow(sm.aabb);
                            current.subMeshes.Add(i);
                            //current.hasDetail[sm.simplificationLevel] = true;
                        }
                    }

                    PropagateAABB(VM, VM.TotalListList?.Where(x => x.Level.Equals(0)).FirstOrDefault());
                }

                return mesh;
            }

            return null;
        }

        public void PropagateAABB(BaseViewModel VM, TimTaskModel current)
        {
            List<TimTaskModel> children = VM.TotalListList.Where(x => x.Level.Equals(current.Level + 1) && x.Parent.Equals(current.Id)).ToList();
            for (int i = 0; i < children.Count(); i++)
            {
                TimTaskModel child = children[i];
                PropagateAABB(VM, child);
                if (!child.aabb.IsEmpty())
                {
                    current.aabb.Grow(child.aabb);
                }
            }
        }

        public void UpdateModel(byte[] data)
        {
            if (model == null)
                model = this.AddChild<ObjectModel>("model");

            //model.LoadModel(data);
        }

        private void AddCameraAndLight()
        {
            var cameraNode = _scene.CreateChild("cameraNode");
            _camera = cameraNode.CreateComponent<Camera>();
            _camera.OrthoSize = Graphics.Height * 0.01f/*PIXEL_SIZE*/; // Set camera ortho size (the value of PIXEL_SIZE is 0.01)

            cameraNode.Position = CameraPosition;

            Urho.Node lightNode = cameraNode.CreateChild("lightNode");
            _light = lightNode.CreateComponent<Light>();
            _light.LightType = LightType.Point;
            _light.Range = 100;
            _light.Brightness = 1f;


            cameraNode.LookAt(Vector3.Zero, Vector3.Up, TransformSpace.World);
        }

        void SetupViewport()
        {
            var renderer = Renderer;
            renderer.DefaultZone.FogColor = Color.Gray;
            renderer.SetViewport(0, new Viewport(Context, _scene, _camera, null));

            UnhandledException += UrhoViewApp_UnhandledException;
#if DEBUG
            Engine.PostRenderUpdate += Engine_PostRenderUpdate;
#endif
        }

        #endregion Initialisation

        private void Input_TouchEnd(TouchEndEventArgs obj)
        {
            TouchedNode = null;
        }

        private void Input_TouchBegin(TouchBeginEventArgs obj)
        {
            Debug.WriteLine($"Input_TouchBegin {obj.X},{obj.Y}");

            Ray cameraRay = Camera.GetScreenRay((float)obj.X / Graphics.Width, (float)obj.Y / Graphics.Height);
            var results = Octree.Raycast(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry);

            TouchedNode = results.Select(x => x.Node).FirstOrDefault();

            if (TouchedNode != null)
            {
                Debug.WriteLine($"Input Touch");
            }
        }
    }

    public static class UrhoHelpers
    {
        public static T AddChild<T>(this UrhoApp app, string label) where T : Component
        {
            if (!app.IsInitialized)
                return null;

            return app.RootNode.AddChild<T>(label);
        }

        public static T AddChild<T>(this Component component, string label) where T : Component
        {
            return component.Node?.AddChild<T>(label);
        }

        public static T AddChild<T>(this Urho.Node node, string label) where T : Component
        {
            var childNode = node.CreateChild(label);
            return childNode.CreateComponent<T>();
        }
    }
}
