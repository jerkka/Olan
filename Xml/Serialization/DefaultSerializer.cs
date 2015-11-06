using System.IO;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Olan.UI;

namespace Olan.Xml.Serialization {
    public interface ISerializationProvider {
        #region Methods

        void Serialize(object data, Stream stream);
        void Serialize(object data, string filePath);
        object Deserialize(Stream stream, object toObject);
        object Deserialize(string filePath, object toObject);

        #endregion
    }

    public static class DefaultSerializer {
        #region Constructors

        static DefaultSerializer() {
            var settings = new SharpSerializerXmlSettings {
                IncludeAssemblyVersionInTypeName = false, IncludeCultureInTypeName = false, IncludePublicKeyTokenInTypeName = false, FindPluginAssembly = true
            };

            settings.AdvancedSettings.PropertiesToIgnore.Add(typeof (RelayCommand));
            settings.AdvancedSettings.AttributesToIgnore.Add(typeof (XmlIgnoreAttribute));
            settings.AdvancedSettings.AttributesToIgnore.Add(typeof (IgnoreDataMemberAttribute));

            SerializationProvider = new SharpSerializer(settings);
        }

        #endregion
        #region Properties

        public static ISerializationProvider SerializationProvider { get; set; }

        #endregion
        #region Methods

        public static void Serialize(object data, Stream stream) {
            SerializationProvider.Serialize(data, stream);
        }

        public static void Serialize(object data, string filePath) {
            SerializationProvider.Serialize(data, filePath);
        }

        public static object Deserialize(Stream stream, object toObject = null) {
            return SerializationProvider.Deserialize(stream, toObject);
        }

        public static object Deserialize(string filePath, object toObject) {
            return SerializationProvider.Deserialize(filePath, toObject);
        }

        #endregion
    }
}