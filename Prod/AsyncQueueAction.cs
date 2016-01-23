using System;
using System.Threading.Tasks;

namespace Olan.Prod {
    public class AsyncQueueAction<TChild> : AsyncQueueAction<string, TChild>
        where TChild : ITaskProducer<object[], Task> { }
    public class AsyncQueueAction<TKey, TChild> : AsyncQueueActionBase<AsyncQueueAction<TKey, TChild>, TKey, TChild>
        where TChild : ITaskProducer<object[], Task> {
        #region Fields

        private readonly ChildrenCompletionOptions? _childrenCompletionOptions;
        private readonly Definition _definition;
        private readonly TaskCompletionOptions _options;
        private readonly Func<TKey, TKey> _stater;

        #endregion
        #region Constructors

        public AsyncQueueAction() { }

        public AsyncQueueAction(ChildrenCompletionOptions? childrenCompletionOptions, Definition definition, TaskCompletionOptions options, Func<TKey, TKey> stater) {
            _childrenCompletionOptions = childrenCompletionOptions;
            _definition = definition;
            _options = options;
            _stater = stater;
        }

        #endregion
        #region Methods

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

        #endregion
    }
}