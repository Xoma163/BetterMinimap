# Better Minimap

**A configurable client-side minimap for Valheim, by AndrewSha.**

[Releases](https://github.com/Xoma163/BetterMinimap/releases) · [Report a bug](https://github.com/Xoma163/BetterMinimap/issues) · [Русский](#русский)

## Features

- **Orientation:** follow the camera, follow the character, or keep north up. Icons and compass letters remain readable.
- **Shape and size:** circular or square, 50–150%. Status effects move left when the map is enlarged.
- **Zoom:** smooth transitions, configurable limits and shortcuts. Custom zoom levels are anchored at ×1, so reaching a limit does not shift the scale.
- **Ship zoom:** separate land/ship magnifications, shared by worlds and characters in the same profile and saved between game sessions. Works for passengers too.
- **Compass:** independently toggle letters and lines; choose solid/dashed lines, color and thickness.
- **HUD layout:** place the biome label above/below the map and the wind arrow at an outside corner or top/bottom center. External outlines improve readability.
- **Lighting:** follow time of day, always day, or always night. Only the minimap material is changed.
- **Settings:** English, or Russian when the game language is Russian. Optional integration with StoreAndCraft's mod settings window.

The large **M** map, world lighting, exploration data, networking and world saves are not modified. **Install on the client, not on the server.**

## Requirements and installation

- Valheim with **BepInEx 5**. Built against Valheim **1.0.17** and BepInExPack **5.4.2351**.
- StoreAndCraft is **optional**. Its mod settings window provides the localized in-game UI; Better Minimap does not install or rebind F10.
- Other minimap/HUD overhaul mods may conflict. Compatibility with every HUD mod, resolution and game update is not guaranteed. Windows is the tested build environment.

**r2modman / Thunderstore Mod Manager:** download the ZIP from [GitHub Releases](https://github.com/Xoma163/BetterMinimap/releases), close the game, then use **Settings → Profile → Import local mod**. The package is `AndrewSha-BetterMinimap`.

**Manual:** install BepInEx 5, then copy `plugins/BetterMinimap.dll` from the ZIP into `BepInEx/plugins/BetterMinimap/`.

Do not install the old **Shtab Rotating Minimap** DLL alongside Better Minimap. The plugin ID and config filename remain unchanged for compatibility.

## Settings and defaults

In StoreAndCraft's mod settings window (usually **F10**), choose **Better Minimap**. Changes apply after **Save**; closing or going back without saving discards them. Other configuration managers are not modified by our localization integration.

Sections: **1. General → 2. Map → 3. Zoom → 4. Shortcuts → 5. Compass**.

| Setting | Mod default |
| --- | --- |
| Enable mod | On |
| Rotation | Camera up |
| Shape / size | Circle / 130% |
| Biome / wind | Above / bottom center |
| Lighting | Time of day |
| Min / Max zoom | ×0.1 / ×4 |
| Custom zoom step | ×1.5 |
| Smooth zoom / separate ship zoom | On |
| Zoom shortcuts | Equals/Minus and NumPad +/− |
| Compass letters / lines | On / off |
| Line style / thickness / color | Dashed / 0.8 / `#505050BB` |

- **Enable mod:** switches off all mod features without losing settings. The settings window remains available.
- **Reset to Vanilla:** stages a vanilla-like preset: square, 100%, north up, default biome/wind positions, daytime lighting, Min 0 / Max 1, and custom rotation, zoom keys, smoothing, ship zoom and compass disabled. It is only a preset, not a locked mode. Automatic outlines and the zoom indicator remain while the mod is enabled. Use the master switch for a completely unmodified minimap.
- **Reset to mod default:** stages the defaults in the table. **Neither reset changes the master enable switch**, including an unsaved manual change to it.
- Both resets apply after Save and reset saved land/ship zoom to ×1, subject to the selected limits. If the mod is disabled, the zoom reset waits until it is manually enabled.
- **Min zoom = 0** removes the custom far limit, but not the technical whole-map limit. If Min exceeds Max, the bounds are swapped internally. Only the small map is affected.
- The configurable zoom step applies to this mod's keys; native game keys retain their own step and respect the limits. Shift+Equals and Shift+Minus work with the default bindings.
- The selected zoom is shown below the map for two seconds after a change. On a ship, the label identifies ship zoom.
- Line thickness is **0.1–3.0**. Colors use `#RRGGBB` or `#RRGGBBAA`.
- Outside labels/arrows need screen space. Large HUD sizes and top-center combinations may extend beyond the screen; choose a different position or size if necessary.

### Configuration and zoom memory

Close the game before manually editing:

```text
BepInEx/config/net.shtab.rotatingminimap.cfg
```

Stable config keys retain their historical Russian names; localized labels are presentation only. Existing settings are preserved on update and older sections are migrated.

Separate land/ship magnifications are saved automatically in:

```text
BepInEx/config/net.shtab.rotatingminimap.cfg.zoom
```

This contains only two zoom values, not game saves. The first time ship zoom is enabled without existing memory, both values start from the current zoom; set your preferred ship zoom aboard. Subsequent sessions restore the appropriate value, subject to Min/Max. A reset after Save resets both values to ×1.

## Building and tests

Use **.NET SDK 8** and reference DLLs from your own installed game and BepInEx profile. These proprietary/runtime DLLs are not included in this repository.

```powershell
dotnet build RotatingMinimap.csproj -c Release `
  '-p:ValheimDir=C:\Games\Valheim' `
  '-p:BepInExCore=C:\ModProfile\BepInEx\core'
dotnet run --project tests/GeometryTests.csproj -c Release
powershell -NoProfile -ExecutionPolicy Bypass -File package.ps1
```

Output: `bin/Release/net48/BetterMinimap.dll`; installable ZIP: `artifacts/`.
Packaging uses Windows PowerShell/System.Drawing. A private `local.props` may supply local reference paths and is ignored by Git.

Automated checks cover geometry, zoom levels and transitions, presets, localization strings and zoom-memory file I/O. **They do not launch Unity or replace in-game testing** of UI, clipping, lighting, resolution changes or compatibility. Report bugs with game/mod versions, reproduction steps and screenshots; remove private data from any attached logs/configs.

## License and acknowledgements

**MIT**, copyright 2026 AndrewSha. See [LICENSE](LICENSE).

The implementation and generated compass icon are original. [Banderi/ValheimMinimapMod](https://github.com/Banderi/ValheimMinimapMod), [AlweStats](https://github.com/zAlweNy26/AlweStats) and [EvuMinimap](https://github.com/EvuMods/EvuMinimap) were studied for reference; their code and game assets are not bundled.

---

## Русский

**Better Minimap — клиентский мод мини-карты Valheim от AndrewSha.**

### Возможности

- Вращение по камере, персонажу или север сверху; читаемые значки и буквы компаса.
- Круглая/квадратная форма и размер 50–150%. При увеличении карты бафы сдвигаются влево.
- Плавный зум, свои клавиши и пределы. Ступени привязаны к ×1 и не смещаются после достижения Min/Max.
- Отдельные масштабы суши и корабля с сохранением между запусками игры, в том числе для пассажиров.
- Независимые линии и буквы компаса; цвет, толщина и сплошной/пунктирный тип линий.
- Положение биома сверху/снизу, стрелки ветра — снаружи по углам или по центру сверху/снизу. Внешние обводки.
- Освещение мини-карты: по времени суток, всегда день или всегда ночь.
- Русские настройки при русском языке игры, английские — при остальных.

Большая карта **M**, освещение мира, исследование, сеть и сохранения мира не изменяются. **На сервер мод устанавливать не нужно.**

### Установка

Нужен **BepInEx 5**. Сборка выполнена с DLL **Valheim 1.0.17** и BepInExPack **5.4.2351**.

Скачайте ZIP из [GitHub Releases](https://github.com/Xoma163/BetterMinimap/releases), закройте игру и импортируйте через **Settings → Profile → Import local mod** в r2modman/Thunderstore Mod Manager.

Вручную: скопируйте `plugins/BetterMinimap.dll` из архива в `BepInEx/plugins/BetterMinimap/`.

StoreAndCraft необязателен; его окно модов (обычно **F10**) предоставляет локализованный интерфейс. Без него остаётся файл конфига. Не держите старую DLL **Shtab Rotating Minimap** одновременно с новой. Совместимость со всеми HUD-модами и версиями игры не гарантируется.

### Настройки

Разделы: **1. Общее → 2. Карта → 3. Зум → 4. Клавиши → 5. Компас**. В окне StoreAndCraft изменения действуют после **Сохранить**. Закрытие или возврат назад без сохранения отменяет изменения.

По умолчанию: мод включён, вращение по камере, **круглая карта 130%**, биом сверху, ветер снизу по центру, освещение по времени суток. Зум **×0,1–×4**, шаг **×1,5**, плавность и отдельный зум корабля включены. Клавиши `= / −` и NumPad `+ / −`. Буквы компаса включены, линии выключены; линии — пунктир, толщина **0,8**, цвет **`#505050BB`**.

- **Мод включён** — общий выключатель всех функций с сохранением настроек и доступа к F10.
- **Сбросить к настройкам игры** — обычный пресет: квадрат 100%, север сверху, штатные положения биома/ветра, дневное освещение, Min 0 / Max 1. Вращение, дополнительные клавиши, плавность, отдельный зум корабля и компас выключены. Обводки и индикатор остаются, пока мод включён; полностью штатную карту возвращает общий выключатель.
- **Сбросить к настройкам мода** — дефолты выше. **Ни один пресет не меняет общий выключатель**, включая несохранённый ручной выбор.
- Сбросы применяются после Save и возвращают память масштабов к ×1 с учётом пределов. При выключенном моде сброс масштаба ждёт его ручного включения.
- **Min = 0** снимает пользовательский предел отдаления, но не техническую границу всей карты. При Min > Max пределы внутренне меняются местами.
- Настроенная кратность действует для клавиш мода; штатные клавиши игры сохраняют свой шаг, но учитывают Min/Max. Работают `Shift+=` и `Shift+−`.
- Индикатор автоматически появляется на две секунды после изменения масштаба. На корабле подпись указывает корабельный зум.
- Толщина линий — **0,1–3**, цвет — `#RRGGBB` или `#RRGGBBAA`.
- Элементам снаружи карты нужно место. При большом размере HUD и комбинациях сверху они могут выйти за экран — измените положение или размер.

Конфиг: `BepInEx/config/net.shtab.rotatingminimap.cfg`. Редактировать вручную при закрытой игре. При обновлении выбранные значения сохраняются, старые разделы мигрируют.

Память масштабов: `BepInEx/config/net.shtab.rotatingminimap.cfg.zoom`. Записывается автоматически; общая для миров, персонажей и кораблей внутри профиля. При первом включении без памяти оба масштаба берутся из текущего; настройте корабельный масштаб после посадки. При следующих входах восстанавливается подходящее значение с учётом Min/Max.

Для сборки нужны .NET SDK 8 и DLL своего клиента; команды приведены в английском разделе. Автотесты проверяют математику, пресеты, переводы и файл памяти, но не запускают Unity. В багрепорте укажите версии, шаги воспроизведения и скриншоты; удалите личные данные из логов и конфигов.

Лицензия **MIT**, автор **AndrewSha**.
