namespace Olan {
    public enum CoroutineStatus {
        None = 0,
        Pending = 1,
        CompletedSynchronously = 2,
        Completed = 4,
        CompletedParallel = 5,
        Running = 6,
        RunningParallel = 7,
        Failed = 8,
        Blacklisted = 9,
        Canceled = 10
    }
}