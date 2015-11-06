using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olan {
    public interface ITask<TArg> {
        #region Properties

        Task CurrentTask { get; }
        Definition Definition { get; }
        Exception Exception { get; }
        TaskCompletionOptions Options { get; }
        CoroutineStatus Status { get; set; }
        TaskStatus TaskStatus { get; }
        Func<TArg, Task> TaskProvider { get; }

        #endregion
        #region Methods

        ITask<TArg> Configure(TaskCompletionOptions options);
        ITask<TArg> Configure(ThreadPriority priority);
        ITask<TArg> Configure(int timeout);
        ITask<TArg> Configure(CancellationToken cancellationToken);
        ITask<TArg> ContinueWith(Action<ITask<TArg>> continuation);
        ITask<TArg> ContinueWith(Func<Task> continuation);
        ITask<TArg> ContinueWith(Func<ITask<TArg>, Task> continuation);
        ITask<TArg> Initialize(Func<ITask<TArg>, ITask<TArg>> initializer);
        ITask<TArg> Initialize(Action<ITask<TArg>> initializer);
        Task Run();
        Task Run(TArg arg);
        Task Run(CancellationToken ct);
        Task Run(TArg arg, CancellationToken ct);
        Task Run(TaskCreationOptions tco);
        Task Run(TArg arg, TaskCreationOptions tco);
        Task Run(TaskScheduler ts);
        Task Run(TArg arg, TaskScheduler ts);
        Task Run(CancellationToken ct, TaskCreationOptions tco);
        Task Run(TArg arg, CancellationToken ct, TaskCreationOptions tco);
        Task Run(CancellationToken ct, TaskScheduler ts);
        Task Run(TArg arg, CancellationToken ct, TaskScheduler ts);
        Task Run(TaskCreationOptions tco, TaskScheduler ts);
        Task Run(TArg arg, TaskCreationOptions tco, TaskScheduler ts);
        Task Run(CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts);
        Task Run(TArg arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts);
        ITask<TArg> StartWith(Action<ITask<TArg>> startAction);
        ITask<TArg> StartWith(Func<Task> startAction);
        ITask<TArg> StartWith(Func<ITask<TArg>, Task> startAction);
        bool Wait(int timeout);
        bool Wait();

        #endregion
        #region Events

        event AsyncEvent<ITask<TArg>, Task> OnStart;
        event AsyncEvent<ITask<TArg>, Task> OnCompleted;

        #endregion
    }
}