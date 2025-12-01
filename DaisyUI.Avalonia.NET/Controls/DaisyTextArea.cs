using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;

namespace DaisyUI.Avalonia.Controls
{
    /// <summary>
    /// A TextBox control styled after DaisyUI's Textarea component (multiline).
    /// </summary>
    public class DaisyTextArea : DaisyInput
    {
        protected override Type StyleKeyOverride => typeof(DaisyTextArea);

        public DaisyTextArea()
        {
            AcceptsReturn = true;
            AcceptsTab = true;
            TextWrapping = TextWrapping.Wrap;
        }
    }
}
