using System;
using Avalonia;
using Avalonia.Controls;

namespace DaisyUI.Avalonia.Controls
{
    public class DaisyCountdown : ContentControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyCountdown);
        
        // DaisyUI countdown uses CSS counter and variable --value. 
        // We just display the number in a monospace font.
    }
}
