using System.Threading;

namespace Olan.AsyncEx.Internal.PlatformEnlightenment {
    public static class SynchronizationContextEnlightenment {
        #region Methods

        public static void SetCurrentSynchronizationContext(SynchronizationContext context) {
            SynchronizationContext.SetSynchronizationContext(context);
        }

        #endregion
    }
}