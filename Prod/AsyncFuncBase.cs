using System;
using System.Threading;
using System.Threading.Tasks;
using Olan.AsyncEx;

namespace Olan.Prod {
    public abstract class AsyncFuncBase<TImplement, TResult> : AsyncFuncBase<TImplement, object[], TResult>
        where TImplement : AsyncFuncBase<TImplement, TResult>, new() { }
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
}