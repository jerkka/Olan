using System.IO;
using Olan.Xml.Serialization.Core;

namespace Olan.Xml.Serialization.Advanced.Deserializing {
    /// <summary>
    ///   Deserializes a stream and gives back a Property
    /// </summary>
    public interface IPropertyDeserializer {
        #region Methods

        /// <summary>
        ///   Open the stream to read
        /// </summary>
        /// <param name = "stream"></param>
        void Open(Stream stream);

        /// <summary>
        ///   Reading the stream
        /// </summary>
        /// <returns></returns>
        Property Deserialize();

        /// <summary>
        ///   Cleans all
        /// </summary>
        void Close();

        #endregion
    }
}