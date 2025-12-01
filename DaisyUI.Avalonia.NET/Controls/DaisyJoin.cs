using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace DaisyUI.Avalonia.Controls
{
    public class DaisyJoin : StackPanel
    {
        protected override Type StyleKeyOverride => typeof(DaisyJoin);
        
        public DaisyJoin()
        {
            Orientation = Orientation.Horizontal;
        }
    }
}
