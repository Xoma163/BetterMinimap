using System;
using System.Collections.Generic;

namespace Shtab.RotatingMinimap;

// One source of truth for config destinations and the in-game row order.
internal static class SettingsLayout
{
    public const string General = "1. Общее";
    public const string Map = "2. Вид карты";
    public const string Zoom = "3. Масштаб и управление";
    public const string Hud = "4. Элементы вокруг карты";
    public const string Navigation = "5. Компас и навигация";
    public const string Players = "6. Игроки";
    public const string Transport = "7. Транспорт";

    private static readonly Dictionary<string, (string Section, int Order)> Rows = Build();

    private static Dictionary<string, (string Section, int Order)> Build()
    {
        var rows = new Dictionary<string, (string, int)>();
        void Add(string section, params string[] keys)
        {
            for (int i = 0; i < keys.Length; i++) rows.Add(keys[i], (section, i));
        }
        Add(General, "Мод включён", "Показывать мини-карту");
        Add(Map, "Форма", "Размер мини-карты", "Освещение карты", "Прозрачность карты", "Вращение", "Режим");
        Add(Zoom, "Min zoom", "Max zoom", "Шаг зума", "Плавный зум", "Сохранять отдельный зум для кораблей",
            "Зум через + и -", "Приблизить", "Отдалить");
        var hudKeys = new string[HudSlots.Count * 2];
        for (int i = 0; i < HudSlots.Count; i++)
        {
            hudKeys[i * 2] = "Slot " + (i + 1);
            hudKeys[i * 2 + 1] = HudSlots.TextSizeKey(i);
        }
        Add(Hud, hudKeys);
        Add(Navigation, "Показывать направления", "Показывать линии компаса", "Тип линии", "Толщина линии", "Цвет линии", "Указатель цели");
        Add(Players, "Показывать никнеймы", "Размер шрифта никнеймов", "Обводка никнеймов");
        Add(Transport, "Показывать транспорт", "Размер иконок транспорта");
        return rows;
    }

    public static string SectionFor(string key) => Rows.TryGetValue(key, out var row)
        ? row.Section : throw new ArgumentException("Unknown setting: " + key, nameof(key));
    public static int Order(string key) => Rows.TryGetValue(key, out var row) ? row.Order : 100;

    // A deliberately saved value in the destination always wins over a legacy section's value.
    public static T MigratedValue<T>(bool destinationSaved, T destination, T legacy) =>
        destinationSaved ? destination : legacy;
}
