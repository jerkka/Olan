using System;

namespace Olan {
    public sealed class BlacklistItem<TEnumBlacklist>
        where TEnumBlacklist : IComparable {
        #region Fields

        private static readonly DateTime DateTimeMinValue = DateTime.MinValue;

        #endregion
        #region Properties

        public TEnumBlacklist Type { get; set; }
        public double BlacklistFor { get; set; } = -1d;
        public double DisableAfterEscapeFor { get; set; } = -1d;
        public string Reason { get; set; } = "None";
        public DateTime TimeOnBlacklisting { get; set; } = DateTimeMinValue;
        public DateTime TimeOnEscape { get; set; } = DateTimeMinValue;

        public double TimeSinceBlacklisting => DateTime.UtcNow.Subtract(TimeOnBlacklisting).TotalMilliseconds;

        public double TimeSinceEscape => DateTime.UtcNow.Subtract(TimeOnEscape).TotalMilliseconds;

        #endregion
        #region Methods

        public bool Equals(BlacklistItem<TEnumBlacklist> other) {
            return other.Type.Equals(Type);
        }

        public bool IsBlacklistingAllowed() {
            if (BlacklistFor <= 0) {
                return false;
            }
            if (DisableAfterEscapeFor <= 0) {
                return true;
            }
            if (TimeOnEscape == DateTimeMinValue) {
                return true;
            }
            if (TimeSinceEscape < DisableAfterEscapeFor) {
                return false;
            }
            TimeOnEscape = DateTimeMinValue;
            return true;
        }

        public bool IsCaptive() {
            if (BlacklistFor <= 0) {
                return false;
            }
            if (TimeOnBlacklisting == DateTime.MinValue) {
                return false;
            }
            if (TimeSinceEscape < BlacklistFor) {
                return true;
            }
            TimeOnBlacklisting = DateTimeMinValue;
            return false;
        }

        #endregion
    }
}