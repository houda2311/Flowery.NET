using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using Flowery.Controls;
using Flowery.NET.Gallery.Examples;

namespace Flowery.NET.Gallery;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, Func<Control>> _categoryControls;
    private ActionsExamples? _actionsExamples;

    /// <summary>
    /// Controls/sections to skip during batch screenshot capture.
    /// These either have animated GIFs in documentation or are non-visual sections.
    /// </summary>
    private static readonly HashSet<string> ScreenshotSkipList = new(StringComparer.OrdinalIgnoreCase)
    {
        "DaisyLoading",         // Has animated GIF (loading_animations.gif)
        "DaisyStatusIndicator", // Has animated GIF (status_animations.gif)
        "css-theme-converter",  // CSS import section, not a control
        "themecontroller",      // Section ID for theme controller (if needed)
        "service-integration",  // Weather service integration docs, not a control
    };

    public MainWindow()
    {
        InitializeComponent();

        _categoryControls = new Dictionary<string, Func<Control>>(StringComparer.OrdinalIgnoreCase)
        {
            ["Home"] = () => CreateHomePage(),
            ["Actions"] = () => GetOrCreateActionsExamples(),
            ["Data Input"] = () => new DataInputExamples(),
            ["Navigation"] = () => new NavigationExamples(),
            ["Data Display"] = () => new DataDisplayExamples(),
            ["Date Display"] = () => new DateDisplayExamples(),
            ["Feedback"] = () => new FeedbackExamples(),
            ["Cards"] = () => new CardsExamples(),
            ["Divider"] = () => new DividerExamples(),
            ["Layout"] = () => new LayoutExamples(),
            ["Theming"] = () => new ThemingExamples(),
            ["Custom Controls"] = () => new CustomControls(),
            ["Color Picker"] = () => new ColorPickerExamples()
        };

        // Restore last viewed page or show home
        var sidebar = this.FindControl<DaisyComponentSidebar>("ComponentSidebar");
        var (lastItemId, category) = sidebar?.GetLastViewedItem() ?? (null, null);
        if (lastItemId != null && category != null)
        {
            var item = category.Items.FirstOrDefault(i => i.Id == lastItemId);
            if (item != null)
                NavigateToCategory(item.TabHeader, item.Id);
        }
        else
        {
            var mainContent = this.FindControl<ContentControl>("MainContent");
            if (mainContent != null)
                mainContent.Content = CreateHomePage();
        }
    }

    private Control CreateHomePage()
    {
        var homePage = new HomePage();
        homePage.BrowseComponentsRequested += OnBrowseComponentsRequested;
        return homePage;
    }

    private void OnBrowseComponentsRequested(object? sender, EventArgs e)
    {
        NavigateToCategory("Actions", "button");
    }

    private void NavigateToCategory(string tabHeader, string? sectionId = null)
    {
        var content = this.FindControl<ContentControl>("MainContent");
        var title = this.FindControl<TextBlock>("CategoryTitle");
        var titleBar = this.FindControl<Border>("CategoryTitleBar");
        if (content == null) return;

        if (title != null)
            title.Text = tabHeader;
        if (titleBar != null)
            titleBar.IsVisible = tabHeader != "Home";

        if (_categoryControls.TryGetValue(tabHeader, out var factory))
        {
            var newContent = factory();
            content.Content = newContent;

            // Auto-scroll to the specific section after content is loaded
            if (sectionId != null && newContent is IScrollableExample scrollable)
            {
                global::Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    scrollable.ScrollToSection(sectionId);
                }, global::Avalonia.Threading.DispatcherPriority.Loaded);
            }
        }
    }

    private Control GetOrCreateActionsExamples()
    {
        if (_actionsExamples == null)
        {
            _actionsExamples = new ActionsExamples();
            _actionsExamples.OpenModalRequested += OnOpenModalRequested;
            _actionsExamples.OpenModalWithRadiiRequested += OnOpenModalWithRadiiRequested;
        }
        return _actionsExamples;
    }

    public void ComponentSidebar_ItemSelected(object? sender, SidebarItemSelectedEventArgs e)
    {
        NavigateToCategory(e.Item.TabHeader, e.Item.Id);
    }

    public void OnOpenModalRequested(object? sender, EventArgs e)
    {
        if (DemoModal == null) return;

        // Reset to default corner radii
        DemoModal.TopLeftRadius = 16;
        DemoModal.TopRightRadius = 16;
        DemoModal.BottomLeftRadius = 16;
        DemoModal.BottomRightRadius = 16;
        SetModalTitle("Hello!");
        DemoModal.IsOpen = true;
    }

    public void OnOpenModalWithRadiiRequested(object? sender, ModalRadiiEventArgs e)
    {
        if (DemoModal == null) return;

        DemoModal.TopLeftRadius = e.TopLeft;
        DemoModal.TopRightRadius = e.TopRight;
        DemoModal.BottomLeftRadius = e.BottomLeft;
        DemoModal.BottomRightRadius = e.BottomRight;
        SetModalTitle(e.Title);
        DemoModal.IsOpen = true;
    }

    private void SetModalTitle(string title)
    {
        var modalTitle = this.FindControl<TextBlock>("ModalTitle");
        if (modalTitle != null)
            modalTitle.Text = title;
    }

    public void CloseModalBtn_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DemoModal != null) DemoModal.IsOpen = false;
    }

    public void OpenGitHub_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        var url = "https://www.github.com/tobitege";
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            Process.Start("xdg-open", url);
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            Process.Start("open", url);
    }

    private bool _isCapturing = false;
    private bool _stopCapture = false;

    public async void CaptureAllScreenshots_Click(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        var button = sender as DaisyButton;
        if (button == null) return;

        // Stop functionality
        if (_isCapturing)
        {
            _stopCapture = true;
            return;
        }

        // Prompt user for output folder
        var storageProvider = StorageProvider;
        var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder for Screenshots",
            AllowMultiple = false
        });

        if (folders == null || folders.Count == 0) return;
        var baseDir = folders[0].Path.LocalPath;

        // Start functionality
        _isCapturing = true;
        _stopCapture = false;
        button.Variant = DaisyButtonVariant.Error; // Red color
        var originalContent = button.Content;
        // button.Content = "⏹"; // Optional: Change icon to stop

        try
        {
            // Resize window for consistent screenshots
            if (Screens.Primary != null)
            {
                WindowState = WindowState.Normal;
                Width = 1200;
                // Cap height to 1200px to ensure it fits comfortably on standard screens and avoids weird off-screen clipping
                Height = Math.Min(1200, Screens.Primary.WorkingArea.Height - 60);

                // Center horizontally, align to top (with small offset)
                var screenWidth = Screens.Primary.WorkingArea.Width;
                var x = Screens.Primary.WorkingArea.X + (screenWidth - Width) / 2;
                var y = Screens.Primary.WorkingArea.Y + 10;
                Position = new Avalonia.PixelPoint((int)x, (int)y);

                // Allow layout to update
                await Task.Delay(500);
            }

            // Ensure output folder exists
            if (!Directory.Exists(baseDir))
                Directory.CreateDirectory(baseDir);

            // Track which control names we've already captured (to handle duplicates)
            var capturedControls = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Iterate through all categories
            foreach (var categoryName in _categoryControls.Keys)
            {
                if (_stopCapture) break;
                if (categoryName == "Home") continue;

                // Navigate to the category
                NavigateToCategory(categoryName);

                // Allow generous time for the view to be created, attached to visual tree, and laid out
                await Task.Delay(1000);

                var mainContent = this.FindControl<ContentControl>("MainContent");
                if (mainContent?.Content is Control view)
                {
                    // Find all section headers in the view
                    var headers = view.GetVisualDescendants().OfType<SectionHeader>().ToList();

                    foreach (var header in headers)
                    {
                        if (_stopCapture) break;

                        // Use ControlName for filename (e.g., "DaisyButton")
                        var controlName = header.ControlName;
                        if (string.IsNullOrEmpty(controlName))
                            controlName = header.SectionId;
                        if (string.IsNullOrEmpty(controlName))
                            controlName = header.Title.Replace(" ", "");

                        // Skip controls in the skip list (have animated GIFs or are non-visual)
                        if (ScreenshotSkipList.Contains(controlName) ||
                            ScreenshotSkipList.Contains(header.SectionId ?? ""))
                            continue;

                        // Skip if we've already captured this control (handles multiple sections mapping to same control)
                        if (capturedControls.Contains(controlName))
                            continue;

                        capturedControls.Add(controlName);

                        var fileName = $"{controlName}.png";
                        var filePath = Path.Combine(baseDir, fileName);

                        await header.SaveScreenshotToFileAsync(filePath, hideScreenshotButton: true);

                        // Pause to ensure UI stays responsive/updated between shots
                        await Task.Delay(1000);
                    }
                }
            }

            // Open the folder if we captured any images
            if (Directory.Exists(baseDir) && Directory.GetFiles(baseDir, "*.png").Length > 0)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    Process.Start(new ProcessStartInfo(baseDir) { UseShellExecute = true });
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    Process.Start("xdg-open", baseDir);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    Process.Start("open", baseDir);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Batch capture failed: {ex.Message}");
        }
        finally
        {
            button.Variant = DaisyButtonVariant.Ghost;
            button.Content = originalContent;
            _isCapturing = false;
            _stopCapture = false;
        }
    }
}
