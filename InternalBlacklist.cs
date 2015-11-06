using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Olan {
    public sealed class InternalBlacklist<TEnumBlacklist>
        where TEnumBlacklist : IComparable {
        #region Fields

        private readonly ConcurrentDictionary<TEnumBlacklist, BlacklistItem<TEnumBlacklist>> _blacklist = new ConcurrentDictionary<TEnumBlacklist, BlacklistItem<TEnumBlacklist>>();

        #endregion
        #region Constructors

        public InternalBlacklist(ConcurrentDictionary<TEnumBlacklist, Tuple<double, double>> blacklistTimes = null) {
            if (!typeof (TEnumBlacklist).IsEnum) {
                throw new ArgumentException($"Argumnent {nameof(TEnumBlacklist)} is not an Enum");
            }
            BlacklistTimes = blacklistTimes ?? new ConcurrentDictionary<TEnumBlacklist, Tuple<double, double>>();
            foreach (var enumValue in
                from object value in Enum.GetValues(typeof (TEnumBlacklist)) select (TEnumBlacklist)value) {
                BlacklistTimes.AddOrUpdate(enumValue, new Tuple<double, double>(0, 0), (type, tuple) => tuple);
            }
        }

        #endregion
        #region Properties

        public ConcurrentDictionary<TEnumBlacklist, Tuple<double, double>> BlacklistTimes { get; }

        public IEnumerable<BlacklistItem<TEnumBlacklist>> CurrentPrisoners {
            get { return _blacklist.Values.Where(i => i.IsCaptive()); }
        }

        #endregion
        #region Methods

        public bool Contains(TEnumBlacklist type) {
            BlacklistItem<TEnumBlacklist> outItem;
            return Contains(type, out outItem);
        }

        public bool Contains(TEnumBlacklist type, out BlacklistItem<TEnumBlacklist> outItem) {
            return Contains(new BlacklistItem<TEnumBlacklist> {
                Type = type, BlacklistFor = BlacklistTimes[type].Item1, DisableAfterEscapeFor = BlacklistTimes[type].Item2, Reason = "Not specified."
            }, out outItem);
        }

        public bool Contains(BlacklistItem<TEnumBlacklist> item) {
            BlacklistItem<TEnumBlacklist> outItem;
            return Contains(item, out outItem);
        }

        public bool Contains(BlacklistItem<TEnumBlacklist> item, out BlacklistItem<TEnumBlacklist> outItem) {
            outItem = _blacklist.GetOrAdd(item.Type, new BlacklistItem<TEnumBlacklist> {
                Type = item.Type, BlacklistFor = item.BlacklistFor, DisableAfterEscapeFor = item.DisableAfterEscapeFor, Reason = item.Reason
            });
            return outItem.IsCaptive();
        }

        public bool IsTypeAllowed(TEnumBlacklist type) {
            return BlacklistTimes[type].Item1 > 0;
        }

        public bool Jail(TEnumBlacklist type, string reason = null) {
            return Jail(new BlacklistItem<TEnumBlacklist> {
                Type = type, BlacklistFor = BlacklistTimes[type].Item1, DisableAfterEscapeFor = BlacklistTimes[type].Item2, Reason = reason ?? "Not specified."
            });
        }

        public bool Jail(BlacklistItem<TEnumBlacklist> item) {
            var flag = false;
            _blacklist.AddOrUpdate(item.Type, new BlacklistItem<TEnumBlacklist> {
                Type = item.Type, BlacklistFor = item.BlacklistFor, DisableAfterEscapeFor = item.DisableAfterEscapeFor, Reason = item.Reason, TimeOnBlacklisting = DateTime.Now
            }, (blacklistType, blacklistItem) => {
                if (!blacklistItem.IsBlacklistingAllowed() || blacklistItem.IsCaptive()) {
                    flag = true;
                    return blacklistItem;
                }
                blacklistItem.BlacklistFor = item.BlacklistFor;
                blacklistItem.DisableAfterEscapeFor = item.DisableAfterEscapeFor;
                blacklistItem.Reason = item.Reason;
                blacklistItem.TimeOnBlacklisting = DateTime.Now;
                return blacklistItem;
            });
            return !flag;
        }

        #endregion
    }
}