using System;

namespace Olan.Xml.Serialization {
    /// <summary>
    ///   All labeled with that Attribute object properties are ignored during the serialization. See PropertyProvider
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ExcludeFromSerializationAttribute : Attribute { }
}