using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Olan {
    public interface ILogic<TKey, TChild> : ITask, IDictionary<TKey, TChild>
        where TChild : ITask {
        #region Properties

        ChildrenCompletionOptions ExecutionOptions { get; }
        Func<TKey, TKey> Stater { get; set; }
        List<Task> Tasks { get; }

        #endregion
        #region Methods

        Task ExecuteChild(TKey child, params object[] args);

        #endregion
    }
}