using System;
using Urho;

namespace mTIM.Scenes
{
    public class MainScene : BaseScene
    {
        private MainSceneInput _mainInput;
        public float movementSize = 0.8f;
        public float movementSpeed = 0.4f;

        public MainScene(int width, int height) : base(width, height)
        {
            CreateScene();
        }

        private void CreateScene()
        {
            InitScene();
            CreateCamera(new Vector3(0f, 0f, 6));

            _mainInput = new MainSceneInput(this);
            _mainInput.OnEvaluateNode += MainInput_OnEvaluateNode;

            SetupViewport();
            CreateEvents();
        }

        private void MainInput_OnEvaluateNode(object sender, TouchEventArgs args)
        {
            RaiseInpuPutEndEvent(args);
        }

        private void CreateEvents()
        {
            Application.Current.Input.TouchBegin += _mainInput.Input_TouchBegin;
            Application.Current.Input.TouchMove += _mainInput.Input_TouchMove;
            Application.Current.Input.TouchEnd += _mainInput.Input_TouchEnd;
            Application.Current.Input.KeyUp += _mainInput.Input_KeyUp;
        }

        public override void Destroy()
        {
            base.Destroy();
            Application.Current.Input.TouchBegin -= _mainInput.Input_TouchBegin;
            Application.Current.Input.TouchMove -= _mainInput.Input_TouchMove;
            Application.Current.Input.TouchEnd -= _mainInput.Input_TouchEnd;
            Application.Current.Input.KeyUp -= _mainInput.Input_KeyUp;
        }
    }
}

