using System;
using Avalonia.Data.Converters;
using System.Globalization;

namespace DaisyUI.Avalonia.Controls
{
    public static class DrawerConverters
    {
        public static readonly IValueConverter NegativeConverter = new FuncValueConverter<double, double>(val => -val);
    }
}
