using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olan.Prod {
    public abstract class AsyncQueueActionBase<TImplement, TKey, TChild> : AsyncQueueActionBase<TImplement, object[], TKey, TChild>
        where TImplement : AsyncQueueActionBase<TImplement, TKey, TChild>, new() where TChild : ITaskProducer<object[], Task> { }
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
    public abstract class AsyncQueueActionBase<TImplement, TArg1, TArg2, TKey, TChild> : AsyncQueueActionBase<TImplement, Tuple<TArg1, TArg2>, TKey, TChild>
        where TImplement : AsyncQueueActionBase<TImplement, TArg1, TArg2, TKey, TChild>, new() where TChild : ITaskProducer<Tuple<TArg1, TArg2>, Task> { }
    public abstract class AsyncQueueActionBase<TImplement, TArg1, TArg2, TArg3, TKey, TChild> : AsyncQueueActionBase<TImplement, Tuple<TArg1, TArg2, TArg3>, TKey, TChild>
        where TImplement : AsyncQueueActionBase<TImplement, TArg1, TArg2, TArg3, TKey, TChild>, new() where TChild : ITaskProducer<Tuple<TArg1, TArg2, TArg3>, Task> { }
}