using System;
using System.Collections.Generic;
using System.IO;
using Olan.Xml.Serialization.Advanced.Deserializing;
using Olan.Xml.Serialization.Advanced.Xml;
using Olan.Xml.Serialization.Core;
using Olan.Xml.Serialization.Core.Xml;
using Olan.Xml.Serialization.Serializing;

namespace Olan.Xml.Serialization.Advanced {
    /// <summary>
    ///   Contains logic to read data stored with XmlPropertySerializer
    /// </summary>
    public sealed class XmlPropertyDeserializer : IPropertyDeserializer {
        #region Fields

        /// <summary>
        /// All reference targets already processed. Used to for reference resolution.
        /// </summary>
        private readonly Dictionary<int, ReferenceTargetProperty> _propertyCache = new Dictionary<int, ReferenceTargetProperty>();
        private readonly IXmlReader _reader;

        #endregion
        #region Constructors

        ///<summary>
        ///</summary>
        ///<param name = "reader"></param>
        public XmlPropertyDeserializer(IXmlReader reader) {
            _reader = reader;
        }

        #endregion
        #region Methods

        private Property deserialize(PropertyArt propertyArt, Type expectedType) {
            // Establish the property name
            var propertyName = _reader.GetAttributeAsString(Attributes.Name);

            // Establish the property type
            var propertyType = _reader.GetAttributeAsType(Attributes.Type);

            // id propertyType is not defined, we'll take the expectedType)
            if (propertyType == null) {
                propertyType = expectedType;
            }

            // create the property from the tag
            var property = Property.CreateInstance(propertyArt, propertyName, propertyType);

            // Null property?
            var nullProperty = property as NullProperty;
            if (nullProperty != null) {
                return nullProperty;
            }

            // is it simple property?
            var simpleProperty = property as SimpleProperty;
            if (simpleProperty != null) {
                ParseSimpleProperty(_reader, simpleProperty);
                return simpleProperty;
            }

            // This is not a null property and not a simple property
            // it could be only ReferenceProperty or a reference

            var referenceId = _reader.GetAttributeAsInt(Attributes.ReferenceId);

            // Adding property to cache, it must be done before deserializing the object.
            // Otherwise stack overflow occures if the object references itself
            var referenceTarget = property as ReferenceTargetProperty;
            if (referenceTarget != null && referenceId > 0 && !_propertyCache.ContainsKey(referenceId)) {
                referenceTarget.Reference = new ReferenceInfo {
                    Id = referenceId, IsProcessed = true
                };
                _propertyCache.Add(referenceId, referenceTarget);
            }

            if (property == null) {
                // Property was not created yet, it can be created as a reference from its id
                if (referenceId < 1) {
                    // there is no reference, so the property cannot be restored
                    return null;
                }

                property = CreateProperty(referenceId, propertyName, propertyType);
                if (property == null) {
                    // Reference was not created
                    return null;
                }

                // property was successfully restored as a reference
                return property;
            }

            var multiDimensionalArrayProperty = property as MultiDimensionalArrayProperty;
            if (multiDimensionalArrayProperty != null) {
                ParseMultiDimensionalArrayProperty(multiDimensionalArrayProperty);
                return multiDimensionalArrayProperty;
            }

            var singleDimensionalArrayProperty = property as SingleDimensionalArrayProperty;
            if (singleDimensionalArrayProperty != null) {
                ParseSingleDimensionalArrayProperty(singleDimensionalArrayProperty);
                return singleDimensionalArrayProperty;
            }

            var dictionaryProperty = property as DictionaryProperty;
            if (dictionaryProperty != null) {
                ParseDictionaryProperty(dictionaryProperty);
                return dictionaryProperty;
            }

            var collectionProperty = property as CollectionProperty;
            if (collectionProperty != null) {
                ParseCollectionProperty(collectionProperty);
                return collectionProperty;
            }

            var complexProperty = property as ComplexProperty;
            if (complexProperty != null) {
                ParseComplexProperty(complexProperty);
                return complexProperty;
            }

            return property;
        }

        private void ParseCollectionProperty(CollectionProperty property) {
            // ElementType
            property.ElementType = property.Type != null ? TypeInfo.GetTypeInfo(property.Type).ElementType : null;

            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Properties) {
                    // Properties
                    ReadProperties(property.Properties, property.Type);
                    continue;
                }

                if (subElement == SubElements.Items) {
                    // Items
                    ReadItems(property.Items, property.ElementType);
                }
            }
        }

        private void ParseDictionaryProperty(DictionaryProperty property) {
            if (property.Type != null) {
                var typeInfo = TypeInfo.GetTypeInfo(property.Type);
                property.KeyType = typeInfo.KeyType;
                property.ValueType = typeInfo.ElementType;
            }

            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Properties) {
                    // Properties
                    ReadProperties(property.Properties, property.Type);
                    continue;
                }
                if (subElement == SubElements.Items) {
                    // Items
                    ReadDictionaryItems(property.Items, property.KeyType, property.ValueType);
                }
            }
        }

        private void ReadDictionaryItems(IList<KeyValueItem> items, Type expectedKeyType, Type expectedValueType) {
            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Item) {
                    ReadDictionaryItem(items, expectedKeyType, expectedValueType);
                }
            }
        }

        private void ReadDictionaryItem(IList<KeyValueItem> items, Type expectedKeyType, Type expectedValueType) {
            Property keyProperty = null;
            Property valueProperty = null;
            foreach (var subElement in _reader.ReadSubElements()) {
                // check if key and value was found
                if (keyProperty != null && valueProperty != null) {
                    break;
                }

                // check if valid tag was found
                var propertyArt = GetPropertyArtFromString(subElement);
                if (propertyArt == PropertyArt.Unknown) {
                    continue;
                }

                // items are as pair key-value defined

                // first is always the key
                if (keyProperty == null) {
                    // Key was not defined yet (the first item was found)
                    keyProperty = deserialize(propertyArt, expectedKeyType);
                    continue;
                }

                // key was defined (the second item was found)
                valueProperty = deserialize(propertyArt, expectedValueType);
            }

            // create the item
            var item = new KeyValueItem(keyProperty, valueProperty);
            items.Add(item);
        }

        private void ParseMultiDimensionalArrayProperty(MultiDimensionalArrayProperty property) {
            property.ElementType = property.Type != null ? TypeInfo.GetTypeInfo(property.Type).ElementType : null;

            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Dimensions) {
                    // Read dimensions
                    ReadDimensionInfos(property.DimensionInfos);
                }

                if (subElement == SubElements.Items) {
                    // Read items
                    ReadMultiDimensionalArrayItems(property.Items, property.ElementType);
                }
            }
        }

        private void ReadMultiDimensionalArrayItems(IList<MultiDimensionalArrayItem> items, Type expectedElementType) {
            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Item) {
                    ReadMultiDimensionalArrayItem(items, expectedElementType);
                }
            }
        }

        private void ReadMultiDimensionalArrayItem(IList<MultiDimensionalArrayItem> items, Type expectedElementType) {
            var indexes = _reader.GetAttributeAsArrayOfInt(Attributes.Indexes);
            foreach (var subElement in _reader.ReadSubElements()) {
                var propertyArt = GetPropertyArtFromString(subElement);
                if (propertyArt == PropertyArt.Unknown) {
                    continue;
                }

                var value = deserialize(propertyArt, expectedElementType);
                var item = new MultiDimensionalArrayItem(indexes, value);
                items.Add(item);
            }
        }

        private void ReadDimensionInfos(IList<DimensionInfo> dimensionInfos) {
            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Dimension) {
                    ReadDimensionInfo(dimensionInfos);
                }
            }
        }

        private void ReadDimensionInfo(IList<DimensionInfo> dimensionInfos) {
            var info = new DimensionInfo();
            info.Length = _reader.GetAttributeAsInt(Attributes.Length);
            info.LowerBound = _reader.GetAttributeAsInt(Attributes.LowerBound);
            dimensionInfos.Add(info);
        }

        private void ParseSingleDimensionalArrayProperty(SingleDimensionalArrayProperty property) {
            // ElementType
            property.ElementType = property.Type != null ? TypeInfo.GetTypeInfo(property.Type).ElementType : null;

            // LowerBound
            property.LowerBound = _reader.GetAttributeAsInt(Attributes.LowerBound);

            // Items
            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Items) {
                    ReadItems(property.Items, property.ElementType);
                }
            }
        }

        private void ReadItems(ICollection<Property> items, Type expectedElementType) {
            foreach (var subElement in _reader.ReadSubElements()) {
                var propertyArt = GetPropertyArtFromString(subElement);
                if (propertyArt != PropertyArt.Unknown) {
                    // Property is found
                    var subProperty = deserialize(propertyArt, expectedElementType);
                    items.Add(subProperty);
                }
            }
        }

        private void ParseComplexProperty(ComplexProperty property) {
            foreach (var subElement in _reader.ReadSubElements()) {
                if (subElement == SubElements.Properties) {
                    ReadProperties(property.Properties, property.Type);
                }
            }
        }

        private void ReadProperties(PropertyCollection properties, Type ownerType) {
            foreach (var subElement in _reader.ReadSubElements()) {
                var propertyArt = GetPropertyArtFromString(subElement);
                if (propertyArt != PropertyArt.Unknown) {
                    // check if the property with the name exists
                    var subPropertyName = _reader.GetAttributeAsString(Attributes.Name);
                    if (string.IsNullOrEmpty(subPropertyName)) {
                        continue;
                    }

                    // estimating the propertyInfo
                    var subPropertyInfo = ownerType.GetProperty(subPropertyName);
                    if (subPropertyInfo != null) {
                        var subProperty = deserialize(propertyArt, subPropertyInfo.PropertyType);
                        properties.Add(subProperty);
                    }
                }
            }
        }

        private void ParseSimpleProperty(IXmlReader reader, SimpleProperty property) {
            property.Value = _reader.GetAttributeAsObject(Attributes.Value, property.Type);
        }

        private Property CreateProperty(int referenceId, string propertyName, Type propertyType) {
            var cachedProperty = _propertyCache[referenceId];
            var property = (ReferenceTargetProperty)Property.CreateInstance(cachedProperty.Art, propertyName, propertyType);
            cachedProperty.Reference.Count++;
            property.MakeFlatCopyFrom(cachedProperty);
            // Reference must be recreated, cause IsProcessed differs for reference and the full property
            property.Reference = new ReferenceInfo {
                Id = referenceId
            };
            return property;
        }

        private static PropertyArt GetPropertyArtFromString(string name) {
            if (name == Elements.SimpleObject) {
                return PropertyArt.Simple;
            }
            if (name == Elements.ComplexObject) {
                return PropertyArt.Complex;
            }
            if (name == Elements.Collection) {
                return PropertyArt.Collection;
            }
            if (name == Elements.SingleArray) {
                return PropertyArt.SingleDimensionalArray;
            }
            if (name == Elements.Null) {
                return PropertyArt.Null;
            }
            if (name == Elements.Dictionary) {
                return PropertyArt.Dictionary;
            }
            if (name == Elements.MultiArray) {
                return PropertyArt.MultiDimensionalArray;
            }
            // is used only for backward compatibility
            if (name == Elements.OldReference) {
                return PropertyArt.Reference;
            }
            // is used since the v.2.12
            if (name == Elements.Reference) {
                return PropertyArt.Reference;
            }

            return PropertyArt.Unknown;
        }

        #endregion
        #region IPropertyDeserializer Members

        /// <summary>
        ///   Open the stream to read
        /// </summary>
        /// <param name = "stream"></param>
        public void Open(Stream stream) {
            _reader.Open(stream);
        }

        /// <summary>
        ///   Reading the property
        /// </summary>
        /// <returns></returns>
        public Property Deserialize() {
            // give the first valid tag back
            var elementName = _reader.ReadElement();

            // In what xml tag is the property saved
            var propertyArt = GetPropertyArtFromString(elementName);

            // check if the property was found
            if (propertyArt == PropertyArt.Unknown) {
                return null;
            }

            var result = deserialize(propertyArt, null);
            return result;
        }

        /// <summary>
        ///   Cleans all
        /// </summary>
        public void Close() {
            _reader.Close();
        }

        #endregion
    }
}