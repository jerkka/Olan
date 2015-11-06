using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Olan.AsyncEx;

namespace Olan {
    public class LogicFuncFrom<TImplement, TKey, TResult> : LogicFrom<TImplement, TKey, ITaskProducer<Task<TResult>>, Task<TResult>>
        where TImplement : LogicFuncFrom<TImplement, TKey, TResult>, new() {
        #region Constructors

        public LogicFuncFrom() { }

        public LogicFuncFrom(Definition definition)
            : base(definition) { }

        public LogicFuncFrom(TaskCompletionOptions options)
            : base(options) { }

        public LogicFuncFrom(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicFuncFrom(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
        #region Properties

        public TResult Result { get; protected set; }

        #endregion
        #region Methods

        protected virtual LoopContinuationType AnalyseChildResult(TResult result) {
            return Equals(default(TResult), result) ? LoopContinuationType.Continue : LoopContinuationType.ReturnResult;
        }

        protected override Func<Task<TResult>> GetTaskBuilder(object[] args) {
            return async () => {
                Result = default(TResult);
                if (this.Any()) {
                    TKey state;
                    if (args != null && args.Any()) {
                        if (!Equals(default(TKey), state = GetArgumentOrDefault<TKey>(0, args)) && ContainsKey(state)) {
                            State = state;
                            return await this[State].Run(args);
                        }
                    }
                    else if (!Equals(default(TKey), state = Stater.Invoke(State)) && ContainsKey(state)) {
                        State = state;
                        return await this[State].Run(args);
                    }
                    _loopThis:
                    var pairs = this.ToList();
                    if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                        pairs = pairs.OrderByDescending(pair => (int)pair.Value.Options.ThreadPriority).ToList();
                    }
                    foreach (var pair in pairs) {
                        _loopChild:
                        State = pair.Key;
                        var loopContinuationType = AnalyseChildResult(Result = await pair.Value.Run(pair.Value.TaskProvider.Invoke(args), pair.Value.Options.CancellationToken, TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach, TaskScheduler.Default));
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
            };
        }

        public override async Task<TResult> Run(Func<Task<TResult>> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            var result = default(TResult);
            using (await Mutex.LockAsync()) {
                return await (CurrentTask = Task.Factory.StartNew(async () => {
                    try {
                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        TaskStatus = TaskStatus.Running;
                        result = await f.Invoke();
                        TaskStatus = TaskStatus.RanToCompletion;
                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        return result;
                    }
                    catch (OperationCanceledException) {
                        TaskStatus = TaskStatus.Canceled;
                        return result;
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

    public class LogicFuncFrom<TImplement, TKey, TArg1, TResult> : LogicFrom<TImplement, TKey, ITaskProducer<Task<TResult>, TArg1>, Task<TResult>>, ITaskProducer<Task<TResult>, TArg1>
        where TImplement : LogicFuncFrom<TImplement, TKey, TArg1, TResult>, new() {
        #region Fields

        private Func<TArg1, object[], Func<Task<TResult>>> _taskBuilder;

        #endregion
        #region Constructors

        public LogicFuncFrom() { }

        public LogicFuncFrom(Definition definition)
            : base(definition) { }

        public LogicFuncFrom(TaskCompletionOptions options)
            : base(options) { }

        public LogicFuncFrom(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicFuncFrom(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
        #region Properties

        public TResult Result { get; protected set; }

        Func<TArg1, object[], Func<Task>> ITask<TArg1>.TaskProvider => TaskProducer;

        public new Func<TArg1, object[], Func<Task<TResult>>> TaskProducer {
            get { return _taskBuilder ?? (_taskBuilder = GetTaskBuilder); }
            protected set { _taskBuilder = value; }
        }

        #endregion
        #region Methods

        protected virtual LoopContinuationType AnalyseChildResult(TResult result) {
            return Equals(default(TResult), result) ? LoopContinuationType.Continue : LoopContinuationType.ReturnResult;
        }

        protected override Func<Task<TResult>> GetTaskBuilder(object[] args) {
            return GetTaskBuilder(GetArgumentOrDefault<TArg1>(1, args), args);
        }

        protected Func<Task<TResult>> GetTaskBuilder(TArg1 arg1, object[] args) {
            return async () => {
                Result = default(TResult);
                if (this.Any()) {
                    TKey state;
                    if (args != null && args.Any()) {
                        if (!Equals(default(TKey), state = GetArgumentOrDefault<TKey>(0, args)) && ContainsKey(state)) {
                            State = state;
                            return await this[State].Run(args);
                        }
                    }
                    else if (!Equals(default(TKey), state = Stater.Invoke(State)) && ContainsKey(state)) {
                        State = state;
                        return await this[State].Run(args);
                    }
                    _loopThis:
                    var pairs = this.ToList();
                    if (ExecutionOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                        pairs = pairs.OrderByDescending(pair => (int)pair.Value.Options.ThreadPriority).ToList();
                    }
                    foreach (var pair in pairs) {
                        _loopChild:
                        State = pair.Key;
                        var loopContinuationType = AnalyseChildResult(Result = await pair.Value.Run((object[])pair.Value.TaskProvider.Invoke(arg1, args), pair.Value.Options.CancellationToken, (TaskCreationOptions)(TaskCreationOptions.HideScheduler | TaskCreationOptions.DenyChildAttach), TaskScheduler.Default));
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
            };
        }

        Task ITask<TArg1>.Execute(TArg1 arg1, params object[] args) {
            return Execute(arg1, args);
        }

        public Task<TResult> Execute(TArg1 arg1, params object[] args) {
            if (TaskProducer == null) {
                throw new ArgumentNullException(nameof(TaskProducer));
            }
            return CurrentTask = Run(TaskProducer.Invoke(arg1, args), Options.CancellationToken, Options.TaskCreationOptions, TaskScheduler.Default);
        }

        public override async Task<TResult> Run(Func<Task<TResult>> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            var result = default(TResult);
            using (await Mutex.LockAsync()) {
                return await (CurrentTask = Task.Factory.StartNew(async () => {
                    try {
                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        TaskStatus = TaskStatus.Running;
                        result = await f.Invoke();
                        TaskStatus = TaskStatus.RanToCompletion;
                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
                        return result;
                    }
                    catch (OperationCanceledException) {
                        TaskStatus = TaskStatus.Canceled;
                        return result;
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