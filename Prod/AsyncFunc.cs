using System;
using System.Threading.Tasks;

namespace Olan.Prod {
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
}