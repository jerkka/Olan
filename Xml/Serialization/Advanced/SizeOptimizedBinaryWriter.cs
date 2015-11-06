using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Olan.Xml.Serialization.Advanced.Binary;
using Olan.Xml.Serialization.Advanced.Serializing;
using Olan.Xml.Serialization.Core.Binary;

namespace Olan.Xml.Serialization.Advanced {
    /// <summary>
    ///   Stores data in a binary format. Data is stored in two steps. At first are all objects stored in a cache and all types are analysed. 
    ///   Then all types and property names are sorted and placed in a list. Duplicates are removed. Serialized objects contain references
    ///   to these types and property names. It decreases file size, especially for serialization of collection (many items of the same type)
    ///   SizeOptimizedBinaryWriter has bigger overhead than BurstBinaryWriter
    /// </summary>
    public sealed class SizeOptimizedBinaryWriter : IBinaryWriter {
        #region Fields

        private readonly Encoding _encoding;
        private readonly ITypeNameConverter _typeNameConverter;
        private List<WriteCommand> _cache;
        private IndexGenerator<string> _names;
        private Stream _stream;
        private IndexGenerator<Type> _types;

        #endregion
        #region Constructors

        ///<summary>
        ///</summary>
        ///<param name = "typeNameConverter"></param>
        ///<param name = "encoding"></param>
        ///<exception cref = "ArgumentNullException"></exception>
        public SizeOptimizedBinaryWriter(ITypeNameConverter typeNameConverter, Encoding encoding) {
            if (typeNameConverter == null) {
                throw new ArgumentNullException("typeNameConverter");
            }
            if (encoding == null) {
                throw new ArgumentNullException("encoding");
            }
            _encoding = encoding;
            _typeNameConverter = typeNameConverter;
        }

        #endregion
        #region Methods

        private static void WriteCache(List<WriteCommand> cache, BinaryWriter writer) {
            foreach (var command in cache) {
                command.Write(writer);
            }
        }

        private void WriteNamesHeader(BinaryWriter writer) {
            // count
            BinaryWriterTools.WriteNumber(_names.Items.Count, writer);

            // Items
            foreach (var name in _names.Items) {
                BinaryWriterTools.WriteString(name, writer);
            }
        }

        private void WriteTypesHeader(BinaryWriter writer) {
            // count
            BinaryWriterTools.WriteNumber(_types.Items.Count, writer);

            // Items
            foreach (var type in _types.Items) {
                var typeName = _typeNameConverter.ConvertToTypeName(type);
                BinaryWriterTools.WriteString(typeName, writer);
            }
        }

        #endregion
        #region Nested type: ByteWriteCommand

        private sealed class ByteWriteCommand : WriteCommand {
            #region Constructors

            public ByteWriteCommand(byte data) {
                Data = data;
            }

            #endregion
            #region Properties

            public byte Data { get; set; }

            #endregion
            #region Methods

            public override void Write(BinaryWriter writer) {
                writer.Write(Data);
            }

            #endregion
        }

        #endregion
        #region Nested type: NumbersWriteCommand

        private sealed class NumbersWriteCommand : WriteCommand {
            #region Constructors

            public NumbersWriteCommand(int[] data) {
                Data = data;
            }

            #endregion
            #region Properties

            public int[] Data { get; set; }

            #endregion
            #region Methods

            public override void Write(BinaryWriter writer) {
                BinaryWriterTools.WriteNumbers(Data, writer);
            }

            #endregion
        }

        #endregion
        #region Nested type: NumberWriteCommand

        private sealed class NumberWriteCommand : WriteCommand {
            #region Constructors

            public NumberWriteCommand(int data) {
                Data = data;
            }

            #endregion
            #region Properties

            public int Data { get; set; }

            #endregion
            #region Methods

            public override void Write(BinaryWriter writer) {
                BinaryWriterTools.WriteNumber(Data, writer);
            }

            #endregion
        }

        #endregion
        #region Nested type: ValueWriteCommand

        private sealed class ValueWriteCommand : WriteCommand {
            #region Constructors

            public ValueWriteCommand(object data) {
                Data = data;
            }

            #endregion
            #region Properties

            public object Data { get; set; }

            #endregion
            #region Methods

            public override void Write(BinaryWriter writer) {
                BinaryWriterTools.WriteValue(Data, writer);
            }

            #endregion
        }

        #endregion
        #region Nested type: WriteCommand

        private abstract class WriteCommand {
            #region Methods

            public abstract void Write(BinaryWriter writer);

            #endregion
        }

        #endregion
        #region IBinaryWriter Members

        /// <summary>
        ///   Writes Property Id
        /// </summary>
        /// <param name = "id"></param>
        public void WriteElementId(byte id) {
            _cache.Add(new ByteWriteCommand(id));
        }

        /// <summary>
        ///   Writes type
        /// </summary>
        /// <param name = "type"></param>
        public void WriteType(Type type) {
            var typeIndex = _types.GetIndexOfItem(type);
            _cache.Add(new NumberWriteCommand(typeIndex));
        }

        /// <summary>
        ///   Writes property name
        /// </summary>
        /// <param name = "name"></param>
        public void WriteName(string name) {
            var nameIndex = _names.GetIndexOfItem(name);
            _cache.Add(new NumberWriteCommand(nameIndex));
        }

        /// <summary>
        ///   Writes a simple value (value of a simple property)
        /// </summary>
        /// <param name = "value"></param>
        public void WriteValue(object value) {
            _cache.Add(new ValueWriteCommand(value));
        }

        /// <summary>
        ///   Writes an integer. It saves the number with the least required bytes
        /// </summary>
        /// <param name = "number"></param>
        public void WriteNumber(int number) {
            _cache.Add(new NumberWriteCommand(number));
        }

        /// <summary>
        ///   Writes an array of numbers. It saves numbers with the least required bytes
        /// </summary>
        /// <param name = "numbers"></param>
        public void WriteNumbers(int[] numbers) {
            _cache.Add(new NumbersWriteCommand(numbers));
        }

        /// <summary>
        ///   Opens the stream for writing
        /// </summary>
        /// <param name = "stream"></param>
        public void Open(Stream stream) {
            _stream = stream;
            _cache = new List<WriteCommand>();
            _types = new IndexGenerator<Type>();
            _names = new IndexGenerator<string>();
        }

        /// <summary>
        ///   Saves the data to the stream, the stream is not closed and can be further used
        /// </summary>
        public void Close() {
            var writer = new BinaryWriter(_stream, _encoding);

            // Write Names
            WriteNamesHeader(writer);

            // Write Types
            WriteTypesHeader(writer);

            // Write Data
            WriteCache(_cache, writer);

            writer.Flush();
        }

        #endregion
    }
}