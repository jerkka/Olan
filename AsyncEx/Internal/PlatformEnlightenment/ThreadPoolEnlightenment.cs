using System;
using System.Threading;

namespace Olan.AsyncEx.Internal.PlatformEnlightenment {
    public static class ThreadPoolEnlightenment {
        #region Methods

        public static IDisposable RegisterWaitForSingleObject(WaitHandle handle, Action<object, bool> callback, object state, TimeSpan timeout) {
            var registration = ThreadPool.RegisterWaitForSingleObject(handle, (innerState, timedOut) => callback(innerState, timedOut), state, timeout, true);
            return new WaitHandleRegistration(registration);
        }

        #endregion
        #region Nested type: WaitHandleRegistration

        private sealed class WaitHandleRegistration : IDisposable {
            #region Fields

            private readonly RegisteredWaitHandle _registration;

            #endregion
            #region Constructors

            public WaitHandleRegistration(RegisteredWaitHandle registration) {
                _registration = registration;
            }

            #endregion
            #region Methods

            public void Dispose() {
                _registration.Unregister(null);
            }

            #endregion
        }

        #endregion
    }
}