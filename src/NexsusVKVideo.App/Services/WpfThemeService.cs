using System.Windows;

namespace NexsusVKVideo.App.Services;

public sealed class WpfThemeService(Application application) : IThemeService
{
    private const string DarkTheme = "Dark";
    private const string LightTheme = "Light";
    private readonly Application _application = application ?? throw new ArgumentNullException(nameof(application));

    public void Apply(string theme)
    {
        var source = theme switch
        {
            DarkTheme => "Resources/Tokens/Colors.xaml",
            LightTheme => "Resources/Tokens/Colors.Light.xaml",
            _ => throw new ArgumentOutOfRangeException(nameof(theme), "Unsupported theme.")
        };

        var dictionaries = _application.Resources.MergedDictionaries;
        var colorsIndex = FindColorsDictionaryIndex(dictionaries);
        dictionaries[colorsIndex] = new ResourceDictionary { Source = new Uri(source, UriKind.Relative) };
    }

    private static int FindColorsDictionaryIndex(IList<ResourceDictionary> dictionaries)
    {
        for (var index = 0; index < dictionaries.Count; index++)
        {
            var source = dictionaries[index].Source?.OriginalString;
            if (source is not null && source.Contains("Resources/Tokens/Colors", StringComparison.Ordinal))
            {
                return index;
            }
        }

        throw new InvalidOperationException("The color resource dictionary is not configured.");
    }
}
