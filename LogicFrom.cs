using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Olan.AsyncEx;
using Olan.Xml;

namespace Olan {
    public abstract partial class LogicFrom<TImplement, TKey, TChild, TTask> : XmlSetting, ILogicFrom<TKey, TChild, TTask>
        where TImplement : LogicFrom<TImplement, TKey, TChild, TTask>, new() where TTask : Task where TChild : ITask {
        #region Properties

        Task ITask.CurrentTask => CurrentTask;

        public TTask CurrentTask { get; set; }

        public Definition Definition {
            get { return _definition ?? (_definition = GetDefinition()); }
            protected set { _definition = value; }
        }

        public Exception Exception {
            get { return _exception = CurrentTask?.Exception; }
            protected set { _exception = value; }
        }

        public ChildrenCompletionOptions ExecutionOptions {
            get { return _executionOptions ?? (_executionOptions = ChildrenCompletionOptions.WaitAllChild | ChildrenCompletionOptions.ChildByChild).Value; }
            protected set { _executionOptions = value; }
        }

        public TaskCompletionOptions Options {
            get { return _options ?? (_options = GetOptions()); }
            set { _options = value; }
        }

        public TKey State { get; set; }

        public Func<TKey, TKey> Stater {
            get { return _stater ?? (_stater = GetStateToExecute); }
            set { _stater = value; }
        }

        public TaskStatus TaskStatus {
            get { return _status = CurrentTask?.Status ?? TaskStatus.Created; }
            set { _status = value; }
        }

        Func<object[], Func<Task>> ITask.TaskProvider => TaskProducer;

        public Func<object[], Func<TTask>> TaskProducer {
            get { return _taskBuilder ?? (_taskBuilder = GetTaskBuilder); }
            protected set { _taskBuilder = value; }
        }

        List<Task> ILogic<TKey, TChild>.Tasks => new List<Task>(Tasks);

        public List<TTask> Tasks { get; protected set; }

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

        ITask ITask.Configure(TaskCompletionOptions options) {
            return Configure(options);
        }

        public TImplement Configure(TaskCompletionOptions options) {
            Options = options;
            return this as TImplement;
        }

        ITask ITask.Configure(ThreadPriority priority) {
            return Configure(priority);
        }

        public TImplement Configure(ThreadPriority priority) {
            Options.ThreadPriority = priority;
            return this as TImplement;
        }

        ITask ITask.Configure(int timeout) {
            return Configure(timeout);
        }

        public TImplement Configure(int timeout) {
            var cts = new CancellationTokenSource(timeout);
            Options.Timeout = timeout;
            StartWith(c => cts.CancelAfter(timeout));
            return Configure(cts.Token);
        }

        ITask ITask.Configure(CancellationToken cancellationToken) {
            return Configure(cancellationToken);
        }

        public TImplement Configure(CancellationToken cancellationToken) {
            Options.CancellationToken = cancellationToken;
            foreach (var child in Values) {
                child.Configure(cancellationToken);
            }
            return this as TImplement;
        }

        ITask ITask.ContinueWith(Action<ITask> continuation) {
            return ContinueWith(continuation);
        }

        public TImplement ContinueWith(Action<ITask> continuation) {
            OnCompleted += c => Task.Run(() => continuation.Invoke(c));
            return this as TImplement;
        }

        ITask ITask.ContinueWith(Func<Task> continuation) {
            return ContinueWith(continuation);
        }

        public TImplement ContinueWith(Func<Task> continuation) {
            OnCompleted += c => continuation.Invoke();
            return this as TImplement;
        }

        ITask ITask.ContinueWith(Func<ITask, Task> continuation) {
            return ContinueWith(continuation);
        }

        public TImplement ContinueWith(Func<ITask, Task> continuation) {
            OnCompleted += continuation.Invoke;
            return this as TImplement;
        }

        Task ITask.Run(params object[] args) {
            return Run(args);
        }

        public TTask Run(params object[] args) {
            if (TaskProducer == null) {
                throw new ArgumentNullException(nameof(TaskProducer));
            }
            return CurrentTask = Run(TaskProducer.Invoke(args), Options.CancellationToken, Options.TaskCreationOptions, TaskScheduler.Default);
        }

        Task ILogic<TKey, TChild>.ExecuteChild(TKey child, params object[] args) {
            return ExecuteChild(child, args);
        }

        public TTask ExecuteChild(TKey child, params object[] args) {
            return CurrentTask = (TTask)this[child].Run(args);
        }

        ITask ITask.Initialize(Func<ITask, ITask> initializer) {
            return initializer.Invoke(this);
        }

        public TImplement Initialize(Func<TImplement, TImplement> initializer) {
            return initializer.Invoke(this as TImplement);
        }

        ITask ITask.Initialize(Action<ITask> initializer) {
            initializer.Invoke(this);
            return this;
        }

        public TImplement Initialize(Action<TImplement> initializer) {
            initializer.Invoke(this as TImplement);
            return this as TImplement;
        }

        async Task ITask.Run(Func<Task> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            using (await Mutex.LockAsync()) {
                await (Task.Factory.StartNew(async () => {
                    try {
                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        TaskStatus = TaskStatus.Running;
                        await f.Invoke();
                        TaskStatus = TaskStatus.RanToCompletion;
                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
                    }
                    catch (OperationCanceledException) {
                        TaskStatus = TaskStatus.Canceled;
                    }
                    catch (Exception) {
                        TaskStatus = TaskStatus.Faulted;
                        throw;
                    }
                    finally {
                        Mutex.ReleaseLock();
                    }
                }, ct, tco, ts).Unwrap());
            }
        }

        public abstract TTask Run(Func<TTask> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts);

        ITask ITask.StartWith(Action<ITask> startAction) {
            return StartWith(startAction);
        }

        public TImplement StartWith(Action<ITask> startAction) {
            OnStart += c => Task.Run(() => startAction.Invoke(c));
            return this as TImplement;
        }

        ITask ITask.StartWith(Func<Task> startAction) {
            return StartWith(startAction);
        }

        public TImplement StartWith(Func<Task> startAction) {
            OnStart += c => startAction.Invoke();
            return this as TImplement;
        }

        ITask ITask.StartWith(Func<ITask, Task> startAction) {
            return StartWith(startAction);
        }

        public TImplement StartWith(Func<ITask, Task> startAction) {
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
        #region Events

        public event AsyncEvent<ITask, Task> OnCompleted {
            add {
                if (_OnCompleted == null || _OnCompleted.GetInvocationList().Contains(value)) {
                    _OnCompleted += value;
                }
            }
            remove { _OnCompleted -= value; }
        }

        public event AsyncEvent<ITask, Task> OnStart {
            add {
                if (_OnStart == null || _OnStart.GetInvocationList().Contains(value)) {
                    _OnStart += value;
                }
            }
            remove { _OnStart -= value; }
        }

        #endregion
    }

    public abstract partial class LogicFrom<TImplement, TKey, TChild, TTask>
        where TImplement : LogicFrom<TImplement, TKey, TChild, TTask>, new() where TTask : Task where TChild : ITask {
        #region Fields

        private static readonly TaskScheduler ParallelScheduler = new ParallelTaskScheduler();
        private static readonly QueuedTaskScheduler QueuedScheduler = new QueuedTaskScheduler(TaskScheduler.Default, 1);
        private readonly Dictionary<TKey, TChild> _childrens = new Dictionary<TKey, TChild>();
        private readonly QueuedTaskScheduler _queuedSchedulerWithChildAwaiter = new QueuedTaskScheduler(TaskScheduler.Default, 1, true);
        private readonly ConcurrentDictionary<ThreadPriority, TaskScheduler> _schedulerQueues = new ConcurrentDictionary<ThreadPriority, TaskScheduler>();
        private readonly ConcurrentDictionary<ThreadPriority, TaskScheduler> _schedulerWithChildAwaiterQueues = new ConcurrentDictionary<ThreadPriority, TaskScheduler>();
        protected readonly AsyncLock Mutex = new AsyncLock();
        private Definition _definition;
        private Exception _exception;
        private ChildrenCompletionOptions? _executionOptions;
        protected AsyncEvent<ITask, Task> _OnCompleted;
        protected AsyncEvent<ITask, Task> _OnStart;
        private TaskCompletionOptions _options;
        private Func<TKey, TKey> _stater;
        private TaskStatus _status;
        private Func<object[], Func<TTask>> _taskBuilder;
        private static TImplement _instance;

        #endregion
        #region Constructors

        protected LogicFrom()
            : this(null, null, ChildrenCompletionOptions.WaitAllChild | ChildrenCompletionOptions.ChildByChild) { }

        protected LogicFrom(Definition definition)
            : this(definition, null, ChildrenCompletionOptions.WaitAllChild | ChildrenCompletionOptions.ChildByChild) { }

        protected LogicFrom(TaskCompletionOptions options)
            : this(null, options, ChildrenCompletionOptions.WaitAllChild | ChildrenCompletionOptions.ChildByChild) { }

        protected LogicFrom(ChildrenCompletionOptions executionOptions)
            : this(null, null, executionOptions) { }

        protected LogicFrom(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions) {
            _definition = definition;
            _options = options;
            _executionOptions = executionOptions;
        }

        #endregion
        #region Properties

        public static TImplement Instance => _instance ?? (_instance = new TImplement());

        #endregion
        #region Methods

        public static async Task Delay(int time) {
            await Task.Run(() => Thread.Sleep(time));
        }

        /// <summary>
        ///     Convert to TArg type a specified array element if exist or get the default TArg value.
        /// </summary>
        /// <typeparam name="TArgument"> The argument type to get. </typeparam>
        /// <param name="index"> The index localisation of the argument value on array. </param>
        /// <param name="args"> The array that should contains the argument. </param>
        /// <returns> The converted value or default. </returns>
        public TArgument GetArgumentOrDefault<TArgument>(int index, params object[] args) {
            return args.Any() ? (TArgument)args.ElementAtOrDefault(index) : default(TArgument);
        }

        protected virtual Definition GetDefinition() {
            return new Definition(GetType());
        }

        protected virtual TaskCompletionOptions GetOptions() {
            return new TaskCompletionOptions();
        }

        protected TaskScheduler GetSchedulerFor(ITask task) {
            if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.ChildByChild)) {
                return ExecutionOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority) ? _schedulerWithChildAwaiterQueues.GetOrAdd(task.Options.ThreadPriority, _queuedSchedulerWithChildAwaiter.ActivateNewQueue(10 - (int)task.Options.ThreadPriority)) : _queuedSchedulerWithChildAwaiter;
            }
            if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                return _schedulerQueues.GetOrAdd(task.Options.ThreadPriority, QueuedScheduler.ActivateNewQueue(10 - (int)task.Options.ThreadPriority));
            }
            return ExecutionOptions.HasFlag(ChildrenCompletionOptions.Parallel) ? ParallelScheduler : TaskScheduler.Default;
        }

        protected virtual TKey GetStateToExecute(TKey currentState) {
            return default(TKey);
        }

        protected abstract Func<TTask> GetTaskBuilder(object[] args);

        #endregion
    }

    public class LogicFrom<TImplement, TKey> : LogicFrom<TImplement, TKey, ITask, Task>
        where TImplement : LogicFrom<TImplement, TKey>, new() {
        #region Constructors

        public LogicFrom() { }

        public LogicFrom(Definition definition)
            : base(definition) { }

        public LogicFrom(TaskCompletionOptions options)
            : base(options) { }

        public LogicFrom(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicFrom(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
        #region Methods

        protected override Func<Task> GetTaskBuilder(object[] args) {
            return async () => {
                if (this.Any()) {
                    TKey state;
                    if (args != null && args.Any()) {
                        if (!Equals(default(TKey), state = GetArgumentOrDefault<TKey>(0, args)) && ContainsKey(state)) {
                            State = state;
                            await this[State].Run(args);
                            return;
                        }
                    }
                    else if (!Equals(default(TKey), state = Stater.Invoke(State)) && ContainsKey(state)) {
                        State = state;
                        await this[State].Run(args);
                        return;
                    }
                    var pairs = this.ToList();
                    if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                        pairs = pairs.OrderByDescending(pair => (int)pair.Value.Options.ThreadPriority).ToList();
                    }
                    Tasks = pairs.Select(pair => (Func<Task>)(() => {
                        State = pair.Key;
                        return pair.Value.Run(pair.Value.TaskProvider.Invoke(args), pair.Value.Options.CancellationToken, TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, GetSchedulerFor(pair.Value));
                    })).ToList().Select(tb => tb.Invoke()).ToList();
                    if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.WaitAllChild)) {
                        await Task.WhenAll(Tasks);
                    }
                    Tasks.Clear();
                }
            };
        }

        public override Task Run(Func<Task> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return CurrentTask = ((ITask)this).Run(f, ct, tco, ts);
        }

        #endregion
    }

    public class LogicFrom<TImplement, TKey, TArg1> : LogicFrom<TImplement, TKey, ITask<TArg1>, Task>, ITask<TArg1>
        where TImplement : LogicFrom<TImplement, TKey, TArg1>, new() {
        #region Fields

        private Func<TArg1, object[], Func<Task>> _taskBuilder;

        #endregion
        #region Constructors

        public LogicFrom() { }

        public LogicFrom(Definition definition)
            : base(definition) { }

        public LogicFrom(TaskCompletionOptions options)
            : base(options) { }

        public LogicFrom(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicFrom(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
        #region Properties

        public new Func<TArg1, object[], Func<Task>> TaskProducer {
            get { return _taskBuilder ?? (_taskBuilder = GetTaskBuilder); }
            protected set { _taskBuilder = value; }
        }

        #endregion
        #region Methods

        protected override Func<Task> GetTaskBuilder(object[] args) {
            return GetTaskBuilder(GetArgumentOrDefault<TArg1>(1, args), args);
        }

        protected Func<Task> GetTaskBuilder(TArg1 arg, params object[] args) {
            return async () => {
                if (this.Any()) {
                    TKey state;
                    if (args != null && args.Any()) {
                        if (!Equals(default(TKey), state = GetArgumentOrDefault<TKey>(0, args)) && ContainsKey(state)) {
                            State = state;
                            await this[State].Run(args);
                            return;
                        }
                    }
                    else if (!Equals(default(TKey), state = Stater.Invoke(State)) && ContainsKey(state)) {
                        State = state;
                        await this[State].Run(args);
                        return;
                    }
                    var pairs = this.ToList();
                    if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                        pairs = pairs.OrderByDescending(pair => (int)pair.Value.Options.ThreadPriority).ToList();
                    }
                    Tasks = pairs.Select(pair => (Func<Task>)(() => {
                        State = pair.Key;
                        return pair.Value.Run(pair.Value.TaskProvider.Invoke(arg, args), pair.Value.Options.CancellationToken, TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, GetSchedulerFor(pair.Value));
                    })).ToList().Select(tb => tb.Invoke()).ToList();
                    if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.WaitAllChild)) {
                        await Task.WhenAll(Tasks.Where(t => t != null));
                    }
                    Tasks.Clear();
                }
            };
        }

        public Task Execute(TArg1 arg1, params object[] args) {
            if (TaskProducer == null) {
                throw new ArgumentNullException(nameof(TaskProducer));
            }
            return CurrentTask = Run(TaskProducer.Invoke(arg1, args), Options.CancellationToken, Options.TaskCreationOptions, TaskScheduler.Default);
        }

        public override async Task Run(Func<Task> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            using (await Mutex.LockAsync()) {
                await (Task.Factory.StartNew(async () => {
                    try {
                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        TaskStatus = TaskStatus.Running;
                        await f.Invoke();
                        TaskStatus = TaskStatus.RanToCompletion;
                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
                    }
                    catch (OperationCanceledException) {
                        TaskStatus = TaskStatus.Canceled;
                    }
                    catch (Exception) {
                        TaskStatus = TaskStatus.Faulted;
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
}