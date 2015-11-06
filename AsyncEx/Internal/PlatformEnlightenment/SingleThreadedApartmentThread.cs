using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olan.AsyncEx.Internal.PlatformEnlightenment {
    public sealed class SingleThreadedApartmentThread {
        #region Fields

        private readonly object _thread;

        #endregion
        #region Constructors

        public SingleThreadedApartmentThread(Action execute, bool sta) {
            _thread = sta ? new ThreadTask(execute) : (object)Task.Factory.StartNew(execute, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        #endregion
        #region Methods

        public Task JoinAsync() {
            var ret = _thread as Task;
            if (ret != null) {
                return ret;
            }
            return ((ThreadTask)_thread).Task;
        }

        #endregion
        #region Nested type: ThreadTask

        private sealed class ThreadTask {
            #region Fields

            private readonly TaskCompletionSource<object> _tcs;
            private readonly Thread _thread;

            #endregion
            #region Constructors

            public ThreadTask(Action execute) {
                _tcs = new TaskCompletionSource<object>();
                _thread = new Thread(() => {
                    try {
                        execute();
                    }
                    finally {
                        _tcs.TrySetResult(null);
                    }
                });
                _thread.SetApartmentState(ApartmentState.STA);
                _thread.Name = "STA AsyncContextThread (Nito.AsyncEx)";
                _thread.Start();
            }

            #endregion
            #region Properties

            public Task Task => _tcs.Task;

            #endregion
        }

        #endregion
    }
}