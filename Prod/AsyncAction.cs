using System;
using System.Threading.Tasks;

namespace Olan.Prod {
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
}