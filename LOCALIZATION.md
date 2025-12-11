# Flowery.NET Localization Guide

Flowery.NET provides built-in localization support for theme names and control text, with easy extensibility for additional languages.

## Supported Languages

The library comes with built-in translations for the following languages:

- 🇺🇸 **English** (Default)
- 🇩🇪 **German** (`de`)
- 🇫🇷 **French** (`fr`)
- 🇪🇸 **Spanish** (`es`)
- 🇨🇳 **Mandarin Simplified** (`zh-CN`)
- 🇰🇷 **Korean** (`ko`)
- 🇯🇵 **Japanese** (`ja`)
- 🇸🇦 **Arabic** (`ar`)
- 🇹🇷 **Turkish** (`tr`)
- 🇺🇦 **Ukrainian** (`uk`)

## Quick Start

### Switching Languages at Runtime

```csharp
using Flowery.Localization;

// Switch to German
FloweryLocalization.SetCulture("de");

// Switch to Mandarin
FloweryLocalization.SetCulture("zh-CN");
```

### Date Formatting

The `DaisyDateTimeline` control uses the `Locale` property for date formatting:

```xml
<controls:DaisyDateTimeline Locale="de-DE" />
```

Or bind to the current UI culture:

```csharp
timeline.Locale = FloweryLocalization.CurrentCulture;
```

## Adding Translations

### 1. Create a Resource File

Copy `Localization/FloweryStrings.resx` to `FloweryStrings.{culture}.resx`:

- `FloweryStrings.de.resx` for German
- `FloweryStrings.it.resx` for Italian
- same for other languages

### 2. Translate the Strings

Open the new file and translate each value. Example for German:

```xml
<data name="Select_Placeholder" xml:space="preserve">
  <value>Auswählen</value>
</data>
<data name="Accessibility_Loading" xml:space="preserve">
  <value>Wird geladen</value>
</data>
```

### 3. Build and Test

After adding the resource file, rebuild the project. The new translations will be automatically available.

## Available Resource Keys

| Key | English (Default) | Description |
|-----|-------------------|-------------|
| `Select_Placeholder` | Pick one | DaisySelect default placeholder |
| `Theme_*` | Theme names | Localized display names for all 35 themes |
| `Accessibility_Loading` | Loading | DaisyLoading accessible text |
| `Accessibility_Progress` | Progress | DaisyProgress/DaisyRadialProgress accessible text |
| `Accessibility_Rating` | Rating | DaisyRating accessible text |
| `Accessibility_LoadingPlaceholder` | Loading placeholder | DaisySkeleton accessible text |
| `Accessibility_Status` | Status | DaisyStatusIndicator default text |
| `Accessibility_Status*` | Online, Error, etc. | DaisyStatusIndicator color variants |

## Handling Culture Changes

When the culture changes, static strings in your UI (like properties bound to `GetString`) won't update automatically. You must handle the `CultureChanged` event to refresh them.

### Strategy 1: ViewModel (Recommended)

In MVVM, trigger `PropertyChanged` for localized properties:

```csharp
public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        FloweryLocalization.CultureChanged += (s, culture) =>
        {
            // Notify the UI that these properties have changed
            OnPropertyChanged(nameof(Greeting));
            OnPropertyChanged(nameof(ButtonLabel));
        };
    }

    // This property will be re-read when PropertyChanged is raised
    public string Greeting => FloweryLocalization.GetString("Greeting_Text");
    public string ButtonLabel => FloweryLocalization.GetString("Button_Submit");
}
```

### Strategy 2: Code-Behind

Manually re-assign properties in your View:

```csharp
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UpdateText();

        FloweryLocalization.CultureChanged += (s, c) => UpdateText();
    }

    private void UpdateText()
    {
        MyTitle.Text = FloweryLocalization.GetString("App_Title");
        MyButton.Content = FloweryLocalization.GetString("App_Button");
    }
}
```

## Contributing Translations

1. Fork the repository
2. Add your `FloweryStrings.{culture}.resx` file
3. Submit a pull request

Contributions for any language are welcome!
