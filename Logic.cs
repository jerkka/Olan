namespace Olan {
    public class Logic : LogicFrom<Logic, string> {
        #region Constructors

        public Logic() { }

        public Logic(Definition definition)
            : base(definition) { }

        public Logic(TaskCompletionOptions options)
            : base(options) { }

        public Logic(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public Logic(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
    }

    public class Logic<TArg1> : LogicFrom<Logic<TArg1>, string, TArg1> {
        #region Constructors

        public Logic() { }

        public Logic(Definition definition)
            : base(definition) { }

        public Logic(TaskCompletionOptions options)
            : base(options) { }

        public Logic(ChildrenCompletionOptions executionOptions)
            : base(executionOptions) { }

        public Logic(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
            : base(definition, options, executionOptions) { }

        #endregion
    }
}