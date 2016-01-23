using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Olan.Prod {
    public abstract class TaskQueueProducer<TImplement, TArg, TKey, TChild, TTask> : TaskProducer<TImplement, Tuple<TKey, TArg>, TTask>, ITaskQueueProducer<TArg, TKey, TChild, TTask>
        where TImplement : TaskQueueProducer<TImplement, TArg, TKey, TChild, TTask>, new() where TChild : ITaskProducer<TArg, TTask> where TTask : Task {
        #region Fields

        private static readonly TaskScheduler _parallelScheduler = new ParallelTaskScheduler();
        private static readonly QueuedTaskScheduler _queuedScheduler = new QueuedTaskScheduler(TaskScheduler.Default, 1);
        private readonly Dictionary<TKey, TChild> _childrens = new Dictionary<TKey, TChild>();
        private readonly QueuedTaskScheduler _queuedSchedulerWithChildAwaiter = new QueuedTaskScheduler(TaskScheduler.Default, 1, true);
        private readonly ConcurrentDictionary<ThreadPriority, TaskScheduler> _schedulerQueues = new ConcurrentDictionary<ThreadPriority, TaskScheduler>();
        private readonly ConcurrentDictionary<ThreadPriority, TaskScheduler> _schedulerWithChildAwaiterQueues = new ConcurrentDictionary<ThreadPriority, TaskScheduler>();
        private ChildrenCompletionOptions? _childrenOptions;
        private Func<TKey, TKey> _stater;

        #endregion
        #region Methods

        protected TaskScheduler GetSchedulerFor(ITaskProducer<TArg, TTask> task) {
            if (ChildrenOptions.HasFlag(ChildrenCompletionOptions.ChildByChild)) {
                return ChildrenOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority) 
                    ? _schedulerWithChildAwaiterQueues.GetOrAdd(task.Options.ThreadPriority, _queuedSchedulerWithChildAwaiter.ActivateNewQueue(10 - (int)task.Options.ThreadPriority)) 
                    : _queuedSchedulerWithChildAwaiter;
            }
            if (ChildrenOptions.HasFlag(ChildrenCompletionOptions.QueuedByPriority)) {
                return _schedulerQueues.GetOrAdd(task.Options.ThreadPriority, _queuedScheduler.ActivateNewQueue(10 - (int)task.Options.ThreadPriority));
            }
            return ChildrenOptions.HasFlag(ChildrenCompletionOptions.Parallel) ? _parallelScheduler : TaskScheduler.Default;
        }

        #endregion
        #region IDictionary<TKey, TChild> Members

        #region Properties

        public ICollection<TKey> Keys => _childrens.Keys;

        public ICollection<TChild> Values => _childrens.Values;

        public int Count => _childrens.Count;

        [Obsolete]
        public bool IsReadOnly => true;

        public TChild this[TKey key] {
            get { return _childrens[key]; }
            set { _childrens[key] = value; }
        }

        #endregion
        #region Methods

        public bool ContainsKey(TKey key) {
            return _childrens.ContainsKey(key);
        }

        public void Add(TKey key, TChild value) {
            _childrens.Add(key, value);
        }

        public bool Remove(TKey key) {
            return _childrens.Remove(key);
        }

        public bool TryGetValue(TKey key, out TChild value) {
            return _childrens.TryGetValue(key, out value);
        }

        public void Add(KeyValuePair<TKey, TChild> item) {
            _childrens.Add(item.Key, item.Value);
        }

        public void Clear() {
            _childrens.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TChild> item) {
            return _childrens.Contains(item);
        }

        [Obsolete]
        public void CopyTo(KeyValuePair<TKey, TChild>[] array, int arrayIndex) { }

        public bool Remove(KeyValuePair<TKey, TChild> item) {
            return _childrens.Remove(item.Key);
        }

        public IEnumerator<KeyValuePair<TKey, TChild>> GetEnumerator() {
            return _childrens.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return _childrens.GetEnumerator();
        }

        #endregion

        #endregion
        #region ITaskQueueProducer<TArg, TKey, TChild, TTask> Members

        #region Properties

        public ChildrenCompletionOptions ChildrenOptions {
            get { return _childrenOptions ?? (_childrenOptions = GetChildrenCompletionOptions()).Value; }
            protected set { _childrenOptions = value; }
        }

        public TKey State { get; set; }

        public Func<TKey, TKey> Stater {
            get { return _stater ?? (_stater = GetStateToExecute); }
            set { _stater = value; }
        }

        public List<TTask> Tasks { get; protected set; }

        #endregion
        #region Methods

        public new TImplement Configure(CancellationToken cancellationToken) {
            Options.CancellationToken = cancellationToken;
            foreach (var child in Values) {
                child.Configure(cancellationToken);
            }
            return this as TImplement;
        }

        Task ITaskQueue<TArg, TKey, TChild>.Run(TKey key, TArg arg) {
            return this[key].Run(arg);
        }

        public TTask Run(TKey key, TArg arg) {
            return this[key].Run(arg);
        }

        protected abstract ChildrenCompletionOptions GetChildrenCompletionOptions();

        protected abstract TKey GetStateToExecute(TKey arg);

        protected override TTask Task(Tuple<TKey, TArg> arg) {
            return Task(arg.Item1, arg.Item2);
        }

        protected abstract TTask Task(TKey state, TArg arg);

        #endregion

        #endregion
    }
}