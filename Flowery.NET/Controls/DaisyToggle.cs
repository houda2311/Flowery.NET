using System;
using Avalonia;
using Avalonia.Controls;

namespace Flowery.Controls
{
    public enum DaisyToggleVariant
    {
        Default,
        Primary,
        Secondary,
        Accent,
        Success,
        Warning,
        Info,
        Error
    }

    /// <summary>
    /// A ToggleSwitch control styled after DaisyUI's Toggle component.
    /// </summary>
    public class DaisyToggle : ToggleSwitch
    {
        protected override Type StyleKeyOverride => typeof(DaisyToggle);

        public static readonly StyledProperty<DaisyToggleVariant> VariantProperty =
            AvaloniaProperty.Register<DaisyToggle, DaisyToggleVariant>(nameof(Variant), DaisyToggleVariant.Default);

        public DaisyToggleVariant Variant
        {
            get => GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }

        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyToggle, DaisySize>(nameof(Size), DaisySize.Medium);

        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// Gets or sets the internal padding of the toggle knob area (maps to --toggle-p).
        /// </summary>
        public static readonly StyledProperty<double> TogglePaddingProperty =
            AvaloniaProperty.Register<DaisyToggle, double>(nameof(TogglePadding), 2.0);

        public double TogglePadding
        {
            get => GetValue(TogglePaddingProperty);
            set => SetValue(TogglePaddingProperty, value);
        }
    }
}
