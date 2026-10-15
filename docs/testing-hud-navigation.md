# HUD slots and navigation — manual verification

Automated math tests and a successful DLL build do **not** verify Unity rendering/input. Run this checklist in an installed client before release. No server installation is needed.

Current target: **1.2.1**, a documentation-only update with the same behavior as **1.2.0**. The checks below cover the features introduced since **1.1.1**. Test a fresh profile, an upgrade from public 1.1.1/1.2.0 and an upgrade from the local 1.1.2 test builds; use copied configs, not the only copy of a live profile. The steps below are a checklist, not a record of completed in-game tests.

Font-size references below assume each slot's size is 100%, unless specified otherwise. All text contents now share the native biome font size at 100%; wind uses its native size.

## Configuration

- General → Show minimap is On by default. Save Off: map, slots, wind, border, compass, zoom popup and small-map overlays are all hidden; buffs return to their native position. M must still open/close the large map, including waypoint selection/clearing. Save On: all small-map elements restore with no alpha or input issues.
- With Show minimap Off, test death/respawn, HUD hide/show, entering a new world, and master mod Off/On. Master-off must restore the native map. Both reset presets return Show minimap to On after Save; cancelling settings must not affect visibility.
- Fresh config: slots are Empty / Biome / Wind / Empty / Empty / Empty; no coordinate switch; target indicator on with no selected target.
- StoreAndCraft: change each slot, Save, reopen. Close/Back without Save must discard changes. Confirm Russian and English names, enum choices and descriptions.
- Set all slots to Empty; then to Biome, Wind, Coordinates, In-game time, Weight and Real time in turn. Repeated contents must work; no stale outlines or native duplicate biome/wind visuals.
- Selecting Coordinates in any slot immediately shows X/Y/Z; selecting Empty hides them. Verify the same base font size as biome and a visible 3-pixel outline, all three values including negative coordinates/zero crossings, and Y changing when jumping/climbing. Check long coordinate strings; reduce the slot percentage if they exceed the width. Biome and player-name outlines must remain unchanged at 2 pixels.
- Upgrade configs: Above+Bottom stays biome 2/wind 3; Below+Bottom becomes biome 3/wind 4; top corners migrate above, bottom corners below. Already saved slot entries win. Restart and verify persistence.
- Upgrade from test 1.1.0: coordinate slots with the old switch off become Empty; with it on, they stay Coordinates. The old switch disappears from the UI and file. Choose Coordinates after migrating and restart: it must remain visible without a switch.
- Reset presets only apply after Save and preserve the master switch. Vanilla-like preset disables coordinates/target; slots remain in default arrangement. Master-off restores the actual native layout.

## Rendering and lifecycle

- Circle/square; camera/character/north-up; rotation disabled; sizes 50/100/130/150%; different HUD scales/resolutions/window aspect ratios.
- Brown border is a single thin band, with no dark outer layer. It follows both shapes without filling the map, survives repeated shape changes, and disappears on master-off and on the large map. No circular stencil clipping of its outer edge.
- Fill both top slots; ensure the block shifts down enough to keep them visible and restores its position when cleared or disabled. Rotate wind through all directions: adjacent slots must not overlap.
- Verify compact row spacing with all six slots filled, including coordinates in 1/6. Empty inner slots must not reserve a blank row. Wind must match its original native size, not the shrunk early-prototype size; it must not touch the border or coordinates.
- Biome text must be visible in every slot, in both languages, at all supported map/HUD sizes. Remain in a biome, cross biome boundaries, wait for the native pulse animation to finish, and reopen M: the persistent slot label must not disappear. At 100%, its font size and the coordinate font size are equal to the native biome's.
- On land and aboard a ship, change zoom repeatedly: popup below the last visible lower slot; no collision with coordinates/wind.
- Open/close M repeatedly, hide/show the HUD, die/respawn, toggle mod off/on, leave/re-enter a world. No leftover overlays, source graphics permanently hidden, cumulative vertical movement or broken buffs.
- Compass, map lighting, markers and smooth zoom still work. Large-map biome, wind and native pin names retain their original behavior.

## Target selection

- Shift+left-click any point on M, including empty/fogged areas: selection feedback and gold marker on both maps, plus minimap distance. Clicking on a saved pin must not change its checked state or ownership.
- Confirmation includes a rounded integer distance, e.g. «Цель выбрана (350 м)» / “Target selected (350 m)”, including 0 m when selecting the player's position.
- Shift+left-click another point: move the goal. Repeating at the same point does not clear it. Shift+right-click anywhere on the map: clear; repeat over a saved pin and confirm it was not deleted. Hold Shift and double-click: never create/rename a pin or send a ping.
- Ordinary click still checks a pin; ordinary double-click still creates one; ordinary drag still pans. A long Shift+drag must not select a target or create a pin.
- Test LeftShift and RightShift with both mouse buttons. Do not select a target while editing a pin name, entering text, or while the feature/mod is disabled. Ordinary right-click must still delete native pins when not using waypoint gestures.
- Place targets north/east/south/west and diagonally, inside/outside both shapes. Camera/character rotation, zoom and resize must preserve correct direction; text stays upright and inside the map.
- Compare distance to known horizontal X/Z differences; elevation should not change it. Stand at the goal: 0 m without invalid coordinates or spinning geometry.
- Remove a native pin under the goal: the independent waypoint remains. Disable the feature/mod or exit/re-enter world: indicator disappears; no selection is written to config, world saves or shared with peers.

## Player pings

- Have another ordinary player ping nearby and far away in each direction; also test your own pings. Fully visible pings use the original ring; clipped/far pings use a blue directional arrow at the correct edge, of the same shape as the gold goal arrow. Its tip must point outward, with upright rings remaining unchanged nearby.
- Place a ping near the edge with its center still inside: when its ring would clip, it must switch to an arrow. The native pulse animation must not cause repeated ring/arrow flickering.
- Test circle and square, all orientation modes, zooming, and moving so that a ping enters/exits the map. In particular, test square corners that lie outside the circle: no clipped duplicate native ping should remain visible.
- Several simultaneous pings: each gets its own edge icon. Wait for expiration: no lingering icon, no extended lifetime and no extra network ping messages.
- Toggle mod off/on, switch map shape, open/close M, hide/show HUD, die/respawn: native icons restore and edge overlays do not leak onto the large map or respawn screen.

## Player names

- In 6. Players, verify Show player names / Player name font size / Player name outline (RU and EN), defaults On / 8 / On, bounds 4–20. Save applies; Close without Save discards. Vanilla-like preset disables the labels. On upgrade, existing 12 remains 12; out-of-range 24 clamps to 20; selecting 8 and restarting must retain 8.
- Solo: no nickname under the local icon. With other players sharing position, each visible remote icon has its own name. Your own name stays hidden, including when toggling public position. Players not sharing position must not acquire icons or labels.
- Font sizes 4, 8 and 20; outline Off/On while stationary and moving. Enabled outline is 2 pixels. Other HUD text and wind size must not change. No black ghost text remains when outline or names are disabled.
- Circle/square and camera/character/north-up: names stay horizontal, centered close beneath the visible player sprite. Text must not overlap the visible head/body. Text and outline must both clip at the map boundary.
- Long/Cyrillic names and literal markup-like characters; no unintended rich-text styling or vertical truncation. Move icons off-map/back, toggle public position, join/leave, die/respawn: no stale labels or swapped names.
- Open/close M and hide/show minimap or mod: no labels leak onto the large map and native large-map names stay unchanged.

## Transport (1.1.1)

- Settings → 7. Transport → Show transport / «Показывать транспорт»: default On in new/upgraded profiles. Save applies; Close/Back cancels. Mod-default preset enables; vanilla-like preset disables. Restart retains the choice.
- Park a cart, raft, karve, longship and drakkar nearby. Each must have exactly one icon on small and large maps without requiring anyone aboard. Pull the cart / move a ship: its icon follows the actual vehicle position, not the player's position.
- Check compatible modded Ship/Vagon vehicles without relying on prefab names. No placement-ghost icons. Fully custom vehicle components are explicitly unsupported rather than promised as working.
- Embark as passenger, then take/release the rudder: no duplicate controlled-ship marker. Setting Off hides all vehicle icons on both maps; master mod Off restores the native controlled-ship marker. Repeat in both map modes and rotation on/off.
- Circle/square, camera/character/north-up, HUD size 50–150%, pan/zoom the large map: icons stay upright at the correct positions and never appear outside the map. Vehicle icons are not edge arrows.
- Hide minimap: no small-map vehicles, but M still shows loaded vehicles. Close/open M repeatedly and die/respawn: no stuck icons or native-marker suppression after disabling the mod.
- Destroy a vehicle, leave its loading range, teleport or change worlds: its icon disappears. Return to a parked loaded vehicle: icon reappears. Distant unloaded vehicles are not tracked.
- No map pins saved or synchronized. Mouse clicks pass through these overlay icons, preserving normal pin actions and waypoint selection/clearing.

## Player icon priority and circle edge (1.1.1)

- Stand on a cart/boat, bed/death pin, target marker/distance, ping and another player's nickname: player icons must remain on top of every icon/text layer on both maps. Local arrow stays above remote player icons at the same position. Disabling nickname display must not affect priority.
- Player icons still clip at the circular/square/large-map boundary. Toggle icon-type visibility, public position, minimap visibility and HUD visibility: promotion must not reveal hidden icons.
- Switch small/large maps, circle/square, rotation modes and size 50–150% repeatedly. Check no position/scale drift, lost icons, frozen headings, flashing names or per-frame native marker recreation. Move the camera while stationary and pan/zoom M.
- Toggle mod off/on, die/respawn, teleport and change worlds: native parents, layouts and raycast states restore; disabling the mod leaves no overlay root, missing local arrow or detached remote icons.
- Ordinary map clicks/pin deletion/name editing and Shift+click waypoints work through player icons. No overlay intercepts pointer input.
- Inspect bright terrain at the circular border at all zoom/HUD sizes (especially the supplied screenshot's diagonal edges). No map-colored fringe outside the brown band or gaps between map and band; same thin brown frame with no added dark outer ring. Repeat with moving camera, sea and dark terrain.

## Coordinate readability and transport size (1.1.1)

- Sky, snow, water, dark forest, day/night: coordinates have NO background rectangle. White text and complete black 3-pixel outline in all eight directions, not just a single offset shadow. Move across positive/negative coordinates and stand still: copies must stay current and not disappear on repeated text reads, including after slot-size changes.
- Coordinates in each slot, multiple coordinate slots, HUD/map sizes 50–150%: row height follows the selected font size, no wind/biome/zoom-label overlap. Change Coordinates → Biome/Wind/Empty → Coordinates without moving: correct text and outline, no stale biome string or ghost text. Reopen M and toggle mod/minimap.
- At 100%, biome and wind retain their native sizes; names, border and icon layering remain unchanged at every slot size.
- 7. Transport → Transport icon size / «Размер иконок транспорта»: default 12 in new/upgraded profiles, bounds 5–20. Test 5, 12 and 20 on both maps. Save applies immediately; Close cancels; restart retains choice; reset restores 12. Silhouettes/outlines scale together. Icons at size 20 do not protrude at round/square map edges.
- Player icons remain the same size and render on top at every transport size, including after repeated map/shape changes. Toggling transport Off/On retains its chosen size.

## Settings layout and upgrade (1.2.0)

- Exactly seven RU/EN sections in the README order, all 39 settings (30 from 1.1.1 plus transparency, Slot 5/6 and six text-size entries) present once, no old headers or duplicate rows. General contains two switches then both reset buttons before Map appearance. Zoom shortcuts follow zoom options; ship zoom stays there, not under Transport.
- Around the map shows the slot-order diagram. Compass and navigation shows informational ping help without an extra toggle; target description still explains Shift+left/right-click.
- Upgrade copied 1.0.x/1.1.0 and local-test configs with non-default switches, shortcuts, slots, colors, zoom limits, name font size and transport size. All values carry over; old bound section keys are removed. Master Off and minimap Off must remain Off. Test fresh config separately: all previous defaults unchanged.
- Restart with a migrated config: values persist. If both old and new sections contain a key, the saved new-section value wins. Test a partially migrated config as well, including slots set to Coordinates and Empty.
- Save/Close/Back still apply or discard correctly. Reset buttons stay under General after a preset rebuild, without duplicates; presets preserve the unsaved master switch. Fractional zoom/line-width sliders still step by 0.1, integer icon/name sizes remain integers.
- Open another mod's settings page and return: no leaked headers, hints, row ordering, preset placement or localization changes. Regrouping alone must not affect in-game rendering or controls.

## In-game time (1.2.0)

- Select In-game time / «Игровое время» in each slot; Save applies and Close/Back cancels. Restart retains Time in config (the stable value is unchanged); a new/upgraded config keeps the old slot defaults and empty Slot 5/6. Both resets restore Empty / Biome / Wind / Empty / Empty / Empty, including when all six slots previously contained Time.
- White 24-hour HH:mm text, same base font size as biome, 2-pixel black outline, no background. Repeat in all slots simultaneously; switch Time → Coordinates/Biome/Wind/Empty → Time within the same minute and while stationary: no stale text/outline; each content follows the saved slot percentage.
- Follow sunrise (~06:00), noon (12:00), sunset (~18:00), midnight (23:59 → 00:00). No 24:00 or 60-minute field. Clock tracks native solar time, so day/night minutes run at different real-time rates; it is not wall-clock or session time. Sleep follows the game's smoothed transition to morning. Test solo pause/resume and server join.
- Change minimap lighting between time-of-day/day/night: actual game time must not change. Open/close M, hide/show HUD/minimap, mod Off/On, death/respawn, leave/rejoin or change world: no orphan labels or previous-world time.
- Circle/square, every rotation mode, map sizes 50–150%, HUD scales: no overlap with adjacent biome/wind/coordinate rows or the zoom popup. Empty inner slots collapse as before.

## Terrain transparency (1.2.0)

- Map appearance → Map transparency (%) / «Вид карты → Прозрачность карты»: integer 0–100, default 0 in fresh/upgraded configs. Save applies; Close/Back cancels; restart retains nonzero values. Both presets stage 0 and retain the master switch; no duplicate settings.
- Compare 0/25/50/75/100% in circle AND square: 0 uses the unchanged native terrain; 100 shows the scene behind the map, with no opaque/dark rectangular backing left. Player/ship/cart icons, pins, target, pings, compass, border and slot text retain their own opacity. Large M remains unchanged.
- At intermediate transparency, check native explored terrain, fog/unexplored areas, sea, mountains, animated clouds and all map-lighting modes. No upside-down/mirrored image, double gamma, black fill, seams, wrong clipping or extra black outline. Native terrain textures, UV center, zoom and rotation must agree with player/pin positions.
- Change circle ↔ square, all rotation modes (including rotation Off), map/HUD size, resolution/fullscreen mode and zoom while moving. Render surface resizes/recreates as needed; no blur beyond normal resolution, marker drift, frame hiding or texture loss.
- Toggle 0 ↔ 50 ↔ 100 repeatedly; open/close M, HUD/minimap Off/On, master Off/On, death/respawn, world change. Default 0 releases the extra render texture/material; master Off restores native graphic visibility/backdrop color. No resource accumulation or stale terrain. Check FPS with transparency enabled, especially at high resolution.
- Test map clicks, zoom keys and waypoint/pin actions on M: the terrain overlay must not intercept input. If transparency rendering fails, the background returns to opaque and all other features keep working; the saved transparency value is unchanged. Exactly one warning, no repeated allocations/log spam each frame. Save 0%, then the desired value: rendering can retry. Repeating an actual failure produces one new warning. Check resource cleanup and hidden/minimap/M state after failure; test with a debugger/instrumented local build if a real GPU failure is impractical to trigger.

## Carry weight and real time (1.2.0)

- All six slots offer Weight / «Вес» and Real time / «Реальное время», separately from In-game time / «Игровое время». Test Save, cancellation, restart and both reset presets. Readout choices add no extra settings; with the six text-size entries there are 39 settings total. Existing Time/Coordinates selections survive the upgrade.
- Weight shows `current / maximum` in white text (same base font size as biome) with a 2-pixel outline, without a background. Compare with the native inventory: both values round up. Pick up/drop/split items, equip/unequip Megingjord, change carry-capacity effects, and exceed capacity: both numbers update, overload is not clamped. Repeat with standard-API weight mods and the game's carry-weight modifier; do not assume support for entirely separate custom inventories.
- Real time matches the computer's local 24-hour HH:mm clock, with leading zeros and a colon in RU/EN. Minute rollover and 23:59 → 00:00 work; pausing/sleeping/changing game time must not accelerate the real clock. On resuming, it catches up to the computer clock. Check after OS time/timezone corrections if practical.
- Put both clocks and weight beside each other, then repeat each in all six slots. Repeated readouts agree. Switch Weight/Real time → Biome/Wind/Coordinates/Empty → Weight/Real time without changing the displayed values: no stale label or outline. Test very large modded capacity values at the smallest HUD/map size.
- Repeat both map shapes, all rotations, sizes/HUD scales, M ↔ small, hidden minimap, master Off/On, death/respawn and world changes. No overlap with wind/biome/zoom, orphan labels or previous-player weights. Transparency only affects terrain, never these readouts.

## Slot 5 / Slot 6 (1.2.0)

- Confirm order: 1 → 2 → map → 3 → 4 → 5 → 6. Slot 5/6 default Empty in a new/upgraded profile and expose all seven choices. StoreAndCraft row labels/descriptions/order and hints are correct in RU/EN. Save, Close/Back, restart and both presets work.
- Upgrade a copy of a four-slot profile with non-default selections: Slot 1–4 remain unchanged, Slot 5/6 are Empty. Set Weight/RealTime in Slot 5/6, save/restart twice: choices must persist and not be overwritten by legacy migration.
- Fill all four lower rows with different-height content (wind, biome, coordinates, clock), then hide intermediate rows in every combination. Rows remain in order without overlap or blank gaps; only Slot 6 filled sits immediately below the map.
- Zoom popup appears below the last filled lower row, including Slot 5/6, and moves back up when they are cleared. Verify vertical viewport fitting with large map/HUD scales; when the whole block cannot fit, the existing top-priority fallback still applies.
- Hide/show the minimap, open/close M, toggle the mod, respawn/change world: all six slots hide/restore/dispose together. No duplicate roots or stale fifth/sixth rows.

## Individual slot text sizes (1.2.0)

- Upgrade from public 1.1.1: six sizes start at 100%, Slot 1–4 selections remain unchanged, Slot 5/6 are Empty. Coordinates now use the native biome font size, not the former size 9; lowering just their slot's percentage leaves biome/wind in other slots unchanged. Upgrade a local 1.1.2 config with different saved sizes: retain all six percentages, contents and transparency after two restarts.

- Around the map: six compact rows, each combining Slot N, its content selector, short "Размер текста слот N" / "Slot N text size" label, slider and numeric input. No per-slot description. Integer range 25–400, default 100; stable config keys retain existing percentages. Narrow windows use two compact lines. Other sections/mod pages are unaffected.
- Set six different sizes; Save and restart twice. Each size remains paired with its slot; changing content does not reset it. Close/Back cancels edits. Both presets stage all six sizes at 100 while preserving the current unsaved master switch.
- Assign repeated coordinates/clocks/weight/biome/wind to all six slots: changing one slider affects only that row. Check 25/50/100/200/400%; at 100% all texts have the native biome font size, and wind has its native size. Wind scales from its base every frame, never cumulatively; alternate 25 ↔ 400 repeatedly while turning. The row reserves the scaled arrow's full diagonal. Empty slots remain collapsed.
- Change 400%-text → biome/wind → 25%-text, while stationary and moving: no stale font, text, scale or outline. Coordinates retain a complete 3-pixel outline and other readouts 2 pixels; no background appears and names/compass/zoom text do not scale with slot contents.
- Mixed row sizes, both shapes, rotations, HUD/map scales, top/bottom screen fitting and intermediate empty slots: rows and zoom popup do not overlap. Test long coordinates/large modded weights; text intentionally remains one line, so reduce that slot's percentage if it exceeds the width.
- Repeat M, hide/show minimap, master Off/On, respawn and world change with all six different sizes. No lost sizes, overlays on M or changed native biome/wind after disabling the mod.
- Compact settings rows: cycle content with its native button, drag slider, type 25/100/400 and invalid/out-of-range values; native validation and Save/Close must still work. Rebuild both presets repeatedly in the same frame/session, reopen, switch RU/EN and visit another mod: no missing widgets, duplicate rows, values bound to the wrong slot or stale inactive rows.
- Resize settings window/HUD across the one-line threshold (720 UI units): row height switches between 46 and 88 without clipped or overlapping controls. Check keyboard navigation and editing the numeric field, not only mouse dragging.
