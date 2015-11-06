using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olan.AsyncEx {
    /// <summary>
    /// A collection of cancelable <see cref="TaskCompletionSource{TResult}"/> instances. Implementations must be threadsafe <b>and</b> must work correctly if the caller is holding a lock.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="object"/>.</typeparam>
    public interface IAsyncWaitQueue<T> {
        #region Properties

        /// <summary>
        /// Gets whether the queue is empty.
        /// </summary>
        bool IsEmpty { get; }

        #endregion
        #region Methods

        /// <summary>
        /// Creates a new entry and queues it to this wait queue. The returned task must support both synchronous and asynchronous waits.
        /// </summary>
        /// <returns>The queued task.</returns>
        Task<T> Enqueue();

        /// <summary>
        /// Removes a single entry in the wait queue. Returns a disposable that completes that entry.
        /// </summary>
        /// <param name="result">The result used to complete the wait queue entry. If this isn't needed, use <c>default(T)</c>.</param>
        IDisposable Dequeue(T result = default(T));

        /// <summary>
        /// Removes all entries in the wait queue. Returns a disposable that completes all entries.
        /// </summary>
        /// <param name="result">The result used to complete the wait queue entries. If this isn't needed, use <c>default(T)</c>.</param>
        IDisposable DequeueAll(T result = default(T));

        /// <summary>
        /// Attempts to remove an entry from the wait queue. Returns a disposable that cancels the entry.
        /// </summary>
        /// <param name="task">The task to cancel.</param>
        /// <returns>A value indicating whether the entry was found and canceled.</returns>
        IDisposable TryCancel(Task task);

        /// <summary>
        /// Removes all entries from the wait queue. Returns a disposable that cancels all entries.
        /// </summary>
        IDisposable CancelAll();

        #endregion
    }

    /// <summary>
    /// Provides extension methods for wait queues.
    /// </summary>
    public static class AsyncWaitQueueExtensions {
        #region Methods

        /// <summary>
        /// Creates a new entry and queues it to this wait queue. If the cancellation token is already canceled, this method immediately returns a canceled task without modifying the wait queue.
        /// </summary>
        /// <param name="this">The wait queue.</param>
        /// <param name="token">The token used to cancel the wait.</param>
        /// <returns>The queued task.</returns>
        [Obsolete("Use the Enqueue overload that takes a synchronization object.")]
        public static Task<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, CancellationToken token) {
            if (token.IsCancellationRequested) {
                return TaskConstants<T>.Canceled;
            }

            var ret = @this.Enqueue();
            if (!token.CanBeCanceled) {
                return ret;
            }

            var registration = token.Register(() => @this.TryCancel(ret).Dispose(), false);
            ret.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return ret;
        }

        /// <summary>
        /// Creates a new entry and queues it to this wait queue. If the cancellation token is already canceled, this method immediately returns a canceled task without modifying the wait queue.
        /// </summary>
        /// <param name="this">The wait queue.</param>
        /// <param name="syncObject">A synchronization object taken while cancelling the entry.</param>
        /// <param name="token">The token used to cancel the wait.</param>
        /// <returns>The queued task.</returns>
        public static Task<T> Enqueue<T>(this IAsyncWaitQueue<T> @this, object syncObject, CancellationToken token) {
            if (token.IsCancellationRequested) {
                return TaskConstants<T>.Canceled;
            }

            var ret = @this.Enqueue();
            if (!token.CanBeCanceled) {
                return ret;
            }

            var registration = token.Register(() => {
                IDisposable finish;
                lock (syncObject)
                    finish = @this.TryCancel(ret);
                finish.Dispose();
            }, false);
            ret.ContinueWith(_ => registration.Dispose(), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
            return ret;
        }

        #endregion
    }

    /// <summary>
    /// The default wait queue implementation, which uses a double-ended queue.
    /// </summary>
    /// <typeparam name="T">The type of the results. If this isn't needed, use <see cref="Object"/>.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy(typeof (DefaultAsyncWaitQueue<>.DebugView))]
    public sealed class DefaultAsyncWaitQueue<T> : IAsyncWaitQueue<T> {
        #region Fields

        private readonly Deque<TaskCompletionSource<T>> _queue = new Deque<TaskCompletionSource<T>>();

        #endregion
        #region Properties

        private int Count {
            get {
                lock (_queue) {
                    return _queue.Count;
                }
            }
        }

        bool IAsyncWaitQueue<T>.IsEmpty => Count == 0;

        #endregion
        #region Methods

        Task<T> IAsyncWaitQueue<T>.Enqueue() {
            var tcs = new TaskCompletionSource<T>();
            lock (_queue)
                _queue.AddToBack(tcs);
            return tcs.Task;
        }

        IDisposable IAsyncWaitQueue<T>.Dequeue(T result) {
            TaskCompletionSource<T> tcs;
            lock (_queue)
                tcs = _queue.RemoveFromFront();
            return new CompleteDisposable(result, tcs);
        }

        IDisposable IAsyncWaitQueue<T>.DequeueAll(T result) {
            TaskCompletionSource<T>[] taskCompletionSources;
            lock (_queue) {
                taskCompletionSources = _queue.ToArray();
                _queue.Clear();
            }
            return new CompleteDisposable(result, taskCompletionSources);
        }

        IDisposable IAsyncWaitQueue<T>.TryCancel(Task task) {
            TaskCompletionSource<T> tcs = null;
            lock (_queue) {
                for (var i = 0; i != _queue.Count; ++i) {
                    if (_queue[i].Task == task) {
                        tcs = _queue[i];
                        _queue.RemoveAt(i);
                        break;
                    }
                }
            }
            if (tcs == null) {
                return new CancelDisposable();
            }
            return new CancelDisposable(tcs);
        }

        IDisposable IAsyncWaitQueue<T>.CancelAll() {
            TaskCompletionSource<T>[] taskCompletionSources;
            lock (_queue) {
                taskCompletionSources = _queue.ToArray();
                _queue.Clear();
            }
            return new CancelDisposable(taskCompletionSources);
        }

        #endregion
        #region Nested type: CancelDisposable

        private sealed class CancelDisposable : IDisposable {
            #region Fields

            private readonly TaskCompletionSource<T>[] _taskCompletionSources;

            #endregion
            #region Constructors

            public CancelDisposable(params TaskCompletionSource<T>[] taskCompletionSources) {
                _taskCompletionSources = taskCompletionSources;
            }

            #endregion
            #region Methods

            public void Dispose() {
                foreach (var cts in _taskCompletionSources) {
                    cts.TrySetCanceled();
                }
            }

            #endregion
        }

        #endregion
        #region Nested type: CompleteDisposable

        private sealed class CompleteDisposable : IDisposable {
            #region Fields

            private readonly T _result;
            private readonly TaskCompletionSource<T>[] _taskCompletionSources;

            #endregion
            #region Constructors

            public CompleteDisposable(T result, params TaskCompletionSource<T>[] taskCompletionSources) {
                _result = result;
                _taskCompletionSources = taskCompletionSources;
            }

            #endregion
            #region Methods

            public void Dispose() {
                foreach (var cts in _taskCompletionSources) {
                    cts.TrySetResult(_result);
                }
            }

            #endregion
        }

        #endregion
        #region Nested type: DebugView

        [DebuggerNonUserCode]
        internal sealed class DebugView {
            #region Fields

            private readonly DefaultAsyncWaitQueue<T> _queue;

            #endregion
            #region Constructors

            public DebugView(DefaultAsyncWaitQueue<T> queue) {
                _queue = queue;
            }

            #endregion
            #region Properties

            [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
            public Task<T>[] Tasks {
                get { return _queue._queue.Select(x => x.Task).ToArray(); }
            }

            #endregion
        }

        #endregion
    }
}