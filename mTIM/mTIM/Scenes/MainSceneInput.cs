using System;
using mTIM.Components;
using mTIM.Helpers;
using System.Diagnostics;
using Urho;
using Urho.Actions;

namespace mTIM.Scenes
{
    internal class MainSceneInput
    {
        protected UrhoApp App => Application.Current as UrhoApp;
        private readonly MainScene _scene;

        public event EventHandler<TouchEventArgs> OnEvaluateNode;

        private bool isMoving = false;

        public MainSceneInput(MainScene scene)
        {
            _scene = scene;
        }

        private RayQueryResult? GetSelectedNode(float objX, float objY)
        {
            Ray cameraRay = _scene.CameraNode.GetComponent<Camera>().GetScreenRay(objX / Application.Current.Graphics.Width, objY / Application.Current.Graphics.Height);
            var result = _scene.Scene.GetComponent<Octree>().RaycastSingle(cameraRay, RayQueryLevel.Triangle, 100);
            return result;
        }

        private ObjectModel touchModel = null;
        public void Input_TouchBegin(TouchBeginEventArgs obj)
        {
            if (App.OptionTwoSelected)
                return;
            RayQueryResult? result = GetSelectedNode((float)obj.X, (float)obj.Y);
            if (result != null)
            {
                if (result?.Node != null)
                {
                    Debug.WriteLine($"Input Touch Node name: " + result?.Node.Name);
                    Debug.WriteLine(String.Format("Input Touch Position: {0} {1} {2}", result.Value.Position.X, result.Value.Position.Y, result.Value.Position.Z));
                    int value = TryGetNumber(result?.Node.Name);
                    if (!TimTaskListHelper.IsParent(value))
                    {
                        touchModel = (ObjectModel)result.Value.Drawable;
                        touchModel?.UpdateSelection();
                    }
                }
            }
        }

        public void Input_TouchMove(TouchMoveEventArgs obj)
        {
            isMoving = true;
            if (touchModel != null)
            {
                touchModel.UndoSelection();
            }
        }

        public void Input_TouchEnd(TouchEndEventArgs obj)
        {
            if (isMoving)
            {
                isMoving = false;
                return;
            }
            RayQueryResult? result = GetSelectedNode((float)obj.X, (float)obj.Y);
            if (result != null && result?.Node != null)
            {
                OnEvaluateNode?.Invoke(obj, new TouchEventArgs() { Node = result?.Node, X = obj.X, Y = obj.Y });
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

        #region Pan
        public enum PanDirection
        {
            Up,
            Down,
            Left,
            Right
        }

        private Vector3 GetVectorForDirection(PanDirection dir)
        {
            Vector3 direction = new Vector3(0, 0, 0);

            float factor = _scene.movementSize;
            switch (dir)
            {
                case PanDirection.Up:
                    direction = -factor * _scene.Scene.Up;
                    break;
                case PanDirection.Down:
                    direction = factor * _scene.Scene.Scene.Up;
                    break;
                case PanDirection.Left:
                    direction = -factor * _scene.Scene.Scene.Right;
                    break;
                case PanDirection.Right:
                    direction = factor * _scene.Scene.Scene.Right;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }
            return direction;
        }

        public void Pan(PanDirection dir)
        {
            var direction = GetVectorForDirection(dir);

            ValueAnimation panAnimation = new ValueAnimation();
            panAnimation.InterpolationMethod = InterpMethod.Linear;
            panAnimation.SetKeyFrame(0.0f, App.RootNode.Position);
            panAnimation.SetKeyFrame(_scene.movementSpeed, App.RootNode.Position + direction);

            ObjectAnimation mainNodeAnimation = new ObjectAnimation();
            mainNodeAnimation.AddAttributeAnimation("Position", panAnimation, WrapMode.Once, 1f);

            App.RootNode.ObjectAnimation = mainNodeAnimation;
        }
        #endregion Pan

        internal void Input_KeyUp(KeyUpEventArgs obj)
        {
            if (obj.Key == Key.I)
            {
                var camera = _scene.CameraNode.GetComponent<Camera>();
                camera.Zoom += 0.05f;
            }
            else if (obj.Key == Key.O)
            {
                var camera = _scene.CameraNode.GetComponent<Camera>();
                camera.Zoom -= 0.05f;
            }
            else if (obj.Key == Key.W)
            {
                Pan(PanDirection.Up);
            }
            else if (obj.Key == Key.S)
            {
                Pan(PanDirection.Down);
            }
            else if (obj.Key == Key.A)
            {
                Pan(PanDirection.Left);
            }
            else if (obj.Key == Key.D)
            {
                Pan(PanDirection.Right);
            }
        }
    }
}

