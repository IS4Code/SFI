using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace IS4.SFI.Application.Tools
{
    internal class EncodingTypeDescriptionProvider : CustomStringConversionTypeDescriptionProviderDelegator<Encoding>
    {
        public EncodingTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent)
        {

        }

        protected override Descriptor GetTypeDescriptor(ICustomTypeDescriptor parent)
        {
            return new CustomDescriptor(parent);
        }

        sealed class CustomDescriptor : Descriptor
        {
            public CustomDescriptor(ICustomTypeDescriptor parent) : base(parent)
            {

            }

            protected override Converter GetConverter(TypeConverter parent)
            {
                return new CustomConverter(parent);
            }
        }

        sealed class CustomConverter : Converter
        {
            public CustomConverter(TypeConverter parent) : base(parent)
            {

            }

            protected override object? ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string name)
            {
                return Encoding.GetEncoding(name);
            }

            protected override string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, Encoding encoding)
            {
                return encoding.WebName;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new(new EncodingConversionCollection(Encoding.GetEncodings()));
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            class EncodingConversionCollection : IList
            {
                readonly EncodingInfo[] values;

                public EncodingConversionCollection(EncodingInfo[] values)
                {
                    this.values = values;
                }

                public object this[int index] {
                    get => values[index].GetEncoding();
                    set => throw new NotSupportedException();
                }

                public int Count => values.Length;

                public bool IsSynchronized => values.IsSynchronized;

                public object SyncRoot => values.SyncRoot;

                public bool IsFixedSize => true;

                public bool IsReadOnly => true;

                public int Add(object value)
                {
                    throw new NotSupportedException();
                }

                public void Clear()
                {
                    throw new NotSupportedException();
                }

                public bool Contains(object value)
                {
                    throw new NotImplementedException();
                }

                public void CopyTo(Array array, int index)
                {
                    for(int i = 0; i < values.Length; i++)
                    {
                        array.SetValue(values[i].GetEncoding(), index + i);
                    }
                }

                public IEnumerator GetEnumerator()
                {
                    foreach(var info in values)
                    {
                        yield return info.GetEncoding();
                    }
                }

                public int IndexOf(object value)
                {
                    throw new NotImplementedException();
                }

                public void Insert(int index, object value)
                {
                    throw new NotSupportedException();
                }

                public void Remove(object value)
                {
                    throw new NotSupportedException();
                }

                public void RemoveAt(int index)
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}
