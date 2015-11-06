using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Olan.AsyncEx.Internal.PlatformEnlightenment;

namespace Olan.AsyncEx {
    /// <summary>
    /// A thread that executes actions within an <see cref="AsyncContext"/>.
    /// </summary>
    [DebuggerTypeProxy(typeof (DebugView))]
    public sealed class AsyncContextThread : IDisposable {
        #region Fields

        /// <summary>
        /// The asynchronous context executed by the child thread.
        /// </summary>
        private readonly AsyncContext _context;
        /// <summary>
        /// The child thread.
        /// </summary>
        private readonly SingleThreadedApartmentThread _thread;

        /// <summary>
        /// A flag used to ensure we only call <see cref="AsyncContext.OperationCompleted"/> once during complex join/dispose operations.
        /// </summary>
        private int _stoppingFlag;

        #endregion
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContextThread"/> class, creating a child thread waiting for commands.
        /// </summary>
        public AsyncContextThread()
            : this(false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncContextThread"/> class, creating a child thread waiting for commands. If <paramref name="sta"/> is <c>true</c>, then the child thread is an STA thread (throwing <see cref="NotSupportedException"/> if the platform does not support STA threads).
        /// </summary>
        public AsyncContextThread(bool sta) {
            _context = new AsyncContext();
            _context.SynchronizationContext.OperationStarted();
            _thread = new SingleThreadedApartmentThread(Execute, sta);
        }

        #endregion
        #region Properties

        /// <summary>
        /// Gets the <see cref="AsyncContext"/> executed by this thread.
        /// </summary>
        public AsyncContext Context => _context;

        /// <summary>
        /// Gets the <see cref="TaskFactory"/> for this thread, which can be used to schedule work to this thread.
        /// </summary>
        public TaskFactory Factory => _context.Factory;

        #endregion
        #region Methods

        private void Execute() {
            using (_context) {
                _context.Execute();
            }
        }

        /// <summary>
        /// Permits the thread to exit, if we have not already done so.
        /// </summary>
        private void AllowThreadToExit() {
            if (Interlocked.CompareExchange(ref _stoppingFlag, 1, 0) == 0) {
                _context.SynchronizationContext.OperationCompleted();
            }
        }

        /// <summary>
        /// Requests the thread to exit and returns a task representing the exit of the thread. The thread will exit when all outstanding asynchronous operations complete.
        /// </summary>
        public Task JoinAsync() {
            AllowThreadToExit();
            return _thread.JoinAsync();
        }

        /// <summary>
        /// Requests the thread to exit.
        /// </summary>
        public void Dispose() {
            AllowThreadToExit();
        }

        #endregion
        #region Nested type: DebugView

        [DebuggerNonUserCode]
        internal sealed class DebugView {
            #region Fields

            private readonly AsyncContextThread _thread;

            #endregion
            #region Constructors

            public DebugView(AsyncContextThread thread) {
                _thread = thread;
            }

            #endregion
            #region Properties

            public AsyncContext Context => _thread.Context;

            public object Thread => _thread._thread;

            #endregion
        }

        #endregion
    }
}