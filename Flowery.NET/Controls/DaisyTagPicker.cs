using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Flowery.Controls
{
    /// <summary>
    /// A selectable chip list for choosing multiple tags.
    /// </summary>
    public class DaisyTagPicker : WrapPanel
    {
        private readonly List<string> _internalSelected = new();

        /// <summary>
        /// Defines the <see cref="Tags"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<string>?> TagsProperty =
            AvaloniaProperty.Register<DaisyTagPicker, IList<string>?>(nameof(Tags));

        /// <summary>
        /// Gets or sets the list of available tags.
        /// </summary>
        public IList<string>? Tags
        {
            get => GetValue(TagsProperty);
            set => SetValue(TagsProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="SelectedTags"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<string>?> SelectedTagsProperty =
            AvaloniaProperty.Register<DaisyTagPicker, IList<string>?>(nameof(SelectedTags));

        /// <summary>
        /// Gets or sets the selected tags. When null, selection is managed internally.
        /// </summary>
        public IList<string>? SelectedTags
        {
            get => GetValue(SelectedTagsProperty);
            set => SetValue(SelectedTagsProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Size"/> property.
        /// </summary>
        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyTagPicker, DaisySize>(nameof(Size), DaisySize.Small);

        /// <summary>
        /// Gets or sets the size of the tag chips.
        /// </summary>
        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// Raised when the selection changes.
        /// </summary>
        public event EventHandler<IReadOnlyList<string>>? SelectionChanged;

        static DaisyTagPicker()
        {
            TagsProperty.Changed.AddClassHandler<DaisyTagPicker>((s, _) => s.Rebuild());
            SelectedTagsProperty.Changed.AddClassHandler<DaisyTagPicker>((s, _) => s.Rebuild());
            SizeProperty.Changed.AddClassHandler<DaisyTagPicker>((s, _) => s.Rebuild());
        }

        public DaisyTagPicker()
        {
            Orientation = Orientation.Horizontal;
            Rebuild();
        }

        private void Rebuild()
        {
            Children.Clear();

            var tags = Tags ?? Array.Empty<string>();
            var selected = SelectedTags ?? _internalSelected;

            foreach (var tag in tags)
            {
                var isSelected = selected.Contains(tag);

                var button = new DaisyButton
                {
                    Content = tag,
                    Size = Size,
                    Variant = isSelected ? DaisyButtonVariant.Primary : DaisyButtonVariant.Neutral,
                    ButtonStyle = isSelected ? DaisyButtonStyle.Soft : DaisyButtonStyle.Outline,
                    Margin = new Thickness(4)
                };

                button.Click += (_, __) => ToggleTag(tag);
                Children.Add(button);
            }
        }

        private void ToggleTag(string tag)
        {
            var selected = SelectedTags ?? _internalSelected;

            var newSelected = selected.Contains(tag)
                ? selected.Where(t => t != tag).ToList()
                : selected.Concat(new[] { tag }).ToList();

            if (SelectedTags != null)
            {
                SetCurrentValue(SelectedTagsProperty, newSelected);
            }
            else
            {
                _internalSelected.Clear();
                _internalSelected.AddRange(newSelected);
                Rebuild();
            }

            SelectionChanged?.Invoke(this, newSelected);
        }
    }
}
