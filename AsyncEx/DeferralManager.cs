using System;
using System.Threading.Tasks;

namespace Olan.AsyncEx {
    /// <summary>
    /// Manages the deferrals for a "command" event that may have asynchonous handlers.
    /// </summary>
    public sealed class DeferralManager {
        #region Fields

        /// <summary>
        /// The countdown event, triggered when all deferrals have completed. This is <c>null</c> if there are no deferrals.
        /// </summary>
        private AsyncCountdownEvent _count;

        #endregion
        #region Methods

        /// <summary>
        /// Gets a deferral. The deferral is complete when disposed.
        /// </summary>
        /// <returns>The deferral.</returns>
        public IDisposable GetDeferral() {
            if (_count == null) {
                _count = new AsyncCountdownEvent(1);
            }
            var ret = new Deferral(_count);
            _count.AddCount();
            return ret;
        }

        /// <summary>
        /// Notifies the manager that all deferrals have been requested, and returns a task that is completed when all deferrals have completed.
        /// </summary>
        /// <returns>A task that is completed when all deferrals have completed.</returns>
        public Task SignalAndWaitAsync() {
            if (_count == null) {
                return TaskConstants.Completed;
            }
            _count.Signal();
            return _count.WaitAsync();
        }

        #endregion
        #region Nested type: Deferral

        /// <summary>
        /// A deferral.
        /// </summary>
        private sealed class Deferral : IDisposable {
            #region Fields

            /// <summary>
            /// The countdown event of the deferral manager.
            /// </summary>
            private AsyncCountdownEvent _count;

            #endregion
            #region Constructors

            /// <summary>
            /// Creates a new deferral referencing the countdown event of the deferral manager.
            /// </summary>
            /// <param name="count">The countdown event of the deferral manager.</param>
            public Deferral(AsyncCountdownEvent count) {
                _count = count;
            }

            #endregion
            #region Methods

            /// <summary>
            /// Completes the deferral.
            /// </summary>
            void IDisposable.Dispose() {
                if (_count == null) {
                    return;
                }
                _count.Signal();
                _count = null;
            }

            #endregion
        }

        #endregion
    }
}