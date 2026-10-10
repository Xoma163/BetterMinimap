# Changelog

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
