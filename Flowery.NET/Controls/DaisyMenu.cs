using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Flowery.Controls
{
    public class DaisyMenu : ListBox
    {
        protected override Type StyleKeyOverride => typeof(DaisyMenu);

        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<DaisyMenu, Orientation>(nameof(Orientation), Orientation.Vertical);

        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Gets or sets the foreground color of active/selected menu items (maps to --menu-active-fg).
        /// </summary>
        public static readonly StyledProperty<IBrush?> ActiveForegroundProperty =
            AvaloniaProperty.Register<DaisyMenu, IBrush?>(nameof(ActiveForeground));

        public IBrush? ActiveForeground
        {
            get => GetValue(ActiveForegroundProperty);
            set => SetValue(ActiveForegroundProperty, value);
        }

        /// <summary>
        /// Gets or sets the background color of active/selected menu items (maps to --menu-active-bg).
        /// </summary>
        public static readonly StyledProperty<IBrush?> ActiveBackgroundProperty =
            AvaloniaProperty.Register<DaisyMenu, IBrush?>(nameof(ActiveBackground));

        public IBrush? ActiveBackground
        {
            get => GetValue(ActiveBackgroundProperty);
            set => SetValue(ActiveBackgroundProperty, value);
        }

        static DaisyMenu()
        {
            SelectionChangedEvent.AddClassHandler<DaisyMenu>((menu, _) => menu.UpdateActiveColors());
            ActiveForegroundProperty.Changed.AddClassHandler<DaisyMenu>((menu, _) => menu.UpdateActiveColors());
            ActiveBackgroundProperty.Changed.AddClassHandler<DaisyMenu>((menu, _) => menu.UpdateActiveColors());
        }

        protected override void OnApplyTemplate(Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            Avalonia.Threading.Dispatcher.UIThread.Post(UpdateActiveColors, Avalonia.Threading.DispatcherPriority.Loaded);
        }

        protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
        {
            base.PrepareContainerForItemOverride(container, item, index);
            if (container is ListBoxItem listBoxItem && listBoxItem.IsSelected)
            {
                ApplyActiveColorsToItem(listBoxItem);
            }
        }

        private void UpdateActiveColors()
        {
            int count = ItemCount;
            for (int i = 0; i < count; i++)
            {
                var container = ContainerFromIndex(i) as ListBoxItem;
                if (container == null) continue;

                if (container.IsSelected)
                {
                    ApplyActiveColorsToItem(container);
                }
                else
                {
                    container.ClearValue(BackgroundProperty);
                    container.ClearValue(ForegroundProperty);
                }
            }
        }

        private void ApplyActiveColorsToItem(ListBoxItem container)
        {
            var activeFg = ActiveForeground;
            var activeBg = ActiveBackground;

            if (activeBg != null)
                container.Background = activeBg;
            if (activeFg != null)
                container.Foreground = activeFg;
        }
    }
}
