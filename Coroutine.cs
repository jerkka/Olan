using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Olan {
    public class Coroutine<TImplement> : TaskProducer<TImplement, Task>
        where TImplement : Coroutine<TImplement>, new() {
        #region Constructors

        public Coroutine() { }

        public Coroutine(Definition definition)
            : base(definition) { }

        public Coroutine(TaskCompletionOptions options)
            : base(options) { }

        public Coroutine(Func<Task> taskBuilder)
            : base(taskBuilder) { }

        public Coroutine(Func<object[], Func<Task>> taskBuilder)
            : base(taskBuilder) { }

        public Coroutine(Definition definition, TaskCompletionOptions options)
            : base(definition, options) { }

        public Coroutine(Definition definition, Func<Task> taskBuilder)
            : base(definition, taskBuilder) { }

        public Coroutine(Definition definition, Func<object[], Func<Task>> taskBuilder)
            : base(definition, taskBuilder) { }

        public Coroutine(TaskCompletionOptions options, Func<Task> taskBuilder)
            : base(options, taskBuilder) { }

        public Coroutine(TaskCompletionOptions options, Func<object[], Func<Task>> taskBuilder)
            : base(options, taskBuilder) { }

        public Coroutine(Definition definition, TaskCompletionOptions options, Func<object[], Func<Task>> taskProducer)
            : base(definition, options, taskProducer) { }

        #endregion
        #region Methods

        public override Task Run(Func<Task> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return CurrentTask = ((ITask)this).Run(f, ct, tco, ts);
        }

        #endregion

        public static implicit operator Coroutine<TImplement>(Task task) {
            return new Coroutine<TImplement>(args => () => task);
        }

        public static explicit operator Task(Coroutine<TImplement> @this) {
            return @this.Run();
        }
    }

    public class Coroutine<TImplement, TArg1> : TaskProducer<TImplement, TArg1, Task>
        where TImplement : TaskProducer<TImplement, TArg1, Task>, new() {
        #region Constructors

        public Coroutine() { }

        public Coroutine(Definition definition)
            : base(definition) { }

        public Coroutine(TaskCompletionOptions options)
            : base(options) { }

        public Coroutine(Func<TArg1, Func<Task>> taskBuilder)
            : base(taskBuilder) { }

        public Coroutine(Func<TArg1, object[], Func<Task>> taskBuilder)
            : base(taskBuilder) { }

        public Coroutine(Definition definition, TaskCompletionOptions options)
            : base(definition, options) { }

        public Coroutine(Definition definition, Func<TArg1, Func<Task>> taskBuilder)
            : base(definition, taskBuilder) { }

        public Coroutine(Definition definition, Func<TArg1, object[], Func<Task>> taskBuilder)
            : base(definition, taskBuilder) { }

        public Coroutine(TaskCompletionOptions options, Func<TArg1, Func<Task>> taskBuilder)
            : base(options, taskBuilder) { }

        public Coroutine(TaskCompletionOptions options, Func<TArg1, object[], Func<Task>> taskBuilder)
            : base(options, taskBuilder) { }

        public Coroutine(Definition definition, TaskCompletionOptions options, Func<TArg1, object[], Func<Task>> taskBuilder)
            : base(definition, options, taskBuilder) { }

        #endregion
        #region Methods

        public override Task Run(Func<Task> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
            return CurrentTask = ((ITask)this).Run(f, ct, tco, ts);
        }

        #endregion

        public static implicit operator Coroutine<TImplement, TArg1>(Task task) {
            return new Coroutine<TImplement, TArg1>(args => () => task);
        }
        
        public static explicit operator Task(Coroutine<TImplement, TArg1> @this) {
            return @this.Run();
        }
    }
}