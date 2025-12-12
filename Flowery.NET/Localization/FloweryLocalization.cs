using System;
using System.Globalization;
using System.Resources;
using System.Threading;

namespace Flowery.Localization
{
    /// <summary>
    /// Provides localization services for Flowery.NET controls.
    /// Use this class to switch languages at runtime and retrieve localized strings.
    /// </summary>
    public static class FloweryLocalization
    {
        private static CultureInfo _currentCulture = CultureInfo.CurrentUICulture;
        private static readonly ResourceManager _resourceManager;

        /// <summary>
        /// Event fired when the culture is changed. Subscribe to this to refresh UI bindings.
        /// </summary>
        public static event EventHandler<CultureInfo>? CultureChanged;

        static FloweryLocalization()
        {
            // Resource name format: {AssemblyName}.{FolderPath}.{FileName}
            // For Flowery.NET project with Localization/FloweryStrings.resx
            _resourceManager = new ResourceManager(
                "Flowery.NET.Localization.FloweryStrings",
                typeof(FloweryLocalization).Assembly);
        }

        /// <summary>
        /// Gets the current UI culture used for localization.
        /// </summary>
        public static CultureInfo CurrentCulture => _currentCulture;

        /// <summary>
        /// Gets the resource manager for FloweryStrings.
        /// </summary>
        public static ResourceManager ResourceManager => _resourceManager;

        /// <summary>
        /// Sets the current UI culture and notifies subscribers.
        /// </summary>
        /// <param name="culture">The culture to switch to.</param>
        public static void SetCulture(CultureInfo culture)
        {
            if (culture == null)
                throw new ArgumentNullException(nameof(culture));

            if (_currentCulture.Name == culture.Name)
                return;

            _currentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            _resourceManager.ReleaseAllResources();
            CultureChanged?.Invoke(null, culture);
        }

        /// <summary>
        /// Sets the current UI culture by name and notifies subscribers.
        /// </summary>
        /// <param name="cultureName">The culture name (e.g., "en-US", "de-DE").</param>
        public static void SetCulture(string cultureName)
        {
            SetCulture(new CultureInfo(cultureName));
        }

        /// <summary>
        /// Gets a localized string by key.
        /// </summary>
        /// <param name="key">The resource key.</param>
        /// <returns>The localized string, or the key if not found.</returns>
        public static string GetString(string key)
        {
            try
            {
                return _resourceManager.GetString(key, _currentCulture) ?? key;
            }
            catch
            {
                return key;
            }
        }

        /// <summary>
        /// Gets the localized display name for a theme.
        /// </summary>
        /// <param name="themeName">The internal theme name (e.g., "Synthwave").</param>
        /// <returns>The localized display name.</returns>
        public static string GetThemeDisplayName(string themeName)
        {
            var key = $"Theme_{themeName}";
            var result = _resourceManager.GetString(key, _currentCulture);

            // Fallback to default resources (English/invariant) if missing in the current culture.
            if (string.IsNullOrEmpty(result))
                result = _resourceManager.GetString(key, CultureInfo.InvariantCulture);

            // Final fallback: use the internal theme name.
            return string.IsNullOrEmpty(result) ? themeName : result;
        }
    }
}
