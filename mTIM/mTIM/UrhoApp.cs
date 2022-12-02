using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using mTIM.Components;
using mTIM.Helpers;
using mTIM.Models;
using mTIM.Models.D;
using mTIM.ViewModels;
using Urho;
using Urho.Gui;
using Urho.Resources;

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
        private Window window;

        private Scene _scene;
        private Octree _octree;
        private Urho.Node _rootNode;
        private Camera _camera;
        private Light _light;
        private Urho.Node _cameraNode;

        public Text SelectedElement;

        private ObjectModel model;
        private ObjectModel linesModel;

        private TimMesh timMesh { get; set; }
        private string selectedNodeName { get; set; }

        public Vector3 CameraPosition => new Vector3(0, 0, 6);
        private XmlFile style;

        private readonly string tag = "UrhoApp";


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
            _camera.Zoom = 1f;
        }

        protected override void Start()
        {
            base.Start();
            intializeAppcenter();
            InitScene();

            AddCameraAndLight();

            SetupViewport();

            AddStuff();

            // Load XML file containing default UI style sheet
            var cache = ResourceCache;
            style = cache.GetXmlFile("UI/DefaultStyle.xml");

            // Set the loaded style as default style
            this.UI.Root.SetDefaultStyle(style);

            CreateText();
            Input.TouchBegin += Input_TouchBegin;
            Input.TouchEnd += Input_TouchEnd;
        }

        private void intializeAppcenter()
        {
            AppCenter.Configure("android={8a2c28d4-8c5b-40c1-8601-14bd82a1aeee};" +
                  "uwp={9e9b5e10-0956-4fb5-84de-4df2f3bc3f60};" +
                  "ios={f28f0f42-4da6-48f0-8826-4bb82f174985};");
            if (AppCenter.Configured)
            {
                AppCenter.Start(typeof(Analytics));
                AppCenter.Start(typeof(Crashes));
            }
        }

        #region Initialisation
        private bool isButtonsActive;
        private bool optionTwoSelected;

        /// <summary>
        /// To show the buttons in model window.
        /// </summary>
        public void ShowButtonsWindow()
        {
            if (isButtonsActive)
                return;
            optionTwoSelected = false;
            isButtonsActive = true;
            // Create the Window and add it to the UI's root node
            window = new Window();
            this.UI.Root.AddChild(window);

            // Set Window size and layout settings
            window.SetMinSize(150, 50);
            window.SetLayout(LayoutMode.Horizontal, 6, new IntRect(6, 6, 6, 6));
            window.SetAlignment(HorizontalAlignment.Right, VerticalAlignment.Top);
            window.SetPosition(-60, 30);
            window.Name = "Window";
            window.LayoutBorder = new IntRect(4, 4, 4, 4);

            // Create the Window's button1
            var button1 = new Button();
            button1.Name = "button1";
            button1.SetMinSize(90, 70);
            button1.SetColor(Color.FromHex("#ECEFF0"));
            var buttonText = new Text();
            buttonText.Name = "1";
            buttonText.Value = "1";
            buttonText.TextEffect = TextEffect.Stroke;
            buttonText.SetColor(Color.Black);
            buttonText.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 30);
            buttonText.SetAlignment(HorizontalAlignment.Center, VerticalAlignment.Center);
            button1.AddChild(buttonText);

            // Create the Window's  button2
            Button button2 = new Button();
            button2.Name = "button2";
            button2.SetMinSize(90, 70);
            var button2Text = new Text();
            button2Text.Name = "2";
            button2Text.Value = "2";
            button2Text.TextEffect = TextEffect.Stroke;
            button2Text.SetColor(Color.Black);
            button2Text.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 30);
            button2Text.SetAlignment(HorizontalAlignment.Center, VerticalAlignment.Center);
            button2.AddChild(button2Text);

            // Add the controls to the title bar
            window.AddChild(button1);
            window.AddChild(button2);


            // Apply styles
            window.SetStyleAuto(null);
            button1.SetStyle("button1", null);
            button2.SetStyle("button2", null);

            button1.Released += args =>
            {
                optionTwoSelected = false;
                button1.SetColor(Color.FromHex("#ECEFF0"));
                button2.SetColor(Color.White);
                UpdateElements(selectedNodeName);
                linesModel?.ApplyColor(Color.Black);
            };

            button2.Released += args =>
            {
                optionTwoSelected = true;
                button2.SetColor(Color.FromHex("#ECEFF0"));
                button1.SetColor(Color.White);
                ShowOnlyActiveElements(selectedNodeName);
                linesModel?.ApplyColor(Color.Transparent);
            };
        }


        public void ShowOnlyActiveElements(string touchNodeName)
        {
            selectedNodeName = touchNodeName;
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
                        if (element.Name == touchNodeName && elementID > 1)
                        {
                            objectModel.UpdateMaterial(true);
                        }
                        else
                        {
                            objectModel.ApplyColor(Color.Transparent);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// To hide the buttons
        /// </summary>
        public void HideButtonsWindow()
        {
            if (window != null)
                this.UI.Root.RemoveChild(window);
            isButtonsActive = false;
        }

        Text textElement;
        public void CreateText()
        {
            // Create Text Element
            textElement = new Text()
            {
                Value = ViewModel?.SelectedItemText,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom,
                ClipChildren = true,
                Enabled = true,
                LayoutSpacing = 6
            };
            textElement.TextEffect = TextEffect.Stroke;
            textElement.SetColor(Color.Black);
            textElement.SetFont(font: CoreAssets.Fonts.AnonymousPro, size: 30);
            // Add to UI Root
            this.UI.Root.AddChild(textElement);
        }

        private int ScaleFontSize(int fontSize)
        {
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS)
            {
                fontSize = (fontSize / 2 + 1);
            }
            return fontSize;
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
            CrashReportManager.ReportError(e.Exception, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
        }

        private void Engine_PostRenderUpdate(PostRenderUpdateEventArgs obj)
        {
            // If draw debug mode is enabled, draw viewport debug geometry, which will show eg. drawable bounding boxes and skeleton
            // bones. Note that debug geometry has to be separately requested each frame. Disable depth test so that we can see the
            // bones properly
            if (this.IsActive)
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
        /// To check the element is already added to window or not. 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool IsElementAvailable(int id)
        {
            var node = _rootNode.Children?.Where(x => x.Name.Equals(id.ToString())).FirstOrDefault();
            if (node != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// To draw the lines
        /// </summary>
        /// <param name="mesh"></param>
        public void LoadLinesDrawing(TimMesh mesh)
        {
            try
            {
                if (mesh != null && !mesh.IsLoaded)
                {
                    linesModel = this.AddChild<ObjectModel>("linesModel");
                    linesModel.LoadLinesMesh(mesh, true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
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
                    timMesh = mesh;
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
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        /// <summary>
        /// To load the each element from mesh
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="isActive"></param>
        /// <param name="skipElements"></param>
        public void LoadEelementsDrawing(TimMesh mesh, bool isActive, int skipElements = 0, bool isParent = false)
        {
            try
            {
                if (mesh != null)
                {
                    timMesh = mesh;
                    mesh.IsLoaded = true;
                    var elements = mesh.elementMeshes.Skip(skipElements);
                    foreach (var element in elements)
                    {
                        if (_rootNode != null && IsElementAvailable(element.listId))
                        {
                            var node = _rootNode.Children?.Where(x => x.Name.Equals(element.listId.ToString())).FirstOrDefault();
                            var objectModel = (ObjectModel)node?.Components?.FirstOrDefault();
                            if (objectModel != null)
                            {
                                objectModel.UpdateMaterial(isParent ? true : !TimTaskListHelper.IsParent(element.listId));
                            }
                            //UpdateElements(element.listId.ToString());
                        }
                        else
                        {
                            var model = this.AddChild<ObjectModel>(element.listId.ToString());
                            model.LoadElementMesh(mesh, element, isActive);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
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

            UnhandledException += UrhoViewApp_UnhandledException;
#if DEBUG
            Engine.PostRenderUpdate += Engine_PostRenderUpdate;
#endif
        }

        #endregion Initialisation
        #region Input Touches

        private void Input_TouchBegin(TouchBeginEventArgs obj)
        {
            try
            {
                if (optionTwoSelected)
                    return;
                //Debug.WriteLine($"Input_TouchBegin {obj.X},{obj.Y}");
                Ray cameraRay = Camera.GetScreenRay((float)obj.X / Graphics.Width, (float)obj.Y / Graphics.Height);
                var result = Octree.RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100, DrawableFlags.Geometry);
                if (result != null)
                {
                    TouchedNode = result.Value.Node;
                    if (TouchedNode != null)
                    {
                        Debug.WriteLine($"Input Touch Node name: " + TouchedNode.Name);
                        Debug.WriteLine(String.Format("Input Touch Position: {0} {1} {2}", result.Value.Position.X, result.Value.Position.Y, result.Value.Position.Z));
                        int value = TryGetNumber(TouchedNode.Name);
                        if (!TimTaskListHelper.IsParent(value))
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
                CrashReportManager.ReportError(ex, System.Reflection.MethodBase.GetCurrentMethod().Name, tag);
            }
        }

        private void Input_TouchEnd(TouchEndEventArgs obj)
        {
            if (optionTwoSelected)
                return;
            Debug.WriteLine("TouchPositionX: " + obj.X + "TouchPositionY: " + obj.Y);
            int id = TryGetNumber(TouchedNode?.Name);
            if (!TimTaskListHelper.IsParent(id))
            {
                //var worldPosition = _camera.ScreenToWorldPoint(TouchedNode.Position);
                //var screenPosition = _camera.WorldToScreenPoint(TouchedNode.Position);
                //Debug.WriteLine("World PositionX: " + worldPosition.X + "World PositionY: " + worldPosition.Y);
                //Debug.WriteLine("Screen PositionX: " + screenPosition.X + "Screen PositionY: " + screenPosition.Y);

                //float xClip = (screenPosition.X + 0.5f) / (Graphics.Width / 2) - 1.0f;
                //float yClip = 1.0f - (screenPosition.Y + 0.5f) / (Graphics.Height / 2);
                //Debug.WriteLine("Window PositionX: " + xClip + " Window PositionY: " + yClip);

                ZoomOut();
                UpdateElements(TouchedNode.Name);
                ShowButtonsWindow();
                ViewModel.SlectedElementPositionIn3D(id);
                MoveToPosition(3000, obj.X, obj.Y);
            }
            TouchedNode = null;
        }

        #endregion

        /// <summary>
        /// This is used to ZoomOut and ZoomIn the selected position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void ZoomIn()
        {
            Urho.Application.InvokeOnMainAsync(() =>
            {
                ObjectAnimation cameraAnimation = new ObjectAnimation();

                ValueAnimation zoomAnimation = new ValueAnimation();
                zoomAnimation.InterpolationMethod = InterpMethod.Linear;
                zoomAnimation.SetKeyFrame(0.0f, Camera.Zoom);
                zoomAnimation.SetKeyFrame(0.3f, 0.3f);
                zoomAnimation.SetKeyFrame(0.7f, 3f);

                cameraAnimation.AddAttributeAnimation("Zoom", zoomAnimation, WrapMode.Once, 0.8f);
                Camera.AnimationEnabled = true;
                Camera.ObjectAnimation = cameraAnimation;
            });
        }

        /// <summary>
        /// This is used to ZoomOut
        /// </summary>
        protected void ZoomOut()
        {
            ObjectAnimation cameraAnimation = new ObjectAnimation();

            ValueAnimation zoomAnimation = new ValueAnimation();
            zoomAnimation.InterpolationMethod = InterpMethod.Linear;
            zoomAnimation.SetKeyFrame(0.0f, Camera.Zoom);
            zoomAnimation.SetKeyFrame(0.3f, 1f);

            cameraAnimation.AddAttributeAnimation("Zoom", zoomAnimation, WrapMode.Once, 0.8f);
            Camera.AnimationEnabled = true;
            Camera.ObjectAnimation = cameraAnimation;
        }

        /// <summary>
        /// Move to the perticular position
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        protected void MoveToPosition(int delta, int x, int y)
        {
            Urho.Application.InvokeOnMainAsync(() =>
            {
                var viewPort = Renderer.GetViewport(0);

                // 3d mouse location before the zoom
                var mouseV = viewPort.ScreenToWorldPoint(x, y, CameraPosition.Z);

                Camera.Zoom += delta * 0.003f;

                // zoom out no panning
                if (delta < 0)
                    return;

                // 3d mouse location after the zoom
                var mouseV2 = viewPort.ScreenToWorldPoint(x, y, CameraPosition.Z);

                var diff = mouseV2 - mouseV;

                RootNode.Position += diff;

                ZoomIn();
            });
        }

        /// <summary>
        /// Updating the drawing file based on touch selection without refreshing the entire model.
        /// </summary>
        public void UpdateElements(string touchNodeName)
        {
            selectedNodeName = touchNodeName;
            foreach (var element in _rootNode.Children)
            {
                Debug.WriteLine(element.Name);
                var elementID = TryGetNumber(element.Name);
                if (elementID > 0)
                {
                    var elementMesh = timMesh.elementMeshes?.Where(x => x.listId.Equals(elementID)).FirstOrDefault();
                    var objectModel = (ObjectModel)element?.Components?.FirstOrDefault();
                    if (objectModel != null)
                    {
                        if (optionTwoSelected)
                        {
                            if (element.Name == touchNodeName && elementID > 1)
                            {
                                objectModel.UpdateMaterial(true);
                            }
                            else
                            {
                                objectModel.ApplyColor(Color.Transparent);
                            }
                        }
                        else
                        {
                            //if (element.Name == touchNodeName && elementID > 1)
                            //{
                            //    UpdateSelectedPosition(element);
                            //}
                            objectModel.UpdateMaterial(element.Name == touchNodeName && elementID > 1);
                        }
                    }
                }
            }
        }

        public void UpdateSelectedPosition(Node element)
        {
            Urho.Application.InvokeOnMainAsync(() =>
            {
                Debug.WriteLine(element.Name);
                var elementID = TryGetNumber(element.Name);
                if (elementID > 0)
                {
                    Vector2 screenPosition = new Vector2();
                    var elementMesh = timMesh.elementMeshes?.Where(x => x.listId.Equals(elementID)).FirstOrDefault();
                    if (elementMesh != null)
                    {
                        //Debug.WriteLine(String.Format("{0},{1},{2},{3}", elementMesh?.aabb.Minimum.X, elementMesh?.aabb.Minimum.Y, elementMesh?.aabb.Maximum.X, elementMesh?.aabb.Maximum.Y));
                        var elementPosition = (Vector3)elementMesh?.aabb.GetCenter();
                        Debug.WriteLine("PositionX: " + elementPosition.X + " PositionY: " + elementPosition.Y);
                        screenPosition = _camera.WorldToScreenPoint(elementPosition);
                    }
                    var objectModel = (ObjectModel)element?.Components?.FirstOrDefault();
                    if (objectModel != null)
                    {
                        ZoomOut();
                        ShowButtonsWindow();
                        Debug.WriteLine("PositionX: " + screenPosition.X + " PositionY: " + screenPosition.Y);
                        MoveToPosition(3000, (int)screenPosition.X, (int)screenPosition.Y);
                    }
                }
            });
        }

        /// <summary>
        /// To reset the drawing to original position.
        /// </summary>
        public void UpdateCameraPosition()
        {
            optionTwoSelected = false;
            _rootNode.SetWorldRotation(Quaternion.Identity);
            _rootNode.Position = new Vector3(0, 0, 0);
            if (Camera.Zoom > 1f)
                ZoomOut();
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
