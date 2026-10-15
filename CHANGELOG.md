# Changelog

<details>
<summary>1.2.1 — Documentation refresh / Обновление документации</summary>

### English

- Moved the screenshot above a single Changelog section, with a separate collapsed block for every version in each language.
- Shortened the English/Russian README, especially settings and HUD/navigation: only essential defaults and controls remain.
- Moved build/test instructions to `docs/building.md`; preserved the complete release history in this file.
- Documentation-only update since **1.2.0**. Mod behavior, defaults and saved settings are unchanged; the version bump delivers the revised README in the Thunderstore package.

### Русский

- Скриншот перенесён выше единого раздела Changelog; каждая версия — отдельный свёрнутый блок в обоих языках.
- Сокращён README RU/EN, особенно настройки и HUD/навигация: оставлены основные значения по умолчанию и управление.
- Инструкции сборки/тестирования вынесены в `docs/building.md`; полная история релизов сохранена в этом файле.
- Только документация относительно **1.2.0**. Поведение мода, дефолты и сохранённые настройки не изменены; новый номер нужен для доставки обновлённого README в пакете Thunderstore.

</details>

<details>
<summary>1.2.0 — Six HUD slots, individual sizes, clocks, weight and transparency / Шесть слотов, индивидуальные размеры, часы, вес и прозрачность</summary>

### English

Changes since **1.1.1**; consolidates the unpublished local test builds.

- Added **Slot 5 and Slot 6** below Slot 4: two upper and four lower slots, with new slots empty by default. Existing choices are preserved; empty rows collapse and the zoom popup stays below the last filled row.
- Each slot has its own **size, 25–400%, default 100%**, including biome text and the wind arrow. All text contents share the native biome font size at 100%; coordinates are no longer smaller by default. Wind scales from its native size; outline widths are unchanged. Sizes persist across content changes; both presets reset them to 100%. Row heights follow the text metrics or scaled wind diagonal.
- Compact StoreAndCraft slot rows combine content selection and size on one line with a short label and no per-slot description. Narrow windows use two compact lines. Native Save/Cancel and input widgets are retained; other mods are not changed.
- Added **In-game time**, **Real time** and **Weight** to all six HUD slots. In-game solar time follows the game's day/night cycle and sleep transitions; real time shows the computer's local clock, both in **HH:mm**. Weight shows **current / maximum**, rounded up like the native inventory and including the current carry-capacity bonuses. All three use white text with a 2-pixel outline. Existing slot-content defaults remain unchanged; saved `Time` selections remain the in-game clock.
- Added **Map appearance → Map transparency (%)**, **0–100**, default **0%** (opaque). Only the minimap terrain/background fades; icons, border, compass, slot labels and the large map are unchanged. Both presets restore 0%.
- At 1–99% transparency, terrain is rendered separately with the game's shader and composited with UI alpha. At 100%, terrain is hidden without running that pass. At 0%, the original rendering path is retained and the extra resources are released.
- If transparency rendering fails, the background falls back to opaque without disabling the other mod features or changing the saved setting. One warning per failure; no per-frame retries. Save 0%, then the desired value to retry. Partial setup is cleaned up too.
- The seven settings sections now contain **39 entries**. Existing saved settings and local-test slot sizes are preserved; new size entries start at 100%. Plugin ID, DLL/config names and client-only installation are unchanged.
- Updated RU/EN documentation and settings, automated clock/weight/opacity, fallback, six-slot layout and size checks, and the in-game checklist. Automated tests do not verify Unity rendering or input.

### Русский

Изменения относительно **1.1.1**; объединены неопубликованные локальные тестовые сборки.

- Добавлены **Slot 5 и Slot 6** ниже Slot 4: два верхних и четыре нижних слота, новые по умолчанию пустые. Прежние значения сохраняются; пустые строки схлопываются, индикатор зума остаётся ниже последней заполненной строки.
- У каждого слота свой **размер 25–400%, по умолчанию 100%**, включая биом и стрелку ветра. При 100% все тексты имеют штатный размер шрифта биома; координаты больше не уменьшены по умолчанию. Ветер масштабируется от штатного размера; толщина обводок не меняется. Размеры сохраняются при смене содержимого; оба пресета возвращают 100%. Высота строк учитывает метрики текста или диагональ увеличенной стрелки.
- В StoreAndCraft содержимое слота и размер объединены в одну компактную строку: короткая подпись, без описания под каждым слотом. На узком окне — две компактные строки. Штатные элементы управления и Save/Cancel сохранены; другие моды не затронуты.
- Во все шесть слотов добавлены **«Игровое время»**, **«Реальное время»** и **«Вес»**. Игровые солнечные часы следуют за циклом дня/ночи и переходом ко сну/утру; реальные показывают местное время компьютера, оба варианта — **HH:mm**. Вес — **текущий / максимальный**, с округлением вверх как в инвентаре и учётом действующих бонусов к переносимому весу. Белый текст с обводкой 2 пикселя. Содержимое прежних слотов по умолчанию не изменено; сохранённое значение `Time` остаётся игровыми часами.
- Добавлено **«Вид карты → Прозрачность карты»**, **0–100%**, по умолчанию **0%** (непрозрачная). Меняется только фон мини-карты; значки, рамка, компас, подписи слотов и большая карта сохраняют свой вид. Оба пресета возвращают 0%.
- При прозрачности 1–99% фон отрисовывается штатным шейдером в отдельную текстуру, затем накладывается с нужной прозрачностью. При 100% фон скрыт без этого прохода отрисовки. При 0% используется прежняя отрисовка, дополнительные ресурсы освобождаются.
- При ошибке отрисовки прозрачности возвращается непрозрачный фон, остальные функции продолжают работать, сохранённая настройка не меняется. Одно предупреждение при сбое, без повторных попыток каждый кадр. Для повтора сохраните 0%, затем нужное значение. Частично созданные ресурсы также освобождаются.
- В семи разделах теперь **39 настроек**. Сохранённые значения и размеры слотов из локальных тестовых сборок сохраняются; новые настройки размера получают 100%. ID плагина, имена DLL/конфига и установка только на клиент не изменены.
- Обновлены RU/EN-документация и настройки, автотесты часов/веса/прозрачности, обработки сбоев, раскладки шести слотов и размеров, а также ручной checklist. Автотесты не проверяют отрисовку и ввод Unity.

</details>

## 1.1.1

Changes since 1.1.0; consolidates the unpublished local test builds.

- Added **Transport → Show transport**, default On: carts, rafts and ships on both maps, including parked vehicles and modded vehicles using standard Ship/Vagon components. Only nearby client-loaded objects are shown; no saved pins, additional network messages or world-wide tracking. Completely custom vehicle systems require separate support.
- Added **Transport icon size**, range **5–20**, default **12**, on both maps. Player icon sizes are unchanged; edge clearance accounts for the selected size. The controlled ship has no duplicate native marker; disabling the mod restores it.
- Player icons now render above all other map icons and labels, with the local arrow highest. Clipping and pointer pass-through are preserved.
- Fixed terrain pixels protruding beyond the circular brown border: a 256-segment geometric stencil shares the frame contour. Border color/thickness are unchanged, with no extra black outer band.
- Fixed missing/stale coordinate outline copies caused by the shipped TextMeshPro numeric text buffer. Coordinates retain font size **9** and a **3-pixel outline**, without a background rectangle.
- Regrouped all **30 settings into seven sections**: General, Map appearance, Zoom and controls, Around the map, Compass and navigation, Players, Transport. Both reset buttons are under General; slot-order and ping help were added.
- Existing settings migrate automatically; saved values in the new sections take precedence. Renamed the keyboard-zoom toggle to **Zoom with extra keys**, preserving its historical config key and behavior.
- Updated English/Russian documentation, automated regression checks and the manual testing checklist.

Изменения относительно 1.1.0; объединены неопубликованные локальные тестовые сборки.

- Добавлен **«Транспорт → Показывать транспорт»**, по умолчанию включён: тележки, плоты и корабли на обеих картах, включая припаркованные и модифицированные на компонентах Ship/Vagon. Только загруженные клиентом объекты рядом с игроком, без постоянных меток, дополнительных сетевых сообщений и поиска по всему миру. Полностью нестандартный транспорт требует отдельной поддержки.
- **Размер иконок транспорта — 5–20, по умолчанию 12**. Иконки игроков не меняются; отступ от края учитывает выбранный размер. Метка управляемого корабля не дублируется и восстанавливается при выключении мода.
- Иконки игроков теперь поверх остальных значков и надписей на обеих картах; своя стрелка выше всех. Обрезка и пропуск кликов сохранены.
- Исправлено выступание пикселей карты за круглую коричневую рамку: маска и рамка используют согласованную геометрию. Цвет и толщина сохранены, чёрный внешний слой не добавлен.
- Исправлена неполная обводка координат из-за числового буфера TextMeshPro. Шрифт **9**, обводка **3 пикселя**, без прямоугольной подложки.
- **30 настроек сгруппированы в семь разделов**: Общее, Вид карты, Масштаб и управление, Элементы вокруг карты, Компас и навигация, Игроки, Транспорт. Кнопки сброса в «Общем»; добавлены схема слотов и подсказка про пинги.
- Сохранённые значения автоматически переносятся; значения в новых разделах имеют приоритет. Переключатель переименован в **«Зум через доп. клавиши»**, старый ключ конфига и поведение сохранены.
- Обновлены RU/EN-документация, автотесты и список ручных проверок.

## 1.1.0

Changes since 1.0.1; consolidates the unpublished local test builds.

- Four configurable HUD slots replace separate biome/wind positions. Defaults: Empty / Biome / Wind / Empty. Centered wind, compact spacing, collapsed empty rows and vertical viewport fitting. Native biome font and wind size are retained; biome labels no longer disappear due to text truncation or animated transparency.
- X/Y/Z coordinates as slot content, without a separate switch: font size 9 and a black 3-pixel outline.
- General → Show minimap (default On) hides the small HUD while keeping the large M map accessible. Disabling the mod restores native visibility and layout.
- Session-only waypoints: Shift+left-click anywhere on the large map to set/move; Shift+right-click to clear without deleting pins. Gold markers, a minimap edge arrow, horizontal distance and selection confirmation with rounded distance.
- Clipped/distant player ping rings become blue directional edge arrows. Fully visible pings and native ping lifetimes are preserved.
- Other players' nicknames close below their minimap icons; no local-player name. Players settings: visibility, font size 4–20 (default 8), optional 2-pixel outline. Only public-position players are labeled; names clip to the map and do not change large-map labels.
- One thin brown border for circular and square maps, without a dark outer band.
- Updated RU/EN settings, presets and old HUD configuration migration. Expanded automated geometry/UI checks and manual testing checklist.
- Replaced the old screenshot gallery with one current reference image; documented the changes in the English and Russian README sections.

Изменения относительно 1.0.1; объединены неопубликованные локальные тестовые сборки.

- Четыре слота вместо отдельных положений биома/ветра: Пусто / Биом / Ветер / Пусто. Ветер по центру, компактные отступы, пустые строки без зазоров, подгонка блока по высоте экрана. Штатные размеры биома и ветра сохранены; исправлено исчезновение биома.
- Координаты X/Y/Z выбираются в слоте: шрифт 9, чёрная обводка 3 пикселя, без отдельного переключателя.
- «Показывать мини-карту» в разделе «Общее» скрывает малую карту, не блокируя M. По умолчанию включено.
- Цель на текущий сеанс: Shift+ЛКМ задаёт/переносит, Shift+ПКМ снимает без удаления меток. Золотые маркеры, стрелка у края, горизонтальное расстояние и подтверждение выбора с расстоянием.
- Не помещающиеся пинги игроков заменяются синими стрелками у края; ближние кольца и штатное время жизни сохранены.
- Никнеймы других игроков близко под иконками; своё имя скрыто. Показ, размер 4–20 (по умолчанию 8) и обводка 2 пикселя настраиваются в разделе «Игроки».
- Одна тонкая коричневая рамка без чёрного внешнего слоя для обеих форм карты.
- Обновлены RU/EN-настройки, пресеты, миграция старого HUD и проверки. В README добавлен список изменений и оставлен один актуальный скриншот.

## 1.0.1

- Added circular, square and ship-zoom screenshots to the README.
- Highlighted English and Russian UI support.
- No gameplay changes.

Добавлены скриншоты круглой, квадратной карты и корабельного зума в README.
Отдельно отмечена поддержка русского и английского интерфейса.
Игровая логика не изменена.

## 1.0.0

First public release.

- Camera-up, character-up and north-up minimap orientation.
- Circular/square shapes, adjustable size and status-effect spacing.
- Smooth zoom, configurable shortcuts and zoom levels anchored at ×1.
- Separate land/ship zoom saved between game sessions.
- Compass letters and configurable line color, thickness and style.
- Biome/wind positions with external outlines.
- Time-of-day, always-day and always-night minimap lighting.
- English/Russian integration with StoreAndCraft's settings window.
- Master enable switch and two reset presets that preserve its state.

---

Первый публичный релиз.

- Вращение мини-карты по камере/персонажу или север сверху.
- Круглая/квадратная форма, размер карты и сдвиг бафов.
- Плавный зум, свои клавиши и ступени с опорой на ×1.
- Отдельные масштабы суши/корабля с сохранением между сеансами.
- Компас, цвет/толщина/тип линий, положения биома/ветра и внешние обводки.
- Освещение по времени суток, постоянный день или ночь.
- Настройки RU/EN в окне StoreAndCraft, общий выключатель и два пресета.
