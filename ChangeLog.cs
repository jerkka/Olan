using System;
using System.Collections.Generic;

namespace Olan {
    public sealed class ChangeLog : Dictionary<VersionDated, string> {
        #region Constructors

        public ChangeLog()
            : base(new Dictionary<VersionDated, string>()) { }

        #endregion
        #region Properties

        public string this[Version version] {
            get {
                string outValue;
                return TryGetValue(new VersionDated(version), out outValue) ? outValue : null;
            }
            set {
                var key = new VersionDated(version);
                if (ContainsKey(key)) {
                    base[key] = value;
                    return;
                }
                Add(key, value);
            }
        }

        #endregion
        #region Methods

        public override string ToString() {
            return this.ToJson();
        }

        #endregion
    }
}