using System.Threading.Tasks;

namespace Olan {
    public interface ILogicFrom<TKey, TChild, TTask> : ILogic<TKey, TChild>, ITaskProducer<TTask>
        where TTask : Task where TChild : ITask {
        #region Methods

        new TTask ExecuteChild(TKey child, params object[] args);

        #endregion
    }
}