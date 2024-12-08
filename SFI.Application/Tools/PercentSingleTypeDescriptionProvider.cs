using System;
using System.ComponentModel;
using System.Globalization;

namespace IS4.SFI.Application.Tools
{
    internal class PercentSingleTypeDescriptionProvider : CustomStringConversionTypeDescriptionProviderDelegator<float>
    {
        public PercentSingleTypeDescriptionProvider(TypeDescriptionProvider parent) : base(parent)
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

            protected override object? ConvertFromString(ITypeDescriptorContext context, CultureInfo culture, string text)
            {
                culture ??= CultureInfo.CurrentCulture;
                text = text.Trim();

                var format = culture.NumberFormat;

                var pct = format.PercentSymbol;
                if(!String.IsNullOrWhiteSpace(pct) && text.IndexOf(format.PercentSymbol, StringComparison.Ordinal) != -1)
                {
                    // Parse as percent
                    format = new NumberFormatInfo()
                    {
                        CurrencyDecimalDigits = format.PercentDecimalDigits,
                        CurrencyDecimalSeparator = format.PercentDecimalSeparator,
                        CurrencyGroupSeparator = format.PercentGroupSeparator,
                        CurrencyGroupSizes = format.PercentGroupSizes,
                        CurrencyNegativePattern = format.PercentNegativePattern,
                        CurrencyPositivePattern = format.PercentPositivePattern,
                        CurrencySymbol = format.PercentSymbol
                    };
                    return Single.Parse(text, NumberStyles.Float | NumberStyles.Currency, format) / 100;
                }
                return Single.Parse(text, NumberStyles.Float, format);
            }

            protected override string ConvertToString(ITypeDescriptorContext context, CultureInfo culture, float value)
            {
                return Parent.ConvertToString(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                return Parent.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
