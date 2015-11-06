using System;
using Newtonsoft.Json;

namespace Olan {
    public struct VersionDated {
        public VersionDated(Version version, DateTime date = new DateTime()) {
            Date = date != new DateTime() ? date : DateTime.Today;
            Version = version;
        }

        [JsonProperty("version")]
        public Version Version { get; }

        [JsonProperty("date")]
        public DateTime Date { get; }

        public bool Equals(Version version) {
            return Equals(Version, version);
        }

        public bool Equals(VersionDated other) {
            return Equals(Version, other.Version);
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            return obj is VersionDated && Equals((VersionDated)obj);
        }

        public override int GetHashCode() {
            unchecked {
                return ((Version?.GetHashCode() ?? 0)*397) ^ Date.GetHashCode();
            }
        }

        public override string ToString() {
            return $"v{Version} ({Date.ToShortDateString()})";
        }
    }
}