using System;
using System.Collections.Generic;
using System.IO;
using Olan.Xml.Serialization.Advanced.Serializing;
using Olan.Xml.Serialization.Advanced.Xml;
using Olan.Xml.Serialization.Core;
using Olan.Xml.Serialization.Core.Xml;
using Olan.Xml.Serialization.Serializing;

namespace Olan.Xml.Serialization.Advanced {
    /// <summary>
    ///   Serializes properties to xml or any other target which supports node/attribute notation
    /// </summary>
    /// <remarks>
    ///   Use an instance of your own IXmlWriter in the constructor to target other storage standards
    /// </remarks>
    public sealed class XmlPropertySerializer : PropertySerializer {
        #region Fields

        private readonly IXmlWriter _writer;

        #endregion
        #region Constructors

        ///<summary>
        ///</summary>
        ///<param name = "writer"></param>
        public XmlPropertySerializer(IXmlWriter writer) {
            if (writer == null) {
                throw new ArgumentNullException("writer");
            }
            _writer = writer;
        }

        #endregion
        #region Methods

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeNullProperty(PropertyTypeInfo<NullProperty> property) {
            // nulls must be serialized also 
            WriteStartProperty(Elements.Null, property.Name, property.ValueType);
            WriteEndProperty();
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeSimpleProperty(PropertyTypeInfo<SimpleProperty> property) {
            if (property.Property.Value == null) {
                return;
            }

            WriteStartProperty(Elements.SimpleObject, property.Name, property.ValueType);

            _writer.WriteAttribute(Attributes.Value, property.Property.Value);

            WriteEndProperty();
        }

        private void WriteEndProperty() {
            _writer.WriteEndElement();
        }

        private void WriteStartProperty(string elementId, string propertyName, Type propertyType) {
            _writer.WriteStartElement(elementId);

            // Name
            if (!string.IsNullOrEmpty(propertyName)) {
                _writer.WriteAttribute(Attributes.Name, propertyName);
            }

            // Type
            if (propertyType != null) {
                _writer.WriteAttribute(Attributes.Type, propertyType);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeMultiDimensionalArrayProperty(PropertyTypeInfo<MultiDimensionalArrayProperty> property) {
            WriteStartProperty(Elements.MultiArray, property.Name, property.ValueType);

            // additional attribute with referenceId
            if (property.Property.Reference.Count > 1) {
                _writer.WriteAttribute(Attributes.ReferenceId, property.Property.Reference.Id);
            }

            // DimensionInfos
            WriteDimensionInfos(property.Property.DimensionInfos);

            // Einträge
            WriteMultiDimensionalArrayItems(property.Property.Items, property.Property.ElementType);

            WriteEndProperty();
        }

        private void WriteMultiDimensionalArrayItems(IEnumerable<MultiDimensionalArrayItem> items, Type defaultItemType) {
            _writer.WriteStartElement(SubElements.Items);
            foreach (var item in items) {
                WriteMultiDimensionalArrayItem(item, defaultItemType);
            }
            _writer.WriteEndElement();
        }

        private void WriteMultiDimensionalArrayItem(MultiDimensionalArrayItem item, Type defaultTypeOfItemValue) {
            _writer.WriteStartElement(SubElements.Item);

            // Write Indexes
            _writer.WriteAttribute(Attributes.Indexes, item.Indexes);

            // Write Data
            SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultTypeOfItemValue));

            _writer.WriteEndElement();
        }

        private void WriteDimensionInfos(IEnumerable<DimensionInfo> infos) {
            _writer.WriteStartElement(SubElements.Dimensions);
            foreach (var info in infos) {
                WriteDimensionInfo(info);
            }
            _writer.WriteEndElement();
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeSingleDimensionalArrayProperty(PropertyTypeInfo<SingleDimensionalArrayProperty> property) {
            WriteStartProperty(Elements.SingleArray, property.Name, property.ValueType);

            // additional attribute with referenceId
            if (property.Property.Reference.Count > 1) {
                _writer.WriteAttribute(Attributes.ReferenceId, property.Property.Reference.Id);
            }

            // LowerBound
            if (property.Property.LowerBound != 0) {
                _writer.WriteAttribute(Attributes.LowerBound, property.Property.LowerBound);
            }

            // items
            WriteItems(property.Property.Items, property.Property.ElementType);

            WriteEndProperty();
        }

        private void WriteDimensionInfo(DimensionInfo info) {
            _writer.WriteStartElement(SubElements.Dimension);
            if (info.Length != 0) {
                _writer.WriteAttribute(Attributes.Length, info.Length);
            }
            if (info.LowerBound != 0) {
                _writer.WriteAttribute(Attributes.LowerBound, info.LowerBound);
            }

            _writer.WriteEndElement();
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeDictionaryProperty(PropertyTypeInfo<DictionaryProperty> property) {
            WriteStartProperty(Elements.Dictionary, property.Name, property.ValueType);

            // additional attribute with referenceId
            if (property.Property.Reference.Count > 1) {
                _writer.WriteAttribute(Attributes.ReferenceId, property.Property.Reference.Id);
            }

            // Properties
            WriteProperties(property.Property.Properties, property.Property.Type);

            // Items
            WriteDictionaryItems(property.Property.Items, property.Property.KeyType, property.Property.ValueType);

            WriteEndProperty();
        }

        private void WriteDictionaryItems(IEnumerable<KeyValueItem> items, Type defaultKeyType, Type defaultValueType) {
            _writer.WriteStartElement(SubElements.Items);
            foreach (var item in items) {
                WriteDictionaryItem(item, defaultKeyType, defaultValueType);
            }
            _writer.WriteEndElement();
        }

        private void WriteDictionaryItem(KeyValueItem item, Type defaultKeyType, Type defaultValueType) {
            _writer.WriteStartElement(SubElements.Item);
            SerializeCore(new PropertyTypeInfo<Property>(item.Key, defaultKeyType));
            SerializeCore(new PropertyTypeInfo<Property>(item.Value, defaultValueType));
            _writer.WriteEndElement();
        }

        private void WriteValueType(Type type) {
            if (type != null) {
                _writer.WriteAttribute(Attributes.ValueType, type);
            }
        }

        private void WriteKeyType(Type type) {
            if (type != null) {
                _writer.WriteAttribute(Attributes.KeyType, type);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeCollectionProperty(PropertyTypeInfo<CollectionProperty> property) {
            WriteStartProperty(Elements.Collection, property.Name, property.ValueType);

            // additional attribute with referenceId
            if (property.Property.Reference.Count > 1) {
                _writer.WriteAttribute(Attributes.ReferenceId, property.Property.Reference.Id);
            }

            // Properties
            WriteProperties(property.Property.Properties, property.Property.Type);

            //Items
            WriteItems(property.Property.Items, property.Property.ElementType);

            WriteEndProperty();
        }

        private void WriteItems(IEnumerable<Property> properties, Type defaultItemType) {
            _writer.WriteStartElement(SubElements.Items);
            foreach (var item in properties) {
                SerializeCore(new PropertyTypeInfo<Property>(item, defaultItemType));
            }
            _writer.WriteEndElement();
        }

        /// <summary>
        ///   Properties are only saved if at least one property exists
        /// </summary>
        /// <param name = "properties"></param>
        /// <param name = "ownerType">to which type belong the properties</param>
        private void WriteProperties(ICollection<Property> properties, Type ownerType) {
            // check if there are properties
            if (properties.Count == 0) {
                return;
            }

            _writer.WriteStartElement(SubElements.Properties);
            foreach (var property in properties) {
                var propertyInfo = ownerType.GetProperty(property.Name);
                if (propertyInfo != null) {
                    SerializeCore(new PropertyTypeInfo<Property>(property, propertyInfo.PropertyType));
                }
                else {
                    SerializeCore(new PropertyTypeInfo<Property>(property, null));
                }
            }
            _writer.WriteEndElement();
        }

        /// <summary>
        /// </summary>
        /// <param name = "property"></param>
        protected override void SerializeComplexProperty(PropertyTypeInfo<ComplexProperty> property) {
            WriteStartProperty(Elements.ComplexObject, property.Name, property.ValueType);

            // additional attribute with referenceId
            if (property.Property.Reference.Count > 1) {
                _writer.WriteAttribute(Attributes.ReferenceId, property.Property.Reference.Id);
            }

            // Properties
            WriteProperties(property.Property.Properties, property.Property.Type);

            WriteEndProperty();
        }

        /// <summary>
        /// Stores only reference to an object, not the object itself
        /// </summary>
        /// <param name="referenceTarget"></param>
        protected override void SerializeReference(ReferenceTargetProperty referenceTarget) {
            WriteStartProperty(Elements.Reference, referenceTarget.Name, null);
            _writer.WriteAttribute(Attributes.ReferenceId, referenceTarget.Reference.Id);
            WriteEndProperty();
        }

        /// <summary>
        ///   Open the writer
        /// </summary>
        /// <param name = "stream"></param>
        public override void Open(Stream stream) {
            _writer.Open(stream);
        }

        /// <summary>
        ///   Close the Write, but do not close the stream
        /// </summary>
        public override void Close() {
            _writer.Close();
        }

        #endregion
    }
}