using System;

namespace PubStar.Io
{
    class AdCallbacks
    {
        public Action OnLoaded;
        public Action<int> OnLoadError;
        public Action OnShowed;
        public Action<string> OnHidden;
        public Action<int> OnShowError;
    }

}