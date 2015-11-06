using System;

namespace Olan {
    [Flags]
    public enum ChildrenCompletionOptions {
        ChildByChild = 2,
        WaitAllChild = 4,
        Queued = 8,
        QueuedByPriority = 16,
        Parallel = 32
    }
}