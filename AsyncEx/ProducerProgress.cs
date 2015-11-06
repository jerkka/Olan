using System;
using System.Collections.Concurrent;

namespace Olan.AsyncEx {
    /// <summary>
    /// A progress implementation that sends progress reports to a producer/consumer collection.
    /// </summary>
    /// <typeparam name="T">The type of progress value.</typeparam>
    public sealed class ProducerProgress<T> : IProgress<T> {
        #region Fields

        /// <summary>
        /// The producer/consumer collection that receives progress reports.
        /// </summary>
        private readonly IProducerConsumerCollection<T> _collection;

        #endregion
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerProgress&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="collection">The producer/consumer collection that receives progress reports.</param>
        public ProducerProgress(IProducerConsumerCollection<T> collection) {
            _collection = collection;
        }

        #endregion
        #region Methods

        void IProgress<T>.Report(T value) {
            _collection.TryAdd(value);
        }

        #endregion
    }
}