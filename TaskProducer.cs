using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Olan.AsyncEx;
using Olan.Xml;

namespace Olan {
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

    public abstract class TaskProducer<TImplement, TTask> : TaskProducer<TImplement, object[], TTask>
        where TImplement : TaskProducer<TImplement, TTask>, new() where TTask : Task { }

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

    public abstract class AsyncActionBase<TImplement, TArg> : TaskProducer<TImplement, TArg, Task>
        where TImplement : AsyncActionBase<TImplement, TArg>, new() {
        #region Methods

        public override Task Run(TArg arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return ((ITask<TArg>)this).Run(arg, ct, tco, ts);
        }

        #endregion
    }

    public abstract class AsyncActionBase<TImplement> : AsyncActionBase<TImplement, object[]>
        where TImplement : AsyncActionBase<TImplement>, new() { }

    public abstract class AsyncActionBase<TImplement, TArg1, TArg2> : AsyncActionBase<TImplement, Tuple<TArg1, TArg2>>
        where TImplement : AsyncActionBase<TImplement, TArg1, TArg2>, new() {
        #region Methods

        protected override Task Task(Tuple<TArg1, TArg2> arg) {
            return Task(arg.Item1, arg.Item2);
        }

        protected abstract Task Task(TArg1 arg1, TArg2 arg2);

        #endregion
    }

    public abstract class AsyncActionBase<TImplement, TArg1, TArg2, TArg3> : AsyncActionBase<TImplement, Tuple<TArg1, TArg2, TArg3>>
        where TImplement : AsyncActionBase<TImplement, TArg1, TArg2, TArg3>, new() {
        #region Methods

        protected override Task Task(Tuple<TArg1, TArg2, TArg3> arg) {
            return Task(arg.Item1, arg.Item2, arg.Item3);
        }

        protected abstract Task Task(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        #endregion
    }

    public class AsyncAction : AsyncActionBase<AsyncAction, object[]> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<Task> _taskProvider;

        #endregion
        #region Constructors

        public AsyncAction() { }

        public AsyncAction(Func<Task> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncAction(Definition definition, Func<Task> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncAction(TaskCompletionOptions options, Func<Task> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncAction(Definition definition, TaskCompletionOptions options, Func<Task> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task Task(object[] arg) {
            return _taskProvider.Invoke();
        }

        #endregion
    }

    public class AsyncAction<TArg1> : AsyncActionBase<AsyncAction<TArg1>, TArg1> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<TArg1, Task> _taskProvider;

        #endregion
        #region Constructors

        public AsyncAction() { }

        public AsyncAction(Func<TArg1, Task> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncAction(Definition definition, Func<TArg1, Task> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncAction(TaskCompletionOptions options, Func<TArg1, Task> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncAction(Definition definition, TaskCompletionOptions options, Func<TArg1, Task> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task Task(TArg1 arg) {
            return _taskProvider.Invoke(arg);
        }

        #endregion
    }

    public class AsyncAction<TArg1, TArg2> : AsyncActionBase<AsyncAction<TArg1, TArg2>, TArg1, TArg2> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<TArg1, TArg2, Task> _taskProvider;

        #endregion
        #region Constructors

        public AsyncAction() { }

        public AsyncAction(Func<TArg1, TArg2, Task> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncAction(Definition definition, Func<TArg1, TArg2, Task> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncAction(TaskCompletionOptions options, Func<TArg1, TArg2, Task> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncAction(Definition definition, TaskCompletionOptions options, Func<TArg1, TArg2, Task> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task Task(TArg1 arg1, TArg2 arg2) {
            return _taskProvider.Invoke(arg1, arg2);
        }

        #endregion
    }

    public class AsyncAction<TArg1, TArg2, TArg3> : AsyncActionBase<AsyncAction<TArg1, TArg2, TArg3>, TArg1, TArg2, TArg3> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<TArg1, TArg2, TArg3, Task> _taskProvider;

        #endregion
        #region Constructors

        public AsyncAction() { }

        public AsyncAction(Func<TArg1, TArg2, TArg3, Task> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncAction(Definition definition, Func<TArg1, TArg2, TArg3, Task> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncAction(TaskCompletionOptions options, Func<TArg1, TArg2, TArg3, Task> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncAction(Definition definition, TaskCompletionOptions options, Func<TArg1, TArg2, TArg3, Task> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task Task(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            return _taskProvider.Invoke(arg1, arg2, arg3);
        }

        #endregion
    }

    public abstract class AsyncFuncBase<TImplement, TArg, TResult> : TaskProducer<TImplement, TArg, Task<TResult>>
        where TImplement : AsyncFuncBase<TImplement, TArg, TResult>, new() {
        #region Properties

        public TResult Result { get; protected set; }

        #endregion
        #region Methods

        public override async Task<TResult> Run(TArg arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            Result = default(TResult);
            using (await Mutex.LockAsync()) {
                return await (CurrentTask = System.Threading.Tasks.Task.Factory.StartNew(async () => {
                    try {
                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        Status = CoroutineStatus.Running;
                        Result = await TaskProvider.Invoke(arg);
                        Status = CoroutineStatus.Completed;
                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        return Result;
                    }
                    catch (OperationCanceledException) {
                        Status = CoroutineStatus.Canceled;
                        return Result;
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

        #endregion
    }

    public abstract class AsyncFuncBase<TImplement, TResult> : AsyncFuncBase<TImplement, object[], TResult>
        where TImplement : AsyncFuncBase<TImplement, TResult>, new() { }

    public abstract class AsyncFuncBase<TImplement, TArg1, TArg2, TResult> : AsyncFuncBase<TImplement, Tuple<TArg1, TArg2>, TResult>
        where TImplement : AsyncFuncBase<TImplement, TArg1, TArg2, TResult>, new() {
        #region Methods

        protected override Task<TResult> Task(Tuple<TArg1, TArg2> arg) {
            return Task(arg.Item1, arg.Item2);
        }

        protected abstract Task<TResult> Task(TArg1 arg1, TArg2 arg2);

        #endregion
    }

    public abstract class AsyncFuncBase<TImplement, TArg1, TArg2, TArg3, TResult> : AsyncFuncBase<TImplement, Tuple<TArg1, TArg2, TArg3>, TResult>
        where TImplement : AsyncFuncBase<TImplement, TArg1, TArg2, TArg3, TResult>, new() {
        #region Methods

        protected override Task<TResult> Task(Tuple<TArg1, TArg2, TArg3> arg) {
            return Task(arg.Item1, arg.Item2, arg.Item3);
        }

        protected abstract Task<TResult> Task(TArg1 arg1, TArg2 arg2, TArg3 arg3);

        #endregion
    }

    public class AsyncFunc<TResult> : AsyncFuncBase<AsyncFunc<TResult>, object[], TResult> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<Task<TResult>> _taskProvider;

        #endregion
        #region Constructors

        public AsyncFunc() { }

        public AsyncFunc(Func<Task<TResult>> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncFunc(Definition definition, Func<Task<TResult>> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncFunc(TaskCompletionOptions options, Func<Task<TResult>> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncFunc(Definition definition, TaskCompletionOptions options, Func<Task<TResult>> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task<TResult> Task(object[] arg) {
            return _taskProvider.Invoke();
        }

        #endregion
    }

    public class AsyncFunc<TArg1, TResult> : AsyncFuncBase<AsyncFunc<TArg1, TResult>, TArg1, TResult> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<TArg1, Task<TResult>> _taskProvider;

        #endregion
        #region Constructors

        public AsyncFunc() { }

        public AsyncFunc(Func<TArg1, Task<TResult>> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncFunc(Definition definition, Func<TArg1, Task<TResult>> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncFunc(TaskCompletionOptions options, Func<TArg1, Task<TResult>> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncFunc(Definition definition, TaskCompletionOptions options, Func<TArg1, Task<TResult>> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task<TResult> Task(TArg1 arg) {
            return _taskProvider.Invoke(arg);
        }

        #endregion
    }

    public class AsyncFunc<TArg1, TArg2, TResult> : AsyncFuncBase<AsyncFunc<TArg1, TArg2, TResult>, TArg1, TArg2, TResult> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<TArg1, TArg2, Task<TResult>> _taskProvider;

        #endregion
        #region Constructors

        public AsyncFunc() { }

        public AsyncFunc(Func<TArg1, TArg2, Task<TResult>> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncFunc(Definition definition, Func<TArg1, TArg2, Task<TResult>> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncFunc(TaskCompletionOptions options, Func<TArg1, TArg2, Task<TResult>> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncFunc(Definition definition, TaskCompletionOptions options, Func<TArg1, TArg2, Task<TResult>> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task<TResult> Task(TArg1 arg1, TArg2 arg2) {
            return _taskProvider.Invoke(arg1, arg2);
        }

        #endregion
    }

    public class AsyncFunc<TArg1, TArg2, TArg3, TResult> : AsyncFuncBase<AsyncFunc<TArg1, TArg2, TArg3, TResult>, TArg1, TArg2, TArg3, TResult> {
        #region Fields

        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<TArg1, TArg2, TArg3, Task<TResult>> _taskProvider;

        #endregion
        #region Constructors

        public AsyncFunc() { }

        public AsyncFunc(Func<TArg1, TArg2, TArg3, Task<TResult>> taskProvider)
            : this(null, null, taskProvider) { }

        public AsyncFunc(Definition definition, Func<TArg1, TArg2, TArg3, Task<TResult>> taskProvider)
            : this(definition, null, taskProvider) { }

        public AsyncFunc(TaskCompletionOptions options, Func<TArg1, TArg2, TArg3, Task<TResult>> taskProvider)
            : this(null, options, taskProvider) { }

        public AsyncFunc(Definition definition, TaskCompletionOptions options, Func<TArg1, TArg2, TArg3, Task<TResult>> taskProvider) {
            if (taskProvider == null) {
                throw new ArgumentNullException(nameof(taskProvider));
            }
            _definition = definition;
            _options = options;
            _taskProvider = taskProvider;
        }

        #endregion
        #region Methods

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override Task<TResult> Task(TArg1 arg1, TArg2 arg2, TArg3 arg3) {
            return _taskProvider.Invoke(arg1, arg2, arg3);
        }

        #endregion
    }

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

    public interface ITaskQueueProducer<TArg, TKey, TChild, TTask> : ITaskQueue<TArg, TKey, TChild>, ITaskProducer<Tuple<TKey, TArg>, TTask>
        where TChild : ITaskProducer<TArg, TTask> where TTask : Task {
        #region Properties

        List<TTask> Tasks { get; }

        #endregion
        #region Methods

        new TTask Run(TKey key, TArg arg);

        #endregion
    }

    public abstract class TaskQueueProducer<TImplement, TArg, TKey, TChild, TTask> : TaskProducer<TImplement, Tuple<TKey, TArg>, TTask>, ITaskQueueProducer<TArg, TKey, TChild, TTask>
        where TImplement : TaskQueueProducer<TImplement, TArg, TKey, TChild, TTask>, new() where TChild : ITaskProducer<TArg, TTask> where TTask : Task {
        #region Fields

        private static readonly TaskScheduler _parallelScheduler = new ParallelTaskScheduler();
        private static readonly QueuedTaskScheduler _queuedScheduler = new QueuedTaskScheduler(TaskScheduler.Default, 1);
        private readonly Dictionary<TKey, TChild> _childrens = new Dictionary<TKey, TChild>();
        private readonly QueuedTaskScheduler _queuedSchedulerWithChildAwaiter = new QueuedTaskScheduler(TaskScheduler.Default, 1, true);
        private readonly ConcurrentDictionary<ThreadPriority, TaskScheduler> _schedulerQueues = new ConcurrentDictionary<ThreadPriority, TaskScheduler>();
        private readonly ConcurrentDictionary<ThreadPriority, TaskScheduler> _schedulerWithChildAwaiterQueues = new ConcurrentDictionary<ThreadPriority, TaskScheduler>();
        private ChildrenCompletionOptions? _childrenOptions;
        private Func<TKey, TKey> _stater;

        #endregion
        #region Methods

        protected TaskScheduler GetSchedulerFor(ITaskProducer<TArg, TTask> task) {
            if (ChildrenOptions.HasFlag(ChildrenCompletionOptions.ChildByChild)) {
                return ChildrenOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority) ? _schedulerWithChildAwaiterQueues.GetOrAdd(task.Options.ThreadPriority, _queuedSchedulerWithChildAwaiter.ActivateNewQueue(10 - (int)task.Options.ThreadPriority)) : _queuedSchedulerWithChildAwaiter;
            }
            if (ChildrenOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                return _schedulerQueues.GetOrAdd(task.Options.ThreadPriority, _queuedScheduler.ActivateNewQueue(10 - (int)task.Options.ThreadPriority));
            }
            return ChildrenOptions.HasFlag(ChildrenCompletionOptions.Parallel) ? _parallelScheduler : TaskScheduler.Default;
        }

        #endregion
        #region IDictionary<TKey, TChild> Members

        #region Properties

        public ICollection<TKey> Keys => _childrens.Keys;

        public ICollection<TChild> Values => _childrens.Values;

        public int Count => _childrens.Count;

        [Obsolete]
        public bool IsReadOnly => true;

        public TChild this[TKey key] {
            get { return _childrens[key]; }
            set { _childrens[key] = value; }
        }

        #endregion
        #region Methods

        public bool ContainsKey(TKey key) {
            return _childrens.ContainsKey(key);
        }

        public void Add(TKey key, TChild value) {
            _childrens.Add(key, value);
        }

        public bool Remove(TKey key) {
            return _childrens.Remove(key);
        }

        public bool TryGetValue(TKey key, out TChild value) {
            return _childrens.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TChild> item) {
            _childrens.Add(item.Key, item.Value);
        }

        public void Clear() {
            _childrens.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TChild> item) {
            return _childrens.Contains(item);
        }

        [Obsolete]
        public void CopyTo(KeyValuePair<TKey, TChild>[] array, int arrayIndex) { }

        public bool Remove(KeyValuePair<TKey, TChild> item) {
            return _childrens.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TChild>> GetEnumerator() {
            return _childrens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _childrens.GetEnumerator();
        }

        #endregion

        #endregion
        #region ITaskQueueProducer<TArg, TKey, TChild, TTask> Members

        #region Properties

        public ChildrenCompletionOptions ChildrenOptions {
            get { return _childrenOptions ?? (_childrenOptions = GetChildrenCompletionOptions()).Value; }
            protected set { _childrenOptions = value; }
        }

        public TKey State { get; set; }

        public Func<TKey, TKey> Stater {
            get { return _stater ?? (_stater = GetStateToExecute); }
            set { _stater = value; }
        }

        public List<TTask> Tasks { get; protected set; }

        #endregion
        #region Methods

        public new TImplement Configure(CancellationToken cancellationToken) {
            Options.CancellationToken = cancellationToken;
            foreach (var child in Values) {
                child.Configure(cancellationToken);
            }
            return this as TImplement;
        }

        Task ITaskQueue<TArg, TKey, TChild>.Run(TKey key, TArg arg) {
            return this[key].Run(arg);
        }

        public TTask Run(TKey key, TArg arg) {
            return this[key].Run(arg);
        }

        protected abstract ChildrenCompletionOptions GetChildrenCompletionOptions();

        protected abstract TKey GetStateToExecute(TKey arg);

        protected override TTask Task(Tuple<TKey, TArg> arg) {
            return Task(arg.Item1, arg.Item2);
        }

        protected abstract TTask Task(TKey state, TArg arg);

        #endregion

        #endregion
    }

    public abstract class AsyncQueueActionBase<TImplement, TArg, TKey, TChild> : TaskQueueProducer<TImplement, TArg, TKey, TChild, Task>
        where TImplement : AsyncQueueActionBase<TImplement, TArg, TKey, TChild>, new() where TChild : ITaskProducer<TArg, Task> {
        #region Methods

        public override Task Run(Tuple<TKey, TArg> arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return ((ITask<Tuple<TKey, TArg>>)this).Run(arg, ct, tco, ts);
        }

        protected override async Task Task(TKey state, TArg arg) {
            if (this.Any()) {
                if (!Equals(default(TKey), state) && ContainsKey(state)) {
                    State = state;
                    await Run(state, arg);
                    return;
                }
                if (!Equals(default(TKey), state = Stater.Invoke(State)) && ContainsKey(state)) {
                    State = state;
                    await Run(state, arg);
                    return;
                }
                var pairs = this.ToList();
                if (ChildrenOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                    pairs = pairs.OrderByDescending(pair => (int)pair.Value.Options.ThreadPriority).ToList();
                }
                Tasks = pairs.Select(pair => (Func<Task>)(() => {
                    State = pair.Key;
                    return pair.Value.Run(arg, TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, GetSchedulerFor(pair.Value));
                })).ToList().Select(tb => tb.Invoke()).ToList();
                if (ChildrenOptions.HasFlag(ChildrenCompletionOptions.WaitAllChild)) {
                    await System.Threading.Tasks.Task.WhenAll(Tasks);
                }
                Tasks.Clear();
            }
        }

        #endregion
    }

    public abstract class AsyncQueueActionBase<TImplement, TKey, TChild> : AsyncQueueActionBase<TImplement, object[], TKey, TChild>
        where TImplement : AsyncQueueActionBase<TImplement, TKey, TChild>, new() where TChild : ITaskProducer<object[], Task> { }

    public class AsyncQueueAction<TKey, TChild> : AsyncQueueActionBase<AsyncQueueAction<TKey, TChild>, TKey, TChild>
        where TChild : ITaskProducer<object[], Task> {
        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly ChildrenCompletionOptions? _childrenCompletionOptions;
        private readonly Func<TKey, TKey> _stater;

        protected override Definition GetDefinition() {
            return _definition ?? new Definition(GetType());
        }

        protected override TaskCompletionOptions GetOptions() {
            return _options ?? new TaskCompletionOptions();
        }

        protected override ChildrenCompletionOptions GetChildrenCompletionOptions() {
            return _childrenCompletionOptions ?? ChildrenCompletionOptions.WaitAllChild | ChildrenCompletionOptions.ChildByChild;
        }

        protected override TKey GetStateToExecute(TKey arg) {
            return _stater != null ? _stater.Invoke(arg) : default(TKey);
        }
    }

    public abstract class AsyncQueueFuncBase<TImplement, TArg, TKey, TChild, TResult> : TaskQueueProducer<TImplement, TArg, TKey, TChild, Task<TResult>>
        where TImplement : AsyncQueueFuncBase<TImplement, TArg, TKey, TChild, TResult>, new() where TChild : ITaskProducer<TArg, Task<TResult>> {
        #region Properties

        public TResult Result { get; protected set; }

        #endregion
        #region Methods

        protected virtual LoopContinuationType GetContinuationType(TResult result) {
            return Equals(default(TResult), result) ? LoopContinuationType.Continue : LoopContinuationType.ReturnResult;
        }

        public override async Task<TResult> Run(Tuple<TKey, TArg> arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            Result = default(TResult);
            using (await Mutex.LockAsync()) {
                return await (CurrentTask = System.Threading.Tasks.Task.Factory.StartNew(async () => {
                    try {
                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        Status = CoroutineStatus.Running;
                        Result = await TaskProvider.Invoke(arg);
                        Status = CoroutineStatus.Completed;
                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        return Result;
                    }
                    catch (OperationCanceledException) {
                        Status = CoroutineStatus.Canceled;
                        return Result;
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

        protected override async Task<TResult> Task(TKey state, TArg arg) {
            Result = default(TResult);
            if (this.Any()) {
                if (arg != null) {
                    if (!Equals(default(TKey), state) && ContainsKey(state)) {
                        State = state;
                        return await Run(state, arg);
                    }
                }
                if (!Equals(default(TKey), state = Stater.Invoke(State)) && ContainsKey(state)) {
                    State = state;
                    return await Run(state, arg);
                }
                _loopThis:
                var pairs = this.ToList();
                if (ChildrenOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                    pairs = pairs.OrderByDescending(pair => (int)pair.Value.Options.ThreadPriority).ToList();
                }
                foreach (var pair in pairs) {
                    _loopChild:
                    State = pair.Key;
                    var loopContinuationType = GetContinuationType(Result = await pair.Value.Run(arg, TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default));
                    switch (loopContinuationType) {
                        case LoopContinuationType.Continue:
                            break;
                        case LoopContinuationType.ReturnResult:
                            return Result;
                        case LoopContinuationType.LoopChild:
                            goto _loopChild;
                        case LoopContinuationType.LoopLogic:
                            goto _loopThis;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
            return Result;
        }

        #endregion
    }

    public abstract class AsyncQueueFuncBase<TImplement, TKey, TChild, TResult> : AsyncQueueFuncBase<TImplement, object[], TKey, TChild, TResult>
        where TImplement : AsyncQueueFuncBase<TImplement, TKey, TChild, TResult>, new() where TChild : ITaskProducer<object[], Task<TResult>> { }
}