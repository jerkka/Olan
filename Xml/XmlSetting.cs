using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;
using Olan.Xml.Serialization;

namespace Olan.Xml {
    public class XmlSetting : INotifyPropertyChanged {
        #region Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        #region Fields

        private Type _baseType;
        private Dictionary<int, PropertyInfo> _embeddedSettings;
        private Dictionary<int, PropertyInfo> _properties;
        private string _settingsPath;
        private Type _type;

        [XmlIgnore]
        [IgnoreDataMember]
        public Dictionary<int, object> Defaults;

        public bool SaveOnDispose = true;

        #endregion
        #region Constructors

        public XmlSetting() {
            SetValuesFromAttributes();
        }

        ~XmlSetting() {
            if (SaveOnDispose) {
                Save();
            }
        }

        #endregion
        #region Properties

        [XmlIgnore]
        public Type BaseType => _baseType ?? (_baseType = GetType());

        [XmlIgnore]
        [IgnoreDataMember]
        public Dictionary<int, PropertyInfo> Properties => _properties ?? (_properties = GetProperties(SystemType));

        [XmlIgnore]
        [IgnoreDataMember]
        public Dictionary<int, PropertyInfo> EmbeddedSettings => _embeddedSettings ?? (_embeddedSettings = GetEmbeddedSettings(SystemType));

        [XmlIgnore]
        [IgnoreDataMember]
        public Type SystemType => _type ?? (_type = GetType());

        [XmlIgnore]
        [IgnoreDataMember]
        public static string SettingsDirectory {
            get {
                var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Settings", "Olan");
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }
                return path;
            }
        }

        [XmlIgnore]
        [IgnoreDataMember]
        public string SettingsPath {
            get { return _settingsPath; }
            set {
                if (_settingsPath != value) {
                    _settingsPath = value;
                    OnPropertyChanged();
                }
            }
        }

        [XmlIgnore]
        [IgnoreDataMember]
        public bool SettingsInitialized { get; set; }

        #endregion
        #region Methods

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = "") {
            if (EqualityComparer<T>.Default.Equals(field, value)) {
                return false;
            }
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected string GetLastMethodFullName(int frame = 1) {
            var stackTrace = new StackTrace();
            var methodBase = stackTrace.GetFrame(frame).GetMethod();
            if (methodBase.DeclaringType != null) {
                return methodBase.DeclaringType.FullName + "." + methodBase.Name + "()";
            }
            return methodBase.Name;
        }

        public void SetValuesFromAttributes() {
            foreach (PropertyDescriptor property in TypeDescriptor.GetProperties(this)) {
                var myAttribute = (DefaultValueAttribute)property.Attributes[typeof (DefaultValueAttribute)];
                if (myAttribute != null) {
                    property.SetValue(this, myAttribute.Value);
                }
            }
        }

        public virtual void LoadSettings(string path = "") {
            try {
                SetDefaults();

                if (string.IsNullOrEmpty(path)) {
                    SettingsPath = Path.Combine(SettingsDirectory, _type.Name + ".xml");
                }
                else {
                    path = Path.GetFileNameWithoutExtension(path);
                    SettingsPath = Path.Combine(SettingsDirectory, path + ".xml");
                }

                if (File.Exists(SettingsPath)) {
                    ReadSettingsFromFile(SettingsPath);
                }
                else {
                    Save();
                }

                SettingsInitialized = true;

                OnSettingsLoaded();
            }
            catch {
                // ignored
            }
        }

        public virtual void OnSettingsLoaded() { }

        private Dictionary<int, PropertyInfo> GetProperties(Type type) {
            var props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty);
            return props.ToDictionary(k => k.GetHashCode(), v => v);
        }

        private Dictionary<int, PropertyInfo> GetEmbeddedSettings(Type type) {
            var settingsType = typeof (XmlSetting);
            var assignableProps = type.GetProperties().Where(p => p.PropertyType != SystemType && settingsType.IsAssignableFrom(p.PropertyType));
            return assignableProps.ToDictionary(k => k.GetHashCode(), v => v);
        }

        private void ReadSettingsFromFile(string path = "") {
            if (!File.Exists(path)) {
                return;
            }

            DefaultSerializer.Deserialize(path, this);
        }

        public XElement ReadFile() {
            if (!File.Exists(SettingsPath)) {
                return null;
            }

            return XElement.Load(SettingsPath, LoadOptions.SetLineInfo);
        }

        public virtual void LoadFrom(object obj) {
            if (obj.GetType() != _type) {
                return;
            }
            foreach (var property in Properties.Values.Where(property => !property.IsDefined(typeof (XmlIgnoreAttribute), false) && !property.IsDefined(typeof (IgnoreDataMemberAttribute), false))) {
                property.SetValue(this, property.GetValue(obj, null), null);
            }
        }

        public virtual void Save() {
            if (string.IsNullOrWhiteSpace(SettingsPath)) {
                return;
            }
            if (!File.Exists(SettingsPath)) {
                var path = Path.GetDirectoryName(SettingsPath);
                if (path != null) {
                    Directory.CreateDirectory(path);
                }
            }
            try {
                DefaultSerializer.Serialize(this, SettingsPath);
            }
            catch (InvalidOperationException) {
                // ignored
            }
        }

        public void SetDefaults() {
            if (Properties == null) {
                return;
            }
            if (Defaults != null) {
                foreach (var pair in Defaults) {
                    if (Defaults.ContainsKey(pair.Key)) {
                        var prop = Properties[pair.Key];
                        prop.SetValue(this, pair.Value, null);
                        OnPropertyChanged(prop.Name);
                    }
                }
                return;
            }
            CacheDefaults();
        }

        private void CacheDefaults() {
            if (Properties == null) {
                return;
            }
            Defaults = new Dictionary<int, object>();
            foreach (var property in Properties.Values) {
                var defaultAttribute = property.GetCustomAttribute(typeof (DefaultValueAttribute), false) as DefaultValueAttribute;
                if (defaultAttribute != null) {
                    try {
                        property.SetValue(this, defaultAttribute.Value, null);
                        Defaults.Add(property.GetHashCode(), defaultAttribute.Value);
                        OnPropertyChanged(property.Name);
                    }
                    catch (ArgumentException) {
                        // ignored
                    }
                }
            }
        }

        /// <summary>
        /// Compress a string into GZIP Base64
        /// </summary>
        /// <param name="text">string to be compressed</param>
        /// <returns>compressed string</returns>
        public static string Compress(string text) {
            var buffer = Encoding.UTF8.GetBytes(text);
            var ms = new MemoryStream();
            using (var zip = new GZipStream(ms, CompressionMode.Compress, true)) {
                zip.Write(buffer, 0, buffer.Length);
            }
            ms.Position = 0;
            var compressed = new byte[ms.Length];
            ms.Read(compressed, 0, compressed.Length);
            var gzBuffer = new byte[compressed.Length + 4];
            Buffer.BlockCopy(compressed, 0, gzBuffer, 4, compressed.Length);
            Buffer.BlockCopy(BitConverter.GetBytes(buffer.Length), 0, gzBuffer, 0, 4);
            return Convert.ToBase64String(gzBuffer);
        }

        /// <summary>
        /// Convert a compressed string back to its original form.
        /// </summary>
        /// <param name="compressedText">string to be decompressed</param>
        /// <returns>original string</returns>
        public static string Decompress(string compressedText) {
            var gzBuffer = Convert.FromBase64String(compressedText);
            using (var ms = new MemoryStream()) {
                var msgLength = BitConverter.ToInt32(gzBuffer, 0);
                ms.Write(gzBuffer, 4, gzBuffer.Length - 4);
                var buffer = new byte[msgLength];
                ms.Position = 0;
                using (var zip = new GZipStream(ms, CompressionMode.Decompress)) {
                    zip.Read(buffer, 0, buffer.Length);
                }
                return Encoding.UTF8.GetString(buffer);
            }
        }

        public virtual void RefreshPropertyChanged() {
            OnPropertyChanged("");
        }

        #endregion
    }
}