using System;
using System.Threading;

namespace Olan {
    public sealed class TaskCompletionOptions {
        #region Fields

        private int _timeout = -1;

        #endregion
        #region Constructors

        public TaskCompletionOptions(ThreadPriority threadPriority = ThreadPriority.Normal, CancellationToken? cancellationToken = null, System.Threading.Tasks.TaskCreationOptions taskCreationOptions = System.Threading.Tasks.TaskCreationOptions.HideScheduler | System.Threading.Tasks.TaskCreationOptions.DenyChildAttach) {
            ThreadPriority = threadPriority;
            CancellationToken = cancellationToken ?? CancellationToken.None;
            TaskCreationOptions = taskCreationOptions;
        }

        #endregion
        #region Properties

        public ThreadPriority ThreadPriority { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public System.Threading.Tasks.TaskCreationOptions TaskCreationOptions { get; set; }

        public int Timeout {
            get { return _timeout; }
            set {
                if (value < 0) {
                    throw new InvalidOperationException("The coroutine timeout must be positive.");
                }
                CancellationToken = new CancellationTokenSource(value).Token;
                _timeout = value;
            }
        }

        #endregion
    }
}