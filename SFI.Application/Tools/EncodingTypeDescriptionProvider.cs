using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;
using System.Text;

namespace IS4.SFI.Application.Tools
{
    internal class EncodingTypeDescriptionProvider : TypeDescriptionProvider
    {
        public EncodingTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent)
        {

        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if(instance is Encoding || typeof(Encoding).IsAssignableFrom(objectType))
            {
                return new Descriptor(base.GetTypeDescriptor(objectType, instance));
            }
            return base.GetTypeDescriptor(objectType, instance);
        }

        class Descriptor : CustomTypeDescriptor
        {
            public Descriptor(ICustomTypeDescriptor parent) : base(parent)
            {

            }

            public override TypeConverter GetConverter()
            {
                return new Converter(base.GetConverter());
            }
        }

        class Converter : TypeConverter
        {
            static readonly Type stringType = typeof(string);

            readonly TypeConverter parent;

            public Converter(TypeConverter parent)
            {
                this.parent = parent;
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return stringType.Equals(sourceType) || parent.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return stringType.Equals(destinationType) || parent.CanConvertTo(context, destinationType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if(value is string name)
                {
                    return Encoding.GetEncoding(name);
                }
                return parent.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if(value is Encoding encoding && stringType.Equals(destinationType))
                {
                    return encoding.WebName;
                }
                return parent.ConvertTo(context, culture, value, destinationType);
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

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                return parent.CreateInstance(context, propertyValues);
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return parent.GetCreateInstanceSupported(context);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return parent.GetProperties(context, value, attributes);
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return parent.GetPropertiesSupported(context);
            }

            public override bool IsValid(ITypeDescriptorContext context, object value)
            {
                return parent.IsValid(context, value);
            }
        }
    }
}
