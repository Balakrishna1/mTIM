using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace mTIM.Helpers
{
    public class TouchHelper
    {
        public static TouchHelper _instance;
        public static TouchHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TouchHelper();
                }

                return _instance;
            }
        }

        public async Task TouchEffectsWithActionStruct<T>(View view,double scaleTo, uint length, T commandparams, Action<T> action) where T : struct 
        {
            await view.ScaleTo(scaleTo, length);
            action?.Invoke(commandparams);
            await view.ScaleTo(1, length);
        }

        public async Task TouchEffectsWithActionClass<T>(View view, double scaleTo, uint length, T commandparams, Action<T> action) where T : class
        {
            await view.ScaleTo(scaleTo, length);
            action?.Invoke(commandparams);
            await view.ScaleTo(1, length);
        }

        public async Task TouchEffectsWithCommandStruct<T>(View view, double scaleTo, uint length, ICommand command, T commandParams) where T : struct
        {
            await view.ScaleTo(scaleTo, length);
            command?.Execute(commandParams);
            await view.ScaleTo(1, length);
        }

        public async Task TouchEffectsWithCommand<T>(View view, double scaleTo, uint length, ICommand command, T commandParams = null) where T : class
        {
            await view.ScaleTo(scaleTo, length);
            command?.Execute(commandParams);
            await view.ScaleTo(1, length);
        }
    }
}
