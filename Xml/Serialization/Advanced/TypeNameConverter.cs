using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Olan.Xml.Serialization.Advanced.Serializing;

namespace Olan.Xml.Serialization.Advanced {
    /// <summary>
    ///   Converts Type to its text representation and vice versa. Since v.2.12 all types serialize to the AssemblyQualifiedName.
    ///   Use overloaded constructor to shorten type names.
    /// </summary>
    public sealed class TypeNameConverter : ITypeNameConverter {
        #region Fields

        private static Dictionary<string, Type> _currentAssemblyTypes;
        private readonly Dictionary<Type, string> _cache = new Dictionary<Type, string>();

        #endregion
        #region Constructors

        /// <summary>
        /// Since v.2.12 as default the type name is equal to Type.AssemblyQualifiedName
        /// </summary>
        public TypeNameConverter() { }

        /// <summary>
        ///   Some values from the Type.AssemblyQualifiedName can be removed
        /// </summary>
        /// <param name = "includeAssemblyVersion"></param>
        /// <param name = "includeCulture"></param>
        /// <param name = "includePublicKeyToken"></param>
        public TypeNameConverter(bool includeAssemblyVersion, bool includeCulture, bool includePublicKeyToken, bool findPluginAssembly) {
            IncludeAssemblyVersion = includeAssemblyVersion;
            IncludeCulture = includeCulture;
            IncludePublicKeyToken = includePublicKeyToken;
            FindPluginAssembly = findPluginAssembly;
        }

        #endregion
        #region Properties

        /// <summary>
        ///   Version=x.x.x.x will be inserted to the type name
        /// </summary>
        public bool IncludeAssemblyVersion { get; private set; }

        /// <summary>
        ///   Culture=.... will be inserted to the type name
        /// </summary>
        public bool IncludeCulture { get; private set; }

        /// <summary>
        ///   PublicKeyToken=.... will be inserted to the type name
        /// </summary>
        public bool IncludePublicKeyToken { get; private set; }

        /// <summary>
        ///   Assembly will be inserted to the type name
        /// </summary>
        public bool FindPluginAssembly { get; private set; }

        #endregion
        #region Methods

        private static string RemovePublicKeyToken(string typename) {
            return Regex.Replace(typename, @", PublicKeyToken=\w+", string.Empty);
        }

        private static string RemoveCulture(string typename) {
            return Regex.Replace(typename, @", Culture=\w+", string.Empty);
        }

        private static string RemoveAssemblyVersion(string typename) {
            return Regex.Replace(typename, @", Version=\d+.\d+.\d+.\d+", string.Empty);
        }

        private static string RemoveAssembly(string typename, Type type) {
            var assemblyName = Path.GetFileNameWithoutExtension(type.Assembly.Location);
            return typename.Replace(", " + assemblyName, string.Empty);
            ;
        }

        #endregion
        #region ITypeNameConverter Members

        /// <summary>
        ///   Gives type as text
        /// </summary>
        /// <param name = "type"></param>
        /// <returns>string.Empty if the type is null</returns>
        public string ConvertToTypeName(Type type) {
            if (type == null) {
                return string.Empty;
            }

            // Search in cache
            if (_cache.ContainsKey(type)) {
                return _cache[type];
            }

            var typename = type.AssemblyQualifiedName;

            if (!IncludeAssemblyVersion) {
                typename = RemoveAssemblyVersion(typename);
            }

            if (!IncludeCulture) {
                typename = RemoveCulture(typename);
            }

            if (!IncludePublicKeyToken) {
                typename = RemovePublicKeyToken(typename);
            }

            if (FindPluginAssembly) {
                typename = RemoveAssembly(typename, type);
            }

            // Adding to cache
            _cache.Add(type, typename);

            return typename;
        }

        /// <summary>
        ///   Gives back Type from the text.
        /// </summary>
        /// <param name = "typeName"></param>
        /// <returns></returns>
        public Type ConvertToType(string typeName) {
            Type type = null;
            try {
                if (string.IsNullOrEmpty(typeName)) {
                    return null;
                }

                var assm = Assembly.GetExecutingAssembly();

                type = FindPluginAssembly ? assm.GetType(typeName, false, true) : Type.GetType(typeName, true);

                if (type == null) {
                    type = GetAnyTypeWithName(assm, typeName);
                }
            }
            catch {
                // ignored
            }

            return type;
        }

        public string ReplaceAssemblyPart(string input, string newAssemblyPart) {
            var assemblyParts = input.Split(',');
            var count = assemblyParts.Count();
            if (count > 1) {
                assemblyParts[1] = " " + newAssemblyPart;
            }
            else if (count == 1) {
                return assemblyParts[0] + ", " + newAssemblyPart;
            }
            return string.Join(",", assemblyParts);
        }

        private static readonly Dictionary<string, Type> CurrentAssemblyTypes = new Dictionary<string, Type>();

        /// <summary>
        /// Finds a type in the current assembly by name
        /// E.g. typeName: Application.Module.Part.MyClass may return OtherApplication.OtherModule.OtherPart.MyClass
        /// </summary>
        /// <param name="assm"></param>
        /// <param name="typeFullName"></param>
        /// <returns></returns>
        private static Type GetAnyTypeWithName(Assembly assm, string typeFullName) {
            var inputTypeName = typeFullName.Split('.').LastOrDefault();
            if (inputTypeName == null) {
                return null;
            }

            Type foundType = null;

            if (!CurrentAssemblyTypes.Any()) {
                foreach (var type in assm.GetTypes()) {
                    if (!CurrentAssemblyTypes.ContainsKey(type.Name)) {
                        CurrentAssemblyTypes.Add(type.Name, type);
                    }
                }
            }

            if (CurrentAssemblyTypes.ContainsKey(inputTypeName)) {
                foundType = CurrentAssemblyTypes[inputTypeName];
            }

            return foundType;
        }

        //[System.Runtime.CompilerServices.MethodImpl(MethodImplOptions.NoInlining)]
        //public static string GetCurrentNamespace()
        //{
        //    var declaringType = System.Reflection.Assembly.GetCallingAssembly().EntryPoint.DeclaringType;
        //    return declaringType != null ? declaringType.Namespace : string.Empty;
        //}

        #endregion
    }
}