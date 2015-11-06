using System;
using Newtonsoft.Json;

namespace Olan {
    public class Definition : IEquatable<Definition> {
        #region Fields

        private readonly Type _type;
        private string _author;
        private string _name;
        private string _tag;

        #endregion
        #region Constructors

        public Definition() { }

        public Definition(Type type) {
            _type = type;
        }

        #endregion
        #region Properties

        [JsonProperty("tag")]
        public string Tag {
            get { return _tag ?? (_tag = Name + "." + GetHashCode()); }
            set { _tag = value; }
        }

        [JsonProperty("name")]
        public string Name {
            get { return _name ?? (_name = _type?.Name); }
            set { _name = value; }
        }

        [JsonProperty("author")]
        public string Author {
            get { return _author ?? (_author = Environment.UserName); }
            set { _author = value; }
        }

        [JsonProperty("version")]
        public Version Version { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("changelog")]
        public ChangeLog ChangeLog { get; set; }

        #endregion
        #region Methods

        public bool Equals(Definition other) {
            if (other == null) {
                return false;
            }
            return Tag == other.Tag && Name == other.Name && Author == other.Author && Version == other.Version && Description == other.Description && ChangeLog == other.ChangeLog;
        }

        bool IEquatable<Definition>.Equals(Definition other) {
            return Equals(other);
        }

        public override string ToString() {
            return this.ToJson();
        }

        #endregion
    }
}