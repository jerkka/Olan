using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olan {
    public interface ITaskProducer<TArg, TTask> : ITask<TArg>
        where TTask : Task {
        #region Properties

        new TTask CurrentTask { get; set; }
        new Func<TArg, TTask> TaskProvider { get; }

        #endregion
        #region Methods

        new TTask Run();
        new TTask Run(TArg arg);
        new TTask Run(CancellationToken ct);
        new TTask Run(TArg arg, CancellationToken ct);
        new TTask Run(TaskCreationOptions tco);
        new TTask Run(TArg arg, TaskCreationOptions tco);
        new TTask Run(TaskScheduler ts);
        new TTask Run(TArg arg, TaskScheduler ts);
        new TTask Run(CancellationToken ct, TaskCreationOptions tco);
        new TTask Run(TArg arg, CancellationToken ct, TaskCreationOptions tco);
        new TTask Run(CancellationToken ct, TaskScheduler ts);
        new TTask Run(TArg arg, CancellationToken ct, TaskScheduler ts);
        new TTask Run(TaskCreationOptions tco, TaskScheduler ts);
        new TTask Run(TArg arg, TaskCreationOptions tco, TaskScheduler ts);
        new TTask Run(CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts);
        new TTask Run(TArg arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts);

        #endregion
    }
}