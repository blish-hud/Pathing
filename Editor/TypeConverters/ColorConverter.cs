using System;
using System.ComponentModel;
using System.Globalization;
using Blish_HUD;
using Microsoft.Xna.Framework;

namespace BhModule.Community.Pathing.Editor.TypeConverters {
    public class ColorConverter : TypeConverter {

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return sourceType == typeof(string)|| base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value?.GetType() == typeof(string)) {
                if (ColorUtil.TryParseHex((string) value, out var result)) {
                    return result;
                }
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override bool IsValid(ITypeDescriptorContext context, object value) {
            if (value?.GetType() == typeof(string)) {
                return ColorUtil.TryParseHex((string) value, out _);
            }

            return base.IsValid(context, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
            var color = (Color)value;

            if (destinationType == typeof(string)) {
                // Unfortunately the packed value is RGBA and not ARGB.
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

    }
}
