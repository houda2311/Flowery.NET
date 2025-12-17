<!-- Supplementary documentation for DaisyTagPicker -->
<!-- This content is merged into auto-generated docs by generate_docs.py -->

# Overview

DaisyTagPicker is a selectable chip list for choosing multiple tags. It renders tags as `DaisyButton` chips (outlined when unselected, soft primary when selected) and supports either internal selection or an externally bound `SelectedTags` list.

## Properties

| Property | Description |
|----------|-------------|
| `Tags` (`IList<string>?`) | Available tags to display. |
| `SelectedTags` (`IList<string>?`) | Selected tags. When null, selection is managed internally. |
| `Size` (`DaisySize`) | Size preset for the tag chips (default `Small`). |

## Events

| Event | Description |
|-------|-------------|
| `SelectionChanged` | Raised whenever the selection changes. |

## Quick Examples

```xml
<!-- Internal selection (no binding required) -->
<controls:DaisyTagPicker>
    <controls:DaisyTagPicker.Tags>
        <x:Array Type="{x:Type sys:String}">
            <sys:String>Avalonia</sys:String>
            <sys:String>DaisyUI</sys:String>
            <sys:String>Desktop</sys:String>
        </x:Array>
    </controls:DaisyTagPicker.Tags>
</controls:DaisyTagPicker>

<!-- Two-way binding (SelectedTags updates with a new list instance on changes) -->
<controls:DaisyTagPicker Tags="{Binding AvailableTags}"
                       SelectedTags="{Binding SelectedTags, Mode=TwoWay}"
                       Size="Medium" />
```

## Tips & Best Practices

- Use unique tag strings; selection is based on string equality.
- If you bind `SelectedTags`, expect it to be replaced with a new list when the user toggles a tag.
- For single-select “pick one” UX, use `DaisyDropdown` or `DaisySelect` instead.
