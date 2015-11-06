namespace Olan {
    public class LogicStatuedFunc<TImplement, TEnumStates, TResult> : LogicFuncFrom<TImplement, TEnumStates, TResult>
        where TEnumStates : struct where TImplement : LogicStatuedFunc<TImplement, TEnumStates, TResult>, new() {
        #region Constructors

        public LogicStatuedFunc() { }

        public LogicStatuedFunc(Definition definition)
            : base(definition) { }

        public LogicStatuedFunc(TaskCompletionOptions options)
            : base(options) { }

        public LogicStatuedFunc(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicStatuedFunc(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
    }

    public class LogicStatuedFunc<TImplement, TEnumStates, TArg1, TResult> : LogicFuncFrom<TImplement, TEnumStates, TArg1, TResult>
        where TEnumStates : struct where TImplement : LogicStatuedFunc<TImplement, TEnumStates, TArg1, TResult>, new() {
        #region Constructors

        public LogicStatuedFunc() { }

        public LogicStatuedFunc(Definition definition)
            : base(definition) { }

        public LogicStatuedFunc(TaskCompletionOptions options)
            : base(options) { }

        public LogicStatuedFunc(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public LogicStatuedFunc(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
    }
}