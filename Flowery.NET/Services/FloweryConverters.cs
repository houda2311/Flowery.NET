using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Flowery.Services
{
    /// <summary>
    /// Converts a nullable value to a Thickness. Returns the specified thickness when value is not null,
    /// otherwise returns zero thickness.
    /// </summary>
    /// <remarks>
    /// Use this converter to conditionally apply margins/paddings based on whether another property has a value.
    /// The ConverterParameter specifies the thickness to use when value is not null (format: "left,top,right,bottom" or "uniform").
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;ContentPresenter Margin="{Binding StartIcon, RelativeSource={RelativeSource TemplatedParent},
    ///     Converter={x:Static services:FloweryConverters.NullToThickness}, ConverterParameter='32,0,0,0'}" /&gt;
    /// </code>
    /// </example>
    public class NullToThicknessConverter : IValueConverter
    {
        /// <summary>
        /// Singleton instance for use in XAML.
        /// </summary>
        public static readonly NullToThicknessConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null)
                return new Thickness(0);

            // Parse the parameter as thickness
            var paramStr = parameter?.ToString();
            if (string.IsNullOrEmpty(paramStr))
                return new Thickness(0);

            return ParseThickness(paramStr);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("NullToThicknessConverter is one-way only.");
        }

        private static Thickness ParseThickness(string value)
        {
            var parts = value.Split(',');
            
            if (parts.Length == 1 && double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var uniform))
                return new Thickness(uniform);
            
            if (parts.Length == 2 && 
                double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var h) &&
                double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                return new Thickness(h, v);
            
            if (parts.Length == 4 &&
                double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var left) &&
                double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var top) &&
                double.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var right) &&
                double.TryParse(parts[3].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var bottom))
                return new Thickness(left, top, right, bottom);

            return new Thickness(0);
        }
    }

    /// <summary>
    /// Converts a double value to a uniform Thickness.
    /// Used internally by ScaleExtension to support Thickness properties (Padding, Margin).
    /// </summary>
    public class DoubleToThicknessConverter : IValueConverter
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly DoubleToThicknessConverter Instance = new();

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double d)
                return new Thickness(d);
            
            return new Thickness(0);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotSupportedException("DoubleToThicknessConverter is one-way only.");
        }
    }

    /// <summary>
    /// Provides static converter instances for use in XAML via x:Static.
    /// </summary>
    public static class FloweryConverters
    {
        /// <summary>
        /// Converts a nullable value to Thickness. Returns ConverterParameter as thickness when not null, zero otherwise.
        /// </summary>
        public static readonly NullToThicknessConverter NullToThickness = NullToThicknessConverter.Instance;

        /// <summary>
        /// Converts a double to a uniform Thickness.
        /// </summary>
        public static readonly DoubleToThicknessConverter DoubleToThickness = DoubleToThicknessConverter.Instance;
    }
}

