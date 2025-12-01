using System;
using Avalonia;
using Avalonia.Controls;

namespace DaisyUI.Avalonia.Controls
{
    public class DaisyDivider : ContentControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyDivider);
        
        public static readonly StyledProperty<bool> HorizontalProperty =
            AvaloniaProperty.Register<DaisyDivider, bool>(nameof(Horizontal), true);

        public bool Horizontal
        {
            get => GetValue(HorizontalProperty);
            set => SetValue(HorizontalProperty, value);
        }
    }
}
