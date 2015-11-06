using System.Collections.Generic;

namespace Olan {
    public interface IContainer<TKey, TValue> : IEnumerable<TValue> {
        #region Properties

        IDictionary<TKey, TValue> Childs { get; }

        #endregion
        #region Methods

        void Add(KeyValuePair<TKey, TValue> item);
        void Add(TKey key, TValue value);
        void Add(TValue value);
        bool Contains(KeyValuePair<TKey, TValue> item);
        bool Contains(TKey key);
        bool Contains(TValue value);
        bool Remove(KeyValuePair<TKey, TValue> item);
        bool Remove(TKey key);

        #endregion
    }
}