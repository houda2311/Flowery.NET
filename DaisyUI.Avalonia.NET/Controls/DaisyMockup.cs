using System;
using Avalonia;
using Avalonia.Controls;

namespace DaisyUI.Avalonia.Controls
{
    public enum DaisyMockupVariant
    {
        Code,
        Window,
        Browser
    }

    public class DaisyMockup : ContentControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyMockup);

        public static readonly StyledProperty<DaisyMockupVariant> VariantProperty =
            AvaloniaProperty.Register<DaisyMockup, DaisyMockupVariant>(nameof(Variant), DaisyMockupVariant.Code);

        public DaisyMockupVariant Variant
        {
            get => GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }
    }
}
