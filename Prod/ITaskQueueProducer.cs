using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olan.Prod {
    public interface ITaskQueueProducer<TArg, TKey, TChild, TTask> : ITaskQueue<TArg, TKey, TChild>, ITaskProducer<Tuple<TKey, TArg>, TTask>
        where TChild : ITaskProducer<TArg, TTask> where TTask : Task {
        #region Properties

        List<TTask> Tasks { get; }

        #endregion
        #region Methods

        new TTask Run(TKey key, TArg arg);

        #endregion
    }
}