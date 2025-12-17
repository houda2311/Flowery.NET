using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace Flowery.Controls
{
    public class DaisyDropdownSelectionChangedEventArgs : EventArgs
    {
        public DaisyDropdownSelectionChangedEventArgs(object? selectedItem)
        {
            SelectedItem = selectedItem;
        }

        public object? SelectedItem { get; }
    }

    /// <summary>
    /// A lightweight dropdown menu control built on a Popup.
    /// </summary>
    public class DaisyDropdown : TemplatedControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyDropdown);

        private Control? _trigger;
        private ListBox? _menu;

        /// <summary>
        /// Defines the <see cref="ItemsSource"/> property.
        /// </summary>
        public static readonly StyledProperty<IEnumerable?> ItemsSourceProperty =
            AvaloniaProperty.Register<DaisyDropdown, IEnumerable?>(nameof(ItemsSource));

        /// <summary>
        /// Gets or sets the dropdown items.
        /// </summary>
        public IEnumerable? ItemsSource
        {
            get => GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="SelectedItem"/> property.
        /// </summary>
        public static readonly StyledProperty<object?> SelectedItemProperty =
            AvaloniaProperty.Register<DaisyDropdown, object?>(nameof(SelectedItem));

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object? SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="PlaceholderText"/> property.
        /// </summary>
        public static readonly StyledProperty<string?> PlaceholderTextProperty =
            AvaloniaProperty.Register<DaisyDropdown, string?>(nameof(PlaceholderText), "Select");

        /// <summary>
        /// Gets or sets the placeholder text displayed when no item is selected.
        /// </summary>
        public string? PlaceholderText
        {
            get => GetValue(PlaceholderTextProperty);
            set => SetValue(PlaceholderTextProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="IsOpen"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> IsOpenProperty =
            AvaloniaProperty.Register<DaisyDropdown, bool>(nameof(IsOpen));

        /// <summary>
        /// Gets or sets whether the dropdown is open.
        /// </summary>
        public bool IsOpen
        {
            get => GetValue(IsOpenProperty);
            set => SetValue(IsOpenProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="PlacementMode"/> property.
        /// </summary>
        public static readonly StyledProperty<PlacementMode> PlacementModeProperty =
            AvaloniaProperty.Register<DaisyDropdown, PlacementMode>(nameof(PlacementMode), PlacementMode.Bottom);

        /// <summary>
        /// Gets or sets the placement mode used by the popup.
        /// </summary>
        public PlacementMode PlacementMode
        {
            get => GetValue(PlacementModeProperty);
            set => SetValue(PlacementModeProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="CloseOnSelection"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> CloseOnSelectionProperty =
            AvaloniaProperty.Register<DaisyDropdown, bool>(nameof(CloseOnSelection), true);

        /// <summary>
        /// Gets or sets whether selecting an item closes the dropdown.
        /// </summary>
        public bool CloseOnSelection
        {
            get => GetValue(CloseOnSelectionProperty);
            set => SetValue(CloseOnSelectionProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Size"/> property.
        /// </summary>
        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyDropdown, DaisySize>(nameof(Size), DaisySize.Medium);

        /// <summary>
        /// Gets or sets the size of this dropdown.
        /// </summary>
        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// Raised when the selected item changes.
        /// </summary>
        public event EventHandler<DaisyDropdownSelectionChangedEventArgs>? SelectedItemChanged;

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            if (_trigger != null)
            {
                _trigger.PointerPressed -= OnTriggerPointerPressed;
            }

            if (_menu != null)
            {
                _menu.SelectionChanged -= OnMenuSelectionChanged;
            }

            _trigger = e.NameScope.Find<Control>("PART_Trigger");
            _menu = e.NameScope.Find<ListBox>("PART_Menu");

            if (_trigger != null)
            {
                _trigger.PointerPressed += OnTriggerPointerPressed;
            }

            if (_menu != null)
            {
                _menu.SelectionChanged += OnMenuSelectionChanged;
            }
        }

        private void OnTriggerPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            var point = e.GetCurrentPoint(this);
            if (point.Properties.IsLeftButtonPressed)
            {
                IsOpen = !IsOpen;
            }
        }

        private void OnMenuSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (_menu == null) return;

            var item = _menu.SelectedItem;
            if (!Equals(SelectedItem, item))
            {
                SetCurrentValue(SelectedItemProperty, item);
            }

            if (e.AddedItems.Count > 0)
            {
                SelectedItemChanged?.Invoke(this, new DaisyDropdownSelectionChangedEventArgs(e.AddedItems[0]));
            }

            if (CloseOnSelection)
            {
                IsOpen = false;
            }
        }
    }
}
