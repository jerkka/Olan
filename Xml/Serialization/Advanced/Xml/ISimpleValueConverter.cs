using System;

namespace Olan.Xml.Serialization.Advanced.Xml {
    /// <summary>
    ///   Converts values of SimpleProperty to/from string
    /// </summary>
    public interface ISimpleValueConverter {
        #region Methods

        /// <summary>
        /// </summary>
        /// <param name = "value"></param>
        /// <returns>string.Empty if the value is null</returns>
        string ConvertToString(object value);

        /// <summary>
        /// </summary>
        /// <param name = "text"></param>
        /// <param name = "type">expected type. Result should be of this type.</param>
        /// <returns>null if the text is null</returns>
        object ConvertFromString(string text, Type type);

        #endregion
    }
}