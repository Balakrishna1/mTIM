using System;
using System.Diagnostics;
using Urho;

namespace mTIM.Components
{
    public class WorldInputHandler: Component
    {
        protected UrhoApp App => Application.Current as UrhoApp;

        public WorldInputHandler()
        {
            ReceiveSceneUpdates = true;
        }

        public override void OnAttachedToNode(Node node)
        {
            base.OnAttachedToNode(node);
            //this.AddChild<RotationInput>("rotations");
        }

        protected override void OnUpdate(float timeStep)
        {
            base.OnUpdate(timeStep);

            var input = Application.Input;

            if ((input.GetMouseButtonDown(MouseButton.Left) || input.NumTouches == 1))
            {
                TouchState state = input.GetTouch(0);
                if (state.Pressure != 1.0)
                    return;

                App.RootNode.Rotate(new Quaternion(state.Delta.Y, -state.Delta.X, 0), TransformSpace.World);

            }
            else if (input.NumTouches == 2)
            {
                TouchState state1 = input.GetTouch(0);
                TouchState state2 = input.GetTouch(1);

                var distance1 = Distance(state1.Position, state2.Position);

                if (distance1 < 120f)
                {
                    // doing a pan
                    float factor = 0.005f;
                    App.RootNode.Position += new Vector3(-state1.Delta.X * factor, -state1.Delta.Y * factor, 0);
                }
                else
                {
                    var distance2 = Distance(state1.LastPosition, state2.LastPosition);

                    var pos1 = new Vector3(state1.Position.X, state1.Position.Y, 0);
                    var pos2 = new Vector3(state2.Position.X, state2.Position.Y, 0);

                    var v = (pos1 + pos2) / 2;

                    // doing a zoom
                    Zoom((int)(distance1 - distance2), (int)v.X, (int)v.Y);
                    //App.Camera.Zoom += (distance1 - distance2) * 0.01f;
                }


            }
        }

        float Distance(IntVector2 v1, IntVector2 v2)
        {
            return (float)Math.Sqrt((v1.X - v2.X) * (v1.X - v2.X) + (v1.Y - v2.Y) * (v1.Y - v2.Y));
        }

        public void RightMouseClickMoved(double xDelta, double yDelta)
        {
            Debug.WriteLine($"RightMouseClickMoved {xDelta}, {yDelta}");

            float factor = 0.005f;
            App.RootNode.Position += new Vector3((float)xDelta * factor, (float)yDelta * factor, 0);
        }

        public void MouseWheelPressed(int delta, double x, double y)
        {
            Zoom(delta, (int)x, (int)y);
        }

        #region Zoom

        protected void Zoom(int delta, int x, int y)
        {
            var viewPort = App.Renderer.GetViewport(0);

            // 3d mouse location before the zoom
            var mouseV = viewPort.ScreenToWorldPoint(x, y, App.CameraPosition.Z);

            App.Camera.Zoom += delta * 0.001f;

            // zoom out no panning
            if (delta < 0)
                return;

            // 3d mouse location after the zoom
            var mouseV2 = viewPort.ScreenToWorldPoint(x, y, App.CameraPosition.Z);

            var diff = mouseV2 - mouseV;

            App.RootNode.Position += diff;
        }

        #endregion Zoom
    }
}
