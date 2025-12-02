using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.Threading;

namespace Flowery.Controls
{
    /// <summary>
    /// A text rotation control that cycles through items with an animation.
    /// Similar to daisyUI's text-rotate component.
    /// </summary>
    public class DaisyTextRotate : ItemsControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyTextRotate);

        private CancellationTokenSource? _animationCts;
        private int _currentIndex;
        private bool _isPausedByHover;

        /// <summary>
        /// Gets or sets the total duration of the animation loop in milliseconds.
        /// </summary>
        public static readonly StyledProperty<double> DurationProperty =
            AvaloniaProperty.Register<DaisyTextRotate, double>(nameof(Duration), 10000.0);

        public double Duration
        {
            get => GetValue(DurationProperty);
            set => SetValue(DurationProperty, value);
        }

        /// <summary>
        /// Gets or sets the index of the currently visible item.
        /// </summary>
        public static readonly StyledProperty<int> CurrentIndexProperty =
            AvaloniaProperty.Register<DaisyTextRotate, int>(nameof(CurrentIndex), 0);

        public int CurrentIndex
        {
            get => GetValue(CurrentIndexProperty);
            set => SetValue(CurrentIndexProperty, value);
        }

        /// <summary>
        /// Gets or sets whether the animation is paused.
        /// </summary>
        public static readonly StyledProperty<bool> IsPausedProperty =
            AvaloniaProperty.Register<DaisyTextRotate, bool>(nameof(IsPaused), false);

        public bool IsPaused
        {
            get => GetValue(IsPausedProperty);
            set => SetValue(IsPausedProperty, value);
        }

        /// <summary>
        /// Gets or sets whether to pause the animation on hover.
        /// </summary>
        public static readonly StyledProperty<bool> PauseOnHoverProperty =
            AvaloniaProperty.Register<DaisyTextRotate, bool>(nameof(PauseOnHover), true);

        public bool PauseOnHover
        {
            get => GetValue(PauseOnHoverProperty);
            set => SetValue(PauseOnHoverProperty, value);
        }

        /// <summary>
        /// Gets or sets the animation easing function.
        /// </summary>
        public static readonly StyledProperty<Easing> EasingProperty =
            AvaloniaProperty.Register<DaisyTextRotate, Easing>(nameof(Easing), new CubicEaseInOut());

        public Easing Easing
        {
            get => GetValue(EasingProperty);
            set => SetValue(EasingProperty, value);
        }

        /// <summary>
        /// Gets or sets the transition duration for each item change in milliseconds.
        /// </summary>
        public static readonly StyledProperty<double> TransitionDurationProperty =
            AvaloniaProperty.Register<DaisyTextRotate, double>(nameof(TransitionDuration), 500.0);

        public double TransitionDuration
        {
            get => GetValue(TransitionDurationProperty);
            set => SetValue(TransitionDurationProperty, value);
        }

        public DaisyTextRotate()
        {
            PointerEntered += OnPointerEntered;
            PointerExited += OnPointerExited;
            ContainerPrepared += OnContainerPrepared;
        }

        private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
        {
            // Set initial opacity when each container is created
            // Only the current item should be visible
            e.Container.Opacity = (e.Index == _currentIndex) ? 1.0 : 0.0;
            e.Container.IsVisible = true;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            // Delay starting the animation loop to ensure containers are ready
            Dispatcher.UIThread.Post(StartAnimationLoop, DispatcherPriority.Loaded);
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ItemCountProperty)
            {
                _currentIndex = 0;
                CurrentIndex = 0;
                ResetContainerOpacities();
                RestartAnimationLoop();
            }
            else if (change.Property == DurationProperty)
            {
                RestartAnimationLoop();
            }
            else if (change.Property == IsPausedProperty)
            {
                if (IsPaused)
                    StopAnimationLoop();
                else
                    StartAnimationLoop();
            }
        }

        private void OnPointerEntered(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (PauseOnHover)
            {
                _isPausedByHover = true;
                StopAnimationLoop();
            }
        }

        private void OnPointerExited(object? sender, Avalonia.Input.PointerEventArgs e)
        {
            if (_isPausedByHover && !IsPaused)
            {
                _isPausedByHover = false;
                StartAnimationLoop();
            }
        }

        private void ResetContainerOpacities()
        {
            // Reset opacity for all existing containers when current index changes
            for (int i = 0; i < ItemCount; i++)
            {
                var container = ContainerFromIndex(i);
                if (container != null)
                {
                    container.Opacity = (i == _currentIndex) ? 1.0 : 0.0;
                }
            }
        }

        private void StartAnimationLoop()
        {
            StopAnimationLoop();

            if (ItemCount <= 1 || IsPaused) return;

            _animationCts = new CancellationTokenSource();
            _ = RunAnimationLoopAsync(_animationCts.Token);
        }

        private void StopAnimationLoop()
        {
            _animationCts?.Cancel();
            _animationCts?.Dispose();
            _animationCts = null;
        }

        private void RestartAnimationLoop()
        {
            if (!IsPaused && !_isPausedByHover)
                StartAnimationLoop();
        }

        private async Task RunAnimationLoopAsync(CancellationToken ct)
        {
            var intervalPerItem = Duration / ItemCount;
            var transitionMs = TransitionDuration;

            // Small initial delay to let containers fully initialize
            try
            {
                await Task.Delay(100, ct);
            }
            catch (TaskCanceledException)
            {
                return;
            }

            while (!ct.IsCancellationRequested)
            {
                // Wait for the display interval (minus transition time since transition is part of the interval)
                var waitTime = Math.Max(100, intervalPerItem - transitionMs);
                try
                {
                    await Task.Delay((int)waitTime, ct);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (ct.IsCancellationRequested) break;

                // Calculate next index
                var previousIndex = _currentIndex;
                _currentIndex = (_currentIndex + 1) % ItemCount;
                CurrentIndex = _currentIndex;

                // Animate the transition on UI thread
                if (ct.IsCancellationRequested) break;

                await Dispatcher.UIThread.InvokeAsync(async () =>
                {
                    if (!ct.IsCancellationRequested)
                    {
                        await AnimateTransitionAsync(previousIndex, _currentIndex, transitionMs, ct);
                    }
                });
            }
        }

        private async Task AnimateTransitionAsync(int fromIndex, int toIndex, double durationMs, CancellationToken ct)
        {
            var fromContainer = ContainerFromIndex(fromIndex);
            var toContainer = ContainerFromIndex(toIndex);

            if (fromContainer == null || toContainer == null) return;

            var duration = TimeSpan.FromMilliseconds(durationMs);
            var easing = Easing;

            // Create fade-out animation for the old item
            var fadeOut = new Animation
            {
                Duration = duration,
                Easing = easing,
                FillMode = FillMode.Both,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, 1.0) } },
                    new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 0.0) } }
                }
            };

            // Create fade-in animation for the new item
            var fadeIn = new Animation
            {
                Duration = duration,
                Easing = easing,
                FillMode = FillMode.Both,
                Children =
                {
                    new KeyFrame { Cue = new Cue(0), Setters = { new Setter(OpacityProperty, 0.0) } },
                    new KeyFrame { Cue = new Cue(1), Setters = { new Setter(OpacityProperty, 1.0) } }
                }
            };

            // Run both animations concurrently
            // FillMode.Both ensures values persist before and after animation
            var fadeOutTask = fadeOut.RunAsync(fromContainer, ct);
            var fadeInTask = fadeIn.RunAsync(toContainer, ct);

            try
            {
                await Task.WhenAll(fadeOutTask, fadeInTask);
            }
            catch (TaskCanceledException)
            {
                // Animation was cancelled - set final state immediately
                fromContainer.Opacity = 0.0;
                toContainer.Opacity = 1.0;
            }
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            StopAnimationLoop();
            PointerEntered -= OnPointerEntered;
            PointerExited -= OnPointerExited;
            ContainerPrepared -= OnContainerPrepared;
        }
    }
}
