using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public Vector3 CameraPosition => new Vector3(0, 0, 6);


        MainViewModel ViewModel = App.Current.MainPage.BindingContext as MainViewModel;

        public UrhoApp(ApplicationOptions options = null) : base(options)
        {
            _scene = null;
        }

        /// <summary>
        /// This is used to reset the window. 
        /// </summary>
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

        /// <summary>
        /// To update the selected element name in the model window.
        /// </summary>
        /// <param name="text"></param>
        public void UpdateText(string text)
        {
            textElement.Value = text;
        }

        /// <summary>
        /// UnhandledException
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UrhoViewApp_UnhandledException(object sender, Urho.UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine("UnhandledException: " + e.Exception.Message);
            e.Handled = true;
        }

        private void Engine_PostRenderUpdate(PostRenderUpdateEventArgs obj)
        {
            // If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
            // bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
            // bones properly
            Renderer.DrawDebugGeometry(false);
        }

        /// <summary>
        /// Intialize the Scene and also creating the component and rootnode.
        /// </summary>
        private void InitScene()
        {
            _scene = new Scene();
            _octree = _scene.CreateComponent<Octree>();
            _rootNode = _scene.CreateChild("rootNode");
        }

        /// <summary>
        /// To Clear and Add the intial nodes.
        /// </summary>
        public void AddStuff()
        {
            _rootNode.RemoveAllChildren();
            this.AddChild<WorldInputHandler>("inputs");
            model = this.AddChild<ObjectModel>("model");
        }

        /// <summary>
        /// To draw the lines
        /// </summary>
        /// <param name="mesh"></param>
        public void LoadLinesDrawing(TimMesh mesh)
        {
            try
            {
                if (mesh != null)
                {
                    linesModel = this.AddChild<ObjectModel>("linesModel");
                    linesModel.LoadLinesMesh(mesh, true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// To update the Selected/Active elements in the model window.
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="fromIndex"></param>
        /// <param name="toIndex"></param>
        public void LoadActiveDrawing(TimMesh mesh, int fromIndex, int toIndex)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// To load the each element mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="isActive"></param>
        /// <param name="skipElements"></param>
        public void LoadEelementsDrawing(TimMesh mesh, bool isActive, int skipElements = 0)
        {
            try
            {
                if (mesh != null)
                {
                    var elements = mesh.elementMeshes.Skip(skipElements);
                    foreach (var element in elements)
                    {
                        var model = this.AddChild<ObjectModel>(element.listId.ToString());
                        model.LoadElementMesh(mesh, element, isActive);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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

        /// <summary>
        /// To Add the Camera and Light.
        /// </summary>
        private void AddCameraAndLight()
        {
            _cameraNode = _scene.CreateChild("cameraNode");
            _camera = _cameraNode.CreateComponent<Camera>();
            _camera.Orthographic = false;

            _camera.OrthoSize = (float)Application.Current.Graphics.Height * Application.PixelSize;

            _camera.Zoom = 1f;
            _cameraNode.Position = CameraPosition;

            Urho.Node lightNode = _cameraNode.CreateChild("lightNode");
            _light = lightNode.CreateComponent<Light>();
            _light.LightType = LightType.Point;
            _light.Range = 100;
            _light.Brightness = 0.9f;


            _cameraNode.LookAt(Vector3.Zero, Vector3.Down, TransformSpace.World);
        }

        /// <summary>
        /// SetupViewport
        /// </summary>
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
            int id = TryGetNumber(TouchedNode?.Name);
            if (id > 1)
            {
                UpdateElements();
                ViewModel.SlectedElementPositionIn3D(id);
            }
            TouchedNode = null;
        }

        /// <summary>
        /// Updating the drawing file based on touch selection without refreshing the entire model.
        /// </summary>
        private void UpdateElements()
        {
            foreach (var element in _rootNode.Children)
            {
                Debug.WriteLine(element.Name);
                var elementID = TryGetNumber(element.Name);
                if (elementID > 0)
                {
                    var objectModel = (ObjectModel)element?.Components?.FirstOrDefault();
                    if (objectModel != null)
                    {
                        //Debug.WriteLine($"Updated Node name: " + element.Name);
                        objectModel.UpdateMaterial((element.Name == TouchedNode.Name && elementID > 1));
                    }
                }
            }
        }

        /// <summary>
        /// This is used to get int from string.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private int TryGetNumber(string id)
        {
            int elementId;
            bool success = int.TryParse(id, out elementId);
            if (success)
                return elementId;
            else
                return -1;
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
                        int value = TryGetNumber(TouchedNode.Name);
                        if (value > 1)
                        {
                            var model = (ObjectModel)result.Value.Drawable;
                            model.UpdateSelection();
                        }
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

        public static void RemoveChild(this UrhoApp app, string label)
        {
            if (!app.IsInitialized)
                return;
            var node = app.RootNode.GetChild(label);
            app.RootNode.RemoveChild(node);
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
