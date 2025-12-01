using Avalonia.Styling;
using Avalonia.Markup.Xaml;

namespace DaisyUI.Avalonia
{
    public class DaisyUITheme : Styles
    {
        public DaisyUITheme()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
