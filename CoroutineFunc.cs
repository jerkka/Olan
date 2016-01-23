//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Windows.Forms;
//using Olan.AsyncEx;
//using Olan.Prod;

//namespace Olan {
//    public sealed class SealedCoroutineFunc<TResult> : CoroutineFunc<SealedCoroutineFunc<TResult>, TResult> {
//        #region Constructors

//        public SealedCoroutineFunc() { }

//        public SealedCoroutineFunc(Definition definition)
//            : base(definition) { }

//        public SealedCoroutineFunc(TaskCompletionOptions options)
//            : base(options) { }

//        public SealedCoroutineFunc(Func<Task<TResult>> taskBuilder)
//            : base(taskBuilder) { }

//        public SealedCoroutineFunc(Func<object[], Func<Task<TResult>>> taskBuilder)
//            : base(taskBuilder) { }

//        public SealedCoroutineFunc(Definition definition, TaskCompletionOptions options)
//            : base(definition, options) { }

//        public SealedCoroutineFunc(Definition definition, Func<Task<TResult>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public SealedCoroutineFunc(Definition definition, Func<object[], Func<Task<TResult>>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public SealedCoroutineFunc(TaskCompletionOptions options, Func<Task<TResult>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public SealedCoroutineFunc(TaskCompletionOptions options, Func<object[], Func<Task<TResult>>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public SealedCoroutineFunc(Definition definition, TaskCompletionOptions options, Func<object[], Func<Task<TResult>>> taskProducer)
//            : base(definition, options, taskProducer) { }

//        #endregion
//    }

//    public class CoroutineFunc<TImplement, TResult> : TaskProducer<TImplement, Task<TResult>>
//        where TImplement : CoroutineFunc<TImplement, TResult>, new() {
//        #region Constructors

//        public CoroutineFunc() { }

//        public CoroutineFunc(Definition definition)
//            : base(definition) { }

//        public CoroutineFunc(TaskCompletionOptions options)
//            : base(options) { }

//        public CoroutineFunc(Func<Task<TResult>> taskBuilder)
//            : base(taskBuilder) { }

//        public CoroutineFunc(Func<object[], Func<Task<TResult>>> taskBuilder)
//            : base(taskBuilder) { }

//        public CoroutineFunc(Definition definition, TaskCompletionOptions options)
//            : base(definition, options) { }

//        public CoroutineFunc(Definition definition, Func<Task<TResult>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public CoroutineFunc(Definition definition, Func<object[], Func<Task<TResult>>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public CoroutineFunc(TaskCompletionOptions options, Func<Task<TResult>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public CoroutineFunc(TaskCompletionOptions options, Func<object[], Func<Task<TResult>>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public CoroutineFunc(Definition definition, TaskCompletionOptions options, Func<object[], Func<Task<TResult>>> taskProducer)
//            : base(definition, options, taskProducer) { }

//        #endregion
//        #region Properties

//        public TResult Result { get; protected set; }

//        #endregion
//        #region Methods

//        public override async Task<TResult> Run(Func<Task<TResult>> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
//            Result = default(TResult);
//            using (await Mutex.LockAsync()) {
//                return await (CurrentTask = System.Threading.Tasks.Task.Factory.StartNew(async () => {
//                    try {
//                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
//                        TaskStatus = TaskStatus.Running;
//                        Result = await f.Invoke();
//                        TaskStatus = TaskStatus.RanToCompletion;
//                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
//                        return Result;
//                    }
//                    catch (OperationCanceledException) {
//                        TaskStatus = TaskStatus.Canceled;
//                        return Result;
//                    }
//                    catch (Exception) {
//                        TaskStatus = TaskStatus.Faulted;
//                        throw;
//                    }
//                    finally {
//                        Mutex.ReleaseLock();
//                    }
//                }, ct, tco, ts).Unwrap());
//            }
//        }

//        public static implicit operator CoroutineFunc<TImplement, TResult>(Task<TResult> task) {
//            return new CoroutineFunc<TImplement, TResult>(args => () => task);
//        }

//        public static explicit operator Task<TResult>(CoroutineFunc<TImplement, TResult> @this) {
//            return @this.Run();
//        }

//        public static SealedCoroutineFunc<TResult> GenerateRun(params object[] args) {
//            return new SealedCoroutineFunc<TResult>(() => Instance.Run(args));
//        }

//        #endregion
//    }

//    public class CoroutineFunc<TImplement, TArg1, TResult> : TaskProducer<TImplement, TArg1, Task<TResult>>
//        where TImplement : CoroutineFunc<TImplement, TArg1, TResult>, new() {
//        #region Constructors

//        public CoroutineFunc() { }

//        public CoroutineFunc(Definition definition)
//            : base(definition) { }

//        public CoroutineFunc(TaskCompletionOptions options)
//            : base(options) { }

//        public CoroutineFunc(Func<TArg1, Func<Task<TResult>>> taskBuilder)
//            : base(taskBuilder) { }

//        public CoroutineFunc(Func<TArg1, object[], Func<Task<TResult>>> taskBuilder)
//            : base(taskBuilder) { }

//        public CoroutineFunc(Definition definition, TaskCompletionOptions options)
//            : base(definition, options) { }

//        public CoroutineFunc(Definition definition, Func<TArg1, Func<Task<TResult>>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public CoroutineFunc(Definition definition, Func<TArg1, object[], Func<Task<TResult>>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public CoroutineFunc(TaskCompletionOptions options, Func<TArg1, Func<Task<TResult>>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public CoroutineFunc(TaskCompletionOptions options, Func<TArg1, object[], Func<Task<TResult>>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public CoroutineFunc(Definition definition, TaskCompletionOptions options, Func<TArg1, object[], Func<Task<TResult>>> taskBuilder)
//            : base(definition, options, taskBuilder) { }

//        #endregion
//        #region Properties

//        public TResult Result { get; protected set; }

//        #endregion
//        #region Methods

//        public override async Task<TResult> Run(Func<Task<TResult>> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
//            Result = default(TResult);
//            using (await Mutex.LockAsync()) {
//                return await (CurrentTask = System.Threading.Tasks.Task.Factory.StartNew(async () => {
//                    try {
//                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
//                        TaskStatus = TaskStatus.Running;
//                        Result = await f.Invoke();
//                        TaskStatus = TaskStatus.RanToCompletion;
//                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
//                        return Result;
//                    }
//                    catch (OperationCanceledException) {
//                        TaskStatus = TaskStatus.Canceled;
//                        return Result;
//                    }
//                    catch (Exception) {
//                        TaskStatus = TaskStatus.Faulted;
//                        throw;
//                    }
//                    finally {
//                        Mutex.ReleaseLock();
//                    }
//                }, ct, tco, ts).Unwrap());
//            }
//        }

//        public static SealedCoroutineFunc<TResult> GenerateRun(TArg1 arg1, params object[] args) {
//            return new SealedCoroutineFunc<TResult>(() => Instance.Execute(arg1, args));
//        }

//        public static implicit operator CoroutineFunc<TImplement, TArg1, TResult>(Task<TResult> task) {
//            return new CoroutineFunc<TImplement, TArg1, TResult>(arg1 => () => task);
//        }

//        public static explicit operator Task<TResult>(CoroutineFunc<TImplement, TArg1, TResult> @this) {
//            return @this.Run();
//        }

//        #endregion
//    }

//    public class CoroutineFunc<TImplement, TArg1, TArg2, TResult> : TaskProducer<TImplement, TArg1, TArg2, Task<TResult>>
//        where TImplement : CoroutineFunc<TImplement, TArg1, TArg2, TResult>, new() {
//        #region Constructors

//        public CoroutineFunc() { }

//        public CoroutineFunc(Definition definition)
//            : base(definition) { }

//        public CoroutineFunc(TaskCompletionOptions options)
//            : base(options) { }

//        public CoroutineFunc(Func<TArg1, TArg2, Func<Task<TResult>>> taskBuilder)
//            : base(taskBuilder) { }

//        public CoroutineFunc(Func<TArg1, TArg2, object[], Func<Task<TResult>>> taskBuilder)
//            : base(taskBuilder) { }

//        public CoroutineFunc(Definition definition, TaskCompletionOptions options)
//            : base(definition, options) { }

//        public CoroutineFunc(Definition definition, Func<TArg1, TArg2, Func<Task<TResult>>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public CoroutineFunc(Definition definition, Func<TArg1, TArg2, object[], Func<Task<TResult>>> taskBuilder)
//            : base(definition, taskBuilder) { }

//        public CoroutineFunc(TaskCompletionOptions options, Func<TArg1, TArg2, Func<Task<TResult>>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public CoroutineFunc(TaskCompletionOptions options, Func<TArg1, TArg2, object[], Func<Task<TResult>>> taskBuilder)
//            : base(options, taskBuilder) { }

//        public CoroutineFunc(Definition definition, TaskCompletionOptions options, Func<TArg1, TArg2, object[], Func<Task<TResult>>> taskBuilder)
//            : base(definition, options, taskBuilder) { }

//        #endregion
//        #region Properties

//        public TResult Result { get; protected set; }

//        #endregion
//        #region Methods

//        public override async Task<TResult> Run(Func<Task<TResult>> f, CancellationToken ct, TaskCreationOptions tco, TaskScheduler ts) {
//            Result = default(TResult);
//            using (await Mutex.LockAsync()) {
//                return await (CurrentTask = System.Threading.Tasks.Task.Factory.StartNew(async () => {
//                    try {
//                        await (_OnStart?.Invoke(this) ?? TaskConstants.BooleanFalse);
//                        TaskStatus = TaskStatus.Running;
//                        Result = await f.Invoke();
//                        TaskStatus = TaskStatus.RanToCompletion;
//                        await (_OnCompleted?.Invoke(this) ?? TaskConstants.BooleanFalse);
//                        return Result;
//                    }
//                    catch (OperationCanceledException) {
//                        TaskStatus = TaskStatus.Canceled;
//                        return Result;
//                    }
//                    catch (Exception) {
//                        TaskStatus = TaskStatus.Faulted;
//                        throw;
//                    }
//                    finally {
//                        Mutex.ReleaseLock();
//                    }
//                }, ct, tco, ts).Unwrap());
//            }
//        }

//        public static SealedCoroutineFunc<TResult> GenerateRun(TArg1 arg1, TArg2 arg2, params object[] args) {
//            return new SealedCoroutineFunc<TResult>(() => Instance.Execute(arg1, arg2, args));
//        }

//        public static implicit operator CoroutineFunc<TImplement, TArg1, TArg2, TResult>(Task<TResult> task) {
//            return new CoroutineFunc<TImplement, TArg1, TArg2, TResult>((arg1, arg2) => () => task);
//        }

//        public static explicit operator Task<TResult>(CoroutineFunc<TImplement, TArg1, TArg2, TResult> @this) {
//            return @this.Run();
//        }

//        #endregion
//    }
//}