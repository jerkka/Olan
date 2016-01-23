using System;
using System.Threading;
using System.Threading.Tasks;

namespace Olan.Prod {
    public abstract class AsyncActionBase<TImplement> : AsyncActionBase<TImplement, object[]>
        where TImplement : AsyncActionBase<TImplement>, new() { }
    public abstract class AsyncActionBase<TImplement, TArg> : TaskProducer<TImplement, TArg, Task>
        where TImplement : AsyncActionBase<TImplement, TArg>, new() {
        #region Methods

        public override Task Run(TArg arg, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return ((ITask<TArg>)this).Run(arg, ct, tco, ts);
        }

        #endregion
    }
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
}