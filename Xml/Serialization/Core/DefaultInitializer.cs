using System.Globalization;
using System.Text;
using System.Xml;
using Olan.Xml.Serialization.Advanced;
using Olan.Xml.Serialization.Advanced.Serializing;
using Olan.Xml.Serialization.Advanced.Xml;

namespace Olan.Xml.Serialization.Core {
    /// <summary>
    ///   Gives standard settings for the framework. Is used only internally.
    /// </summary>
    internal static class DefaultInitializer {
        #region Methods

        public static XmlWriterSettings GetXmlWriterSettings() {
            return GetXmlWriterSettings(Encoding.UTF8);
        }

        public static XmlWriterSettings GetXmlWriterSettings(Encoding encoding) {
            var settings = new XmlWriterSettings();
            settings.Encoding = encoding;
            settings.Indent = true;
            settings.OmitXmlDeclaration = true;
            return settings;
        }

        public static XmlReaderSettings GetXmlReaderSettings() {
            var settings = new XmlReaderSettings();
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            return settings;
        }

        public static ITypeNameConverter GetTypeNameConverter(bool includeAssemblyVersion, bool includeCulture, bool includePublicKeyToken, bool findPluginAssembly) {
            return new TypeNameConverter(includeAssemblyVersion, includeCulture, includePublicKeyToken, findPluginAssembly);
        }

        public static ISimpleValueConverter GetSimpleValueConverter(CultureInfo cultureInfo, ITypeNameConverter typeNameConverter) {
            return new SimpleValueConverter(cultureInfo, typeNameConverter);
        }

        #endregion
    }
}