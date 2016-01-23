using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Olan.AsyncEx;
using Olan.Xml;

namespace Olan.Prod {
    public abstract class TaskProducer<TImplement, TTask> : TaskProducer<TImplement, object[], TTask>
        where TImplement : TaskProducer<TImplement, TTask>, new() where TTask : Task { }
    public abstract class TaskProducer<TImplement, TArg, TTask> : XmlSetting, ITaskProducer<TArg, TTask>
        where TImplement : TaskProducer<TImplement, TArg, TTask>, new() where TTask : Task {
        #region Fields

        private static TImplement _instance;
        protected readonly AsyncLock Mutex = new AsyncLock();
        private Definition _definition;
        private Exception _exception;
        protected AsyncEvent<ITask<TArg>, Task> _OnCompleted;
        protected AsyncEvent<ITask<TArg>, Task> _OnStart;
        private TaskCompletionOptions _options;
        private Func<TArg, TTask> _taskProducer;

        #endregion
        #region Properties

        [XmlIgnore]
        public static TImplement Instance => _instance ?? (_instance = new TImplement());

        #endregion
        #region Methods

        protected abstract Definition GetDefinition();

        protected abstract TaskCompletionOptions GetOptions();

        protected abstract TTask Task(TArg arg);

        #endregion
        #region ITaskProducer<TArg, TTask> Members

        #region Properties

        [XmlIgnore]
        Task ITask<TArg>.CurrentTask => CurrentTask;

        [XmlIgnore]
        public TTask CurrentTask { get; set; }

        [XmlIgnore]
        public Definition Definition {
            get { return _definition ?? (_definition = GetDefinition()); }
            protected set { _definition = value; }
        }

        [XmlIgnore]
        public Exception Exception {
            get { return _exception = CurrentTask?.Exception; }
            protected set { _exception = value; }
        }

        [Setting]
        public TaskCompletionOptions Options {
            get { return _options ?? (_options = GetOptions()); }
            protected set { _options = value; }
        }

        [XmlIgnore]
        public CoroutineStatus Status { get; set; }

        [XmlIgnore]
        public TaskStatus TaskStatus => CurrentTask?.Status ?? TaskStatus.Created;

        [XmlIgnore]
        Func<TArg, Task> ITask<TArg>.TaskProvider => TaskProvider;

        [XmlIgnore]
        public Func<TArg, TTask> TaskProvider {
            get { return _taskProducer ?? (_taskProducer = Task); }
            protected set { _taskProducer = value; }
        }

        #endregion
        #region Methods

        ITask<TArg> ITask<TArg>.Configure(TaskCompletionOptions options) {
            return Configure(options);
        }

        public TImplement Configure(TaskCompletionOptions options) {
            Options = options;
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.Configure(ThreadPriority priority) {
            return Configure(priority);
        }

        public TImplement Configure(ThreadPriority priority) {
            Options.ThreadPriority = priority;
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.Configure(int timeout) {
            return Configure(timeout);
        }

        public TImplement Configure(int timeout) {
            var cts = new CancellationTokenSource(timeout);
            Options.Timeout = timeout;
            StartWith(c => cts.CancelAfter(timeout));
            return Configure(cts.Token);
        }

        ITask<TArg> ITask<TArg>.Configure(CancellationToken cancellationToken) {
            return Configure(cancellationToken);
        }

        public TImplement Configure(CancellationToken cancellationToken) {
            Options.CancellationToken = cancellationToken;
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.ContinueWith(Action<ITask<TArg>> continuation) {
            return ContinueWith(continuation);
        }

        public TImplement ContinueWith(Action<ITask<TArg>> continuation) {
            OnCompleted += c => System.Threading.Tasks.Task.Run(() => continuation.Invoke(c));
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.ContinueWith(Func<Task> continuation) {
            return ContinueWith(continuation);
        }

        public TImplement ContinueWith(Func<Task> continuation) {
            OnCompleted += c => continuation.Invoke();
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.ContinueWith(Func<ITask<TArg>, Task> continuation) {
            return ContinueWith(continuation);
        }

        public TImplement ContinueWith(Func<ITask<TArg>, Task> continuation) {
            OnCompleted += continuation.Invoke;
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.Initialize(Func<ITask<TArg>, ITask<TArg>> initializer) {
            return initializer.Invoke(this);
        }

        public TImplement Initialize(Func<TImplement, TImplement> initializer) {
            return initializer.Invoke(this as TImplement);
        }

        ITask<TArg> ITask<TArg>.Initialize(Action<ITask<TArg>> initializer) {
            initializer.Invoke(this);
            return this;
        }

        public TImplement Initialize(Action<TImplement> initializer) {
            initializer.Invoke(this as TImplement);
            return this as TImplement;
        }

        Task ITask<TArg>.Run() {
            return Run();
        }

        public TTask Run() {
            return Run(default(TArg), Options.CancellationToken, Options.TaskCreationOptions, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(TArg arg) {
            return Run(arg);
        }

        public TTask Run(TArg arg) {
            return Run(arg, Options.CancellationToken, Options.TaskCreationOptions, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(CancellationToken ct) {
            return Run(ct);
        }

        public TTask Run(CancellationToken ct) {
            return Run(default(TArg), ct, Options.TaskCreationOptions, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(TArg arg, CancellationToken ct) {
            return Run(arg, ct);
        }

        public TTask Run(TArg arg, CancellationToken ct) {
            return Run(arg, ct, Options.TaskCreationOptions, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(TaskCreationOptions tco) {
            return Run(tco);
        }

        public TTask Run(TaskCreationOptions tco) {
            return Run(default(TArg), Options.CancellationToken, tco, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(TArg arg, TaskCreationOptions tco) {
            return Run(arg, tco);
        }

        public TTask Run(TArg arg, TaskCreationOptions tco) {
            return Run(arg, Options.CancellationToken, tco, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(TaskScheduler ts) {
            return Run(ts);
        }

        public TTask Run(TaskScheduler ts) {
            return Run(default(TArg), Options.CancellationToken, Options.TaskCreationOptions, ts);
        }

        Task ITask<TArg>.Run(TArg arg, TaskScheduler ts) {
            return Run(arg, ts);
        }

        public TTask Run(TArg arg, TaskScheduler ts) {
            return Run(arg, Options.CancellationToken, Options.TaskCreationOptions, ts);
        }

        Task ITask<TArg>.Run(CancellationToken ct, TaskCreationOptions tco) {
            return Run(ct, tco);
        }

        public TTask Run(CancellationToken ct, TaskCreationOptions tco) {
            return Run(default(TArg), ct, tco, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(TArg arg, CancellationToken ct, TaskCreationOptions tco) {
            return Run(arg, ct, tco);
        }

        public TTask Run(TArg arg, CancellationToken ct, TaskCreationOptions tco) {
            return Run(arg, ct, tco, TaskScheduler.Default);
        }

        Task ITask<TArg>.Run(CancellationToken ct, TaskScheduler ts) {
            return Run(ct, ts);
        }

        public TTask Run(CancellationToken ct, TaskScheduler ts) {
            return Run(default(TArg), ct, Options.TaskCreationOptions, ts);
        }

        Task ITask<TArg>.Run(TArg arg, CancellationToken ct, TaskScheduler ts) {
            return Run(arg, ct, ts);
        }

        public TTask Run(TArg arg, CancellationToken ct, TaskScheduler ts) {
            return Run(arg, ct, Options.TaskCreationOptions, ts);
        }

        Task ITask<TArg>.Run(TaskCreationOptions tco, TaskScheduler ts) {
            return Run(tco, ts);
        }

        public TTask Run(TaskCreationOptions tco, TaskScheduler ts) {
            return Run(default(TArg), Options.CancellationToken, tco, ts);
        }

        Task ITask<TArg>.Run(TArg arg, TaskCreationOptions tco, TaskScheduler ts) {
            return Run(arg, tco, ts);
        }

        public TTask Run(TArg arg, TaskCreationOptions tco, TaskScheduler ts) {
            return Run(arg, Options.CancellationToken, tco, ts);
        }

        Task ITask<TArg>.Run(CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return Run(ct, tco, ts);
        }

        public TTask Run(CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return Run(default(TArg), ct, tco, ts);
        }

        async Task ITask<TArg>.Run(TArg arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            if (TaskProvider == null) {
                throw new ArgumentNullException(nameof(TaskProvider));
            }
            using (await Mutex.LockAsync()) {
                await (System.Threading.Tasks.Task.Factory.StartNew(async () => {
                    try {
                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        Status = CoroutineStatus.Running;
                        await TaskProvider.Invoke(arg);
                        Status = CoroutineStatus.Completed;
                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
                    }
                    catch (OperationCanceledException) {
                        Status = CoroutineStatus.Canceled;
                    }
                    catch (Exception) {
                        Status = CoroutineStatus.Failed;
                        throw;
                    }
                    finally {
                        Mutex.ReleaseLock();
                    }
                }, ct, tco, ts).Unwrap());
            }
        }

        public abstract TTask Run(TArg arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts);

        ITask<TArg> ITask<TArg>.StartWith(Action<ITask<TArg>> startAction) {
            return StartWith(startAction);
        }

        public TImplement StartWith(Action<ITask<TArg>> startAction) {
            OnStart += c => System.Threading.Tasks.Task.Run(() => startAction.Invoke(c));
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.StartWith(Func<Task> startAction) {
            return StartWith(startAction);
        }

        public TImplement StartWith(Func<Task> startAction) {
            OnStart += c => startAction.Invoke();
            return this as TImplement;
        }

        ITask<TArg> ITask<TArg>.StartWith(Func<ITask<TArg>, Task> startAction) {
            return StartWith(startAction);
        }

        public TImplement StartWith(Func<ITask<TArg>, Task> startAction) {
            OnStart += startAction.Invoke;
            return this as TImplement;
        }

        public bool Wait(int timeout) {
            return Configure(timeout).Wait();
        }

        public bool Wait() {
            CurrentTask.Wait();
            return !CurrentTask.IsCanceled;
        }

        #endregion
        #region Events

        public event AsyncEvent<ITask<TArg>, Task> OnCompleted {
            add {
                if (_OnCompleted == null || _OnCompleted.GetInvocationList().Contains(value)) {
                    _OnCompleted += value;
                }
            }
            remove { _OnCompleted -= value; }
        }

        public event AsyncEvent<ITask<TArg>, Task> OnStart {
            add {
                if (_OnStart == null || _OnStart.GetInvocationList().Contains(value)) {
                    _OnStart += value;
                }
            }
            remove { _OnStart -= value; }
        }

        #endregion

        #endregion
    }
    public abstract class TaskProducer<TImplement, TArg1, TArg2, TTask> : TaskProducer<TImplement, Tuple<TArg1, TArg2>, TTask>
        where TImplement : TaskProducer<TImplement, TArg1, TArg2, TTask>, new() where TTask : Task {
        #region Methods

        protected override TTask Task(Tuple<TArg1, TArg2> arg) {
            return Task(arg.Item1, arg.Item2);
        }

        protected abstract TTask Task(TArg1 arg1, TArg2 arg2);

        #endregion
    }
    public abstract class TaskProducer<TImplement, TArg1, TArg2, TArg3, TTask> : TaskProducer<TImplement, Tuple<TArg1, TArg2, TArg3>, TTask>
        where TImplement : TaskProducer<TImplement, TArg1, TArg2, TArg3, TTask>, new() where TTask : Task {
        #region Methods

        protected override TTask Task(Tuple<TArg1, TArg2, TArg3> arg) {
            return Task(arg.Item1, arg.Item2, arg.Item3);
        }

        protected abstract TTask Task(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        #endregion
    }
}