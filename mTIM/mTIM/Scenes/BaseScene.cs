using System;
using Urho;

namespace mTIM.Scenes
{
    public class BaseScene
    {
        private int width;
        private int height;
        public Scene Scene;
        public Camera Camera;
        public Node CameraNode { get; set; }
        public event EventHandler<TouchEventArgs> OnEndEvaluateNode;

        public BaseScene(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public void InitScene()
        {
            Scene = new Scene();
            Scene.CreateComponent<Octree>();
        }

        public void CreateCamera(Vector3 vector3)
        {
            CameraNode = Scene.CreateChild("Camera");
            CameraNode.Position = vector3;
            Camera = CameraNode.CreateComponent<Camera>();
            Camera.Orthographic = true;

            Camera.OrthoSize = (float)Application.Current.Graphics.Height * Application.PixelSize;

            Camera.Zoom = 1f;

            Urho.Node lightNode = CameraNode.CreateChild("lightNode");
            var _light = lightNode.CreateComponent<Light>();
            _light.LightType = LightType.Directional;
            _light.Range = 100f;
            _light.Brightness = 0.9f;

            CameraNode.LookAt(Vector3.Zero, Vector3.Down, TransformSpace.World);
        }

        public void SetupViewport()
        {
            var renderer = Application.Current.Renderer;
            Viewport vp = new Viewport(Application.Current.Context, Scene, CameraNode.GetComponent<Camera>());
            renderer.SetViewport(0, vp);
            vp.SetClearColor(Color.White);
        }

        public void RaiseInpuPutEndEvent(TouchEventArgs args)
        {
            OnEndEvaluateNode?.Invoke(this, args);
        }

        public virtual void OnUpdate(float timeStep)
        {
        }

        public virtual void Destroy()
        {
        }
    }
}

