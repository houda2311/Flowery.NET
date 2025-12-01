using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace DaisyUI.Avalonia.Controls
{
    public enum FabLayout
    {
        Vertical,
        Flower
    }

    public class DaisyFab : Grid
    {
        public static readonly StyledProperty<FabLayout> LayoutProperty =
            AvaloniaProperty.Register<DaisyFab, FabLayout>(nameof(Layout), FabLayout.Vertical);

        public static readonly StyledProperty<bool> IsOpenProperty =
            AvaloniaProperty.Register<DaisyFab, bool>(nameof(IsOpen));

        public static readonly StyledProperty<DaisyButtonVariant> TriggerVariantProperty =
            AvaloniaProperty.Register<DaisyFab, DaisyButtonVariant>(nameof(TriggerVariant), DaisyButtonVariant.Primary);

        public static readonly StyledProperty<object?> TriggerContentProperty =
            AvaloniaProperty.Register<DaisyFab, object?>(nameof(TriggerContent), "+");

        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyFab, DaisySize>(nameof(Size), DaisySize.Large);

        public FabLayout Layout
        {
            get => GetValue(LayoutProperty);
            set => SetValue(LayoutProperty, value);
        }

        public bool IsOpen
        {
            get => GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        public DaisyButtonVariant TriggerVariant
        {
            get => GetValue(TriggerVariantProperty);
            set => SetValue(TriggerVariantProperty, value);
        }

        public object? TriggerContent
        {
            get => GetValue(TriggerContentProperty);
            set => SetValue(TriggerContentProperty, value);
        }

        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        private DaisyButton? _triggerButton;

        public DaisyFab()
        {
            HorizontalAlignment = HorizontalAlignment.Right;
            VerticalAlignment = VerticalAlignment.Bottom;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            EnsureTriggerButton();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == TriggerVariantProperty ||
                change.Property == TriggerContentProperty ||
                change.Property == SizeProperty)
            {
                UpdateTriggerButton();
            }
        }

        private void EnsureTriggerButton()
        {
            if (_triggerButton != null) return;

            _triggerButton = new DaisyButton
            {
                Shape = DaisyButtonShape.Circle,
                Size = Size,
                Variant = TriggerVariant,
                Content = TriggerContent,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Bottom
            };
            _triggerButton.Classes.Add("fab-trigger");
            _triggerButton.Click += OnTriggerClick;

            Children.Add(_triggerButton);
        }

        private void UpdateTriggerButton()
        {
            if (_triggerButton == null) return;
            _triggerButton.Size = Size;
            _triggerButton.Variant = TriggerVariant;
            _triggerButton.Content = TriggerContent;
        }

        private void OnTriggerClick(object? sender, RoutedEventArgs e)
        {
            IsOpen = !IsOpen;
        }
    }
}
