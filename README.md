# Better Minimap

**A configurable client-side minimap for Valheim, by AndrewSha.**

[Releases](https://github.com/Xoma163/BetterMinimap/releases) · [Report a bug](https://github.com/Xoma163/BetterMinimap/issues) · [Русский](#русский)

## What's new in 1.1.1

Changes since **1.1.0**:

- **7. Transport:** carts, rafts and ships on both maps, including parked vehicles. Show transport defaults to On; **icon size 5–20, default 12**. Uses standard Ship/Vagon components, including compatible modded vehicles. Only nearby client-loaded objects are shown, not all vehicles in the world; no saved pins or network changes. No duplicate controlled-ship marker.
- **Player icon priority:** player icons are above other map icons and labels; your own arrow is highest. Player icon sizes, clipping and pointer pass-through are preserved.
- **Clean circular edge:** geometric mask and brown frame share their contour, fixing terrain pixels protruding beyond the outline. Frame color/thickness are unchanged; no black outer band.
- **Coordinate readability:** fixed missing/stale outline copies. Font size **9**, full **3-pixel outline**, **no background rectangle**.
- **Seven settings sections:** General, Map appearance, Zoom and controls, Around the map, Compass and navigation, Players, Transport. Reset buttons are in General; slot-order and ping help are included.
- **Automatic migration:** previous values are retained, including shortcuts and slot choices; explicitly saved new-section values win. Keyboard zoom is now labeled **Zoom with extra keys**, with the same config key and behavior.
- Updated RU/EN documentation and regression tests. See [CHANGELOG.md](CHANGELOG.md) for details.

## What's new in 1.1.0

Changes since **1.0.1**:

- Replaced separate biome/wind positions with **four configurable HUD slots**: two above and two below the map. Defaults: Empty / Biome / Wind / Empty. Wind is centered, empty rows collapse, and the HUD block stays within the screen where space permits. Biome text and wind retain their native sizes.
- Added **X/Y/Z coordinates** as slot content: font size **9**, black **3-pixel outline**, no separate visibility switch.
- Added **Show minimap** in General (On by default), without blocking the large **M** map.
- Added **session-only waypoints**: Shift+left-click anywhere on the large map to set/move a target; Shift+right-click to clear without deleting pins. Gold markers, an off-screen direction arrow, horizontal distance and a distance-bearing selection confirmation.
- Added **blue edge arrows for player pings** that do not fit on the minimap. Nearby pings keep their native rings and all pings retain their normal lifetime.
- Added **other players' nicknames** close below their icons; your own name is hidden. New Players settings: visibility, font size **4–20** (default **8**) and optional **2-pixel outline**. Only players sharing their position are labeled.
- Added a **single thin brown map border**, without an extra black outer band, for both map shapes.
- Updated English/Russian settings, presets and migration of old HUD settings; expanded automated checks and replaced the old screenshot gallery with **one current reference image**.

## Features

**Two supported UI languages: English and Russian.** Russian is selected automatically when the game language is Russian; English is used otherwise.

- **Orientation:** follow the camera, follow the character, or keep north up. Icons and compass letters remain readable.
- **Shape and size:** circular or square, 50–150%. Status effects move left when the map is enlarged.
- **Zoom:** smooth transitions, configurable limits and shortcuts. Custom zoom levels are anchored at ×1, so reaching a limit does not shift the scale.
- **Ship zoom:** separate land/ship magnifications, shared by worlds and characters in the same profile and saved between game sessions. Works for passengers too.
- **Compass:** independently toggle letters and lines; choose solid/dashed lines, color and thickness.
- **HUD slots:** two centered slots above the minimap and two below. Assign biome, wind, coordinates or nothing to each slot. External outlines improve readability.
- **Coordinates:** compact X/Y/Z coordinates in meters. Select Coordinates in any HUD slot to show them; there is no separate switch.
- **Waypoint:** Shift+left-click anywhere on the large map to set a target; Shift+right-click to clear. The minimap shows the target and horizontal distance, with an edge arrow when it is off-screen.
- **Edge pings:** pings that do not fit on the minimap become blue directional arrows at its edge until the native ping expires. Fully visible pings keep their normal ring icons.
- **Visibility:** General → Show minimap hides the small map and its entire HUD block; the large M map remains available.
- **Border:** a single thin brown outline follows the circular or square map edge, without a dark outer band.
- **Player names:** horizontal nicknames close below other players' icons, for players sharing their position. Your own name is hidden. Font size 4–20 (default 8) and an optional outline, independent of coordinates and biome text.
- **Transport:** one default-On switch for nearby client-loaded carts, rafts and ships on both maps; parked vehicles are included. Standard Ship/Vagon-based modded vehicles are supported too.
- **Lighting:** follow time of day, always day, or always night. Only the minimap material is changed.
- **Settings:** English, or Russian when the game language is Russian. Optional integration with StoreAndCraft's mod settings window.

The large **M** map keeps its normal controls; Shift+left/right-click, a temporary waypoint marker and optional transport icons are added. World lighting, exploration data, networking and world saves are not modified. **Install on the client, not on the server.**

## Screenshot

![Better Minimap — референс мини-карты](https://raw.githubusercontent.com/Xoma163/BetterMinimap/main/docs/images/reference.png)

## Requirements and installation

- Valheim with **BepInEx 5**. Built against Valheim **1.0.17** and BepInExPack **5.4.2351**.
- StoreAndCraft is **optional**. Its mod settings window provides the localized in-game UI; Better Minimap does not install or rebind F10.
- Other minimap/HUD overhaul mods may conflict. Compatibility with every HUD mod, resolution and game update is not guaranteed. Windows is the tested build environment.

**r2modman / Thunderstore Mod Manager:** download the ZIP from [GitHub Releases](https://github.com/Xoma163/BetterMinimap/releases), close the game, then use **Settings → Profile → Import local mod**. The package is `AndrewSha-BetterMinimap`.

**Manual:** install BepInEx 5, then copy `plugins/BetterMinimap.dll` from the ZIP into `BepInEx/plugins/BetterMinimap/`.

Do not install the old **Shtab Rotating Minimap** DLL alongside Better Minimap. The plugin ID and config filename remain unchanged for compatibility.

## Settings and defaults

In StoreAndCraft's mod settings window (usually **F10**), choose **Better Minimap**. Changes apply after **Save**; closing or going back without saving discards them. Other configuration managers are not modified by our localization integration.

| Section | Settings |
| --- | --- |
| **1. General** | Enable mod, show minimap, both reset buttons |
| **2. Map appearance** | Shape, size, lighting, rotation, orientation |
| **3. Zoom and controls** | Min/Max zoom, step, smoothing, separate ship zoom, keyboard zoom, zoom-in/out keys |
| **4. Around the map** | Slots 1–4: Empty / Biome / Wind / Coordinates; slot-order diagram |
| **5. Compass and navigation** | Directions, lines, line style/thickness/color, target indicator; waypoint controls and ping help |
| **6. Players** | Show names, name font size, name outline |
| **7. Transport** | Show transport, transport icon size |

Saved values are moved into these sections automatically; there is no need to reset your profile. Ship zoom remains in Zoom and controls because it controls magnification rather than vehicle icons.

| Setting | Mod default |
| --- | --- |
| Enable mod | On |
| Show minimap | On |
| Show transport | On; nearby client-loaded vehicles on both maps |
| Transport icon size | 12 (range 5–20); player icons unchanged |
| Rotation | Camera up |
| Shape / size | Circle / 130% |
| Slots 1 / 2 / 3 / 4 | Empty / biome / wind / empty |
| Coordinates | Not assigned to a slot |
| Target indicator | On; no target selected |
| Player names / font size / outline | On / 8 / on (2 pixels) |
| Lighting | Time of day |
| Min / Max zoom | ×0.1 / ×4 |
| Custom zoom step | ×1.5 |
| Smooth zoom / separate ship zoom | On |
| Zoom shortcuts | Equals/Minus and NumPad +/− |
| Compass letters / lines | On / off |
| Line style / thickness / color | Dashed / 0.8 / `#505050BB` |

- **Enable mod:** switches off all mod features without losing settings. The settings window remains available.
- **Show minimap** (General): hides the small map, slots, compass and overlays without disabling the mod or the large M map. Enabled by default; both reset presets restore it to On. The master mod switch takes precedence and restores the native map when disabled.
- **Show transport** (7. Transport): controls all vehicle icons, including the controlled ship, on both maps. Carts use a wheeled icon; rafts/ships use a sailboat icon. Only currently loaded Ship/Vagon objects are shown, not distant unloaded vehicles. No saved pins are created. Disabling the master mod switch restores the native controlled-ship marker. Icon size is configured here too: 5–20, default 12.
- **Reset to Vanilla:** stages a vanilla-like preset: square, 100%, north up, daytime lighting, Min 0 / Max 1, and custom rotation, zoom keys, smoothing, ship zoom, compass, coordinates and target indicator disabled. It is only a preset, not a locked mode. The default slot layout, outlines and zoom indicator remain while the mod is enabled. Use the master switch for a completely unmodified minimap, including its original biome/wind positions.
- **Reset to mod default:** stages the defaults in the table. **Neither reset changes the master enable switch**, including an unsaved manual change to it.
- Both resets apply after Save and reset saved land/ship zoom to ×1, subject to the selected limits. If the mod is disabled, the zoom reset waits until it is manually enabled.
- **Min zoom = 0** removes the custom far limit, but not the technical whole-map limit. If Min exceeds Max, the bounds are swapped internally. Only the small map is affected.
- The configurable zoom step applies to this mod's keys; native game keys retain their own step and respect the limits. Shift+Equals and Shift+Minus work with the default bindings.
- The selected zoom is shown below the map for two seconds after a change. On a ship, the label identifies ship zoom.
- Line thickness is **0.1–3.0**. Colors use `#RRGGBB` or `#RRGGBBAA`.
- The HUD block shifts vertically when needed to keep its slots on-screen. If the entire block is taller than the viewport, reduce the HUD/minimap size.

### HUD slots and navigation

```text
Slot 1  (empty)
Slot 2  (biome)
[ minimap ]
Slot 3  (wind)
Slot 4  (empty)
```

Each slot offers **Empty / Biome / Wind / Coordinates**. Elements can be repeated. Empty slots take no space; filled slots use compact spacing. The biome font and wind arrow retain their native sizes; the biome row reserves enough height for its actual font metrics. Select **Coordinates** to immediately show X/Y/Z with a 9-unit font and a 3-pixel outline; select **Empty** to hide them. Y is the player's world height. There is no separate coordinate toggle. The temporary zoom label appears below the last visible lower slot.

**Players:** names are enabled by default, with font size **8** (range **4–20**) and a **2-pixel** outline. Change these independently in **6. Players**. Names stay horizontal and close below the visible icon, discounting the sprite's transparent margins, and clip at the minimap boundary. Only the game's existing other public-position players are labeled; your own name is hidden and private player locations are not discovered. The large map is unchanged. The vanilla-like preset disables these labels. Existing saved font sizes are retained within the new bounds; select 8 manually to adopt the new default in an existing profile.

With **Target indicator** enabled, open **M** and **Shift+left-click any point**, including empty map space, to set or move a waypoint. A gold diamond shows the selected point on the large map and minimap; an edge arrow indicates an off-screen minimap target. Distance is measured horizontally in meters. **Shift+right-click** the large map to clear the waypoint without deleting a pin below the cursor. Ordinary clicks, pin checking, deletion and double-click creation retain their native behavior; the waypoint gestures do not modify pins. Selection is local to this session and clears when the feature/mod is disabled or the minimap is recreated. It is not saved or shared with other players.

Selecting a waypoint displays a confirmation such as **Target selected (350 m)** with rounded horizontal distance. Player ping rings that would cross the minimap border become blue arrows of the same shape as the gold waypoint arrow, following map rotation and shape. Fully visible pings remain native; edge indicators disappear with the game's ping lifetime and do not create extra network messages.

### Configuration and zoom memory

Close the game before manually editing:

```text
BepInEx/config/net.shtab.rotatingminimap.cfg
```

Stable config keys retain their historical Russian names; localized labels are presentation only. Existing settings are preserved on update and older sections are migrated. Old biome/wind positions migrate into slots; corner wind positions become centered on the same side. Explicitly saved slot settings take precedence. On upgrade from the 1.1.0 test build, coordinate slots that were disabled by its old switch become Empty; enabled coordinate slots are preserved. The obsolete switch is removed.

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

Automated checks cover geometry, target clamping, slot spacing/migration, viewport fitting, zoom levels and transitions, presets, localization strings and zoom-memory file I/O. **They do not launch Unity or replace in-game testing** of UI, input, clipping, lighting, resolution changes or compatibility. See [the manual test checklist](docs/testing-hud-navigation.md). Report bugs with game/mod versions, reproduction steps and screenshots; remove private data from any attached logs/configs.

## License and acknowledgements

**MIT**, copyright 2026 AndrewSha. See [LICENSE](LICENSE).

The implementation and generated compass icon are original. [Banderi/ValheimMinimapMod](https://github.com/Banderi/ValheimMinimapMod), [AlweStats](https://github.com/zAlweNy26/AlweStats) and [EvuMinimap](https://github.com/EvuMods/EvuMinimap) were studied for reference; their code and game assets are not bundled.

---

## Русский

**Better Minimap — клиентский мод мини-карты Valheim от AndrewSha.**

### Что изменилось в 1.1.1

По сравнению с **1.1.0**:

- **7. Транспорт:** тележки, плоты и корабли на обеих картах, включая припаркованные. Показ включён по умолчанию; **размер иконок 5–20, по умолчанию 12**. Поддерживаются стандартные компоненты Ship/Vagon и совместимые модификации. Только загруженные рядом объекты, не весь транспорт мира; без постоянных меток и изменения сети. Управляемый корабль не получает дублирующую метку.
- **Приоритет игроков:** иконки игроков выше остальных значков и надписей, своя стрелка — выше всех. Размеры иконок игроков, обрезка и пропуск кликов сохранены.
- **Ровный край карты:** маска круга и коричневая рамка используют согласованную геометрию — исправлены выступающие пиксели карты. Цвет и толщина рамки сохранены, чёрного внешнего слоя нет.
- **Читаемые координаты:** исправлены пропадающие копии обводки. Шрифт **9**, полноценная обводка **3 пикселя**, **без прямоугольной подложки**.
- **Семь разделов настроек:** Общее, Вид карты, Масштаб и управление, Элементы вокруг карты, Компас и навигация, Игроки, Транспорт. Кнопки сброса в «Общем»; добавлены схема слотов и подсказка про пинги.
- **Автоматический перенос:** старые значения сохраняются, включая клавиши и слоты; явно сохранённые новые разделы имеют приоритет. Переключатель зума называется **«Зум через доп. клавиши»**, ключ конфига и поведение прежние.
- Обновлены RU/EN-документация и тесты. Подробности — в [CHANGELOG.md](CHANGELOG.md).

### Что изменилось в 1.1.0

По сравнению с **1.0.1**:

- Вместо отдельных положений биома и ветра — **четыре настраиваемых слота**: два над картой и два под ней. По умолчанию: Пусто / Биом / Ветер / Пусто. Ветер по центру, пустые строки не занимают место, блок HUD удерживается в пределах экрана, если хватает места. Размеры биома и стрелки ветра остались штатными.
- Добавлены **координаты X/Y/Z** как содержимое слота: шрифт **9**, чёрная обводка **3 пикселя**, без отдельного переключателя показа.
- В «Общее» добавлен переключатель **«Показывать мини-карту»** (по умолчанию включён). Большая карта **M** остаётся доступной.
- Добавлена **цель на текущий сеанс**: Shift+ЛКМ по любой точке большой карты задаёт/переносит цель, Shift+ПКМ снимает её без удаления меток. Золотые маркеры, стрелка направления для далёкой цели, расстояние по горизонтали и подтверждение выбора с расстоянием.
- Пинги игроков, которые не помещаются на мини-карте, показываются **синими стрелками у края**. Ближние пинги сохраняют штатные кольца; время жизни пингов не меняется.
- Добавлены **никнеймы других игроков** близко под иконками; своё имя скрыто. В разделе «Игроки» — показ, размер шрифта **4–20** (по умолчанию **8**) и отключаемая обводка **2 пикселя**. Подписываются только игроки с открытой позицией.
- Добавлена **тонкая коричневая рамка** для обеих форм карты, без дополнительной чёрной обводки снаружи.
- Обновлены RU/EN-настройки, пресеты и перенос старых настроек HUD; расширены автоматические проверки. Вместо прежней галереи оставлен **один актуальный скриншот-референс**.

### Возможности

**Поддерживаются два языка интерфейса: русский и английский.** При русском языке игры автоматически выбирается русский, при остальных — английский.

- Вращение по камере, персонажу или север сверху; читаемые значки и буквы компаса.
- Круглая/квадратная форма и размер 50–150%. При увеличении карты бафы сдвигаются влево.
- Плавный зум, свои клавиши и пределы. Ступени привязаны к ×1 и не смещаются после достижения Min/Max.
- Отдельные масштабы суши и корабля с сохранением между запусками игры, в том числе для пассажиров.
- Независимые линии и буквы компаса; цвет, толщина и сплошной/пунктирный тип линий.
- Четыре слота: два над мини-картой и два под ней. В каждом — биом, ветер, координаты или пусто. Ветер только по центру; внешние обводки.
- Координаты X/Y/Z в метрах, уменьшенный шрифт. Появляются при выборе «Координаты» в любом слоте, без отдельного переключателя.
- Shift+ЛКМ по любой точке большой карты — указать цель, Shift+ПКМ — снять. На мини-карте — указатель, расстояние и стрелка у края для далёкой цели.
- Не помещающиеся на мини-карте пинги игроков заменяются синими стрелками у края до истечения штатного времени пинга.
- «Общее → Показывать мини-карту» скрывает мини-карту и весь её блок; большая карта M остаётся доступной.
- Тонкая коричневая рамка по контуру круглой/квадратной карты, без дополнительного тёмного слоя снаружи.
- Никнеймы близко под иконками других игроков с открытой позицией; своё имя скрыто. Размер шрифта 4–20 (по умолчанию 8) и отдельный переключатель обводки.
- Общий переключатель транспорта: тележки, плоты и корабли, включая припаркованные, на обеих картах. Включён по умолчанию; показывает загруженные рядом объекты, включая модифицированные на компонентах Ship/Vagon.
- Освещение мини-карты: по времени суток, всегда день или всегда ночь.
- Русские настройки при русском языке игры, английские — при остальных.

Обычное управление большой картой **M** не меняется; добавлены Shift+ЛКМ/ПКМ, временный маркер цели и отключаемые иконки транспорта. Освещение мира, исследование, сеть и сохранения мира не изменяются. **На сервер мод устанавливать не нужно.**

### Скриншот

[Референс мини-карты — выше](#screenshot).

### Установка

Нужен **BepInEx 5**. Сборка выполнена с DLL **Valheim 1.0.17** и BepInExPack **5.4.2351**.

Скачайте ZIP из [GitHub Releases](https://github.com/Xoma163/BetterMinimap/releases), закройте игру и импортируйте через **Settings → Profile → Import local mod** в r2modman/Thunderstore Mod Manager.

Вручную: скопируйте `plugins/BetterMinimap.dll` из архива в `BepInEx/plugins/BetterMinimap/`.

StoreAndCraft необязателен; его окно модов (обычно **F10**) предоставляет локализованный интерфейс. Без него остаётся файл конфига. Не держите старую DLL **Shtab Rotating Minimap** одновременно с новой. Совместимость со всеми HUD-модами и версиями игры не гарантируется.

### Настройки

| Раздел | Настройки |
| --- | --- |
| **1. Общее** | Мод включён, показывать мини-карту, обе кнопки сброса |
| **2. Вид карты** | Форма, размер, освещение, вращение, режим вращения |
| **3. Масштаб и управление** | Min/Max зум, шаг, плавность, отдельный зум корабля, зум клавишами, приблизить/отдалить |
| **4. Элементы вокруг карты** | Слоты 1–4: Пусто / Биом / Ветер / Координаты; схема их порядка |
| **5. Компас и навигация** | Направления, линии, тип/толщина/цвет линий, указатель цели; подсказки про управление целью и пинги |
| **6. Игроки** | Показ никнеймов, размер шрифта, обводка |
| **7. Транспорт** | Показ транспорта, размер иконок |

Сохранённые значения переносятся автоматически, сбрасывать профиль не нужно. Отдельный зум корабля остаётся в «Масштабе и управлении»: это настройка масштаба, не иконок транспорта. В окне StoreAndCraft изменения действуют после **Сохранить**. Закрытие или возврат назад без сохранения отменяет изменения.

По умолчанию: мод включён, вращение по камере, **круглая карта 130%**, Slot 1 пустой, биом в Slot 2, ветер в Slot 3, Slot 4 пустой, освещение по времени суток. Указатель цели включён, но цель не выбрана. Зум **×0,1–×4**, шаг **×1,5**, плавность и отдельный зум корабля включены. Клавиши `= / −` и NumPad `+ / −`. Буквы компаса включены, линии выключены; линии — пунктир, толщина **0,8**, цвет **`#505050BB`**.

- **Мод включён** — общий выключатель всех функций с сохранением настроек и доступа к F10.
- **Показывать мини-карту** (раздел «Общее») — скрывает мини-карту, слоты, компас и указатели, но не отключает мод и большую карту M. По умолчанию включено; оба пресета возвращают значение «Включено». Общий выключатель мода имеет приоритет: при выключенном моде возвращается штатная карта.
- **Показывать транспорт** (раздел «7. Транспорт») — иконки тележек, плотов и кораблей на обеих картах, включая управляемый корабль. По умолчанию включено. Только загруженный рядом транспорт на компонентах Ship/Vagon; без постоянных меток и поиска по всему миру. Выключение самого мода возвращает штатную метку управляемого корабля. Здесь же размер иконок: 5–20, по умолчанию 12.
- **Сбросить к настройкам игры** — обычный пресет: квадрат 100%, север сверху, дневное освещение, Min 0 / Max 1. Вращение, дополнительные клавиши, плавность, отдельный зум корабля, компас, координаты и указатель цели выключены. Стандартная раскладка слотов, обводки и индикатор зума остаются; полностью штатную карту с исходными положениями биома/ветра возвращает общий выключатель.
- **Сбросить к настройкам мода** — дефолты выше. **Ни один пресет не меняет общий выключатель**, включая несохранённый ручной выбор.
- Сбросы применяются после Save и возвращают память масштабов к ×1 с учётом пределов. При выключенном моде сброс масштаба ждёт его ручного включения.
- **Min = 0** снимает пользовательский предел отдаления, но не техническую границу всей карты. При Min > Max пределы внутренне меняются местами.
- Настроенная кратность действует для клавиш мода; штатные клавиши игры сохраняют свой шаг, но учитывают Min/Max. Работают `Shift+=` и `Shift+−`.
- Индикатор автоматически появляется на две секунды после изменения масштаба. На корабле подпись указывает корабельный зум.
- Толщина линий — **0,1–3**, цвет — `#RRGGBB` или `#RRGGBBAA`.
- Блок автоматически сдвигается по вертикали, чтобы слоты не выходили за экран. Если он целиком выше доступного пространства, уменьшите размер HUD/мини-карты.

**Слоты:** порядок сверху вниз — Slot 1, Slot 2, мини-карта, Slot 3, Slot 4. Для каждого доступно «Пусто / Биом / Ветер / Координаты»; элементы можно повторять. Пустые слоты не занимают место, отступы компактные. Шрифт биома и размер стрелки ветра остаются штатными; высота строки биома учитывает реальные размеры текста. Выбор «Координаты» сразу показывает X/Y/Z шрифтом 9 единиц с обводкой 3 пикселя; Y — высота игрока в мире. Отдельного переключателя нет. Индикатор зума появляется ниже последнего видимого нижнего слота.

**Никнеймы:** раздел **6. Игроки** — «Показывать никнеймы», «Размер шрифта никнеймов» (**4–20**) и «Обводка никнеймов». По умолчанию включены, размер **8**, обводка **2 пикселя** включена. Текст находится близко под видимой иконкой, без лишнего отступа от прозрачных полей спрайта, и не вращается вместе с картой; у края карты обрезается по её форме. Подписываются только другие игроки, которые делятся позицией; своё имя скрыто. Большая карта и скрытые позиции игроков не меняются. Пресет настроек игры выключает подписи. Ранее сохранённый размер шрифта сохраняется в новых пределах; чтобы использовать новый дефолт в старом профиле, выберите 8 вручную.

**Цель:** откройте M, нажмите Shift+ЛКМ по любой точке, в том числе пустому месту. Повторный Shift+ЛКМ переносит цель. На большой и мини-карте точка обозначается золотым ромбом, за пределами мини-карты — стрелкой; рядом расстояние по горизонтали в метрах. Shift+ПКМ снимает цель и не удаляет метки под курсором. Эти жесты не меняют обычные метки. Выбор не сохраняется в мир и не передаётся другим игрокам; сбрасывается при выключении функции/мода или пересоздании мини-карты.

При выборе цели появляется подтверждение вида **«Цель выбрана (350 м)»** с округлённым расстоянием по горизонтали.

**Пинги:** если круг пинга не помещается на мини-карте, у края появляется синяя стрелка такой же формы, как у жёлтой цели, с учётом формы карты и вращения. Полностью видимые пинги остаются штатными; стрелка исчезает вместе с пингом, не продлевая его время и не отправляя новых сообщений.

Конфиг: `BepInEx/config/net.shtab.rotatingminimap.cfg`. Редактировать вручную при закрытой игре. При обновлении выбранные значения сохраняются, старые разделы мигрируют. Старые положения биома/ветра переносятся в слоты; угловой ветер становится центральным на той же стороне. Уже сохранённые настройки слотов имеют приоритет. При обновлении с тестовой 1.1.0 скрытые старым переключателем координаты становятся пустыми слотами, включённые сохраняются. Устаревший переключатель удаляется.

Память масштабов: `BepInEx/config/net.shtab.rotatingminimap.cfg.zoom`. Записывается автоматически; общая для миров, персонажей и кораблей внутри профиля. При первом включении без памяти оба масштаба берутся из текущего; настройте корабельный масштаб после посадки. При следующих входах восстанавливается подходящее значение с учётом Min/Max.

Для сборки нужны .NET SDK 8 и DLL своего клиента; команды приведены в английском разделе. Автотесты проверяют математику, пресеты, переводы и файл памяти, но не запускают Unity. В багрепорте укажите версии, шаги воспроизведения и скриншоты; удалите личные данные из логов и конфигов.

Лицензия **MIT**, автор **AndrewSha**.
