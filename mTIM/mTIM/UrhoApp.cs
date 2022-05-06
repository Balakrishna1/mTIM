using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using mTIM.Components;
using mTIM.Models.D;
using mTIM.ViewModels;
using Urho;
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
        public Urho.Node CameraNode => _cameraNode;
        public Urho.Node TouchedNode;

        private Scene _scene;
        private Octree _octree;
        private Urho.Node _rootNode;
        private Camera _camera;
        private Light _light;
        private Urho.Node _cameraNode;

        public Text SelectedElement;

        private ObjectModel model;
        private ObjectModel linesModel;
        private ObjectModel inactiveModel;

        public Vector3 CameraPosition => new Vector3(1, 1, 5);


        MainViewModel ViewModel = App.Current.MainPage.BindingContext as MainViewModel;

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
            //_camera.Zoom = 1.1f;
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

        Text textElement;
        UIElement element;

        public void CreateText()
        {
            // Create Text Element
            textElement = new Text()
            {
                Value = ViewModel?.SelectedItemText,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                ClipChildren = true,
                Enabled = true
            };
            textElement.TextEffect = TextEffect.Stroke;
            textElement.SetColor(Color.Black);
            textElement.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 30);
            // Add to UI Root
            this.UI.Root.AddChild(textElement);

            var text = new Text()
            {
                Value = "1",
                Height = 50,
                Width = 50,
                SelectionColor = Color.Gray,
                EffectColor = Color.Gray,
                LayoutBorder = new IntRect(5, 5, 5, 5),
                ClipBorder = new IntRect(5, 5, 5, 5),
                LayoutMode = LayoutMode.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new IntVector2(-60, 30),
                ClipChildren = true,
                Enabled = true
            };
            text.TextEffect = TextEffect.Stroke;
            text.SetColor(Color.Black);
            text.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 30);
            //element = new UIElement();
            //element.AddChild(text);
            //element.HorizontalAlignment = HorizontalAlignment.Right;
            //element.VerticalAlignment = VerticalAlignment.Top;

            //this.UI.Root.AddChild(element);
        }

        public void UpdateText(string text)
        {
            textElement.Value = text;
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
            //var debugRenderer = _scene.CreateComponent<DebugRenderer>();
            //var physicsWorld = _scene.CreateComponent<PhysicsWorld>();
            //if (physicsWorld != null)
            //    physicsWorld.SetDebugRenderer(debugRenderer);
        }

        private void InitScene()
        {
            _scene = new Scene();
            _octree = _scene.CreateComponent<Octree>();
            _rootNode = _scene.CreateChild("rootNode");
        }

        public async void AddStuff()
        {
            _rootNode.RemoveAllChildren();
            this.AddChild<WorldInputHandler>("inputs");
            model = this.AddChild<ObjectModel>("model");
        }

        public void LoadInActiveDrawing(TimMesh mesh)
        {
            if (mesh != null)
            {
                inactiveModel = this.AddChild<ObjectModel>("inactiveModel");
                inactiveModel.LoadMesh(mesh, false);
            }
        }

        public void LoadLinesDrawing(TimMesh mesh)
        {
            if (mesh != null)
            {
                linesModel = this.AddChild<ObjectModel>("linesModel");
                linesModel.LoadMesh(mesh, true);
            }
        }

        public void LoadActiveDrawing(TimMesh mesh, int fromIndex, int toIndex)
        {
            if (mesh != null)
            {
                model.LoadMesh(mesh, fromIndex, toIndex);
                //var bbx = mesh.GetBoundingBox();
                //var max = new Vector3(bbx.Max.X, bbx.Max.Y, bbx.Max.Z);

                //await CameraNode.RunActionsAsync(new EaseInOut(new MoveBy(3, Vector3.Add(max, new Vector3(0f, 0f, 3f))), 10f));
                //CameraNode.LookAt(Scene.Children.Last().Position, Vector3.Up);
            }
        }

        private T Deserialize<T>(byte[] param)
        {
            using (MemoryStream ms = new MemoryStream(param))
            {
                IFormatter br = new BinaryFormatter();
                return (T)br.Deserialize(ms);
            }
        }

        private void AddCameraAndLight()
        {
            _cameraNode = _scene.CreateChild("cameraNode");
            _camera = _cameraNode.CreateComponent<Camera>();
            _camera.Orthographic = true;

            _camera.OrthoSize = (float)Application.Current.Graphics.Height * Application.PixelSize;

            _camera.Zoom = 1.1f;
            _cameraNode.Position = CameraPosition;

            Urho.Node lightNode = _cameraNode.CreateChild("lightNode");
            _light = lightNode.CreateComponent<Light>();
            _light.LightType = LightType.Point;
            _light.Range = 100;
            _light.Brightness = 0.9f;


            _cameraNode.LookAt(Vector3.Zero, Vector3.Down, TransformSpace.World);
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

        private void Input_TouchBegin(TouchBeginEventArgs obj)
        {
            try
            {
                Debug.WriteLine($"Input_TouchBegin {obj.X},{obj.Y}");

                Ray cameraRay = Camera.GetScreenRay((float)obj.X / Graphics.Width, (float)obj.Y / Graphics.Height);
                var result = Octree.RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry);
                if (result != null)
                {
                    TouchedNode = result.Value.Node;
                    if (TouchedNode != null)
                    {
                        Debug.WriteLine($"Input Touch Node name: " + TouchedNode.Name);
                        Debug.WriteLine($"Input Touch Position: {0} {1} {2}" + result.Value.Position.X, result.Value.Position.Y, result.Value.Position.Z);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Touch_Exception: { ex.Message}");
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
