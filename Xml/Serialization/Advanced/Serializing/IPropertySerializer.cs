using System.IO;
using Olan.Xml.Serialization.Core;

namespace Olan.Xml.Serialization.Advanced.Serializing {
    /// <summary>
    ///   Serializes property to a stream
    /// </summary>
    public interface IPropertySerializer {
        #region Methods

        /// <summary>
        ///   Open the stream for writing
        /// </summary>
        /// <param name = "stream"></param>
        void Open(Stream stream);

        /// <summary>
        ///   Serializes property
        /// </summary>
        /// <param name = "property"></param>
        void Serialize(Property property);

        /// <summary>
        ///   Cleaning, but the stream can be used further
        /// </summary>
        void Close();

        #endregion
    }
}