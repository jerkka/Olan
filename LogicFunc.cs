namespace Olan {
    public class LogicFunc<TImplement, TResult> : LogicFuncFrom<TImplement, string, TResult>
        where TImplement : LogicFunc<TImplement, TResult>, new() {
        #region Constructors

        public LogicFunc() { }

        public LogicFunc(Definition definition)
            : base(definition) { }

        public LogicFunc(TaskCompletionOptions options)
            : base(options) { }

        public LogicFunc(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicFunc(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
    }

    public class LogicFunc<TImplement, TArg1, TResult> : LogicFuncFrom<TImplement, string, TArg1, TResult>
        where TImplement : LogicFunc<TImplement, TArg1, TResult>, new() {
        #region Constructors

        public LogicFunc() { }

        public LogicFunc(Definition definition)
            : base(definition) { }

        public LogicFunc(TaskCompletionOptions options)
            : base(options) { }

        public LogicFunc(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicFunc(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
    }
}