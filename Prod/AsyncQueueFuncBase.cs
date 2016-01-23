using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Olan.AsyncEx;

namespace Olan.Prod {
    public abstract class AsyncQueueFuncBase<TImplement, TKey, TChild, TResult> : AsyncQueueFuncBase<TImplement, object[], TKey, TChild, TResult>
        where TImplement : AsyncQueueFuncBase<TImplement, TKey, TChild, TResult>, new() where TChild : ITaskProducer<object[], Task<TResult>> { }
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
}