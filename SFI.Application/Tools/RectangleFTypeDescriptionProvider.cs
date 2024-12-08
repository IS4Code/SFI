using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;

namespace IS4.SFI.Application.Tools
{
    internal class RectangleFTypeDescriptionProvider : CustomStringConversionTypeDescriptionProviderDelegator<RectangleF>
    {
        public RectangleFTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent)
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

            protected override object? ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string value)
            {
                string text = value.Trim();
                if(text.Length == 0)
                {
                    return null;
                }

                culture ??= CultureInfo.CurrentCulture;
                var components = text.Split(culture.TextInfo.ListSeparator[0]);

                var floatConverter = TypeDescriptor.GetConverter(typeof(float));

                float ParseValue(int index, bool last = false)
                {
                    if(last ? components.Length != index + 1 : components.Length <= index)
                    {
                        throw new ArgumentException("The string value must be in the format 'x, y, width, height'.", nameof(value));
                    }
                    return (float)floatConverter.ConvertFromString(context, culture, components[index])!;
                }

                return new RectangleF(ParseValue(0), ParseValue(1), ParseValue(2), ParseValue(3, true));
            }

            protected override string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, RectangleF rectangle)
            {
                culture ??= CultureInfo.CurrentCulture;

                var floatConverter = TypeDescriptor.GetConverter(typeof(float));

                return string.Join(culture.TextInfo.ListSeparator + " ", new[]
                {
                    floatConverter.ConvertToString(context, culture, rectangle.X),
                    floatConverter.ConvertToString(context, culture, rectangle.Y),
                    floatConverter.ConvertToString(context, culture, rectangle.Width),
                    floatConverter.ConvertToString(context, culture, rectangle.Height)
                });
            }
        }
    }
}
