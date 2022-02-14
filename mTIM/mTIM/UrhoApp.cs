using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using mTIM.Components;
using mTIM.Helpers;
using mTIM.Models.D;
using mTIM.ViewModels;
using Urho;
using Urho.Actions;
using Urho.Gui;

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

        public Text SelectedElement;

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

            CreateText();
            Input.TouchBegin += Input_TouchBegin;
            Input.TouchEnd += Input_TouchEnd;
        }

        #region Initialisation

        public void CreateText()
        {
            // Create Text Element
            SelectedElement = new Text()
            {
                Value = "Hello World!",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            SelectedElement.SetColor(Color.Black);
            SelectedElement.FontSize = 20;
            //text.SetFont(font: ResourceCache.GetFont("Fonts/Anonymous Pro.ttf"), size: 30);
            // Add to UI Root
            UI.Root.AddChild(SelectedElement);
        }


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

        public async void AddStuff()
        {
            _rootNode.RemoveAllChildren();
            this.AddChild<WorldInputHandler>("inputs");
            model = this.AddChild<ObjectModel>("model");
            var chaildmodel = this.AddChild<ObjectModel>("chaildmodel");

            //await model.LoadMesh("mTIM.Meshes.Cube.obj", true);
            if (!FileHelper.IsFileExists(GlobalConstants.GraphicsBlob_FILE))
            {
                return;
            }
            var compressedData = await FileHelper.ReadAllBytesAsync(GlobalConstants.GraphicsBlob_FILE);
            if (compressedData != null && compressedData.Length > 0)
            {
                var result = GZipHelper.DeserializeResult(compressedData);
                Debug.WriteLine(result.Geometries.Count());
                var mesh = CreateMesh(result);
                if (mesh != null)
                {
                    chaildmodel.LoadMesh(mesh, true);
                    model.LoadMesh(mesh);
                }
            }
            //model.LoadModel("HoverBike.mdl", null);
        }

        private T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

        public TimMesh CreateMesh(Result result)
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

        private void AddCameraAndLight()
        {
            var cameraNode = _scene.CreateChild("cameraNode");
            _camera = cameraNode.CreateComponent<Camera>();
            _camera.Orthographic = true;

            _camera.OrthoSize = (float)Application.Current.Graphics.Height * Application.PixelSize;

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

            var renderer = Application.Current.Renderer;
            Viewport vp = new Viewport(Application.Current.Context, Scene, _camera);
            renderer.SetViewport(0, vp);
            vp.SetClearColor(Color.White);

            //var renderer = Renderer;
            //renderer.DefaultZone.FogColor = Color.White;
            //renderer.SetViewport(0, new Viewport(Context, _scene, _camera, null));

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

        private async void Input_TouchBegin(TouchBeginEventArgs obj)
        {
            Debug.WriteLine($"Input_TouchBegin {obj.X},{obj.Y}");

            Ray cameraRay = Camera.GetScreenRay((float)obj.X / Graphics.Width, (float)obj.Y / Graphics.Height);
            var results = Octree.Raycast(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry);

            TouchedNode = results.Select(x => x.Node).FirstOrDefault();

            if (TouchedNode != null)
            {
                Debug.WriteLine($"Input Touch : "+ TouchedNode.Name);
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
