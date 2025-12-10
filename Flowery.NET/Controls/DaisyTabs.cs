using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Flowery.Controls
{
    /// <summary>
    /// Defines how tab widths are calculated.
    /// </summary>
    public enum DaisyTabWidthMode
    {
        /// <summary>
        /// Each tab sizes to fit its content (default behavior).
        /// </summary>
        Auto,

        /// <summary>
        /// All tabs have equal width, based on the widest tab.
        /// </summary>
        Equal,

        /// <summary>
        /// All tabs have a fixed width specified by TabWidth property.
        /// </summary>
        Fixed
    }

    public enum DaisyTabVariant
    {
        None,
        Bordered,
        Lifted,
        Boxed
    }

    public class DaisyTabs : TabControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyTabs);

        public static readonly StyledProperty<DaisyTabVariant> VariantProperty =
            AvaloniaProperty.Register<DaisyTabs, DaisyTabVariant>(nameof(Variant), DaisyTabVariant.Bordered);

        public DaisyTabVariant Variant
        {
            get => GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }

        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyTabs, DaisySize>(nameof(Size), DaisySize.Medium);

        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly StyledProperty<DaisyTabWidthMode> TabWidthModeProperty =
            AvaloniaProperty.Register<DaisyTabs, DaisyTabWidthMode>(nameof(TabWidthMode), DaisyTabWidthMode.Auto);

        /// <summary>
        /// Gets or sets how tab widths are calculated.
        /// </summary>
        public DaisyTabWidthMode TabWidthMode
        {
            get => GetValue(TabWidthModeProperty);
            set => SetValue(TabWidthModeProperty, value);
        }

        public static readonly StyledProperty<double> TabWidthProperty =
            AvaloniaProperty.Register<DaisyTabs, double>(nameof(TabWidth), double.NaN);

        /// <summary>
        /// Gets or sets the fixed width for tabs when TabWidthMode is Fixed.
        /// </summary>
        public double TabWidth
        {
            get => GetValue(TabWidthProperty);
            set => SetValue(TabWidthProperty, value);
        }

        public static readonly StyledProperty<double> TabMaxWidthProperty =
            AvaloniaProperty.Register<DaisyTabs, double>(nameof(TabMaxWidth), double.PositiveInfinity);

        /// <summary>
        /// Gets or sets the maximum width for each tab. Works with any TabWidthMode.
        /// </summary>
        public double TabMaxWidth
        {
            get => GetValue(TabMaxWidthProperty);
            set => SetValue(TabMaxWidthProperty, value);
        }

        public static readonly StyledProperty<double> TabMinWidthProperty =
            AvaloniaProperty.Register<DaisyTabs, double>(nameof(TabMinWidth), 0d);

        /// <summary>
        /// Gets or sets the minimum width for each tab.
        /// </summary>
        public double TabMinWidth
        {
            get => GetValue(TabMinWidthProperty);
            set => SetValue(TabMinWidthProperty, value);
        }

        static DaisyTabs()
        {
            TabWidthModeProperty.Changed.AddClassHandler<DaisyTabs>((x, _) => x.UpdateTabWidths());
            TabWidthProperty.Changed.AddClassHandler<DaisyTabs>((x, _) => x.UpdateTabWidths());
            TabMaxWidthProperty.Changed.AddClassHandler<DaisyTabs>((x, _) => x.UpdateTabWidths());
            TabMinWidthProperty.Changed.AddClassHandler<DaisyTabs>((x, _) => x.UpdateTabWidths());
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            UpdateTabWidths();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ItemCountProperty)
            {
                // Defer to allow items to be realized
                Avalonia.Threading.Dispatcher.UIThread.Post(UpdateTabWidths);
            }
        }

        private void UpdateTabWidths()
        {
            var mode = TabWidthMode;
            var fixedWidth = TabWidth;
            var maxWidth = TabMaxWidth;
            var minWidth = TabMinWidth;

            // Get all TabItem children from Items collection
            var tabItems = Items
                .OfType<TabItem>()
                .ToList();

            if (tabItems.Count == 0)
                return;

            foreach (var tabItem in tabItems)
            {
                switch (mode)
                {
                    case DaisyTabWidthMode.Auto:
                        tabItem.Width = double.NaN;
                        tabItem.MinWidth = minWidth;
                        tabItem.MaxWidth = maxWidth;
                        break;

                    case DaisyTabWidthMode.Equal:
                        // For Equal mode, we need to measure all tabs first
                        // Reset to auto, measure, then apply the max
                        tabItem.Width = double.NaN;
                        tabItem.MinWidth = 0;
                        tabItem.MaxWidth = double.PositiveInfinity;
                        break;

                    case DaisyTabWidthMode.Fixed:
                        if (!double.IsNaN(fixedWidth) && fixedWidth > 0)
                        {
                            tabItem.Width = Math.Min(fixedWidth, maxWidth);
                            tabItem.MinWidth = minWidth;
                            tabItem.MaxWidth = maxWidth;
                        }
                        break;
                }
            }

            if (mode == DaisyTabWidthMode.Equal)
            {
                // Force layout to measure natural sizes
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    var measuredWidths = tabItems
                        .Select(t => t.DesiredSize.Width)
                        .Where(w => w > 0)
                        .ToList();

                    if (measuredWidths.Count > 0)
                    {
                        var equalWidth = measuredWidths.Max();
                        equalWidth = Math.Max(equalWidth, minWidth);
                        equalWidth = Math.Min(equalWidth, maxWidth);

                        foreach (var tabItem in tabItems)
                        {
                            tabItem.Width = equalWidth;
                            tabItem.MinWidth = equalWidth;
                            tabItem.MaxWidth = equalWidth;
                        }
                    }
                }, Avalonia.Threading.DispatcherPriority.Render);
            }
        }
    }
}
