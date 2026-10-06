using System;

namespace Shtab.RotatingMinimap;

internal static class ResetPreset
{
    public static bool Includes(string key) => key != "Мод включён";
    public static object Value(string key, object modDefault, bool vanilla)
    {
        if (!vanilla) return modDefault;
        switch (key)
        {
            case "Вращение":
            case "Показывать линии компаса":
            case "Показывать направления":
            case "Зум через + и -":
            case "Плавный зум":
            case "Сохранять отдельный зум для кораблей": return false;
            case "Режим": return Enum.Parse(modDefault.GetType(), "NorthUp");
            case "Освещение карты": return Enum.Parse(modDefault.GetType(), "Day");
            case "Размер мини-карты": return 100;
            case "Min zoom": return 0f;
            case "Max zoom": return 1f;
            case "Форма": return Enum.Parse(modDefault.GetType(), "Square");
            case "Положение биома":
            case "Положение ветра": return Enum.Parse(modDefault.GetType(), "Vanilla");
            // Keep the vanilla layout independent of the mod's preferred appearance.
            default: return modDefault;
        }
    }
}
