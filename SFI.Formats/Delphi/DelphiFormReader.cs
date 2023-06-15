using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace IS4.SFI.Formats.Data
{
    /// <summary>
    /// Provides methods for reading Delphi objects.
    /// </summary>
    public static class DelphiFormReader
    {
        /// <summary>
        /// Reads an instance of <see cref="DelphiObject"/> from an input stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The encoding for strings in the file.</param>
        /// <returns>An instance representing the contents of the stream.</returns>
        public static DelphiObject Read(Stream input, Encoding encoding)
        {
            var reader = new BinaryReader(input, encoding);
            if(reader.ReadUInt32() != 0x30465054)
            {
                throw new ArgumentException("Not a valid TPF0 file.", nameof(input));
            }
            return new DelphiObject(reader);
        }
    }

    /// <summary>
    /// Represents a Delphi object.
    /// </summary>
    public class DelphiObject : IReadOnlyDictionary<string?, object?>
    {
        /// <summary>
        /// The name of the object.
        /// </summary>
        public string? Name { get; }

        /// <summary>
        /// The type of the object.
        /// </summary>
        public string? BaseType { get; }

        /// <summary>
        /// Stores the properties of the object.
        /// </summary>
        public IDictionary<string?, object?> Properties { get; } = new Dictionary<string?, object?>(StringComparer.OrdinalIgnoreCase);

        static readonly Regex validIdentifier = new(@"^[a-zA-Z_][a-zA-Z_0-9]*(\.[a-zA-Z_][a-zA-Z_0-9])*$", RegexOptions.Compiled);

        static readonly object end = new();

        /// <summary>
        /// Creates a new instance of the object from <see cref="BinaryReader"/>.
        /// </summary>
        /// <param name="reader">The object to read the data from.</param>
        /// <exception cref="ArgumentException">
        /// The input is not a valid serialization of a Delphi object.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The input uses field types unsupported by this implementation.
        /// </exception>
        public DelphiObject(BinaryReader reader)
        {
            string ReadString(bool validate = false)
            {
                int len = reader.ReadByte();
                var result = new string(reader.ReadChars(len));
                if(len > 0 && validate && !validIdentifier.IsMatch(result))
                {
                    throw new ArgumentException("Not a valid Pascal identifier.", nameof(reader));
                }
                return result;
            }

            BaseType = ReadString(true);
            if(BaseType == "") return;
            Name = ReadString(true);

            object? ReadObject()
            {
                byte type = reader.ReadByte();
                switch(type)
                {
                    case 0x00:
                        return end;
                    case 0x01:
                        var objectList = new List<object?>();
                        object? elem;
                        while((elem = ReadObject()) != end)
                        {
                            objectList.Add(elem);
                        }
                        return objectList;
                    case 0x02:
                        return reader.ReadSByte();
                    case 0x03:
                        return reader.ReadInt16();
                    case 0x04:
                        return reader.ReadInt32();
                    case 0x05:
                        return reader.ReadDouble();
                    case 0x06:
                        return ReadString();
                    case 0x07:
                        return ReadString(); //symbol
                    case 0x08:
                        return false;
                    case 0x09:
                        return true;
                    case 0x0A:
                        var blobLen = reader.ReadInt32();
                        return reader.ReadBytes(blobLen);
                    case 0x0B:
                        var symbolList = new List<string>(); //symbol
                        string symbol;
                        while((symbol = ReadString()) != "")
                        {
                            symbolList.Add(symbol);
                        }
                        return symbolList;
                    case 0x0D:
                        return null;
                    case 0x0E:
                        var itemList = new List<Dictionary<string?, object?>>();
                        byte itemType;
                        while((itemType = reader.ReadByte()) != 0)
                        {
                            if(itemType != 1)
                            {
                                throw new NotSupportedException($"Unknown item type 0x{itemType:X2}.");
                            }
                            var dict = new Dictionary<string?, object?>();
                            ReadProperties(dict);
                            itemList.Add(dict);
                        }
                        return itemList;
                    default:
                        throw new NotSupportedException($"Unknown property type 0x{type:X2}.");
                }
            }

            void ReadProperties(IDictionary<string?, object?> properties)
            {
                string name;
                while((name = ReadString()) != "")
                {
                    properties[name] = ReadObject();
                }
            }

            ReadProperties(Properties);

            DelphiObject obj;
            while((obj = new DelphiObject(reader)).BaseType != "")
            {
                Properties[obj.Name] = obj;
            }
        }

        IEnumerable<string?> IReadOnlyDictionary<string?, object?>.Keys => Properties.Keys;

        IEnumerable<object?> IReadOnlyDictionary<string?, object?>.Values => Properties.Values;

        int IReadOnlyCollection<KeyValuePair<string?, object?>>.Count => Properties.Count;

        object? IReadOnlyDictionary<string?, object?>.this[string? key] => Properties[key];

        bool IReadOnlyDictionary<string?, object?>.ContainsKey(string? key)
        {
            return Properties.ContainsKey(key);
        }

        bool IReadOnlyDictionary<string?, object?>.TryGetValue(string? key, out object? value)
        {
            return Properties.TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<string?, object?>> IEnumerable<KeyValuePair<string?, object?>>.GetEnumerator()
        {
            return Properties.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Properties.GetEnumerator();
        }
    }
}
