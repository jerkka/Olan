using System.Threading.Tasks;
using Olan.AsyncEx.Internal;

namespace Olan.AsyncEx {
    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    public static class TaskConstants {
        #region Fields

        private static readonly Task<bool> booleanTrue = TaskShim.FromResult(true);
        private static readonly Task<int> IntNegativeOne = TaskShim.FromResult(-1);

        #endregion
        #region Properties

        /// <summary>
        /// A task that has been completed with the value <c>true</c>.
        /// </summary>
        public static Task<bool> BooleanTrue => booleanTrue;

        /// <summary>
        /// A task that has been completed with the value <c>false</c>.
        /// </summary>
        public static Task<bool> BooleanFalse => TaskConstants<bool>.Default;

        /// <summary>
        /// A task that has been completed with the value <c>0</c>.
        /// </summary>
        public static Task<int> Int32Zero => TaskConstants<int>.Default;

        /// <summary>
        /// A task that has been completed with the value <c>-1</c>.
        /// </summary>
        public static Task<int> Int32NegativeOne => IntNegativeOne;

        /// <summary>
        /// A <see cref="Task"/> that has been completed.
        /// </summary>
        public static Task Completed => booleanTrue;

        /// <summary>
        /// A <see cref="Task"/> that will never complete.
        /// </summary>
        public static Task Never => TaskConstants<bool>.Never;

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task Canceled => TaskConstants<bool>.Canceled;

        #endregion
    }

    /// <summary>
    /// Provides completed task constants.
    /// </summary>
    /// <typeparam name="T">The type of the task result.</typeparam>
    public static class TaskConstants<T> {
        #region Fields

        private static readonly Task<T> DefaultValue = TaskShim.FromResult(default(T));
        private static readonly Task<T> never = new TaskCompletionSource<T>().Task;
        private static readonly Task<T> canceled = CanceledTask();

        #endregion
        #region Properties

        /// <summary>
        /// A task that has been completed with the default value of <typeparamref name="T"/>.
        /// </summary>
        public static Task<T> Default => DefaultValue;

        /// <summary>
        /// A <see cref="Task"/> that will never complete.
        /// </summary>
        public static Task<T> Never => never;

        /// <summary>
        /// A task that has been canceled.
        /// </summary>
        public static Task<T> Canceled => canceled;

        #endregion
        #region Methods

        private static Task<T> CanceledTask() {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetCanceled();
            return tcs.Task;
        }

        #endregion
    }
}