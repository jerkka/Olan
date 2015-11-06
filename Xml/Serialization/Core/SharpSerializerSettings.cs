using System;
using System.Collections.Generic;
using Olan.Xml.Serialization.Advanced;
using Olan.Xml.Serialization.Advanced.Serializing;
using Olan.Xml.Serialization.Advanced.Xml;

namespace Olan.Xml.Serialization.Core {
    /// <summary>
    ///   Base class for the settings of the SharpSerializer. Is passed to its constructor.
    /// </summary>
    /// <typeparam name = "T"></typeparam>
    public abstract class SharpSerializerSettings<T>
        where T : AdvancedSharpSerializerSettings, new() {
        #region Fields

        private T _advancedSettings;

        #endregion
        #region Constructors

        /// <summary>
        /// IncludeAssemblyVersionInTypeName, IncludeCultureInTypeName and IncludePublicKeyTokenInTypeName are true
        /// </summary>
        protected SharpSerializerSettings() {
            IncludeAssemblyVersionInTypeName = true;
            IncludeCultureInTypeName = true;
            IncludePublicKeyTokenInTypeName = true;
            FindPluginAssembly = false;
        }

        #endregion
        #region Properties

        /// <summary>
        ///   Contains mostly classes from the namespace Polenter.Serialization.Advanced
        /// </summary>
        public T AdvancedSettings {
            get {
                if (_advancedSettings == default(T)) {
                    _advancedSettings = new T();
                }
                return _advancedSettings;
            }
            set { _advancedSettings = value; }
        }

        /// <summary>
        ///   Version=x.x.x.x will be inserted to the type name
        /// </summary>
        public bool IncludeAssemblyVersionInTypeName { get; set; }

        /// <summary>
        ///   Culture=.... will be inserted to the type name
        /// </summary>
        public bool IncludeCultureInTypeName { get; set; }

        /// <summary>
        ///   PublicKeyToken=.... will be inserted to the type name
        /// </summary>
        public bool IncludePublicKeyTokenInTypeName { get; set; }

        /// <summary>
        ///   Show assembly name in type name
        /// </summary>
        public bool FindPluginAssembly { get; set; }

        #endregion
    }

    ///<summary>
    ///  Base class for the advanced settings. Is common for the binary and xml serialization.
    ///</summary>
    public sealed class AdvancedSharpSerializerXmlSettings : AdvancedSharpSerializerSettings {
        #region Properties

        /// <summary>
        ///   Converts simple values to string and vice versa. Default it is an instance of SimpleValueConverter with CultureInfo.InvariantCulture.
        ///   You can override the default converter to implement your own converting to/from string.
        /// </summary>
        public ISimpleValueConverter SimpleValueConverter { get; set; }

        #endregion
    }

    ///<summary>
    ///</summary>
    public sealed class AdvancedSharpSerializerBinarySettings : AdvancedSharpSerializerSettings { }

    ///<summary>
    ///</summary>
    public class AdvancedSharpSerializerSettings {
        #region Fields

        private IList<Type> _attributesToIgnore;
        private PropertiesToIgnore _propertiesToIgnore;

        #endregion
        #region Constructors

        ///<summary>
        ///</summary>
        public AdvancedSharpSerializerSettings() {
            AttributesToIgnore.Add(typeof (ExcludeFromSerializationAttribute));
            RootName = "Root";
        }

        #endregion
        #region Properties

        /// <summary>
        ///   Which properties should be ignored during the serialization.
        /// </summary>
        /// <remarks>
        ///   In your business objects you can mark these properties with ExcludeFromSerializationAttribute
        ///   In built in .NET Framework classes you can not do this. Therefore you define these properties here.
        ///   I.e. System.Collections.Generic.List has property Capacity which is irrelevant for
        ///   the whole Serialization and should be ignored.
        /// </remarks>
        public PropertiesToIgnore PropertiesToIgnore {
            get {
                if (_propertiesToIgnore == null) {
                    _propertiesToIgnore = new PropertiesToIgnore();
                }
                return _propertiesToIgnore;
            }
            set { _propertiesToIgnore = value; }
        }

        /// <summary>
        /// All Properties marked with one of the contained attribute-types will be ignored on save.
        /// As default, this list contains only ExcludeFromSerializationAttribute.
        /// For performance reasons it would be better to clear this list if this attribute 
        /// is not used in serialized classes.
        /// </summary>
        public IList<Type> AttributesToIgnore {
            get {
                if (_attributesToIgnore == null) {
                    _attributesToIgnore = new List<Type>();
                }
                return _attributesToIgnore;
            }
            set { _attributesToIgnore = value; }
        }

        /// <summary>
        ///   What name has the root item of your serialization. Default is "Root".
        /// </summary>
        public string RootName { get; set; }

        /// <summary>
        ///   Converts Type to string and vice versa. Default is an instance of TypeNameConverter which serializes Types as "type name, assembly name"
        ///   If you want to serialize your objects as fully qualified assembly name, you should set this setting with an instance of TypeNameConverter
        ///   with overloaded constructor.
        /// </summary>
        public ITypeNameConverter TypeNameConverter { get; set; }

        #endregion
    }
}