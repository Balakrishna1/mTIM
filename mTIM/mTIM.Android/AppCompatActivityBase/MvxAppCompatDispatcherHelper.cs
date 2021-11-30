using System;
using System.Text;
using Android.App;

namespace mTIM.Droid
{

    /// <summary>
    ///     Helper class for dispatcher operations on the UI thread in Android.
    /// </summary>
    /// <remarks>Original code by Laurent Bugnion</remarks>
    public static class MvxAppCompatDispatcherHelper
    {
        /// <summary>
        ///     Executes an action on the UI thread. If this method is called
        ///     from the UI thread, the action is executed immendiately. If the
        ///     method is called from another thread, the action will be enqueued
        ///     on the UI thread's dispatcher and executed asynchronously.
        /// </summary>
        /// <param name="action">
        ///     The action that will be executed on the UI
        ///     thread.
        /// </param>
        // ReSharper disable InconsistentNaming
        public static void CheckBeginInvokeOnUI(Action action)
        {
            if (action == null)
            {
                return;
            }

            Activity current = CheckDispatcher();
            current.RunOnUiThread(new Java.Lang.Runnable(action));
        }

        private static Activity CheckDispatcher()
        {
            if (MvxAppCompatActivityBase.CurrentActivity == null)
            {
                var error = new StringBuilder($"The {nameof(MvxAppCompatDispatcherHelper)} cannot be called.");
                error.AppendLine();
                error.Append($"Make sure that your main Activity derives from {nameof(MvxAppCompatActivityBase)}.");

                throw new InvalidOperationException(error.ToString());
            }

            return MvxAppCompatActivityBase.CurrentActivity;
        }
    }
}
