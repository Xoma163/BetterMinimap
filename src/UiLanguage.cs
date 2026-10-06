namespace Shtab.RotatingMinimap;

internal static class UiLanguage
{
    public static bool Russian => Localization.instance != null &&
        Localization.instance.GetSelectedLanguage() == "Russian";
    public static string Pick(string ru, string en) => SettingsText.Pick(Russian, ru, en);
}
