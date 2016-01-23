using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olan.Prod {
    public interface ITaskQueue<TArg, TKey, TChild> : IDictionary<TKey, TChild>, ITask<Tuple<TKey, TArg>>
        where TChild : ITask<TArg> {
        #region Properties

        ChildrenCompletionOptions ChildrenOptions { get; }
        TKey State { get; set; }
        Func<TKey, TKey> Stater { get; set; }

        #endregion
        #region Methods

        Task Run(TKey key, TArg arg);

        #endregion
    }
}