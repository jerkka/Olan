using System;
using System.IO;
using System.Text;
using System.Xml;
using Olan.Xml.Serialization.Advanced.Serializing;
using Olan.Xml.Serialization.Advanced.Xml;

namespace Olan.Xml.Serialization.Advanced {
    /// <summary>
    ///   Stores data in xml format
    /// </summary>
    public sealed class DefaultXmlWriter : IXmlWriter {
        #region Fields

        private readonly XmlWriterSettings _settings;
        private readonly ISimpleValueConverter _simpleValueConverter;
        private readonly ITypeNameConverter _typeNameProvider;

        private XmlWriter _writer;

        #endregion
        #region Constructors

        /// <summary>
        ///   Constructor with custom ITypeNameProvider and ISimpleValueConverter and custom XmlWriterSettings
        /// </summary>
        public DefaultXmlWriter(ITypeNameConverter typeNameProvider, ISimpleValueConverter simpleValueConverter, XmlWriterSettings settings) {
            if (typeNameProvider == null) {
                throw new ArgumentNullException("typeNameProvider");
            }
            if (simpleValueConverter == null) {
                throw new ArgumentNullException("simpleValueConverter");
            }
            if (settings == null) {
                throw new ArgumentNullException("settings");
            }

            _simpleValueConverter = simpleValueConverter;
            _settings = settings;
            _typeNameProvider = typeNameProvider;
        }

        #endregion
        #region Methods

        /// <summary>
        ///   Converts int[] {1,2,3,4,5} to text "1,2,3,4,5"
        /// </summary>
        /// <param name = "values"></param>
        /// <returns></returns>
        private static string GetArrayOfIntAsText(int[] values) {
            if (values.Length == 0) {
                return string.Empty;
            }
            var sb = new StringBuilder();
            foreach (var index in values) {
                sb.Append(index.ToString());
                sb.Append(",");
            }
            var result = sb.ToString().TrimEnd(',');
            return result;
        }

        #endregion
        #region IXmlWriter Members

        ///<summary>
        ///  Writes start tag/node/element
        ///</summary>
        ///<param name = "elementId"></param>
        public void WriteStartElement(string elementId) {
            _writer.WriteStartElement(elementId);
        }

        ///<summary>
        ///  Writes end tag/node/element
        ///</summary>
        public void WriteEndElement() {
            _writer.WriteEndElement();
        }

        ///<summary>
        ///  Writes attribute of type string
        ///</summary>
        ///<param name = "attributeId"></param>
        ///<param name = "text"></param>
        public void WriteAttribute(string attributeId, string text) {
            if (text == null) {
                return;
            }
            _writer.WriteAttributeString(attributeId, text);
        }

        ///<summary>
        ///  Writes attribute of type Type
        ///</summary>
        ///<param name = "attributeId"></param>
        ///<param name = "type"></param>
        public void WriteAttribute(string attributeId, Type type) {
            if (type == null) {
                return;
            }
            var valueAsText = _typeNameProvider.ConvertToTypeName(type);
            WriteAttribute(attributeId, valueAsText);
        }

        ///<summary>
        ///  Writes attribute of type integer
        ///</summary>
        ///<param name = "attributeId"></param>
        ///<param name = "number"></param>
        public void WriteAttribute(string attributeId, int number) {
            _writer.WriteAttributeString(attributeId, number.ToString());
        }

        ///<summary>
        ///  Writes attribute of type array of int
        ///</summary>
        ///<param name = "attributeId"></param>
        ///<param name = "numbers"></param>
        public void WriteAttribute(string attributeId, int[] numbers) {
            var valueAsText = GetArrayOfIntAsText(numbers);
            _writer.WriteAttributeString(attributeId, valueAsText);
        }

        ///<summary>
        ///  Writes attribute of a simple type (value of a SimpleProperty)
        ///</summary>
        ///<param name = "attributeId"></param>
        ///<param name = "value"></param>
        public void WriteAttribute(string attributeId, object value) {
            if (value == null) {
                return;
            }
            var valueAsText = _simpleValueConverter.ConvertToString(value);
            _writer.WriteAttributeString(attributeId, valueAsText);
        }

        ///<summary>
        ///  Opens the stream
        ///</summary>
        ///<param name = "stream"></param>
        public void Open(Stream stream) {
            _writer = XmlWriter.Create(stream, _settings);

            _writer.WriteStartDocument(true);
        }

        /// <summary>
        ///   Writes all data to the stream, the stream can be further used.
        /// </summary>
        public void Close() {
            _writer.WriteEndDocument();
#if PORTABLE
            _writer.Dispose();
#else
            _writer.Close();
#endif
        }

        #endregion
    }
}