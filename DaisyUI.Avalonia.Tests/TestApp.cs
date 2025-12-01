using Avalonia;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;

[assembly: AvaloniaTestApplication(typeof(DaisyUI.Avalonia.Tests.TestAppBuilder))]

namespace DaisyUI.Avalonia.Tests
{
    public class TestAppBuilder
    {
        public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions());
    }

    public class App : Application
    {
        public override void Initialize()
        {
            Styles.Add(new DaisyUITheme());
        }
    }
}
