using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Olan {
    public enum BlacklistType {
        OnFailed,
        OnTimeout,
        OnCanceled
    }

    public sealed class Blacklist : ConcurrentDictionary<BlacklistType, BlacklistItem> {
        public Blacklist(IEnumerable<KeyValuePair<BlacklistType, BlacklistItem>> collection)
            : base(collection) { }

        public Blacklist()
            : base(new Dictionary<BlacklistType, BlacklistItem> {
                { BlacklistType.OnCanceled, new BlacklistItem(TimeSpan.FromSeconds(5)) },
                { BlacklistType.OnFailed, new BlacklistItem(TimeSpan.FromSeconds(5)) },
                { BlacklistType.OnTimeout, new BlacklistItem(TimeSpan.FromSeconds(5)) }
            }) { }

        public bool AnyBlacklisted {
            get { return this.Any(bi => bi.Value.IsJail); }
        }

        public bool IsBlacklisted(BlacklistType blacklistType) {
            return this[blacklistType].IsJail;
        }

        public bool Jail(BlacklistType blacklistType, string reason = null) {
            return this[blacklistType].Jail(reason);
        }
    }

    public sealed class BlacklistItem {
        public BlacklistItem(TimeSpan jailTimeSpan) {
            JailTimeSpan = jailTimeSpan;
            TimeOnJail = DateTime.MinValue;
            DisableOnFreeForTimeSpan = TimeSpan.FromSeconds(5);
        }

        public TimeSpan DisableOnFreeForTimeSpan { get; set; }
        public TimeSpan JailTimeSpan { get; set; }
        public DateTime TimeOnJail { get; set; }
        public string Reason { get; set; }

        public bool IsJail => DateTime.UtcNow.Subtract(TimeOnJail) < JailTimeSpan;

        public bool Jail(string reason = null) {
            if (!IsJail) {
                if (DateTime.UtcNow.Subtract(TimeOnJail) + JailTimeSpan < DisableOnFreeForTimeSpan) {
                    return false;
                }
                Reason = reason;
                TimeOnJail = DateTime.UtcNow;

                return true;
            }
            return false;
        }
    }
}