using System;
using System.Collections;
using System.ComponentModel;
using System.Globalization;

namespace IS4.SFI.Application.Tools
{
    internal abstract class CustomStringConversionTypeDescriptionProviderDelegator<T> : TypeDescriptionProvider
    {
        public CustomStringConversionTypeDescriptionProviderDelegator(TypeDescriptionProvider parent) : base(parent)
        {

        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if(instance is T || typeof(T).IsAssignableFrom(objectType))
            {
                return GetTypeDescriptor(base.GetTypeDescriptor(objectType, instance));
            }
            return base.GetTypeDescriptor(objectType, instance);
        }

        protected abstract Descriptor GetTypeDescriptor(ICustomTypeDescriptor parent);

        protected abstract class Descriptor : CustomTypeDescriptor
        {
            public Descriptor(ICustomTypeDescriptor parent) : base(parent)
            {

            }

            public sealed override TypeConverter GetConverter()
            {
                return GetConverter(base.GetConverter());
            }

            protected abstract Converter GetConverter(TypeConverter parent);
        }

        protected abstract class Converter : TypeConverter
        {
            static readonly Type stringType = typeof(string);

            protected TypeConverter Parent { get; }

            public Converter(TypeConverter parent)
            {
                Parent = parent;
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return stringType.Equals(sourceType) || Parent.CanConvertFrom(context, sourceType);
            }

            public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
            {
                return stringType.Equals(destinationType) || Parent.CanConvertTo(context, destinationType);
            }

            protected new abstract object? ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string value);

            public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                if(value is string strValue)
                {
                    return ConvertFromString(context, culture, strValue);
                }
                return Parent.ConvertFrom(context, culture, value);
            }

            protected abstract string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, T value);

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                if(value is T val && stringType.Equals(destinationType))
                {
                    return ConvertToString(context, culture, val);
                }
                return Parent.ConvertTo(context, culture, value, destinationType);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return Parent.GetStandardValues(context);
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return Parent.GetStandardValuesExclusive(context);
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return Parent.GetStandardValuesSupported(context);
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                return Parent.CreateInstance(context, propertyValues);
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return Parent.GetCreateInstanceSupported(context);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return Parent.GetProperties(context, value, attributes);
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return Parent.GetPropertiesSupported(context);
            }

            public override bool IsValid(ITypeDescriptorContext context, object value)
            {
                return Parent.IsValid(context, value);
            }
        }
    }
}
