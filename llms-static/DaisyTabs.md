<!-- Supplementary documentation for DaisyTabs -->
<!-- This content is merged into auto-generated docs by generate_docs.py -->

# Overview

DaisyTabs is a styled `TabControl` with four header variants (None, Bordered, Lifted, Boxed) and size presets. It uses a WrapPanel for tab headers and supports standard tab behaviors (`SelectedIndex`, `Items`, `TabItem` content).

## Variant Options

| Variant | Description |
|---------|-------------|
| None | Text-only tabs; selected text is bold; no underline/box. |
| Bordered (default) | Underline on hover/selected. |
| Lifted | Folder-tab style with top/sides border; selected tab has background. |
| Boxed | Rounded pills with visible borders; selected uses primary border. |

## Size Options

| Size | Padding | Font Size |
|------|---------|-----------| 
| ExtraSmall | 8,4 | 10 |
| Small | 12,6 | 12 |
| Medium (default) | 16,8 | 14 |
| Large | 20,12 | 18 |

## Tab Width Options

Control tab sizing behavior to prevent layout shifts or create uniform navigation.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| TabWidthMode | Enum | Auto | `Auto`, `Equal`, or `Fixed` |
| TabWidth | double | NaN | Fixed width when mode is `Fixed` |
| TabMinWidth | double | 0 | Minimum width for each tab |
| TabMaxWidth | double | ∞ | Maximum width for each tab |

| Mode | Behavior |
|------|----------|
| Auto | Each tab sizes to fit its content (default) |
| Equal | All tabs use the width of the widest tab |
| Fixed | All tabs use the specified `TabWidth` value |

## Quick Examples

```xml
<!-- Bordered (default) -->
<controls:DaisyTabs>
    <TabItem Header="Tab 1"><TextBlock Text="Content 1" Margin="8" /></TabItem>
    <TabItem Header="Tab 2"><TextBlock Text="Content 2" Margin="8" /></TabItem>
</controls:DaisyTabs>

<!-- Boxed -->
<controls:DaisyTabs Variant="Boxed">
    <TabItem Header="Tab 1"><TextBlock Text="Content 1" Margin="8" /></TabItem>
    <TabItem Header="Tab 2"><TextBlock Text="Content 2" Margin="8" /></TabItem>
</controls:DaisyTabs>

<!-- Lifted, small -->
<controls:DaisyTabs Variant="Lifted" Size="Small">
    <TabItem Header="Tab 1"><TextBlock Text="Content 1" Margin="8" /></TabItem>
    <TabItem Header="Tab 2"><TextBlock Text="Content 2" Margin="8" /></TabItem>
</controls:DaisyTabs>

<!-- Equal width tabs (prevents layout shift) -->
<controls:DaisyTabs Variant="Boxed" TabWidthMode="Equal">
    <TabItem Header="Short"><TextBlock Text="Content 1" Margin="8" /></TabItem>
    <TabItem Header="Much Longer Tab"><TextBlock Text="Content 2" Margin="8" /></TabItem>
    <TabItem Header="Med"><TextBlock Text="Content 3" Margin="8" /></TabItem>
</controls:DaisyTabs>

<!-- Fixed width tabs -->
<controls:DaisyTabs Variant="Boxed" TabWidthMode="Fixed" TabWidth="120">
    <TabItem Header="Tab 1"><TextBlock Text="Content 1" Margin="8" /></TabItem>
    <TabItem Header="Tab 2"><TextBlock Text="Content 2" Margin="8" /></TabItem>
</controls:DaisyTabs>

<!-- Auto with max width constraint -->
<controls:DaisyTabs TabMaxWidth="150">
    <TabItem Header="This is a very long tab header"><TextBlock Text="Content" Margin="8" /></TabItem>
    <TabItem Header="Short"><TextBlock Text="Content" Margin="8" /></TabItem>
</controls:DaisyTabs>
```

## Tips & Best Practices

- Use **Lifted** for app-like tabs over panels; **Boxed** for pill-style segmented navigation.
- Use **TabWidthMode="Equal"** to prevent layout shifts when tab selection causes font weight changes.
- Keep headers short; WrapPanel allows wrapping but concise labels prevent clutter.
- Adjust `Size` to match surrounding controls; Small/XS for toolbars, Large for hero sections.
- For tab content padding, set `Padding` on DaisyTabs or inside each TabItem as needed.
