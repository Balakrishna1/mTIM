using Android.Content;
using Android.OS;
using Android.Runtime;

namespace mTIM.Droid
{
    [Register("mTIM.Droid.MvxAppCompatActivityBase")]
    public class MvxAppCompatActivityBase : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MvxAppCompatActivityBase CurrentActivity { get; private set; }
        public static bool IsKeyboardVisible { get; set; }
       
        protected override void OnCreate(Bundle bundle)
        {
            CurrentActivity = this;
            base.OnCreate(bundle);
        }

        protected override void OnStart()
        {
            CurrentActivity = this;
            base.OnStart();
        }

        protected override void OnResume()
        {
            CurrentActivity = this;
            base.OnResume();
        }

        public void MvxInternalStartActivityForResult(Intent intent, int requestCode)
        {
            StartActivityForResult(intent, requestCode);
        }
    }
}
