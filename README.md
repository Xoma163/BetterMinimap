# Better Minimap

**A configurable client-side minimap for Valheim, by AndrewSha.**

[Thunderstore](https://thunderstore.io/c/valheim/p/AndrewSha/BetterMinimap/) · [Releases](https://github.com/Xoma163/BetterMinimap/releases) · [Report a bug](https://github.com/Xoma163/BetterMinimap/issues) · [Русский](#русский)

## Screenshot

![Better Minimap — референс мини-карты](https://raw.githubusercontent.com/Xoma163/BetterMinimap/main/docs/images/reference.png)

## Changelog

<details>
<summary>1.2.1</summary>

- Shorter English/Russian README: screenshot first, collapsible version history and essential settings/controls only.
- Build instructions moved to a separate document. Mod behavior and saved settings are unchanged from 1.2.0.

</details>

<details>
<summary>1.2.0</summary>

- Six HUD slots, with separate **25–400%** size controls and compact settings rows.
- In-game clock, local computer clock and carry weight as slot content.
- Minimap background transparency: **0–100%**, without fading icons or text.
- At 100% slot size, all text uses the biome font size; coordinates are no longer smaller by default. Existing settings are preserved.

</details>

<details>
<summary>1.1.1</summary>

- Cart, raft and ship icons on both maps, with adjustable size.
- Player icons above other markers; fixes for circular map edges and coordinate outlines.
- Seven settings sections with automatic migration of saved values.

</details>

<details>
<summary>1.1.0</summary>

- Configurable HUD slots, XYZ coordinates and a minimap visibility switch.
- Session waypoints and blue edge arrows for off-screen pings.
- Other players' names and a map border for both shapes.

</details>

<details>
<summary>1.0.1</summary>

- README screenshots and clearer English/Russian language information; no gameplay changes.

</details>

<details>
<summary>1.0.0</summary>

- First release: rotating minimap, circular/square shapes, smooth zoom, separate ship zoom, compass, lighting and English/Russian settings.

</details>

[Full changelog](https://github.com/Xoma163/BetterMinimap/blob/main/CHANGELOG.md)

## Features

- Camera-up, character-up or north-up orientation; circular or square map, adjustable size and transparency.
- Smooth zoom with custom shortcuts and separate land/ship zoom saved between sessions.
- Six HUD slots for biome, wind, coordinates, clocks and carry weight, each with its own size.
- Compass, waypoints and directional arrows for off-screen pings.
- Names of other players sharing their position; nearby loaded carts, rafts and ships on both maps, including compatible Ship/Vagon-based modded vehicles.

**English and Russian UI. Client-only:** no server installation or changes to world saves, exploration or networking.

## Requirements and installation

- **BepInEx 5** required. Built against Valheim **1.0.17** and BepInExPack **5.4.2351**.
- Install **AndrewSha-BetterMinimap** through r2modman/Thunderstore Mod Manager, or import the release ZIP via **Settings → Profile → Import local mod** with the game closed.
- Manual install: copy `plugins/BetterMinimap.dll` from the ZIP into `BepInEx/plugins/BetterMinimap/`.

StoreAndCraft is optional and provides the in-game settings window. Other minimap/HUD overhauls may conflict. Do not keep the old **Shtab Rotating Minimap** DLL installed alongside this mod.

## Settings and defaults

In StoreAndCraft's mod window (usually **F10**), select **Better Minimap**. **Save** applies changes; Close/Back discards unsaved edits.

| Section | Main settings and defaults |
| --- | --- |
| **1. General** | Mod and minimap On; reset buttons |
| **2. Map appearance** | Circle, 130% size (50–150%), camera-up, time-of-day lighting; transparency 0% (0–100%) |
| **3. Zoom and controls** | ×0.1–×4, step ×1.5; smoothing and separate ship zoom On; `= / −` and NumPad `+ / −` |
| **4. Around the map** | Six slot contents; size 100% each (25–400%, including wind) |
| **5. Compass and navigation** | Directions On, lines Off, target indicator On |
| **6. Players** | Names On; font 8 (4–20), outline On |
| **7. Transport** | Icons On; size 12 (5–20) |

- **Show minimap** hides the small HUD but keeps **M** available. **Enable mod → Off** restores the native HUD without losing settings.
- Both reset presets preserve the master switch and reset zoom to ×1 within your limits. **Reset to Vanilla** is a vanilla-like preset, not a full mod disable.
- Transparency affects only the minimap background, not icons, labels or the large map.

### HUD slots and navigation

**Order:** `1 → 2 → minimap → 3 → 4 → 5 → 6`. **Defaults:** Empty / Biome / Wind / Empty / Empty / Empty.

- Choose **Empty / Biome / Wind / Coordinates / In-game time / Weight / Real time** in any slot. Repeats are allowed; empty slots take no space.
- At 100%, text uses the native biome font size and wind its native size. Reduce an individual slot's percentage if its text is too wide.
- Both clocks use **HH:mm**: game time or your computer's local time. Weight shows **current / capacity**, including bonuses.
- On **M**: **Shift+left-click** sets or moves a waypoint; **Shift+right-click** clears it without deleting pins. The target and distance are local to the current session.
- Off-screen pings appear as **blue edge arrows**; nearby pings keep their normal rings.

### Configuration

Without StoreAndCraft, edit `BepInEx/config/net.shtab.rotatingminimap.cfg` with the game closed. Saved settings migrate automatically. Land/ship zoom is stored in the adjacent `.cfg.zoom` file.

## Development and license

[Build instructions and automated tests](https://github.com/Xoma163/BetterMinimap/blob/main/docs/building.md) · [In-game test checklist](https://github.com/Xoma163/BetterMinimap/blob/main/docs/testing-hud-navigation.md). Automated tests do not verify Unity rendering or input.

**MIT**, copyright 2026 AndrewSha. See [LICENSE](https://github.com/Xoma163/BetterMinimap/blob/main/LICENSE).

The implementation and generated compass icon are original. [Banderi/ValheimMinimapMod](https://github.com/Banderi/ValheimMinimapMod), [AlweStats](https://github.com/zAlweNy26/AlweStats) and [EvuMinimap](https://github.com/EvuMods/EvuMinimap) were studied for reference; their code and game assets are not bundled.

---

## Русский

**Better Minimap — настраиваемый клиентский мод мини-карты Valheim от AndrewSha.**

[Скриншот — выше](#screenshot).

### Changelog

<details>
<summary>1.2.1</summary>

- Сокращён README на обоих языках: скриншот выше истории версий, версии свёрнуты, в настройках оставлено главное.
- Инструкции сборки вынесены отдельно. Поведение мода и сохранённые настройки не изменены относительно 1.2.0.

</details>

<details>
<summary>1.2.0</summary>

- Шесть слотов HUD, индивидуальный размер **25–400%** и компактные строки настроек.
- Игровые часы, местное время компьютера и переносимый вес в любом слоте.
- Прозрачность фона мини-карты **0–100%**, без изменения значков и текста.
- При размере 100% все тексты используют размер шрифта биома; координаты больше не уменьшены по умолчанию. Сохранённые настройки не сбрасываются.

</details>

<details>
<summary>1.1.1</summary>

- Иконки тележек, плотов и кораблей на обеих картах с настройкой размера.
- Иконки игроков поверх остальных; исправлены край круглой карты и обводка координат.
- Семь разделов настроек с автоматическим переносом сохранённых значений.

</details>

<details>
<summary>1.1.0</summary>

- Настраиваемые слоты HUD, координаты XYZ и переключатель видимости мини-карты.
- Цель на текущий сеанс и синие стрелки для пингов за пределами карты.
- Никнеймы других игроков и рамка для обеих форм карты.

</details>

<details>
<summary>1.0.1</summary>

- Скриншоты в README и информация о поддержке русского и английского языков; без изменений игровой логики.

</details>

<details>
<summary>1.0.0</summary>

- Первый релиз: вращение, круглая/квадратная карта, плавный и отдельный корабельный зум, компас, освещение и настройки RU/EN.

</details>

[Полная история изменений](https://github.com/Xoma163/BetterMinimap/blob/main/CHANGELOG.md)

### Возможности

- Вращение по камере, персонажу или север сверху; круглая/квадратная форма, размер и прозрачность карты.
- Плавный зум, свои клавиши и отдельные масштабы суши/корабля с сохранением между сеансами.
- Шесть слотов для биома, ветра, координат, часов и веса с индивидуальным размером.
- Компас, цель и стрелки направления для далёких пингов.
- Никнеймы игроков с открытой позицией; загруженные рядом тележки, плоты и корабли на обеих картах, включая совместимый модифицированный транспорт на Ship/Vagon.

**Интерфейс на русском и английском. Только клиент:** установка на сервер не нужна; сохранения мира, исследование карты и сеть не меняются.

### Установка

- Нужен **BepInEx 5**. Сборка с DLL Valheim **1.0.17** и BepInExPack **5.4.2351**.
- Установите **AndrewSha-BetterMinimap** через r2modman/Thunderstore Mod Manager либо импортируйте ZIP релиза: **Settings → Profile → Import local mod** при закрытой игре.
- Вручную: `plugins/BetterMinimap.dll` из ZIP → `BepInEx/plugins/BetterMinimap/`.

StoreAndCraft необязателен и предоставляет окно настроек в игре. Другие моды мини-карты/HUD могут конфликтовать. Не оставляйте старую DLL **Shtab Rotating Minimap** одновременно с новой.

### Настройки и значения по умолчанию

В окне модов StoreAndCraft (обычно **F10**) выберите **Better Minimap**. **Сохранить** применяет изменения; закрытие/возврат отменяет несохранённое.

| Раздел | Главное и значения по умолчанию |
| --- | --- |
| **1. Общее** | Мод и мини-карта включены; кнопки сброса |
| **2. Вид карты** | Круг, размер 130% (50–150%), вращение по камере, освещение по времени суток; прозрачность 0% (0–100%) |
| **3. Масштаб и управление** | ×0,1–×4, шаг ×1,5; плавность и отдельный зум корабля включены; `= / −` и NumPad `+ / −` |
| **4. Элементы вокруг карты** | Содержимое шести слотов; размер каждого 100% (25–400%, включая ветер) |
| **5. Компас и навигация** | Направления включены, линии выключены, указатель цели включён |
| **6. Игроки** | Никнеймы включены; шрифт 8 (4–20), обводка включена |
| **7. Транспорт** | Иконки включены; размер 12 (5–20) |

- **Показывать мини-карту** скрывает малый HUD, оставляя **M** доступной. Выключение самого мода возвращает штатный HUD, сохраняя настройки.
- Оба пресета сохраняют общий выключатель и сбрасывают зум к ×1 с учётом пределов. **Сбросить к настройкам игры** — приближённый к игре пресет, а не полное отключение мода.
- Прозрачность меняет только фон мини-карты, не значки, подписи или большую карту.

### Слоты HUD и навигация

**Порядок:** `1 → 2 → мини-карта → 3 → 4 → 5 → 6`. **По умолчанию:** Пусто / Биом / Ветер / Пусто / Пусто / Пусто.

- В любом слоте: **Пусто / Биом / Ветер / Координаты / Игровое время / Вес / Реальное время**. Повторения разрешены; пустые слоты не занимают место.
- При 100% у текста штатный размер шрифта биома, у ветра — штатный размер стрелки. Если текст слишком широкий, уменьшите процент только его слота.
- Оба варианта часов — **HH:mm**: игровое или местное время компьютера. Вес — **текущий / грузоподъёмность**, с учётом бонусов.
- На **M**: **Shift+ЛКМ** задаёт/переносит цель, **Shift+ПКМ** снимает её без удаления пинов. Цель и расстояние локальны текущему сеансу.
- Далёкие пинги — **синие стрелки у края**; ближние сохраняют обычные кольца.

### Конфиг

Без StoreAndCraft редактируйте `BepInEx/config/net.shtab.rotatingminimap.cfg` при закрытой игре. Сохранённые настройки переносятся автоматически. Масштабы суши/корабля хранятся в соседнем `.cfg.zoom`.

[Сборка и автотесты](https://github.com/Xoma163/BetterMinimap/blob/main/docs/building.md) · [Ручные проверки в игре](https://github.com/Xoma163/BetterMinimap/blob/main/docs/testing-hud-navigation.md). Автотесты не проверяют отрисовку и ввод Unity.

Лицензия **MIT**, автор **AndrewSha**.
