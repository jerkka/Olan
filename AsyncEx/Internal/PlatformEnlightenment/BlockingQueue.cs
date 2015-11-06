using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Olan.AsyncEx.Internal.PlatformEnlightenment {
    public sealed class BlockingQueue<T> : IDisposable {
        #region Fields

        private readonly BlockingCollection<T> _queue;

        #endregion
        #region Constructors

        public BlockingQueue() {
            _queue = new BlockingCollection<T>();
        }

        #endregion
        #region Methods

        public bool TryAdd(T item) {
            try {
                return _queue.TryAdd(item);
            }
            catch (InvalidOperationException) {
                // vexing exception
                return false;
            }
        }

        public IEnumerable<T> GetConsumingEnumerable() {
            return _queue.GetConsumingEnumerable();
        }

        [System.Diagnostics.DebuggerNonUserCode]
        public IEnumerable<T> EnumerateForDebugger() {
            return _queue;
        }

        public void CompleteAdding() {
            _queue.CompleteAdding();
        }

        public void Dispose() {
            _queue.Dispose();
        }

        #endregion
    }
}