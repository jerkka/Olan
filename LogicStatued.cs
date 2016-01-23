//namespace Olan {
//    public class LogicStatued<TImplement, TEnumStates> : LogicFrom<TImplement, TEnumStates>
//        where TEnumStates : struct where TImplement : LogicStatued<TImplement, TEnumStates>, new() {
//        #region Constructors

//        public LogicStatued() { }

//        public LogicStatued(Definition definition)
//            : base(definition) { }

//        public LogicStatued(TaskCompletionOptions options)
//            : base(options) { }

//        public LogicStatued(ChildrenCompletionOptions executionOptions)
//            : base(executionOptions) { }

//        public LogicStatued(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
//            : base(definition, options, executionOptions) { }

//        #endregion
//    }

//    public class LogicStatued<TImplement, TEnumStates, TArg1> : LogicFrom<TImplement, TEnumStates, TArg1>
//        where TEnumStates : struct where TImplement : LogicStatued<TImplement, TEnumStates, TArg1>, new() {
//        #region Constructors

//        public LogicStatued() { }

//        public LogicStatued(Definition definition)
//            : base(definition) { }

//        public LogicStatued(TaskCompletionOptions options)
//            : base(options) { }

//        public LogicStatued(ChildrenCompletionOptions executionOptions)
//            : base(executionOptions) { }

//        public LogicStatued(Definition definition, TaskCompletionOptions options, ChildrenCompletionOptions executionOptions)
//            : base(definition, options, executionOptions) { }

//        #endregion
//    }
//}