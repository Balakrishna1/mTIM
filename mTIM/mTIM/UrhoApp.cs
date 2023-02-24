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
using mTIM.Scenes;
using mTIM.ViewModels;
using Urho;
using Urho.Gui;
using Urho.Resources;

namespace mTIM
{
    public class UrhoApp : Application
    {
        public BaseScene BaseScene => _mainScene;
        public Octree Octree => _octree;
        public Urho.Node RootNode => _rootNode;
        public Camera Camera => _mainScene.Camera;
        public bool IsInitialized => _mainScene != null;
        public bool OptionTwoSelected => optionTwoSelected;
        public Vector3 CameraPosition => new Vector3(0, 0, 6);

        private Window window;
        private BaseScene _mainScene;
        private Octree _octree;
        private Urho.Node _rootNode;
        private ObjectModel model;
        private ObjectModel linesModel;
        private TimMesh timMesh { get; set; }
        private string selectedNodeName { get; set; }
        private bool optionTwoSelected { get; set; }
        private XmlFile style;
        private readonly string tag = "UrhoApp";

        MainViewModel ViewModel = App.Current.MainPage.BindingContext as MainViewModel;

        [Preserve]
        public UrhoApp(ApplicationOptions options = null) : base(options)
        {
            _mainScene = null;
        }

        /// <summary>
        /// This is used to reset the window. 
        /// </summary>
        public void Reset()
        {
            if (!IsInitialized)
                return;
            _rootNode.RemoveAllChildren();
            _rootNode.SetWorldRotation(Quaternion.Identity);
            _mainScene.Camera.Zoom = 1f;
        }

        protected override void Start()
        {
            base.Start();
            Input.Enabled = true;
            Input.SetMouseVisible(true, false);
            Input.TouchEmulation = true;
            StartMainScene();
            AddStuff();

            // Load XML file containing default UI style sheet
            var cache = ResourceCache;
            style = cache.GetXmlFile("UI/DefaultStyle.xml");

            // Set the loaded style as default style
            this.UI.Root.SetDefaultStyle(style);
            CreateText();
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);
            _mainScene?.OnUpdate(timeStep);
        }

        #region Initialisation
        private bool isButtonsActive;

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
                optionTwoSelected = false;
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
            Urho.Application.InvokeOnMainAsync(() =>
            {
                textElement.Value = text;
            });
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

        /// <summary>
        /// This is used to create and set Scene,ViewPort and Camara to model view.
        /// </summary>
        private void StartMainScene()
        {
            _mainScene?.Destroy();
            Current.UI.Root.RemoveAllChildren();
            _mainScene = new MainScene(Graphics.Width, Graphics.Height);
            _rootNode = _mainScene.Scene.CreateChild("rootNode");
            _mainScene.OnEndEvaluateNode += _mainScene_OnEndEvaluateNode;
        }

        private void _mainScene_OnEndEvaluateNode(object sender, TouchEventArgs args)
        {
            if (optionTwoSelected)
                return;
            int id = TryGetNumber(args.Node?.Name);
            if (!TimTaskListHelper.IsParent(id))
            {
                ZoomOut();
                UpdateSelectedElement(args.Node.Name);
                ShowButtonsWindow();
                ViewModel.SlectedElementPositionIn3D(id);
                MoveToPosition(3000, args.X, args.Y);
            }
        }

        /// <summary>
        /// To Clear and Add the intial nodes.
        /// </summary>
        public void AddStuff()
        {
            _rootNode?.RemoveAllChildren();
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
                    slectedNode = null;
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

        #endregion Initialisation

        private ObjectModel slectedNode;
        public void UpdateSelectedElement(string name)
        {
            if (slectedNode == null)
            {
                UpdateElements(name);
            }
            else
            {
                slectedNode.UpdateMaterial(false);
                var slectedelememt = _rootNode.Children.Where(x => x.Name.Equals(name)).FirstOrDefault();
                if (slectedelememt != null)
                {
                    var objectModel = (ObjectModel)slectedelememt?.Components?.FirstOrDefault();
                    objectModel?.UpdateMaterial(true);
                    slectedNode = objectModel;
                }
            }
        }

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
                zoomAnimation.SetKeyFrame(0.7f, 2.5f);

                cameraAnimation.AddAttributeAnimation("Zoom", zoomAnimation, WrapMode.Once, 0.8f);
                Camera.AnimationEnabled = true;
                Camera.ObjectAnimation = cameraAnimation;
            });
        }

        /// <summary>
        /// This is used to ZoomOut
        /// </summary>
        internal void ZoomOut()
        {
            Urho.Application.InvokeOnMainAsync(() =>
            {
                ObjectAnimation cameraAnimation = new ObjectAnimation();

                ValueAnimation zoomAnimation = new ValueAnimation();
                zoomAnimation.InterpolationMethod = InterpMethod.Linear;
                zoomAnimation.SetKeyFrame(0.0f, Camera.Zoom);
                zoomAnimation.SetKeyFrame(0.3f, 1f);

                cameraAnimation.AddAttributeAnimation("Zoom", zoomAnimation, WrapMode.Once, 0.8f);
                Camera.AnimationEnabled = true;
                Camera.ObjectAnimation = cameraAnimation;
            });
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
                        screenPosition = _mainScene.Camera.WorldToScreenPoint(elementPosition);
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
            Urho.Application.InvokeOnMainAsync(() =>
            {
                if (_rootNode != null)
                {
                    _rootNode.SetWorldRotation(Quaternion.Identity);
                    _rootNode.Position = new Vector3(0, 0, 0);
                    if (Camera.Zoom > 1f)
                        ZoomOut();
                }
            });
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
