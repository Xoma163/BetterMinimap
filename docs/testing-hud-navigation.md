# HUD slots and navigation — manual verification

Automated math tests and a successful DLL build do **not** verify Unity rendering/input. Run this checklist in an installed client before release. No server installation is needed.

## Configuration

- General → Show minimap is On by default. Save Off: map, slots, wind, border, compass, zoom popup and small-map overlays are all hidden; buffs return to their native position. M must still open/close the large map, including waypoint selection/clearing. Save On: all small-map elements restore with no alpha or input issues.
- With Show minimap Off, test death/respawn, HUD hide/show, entering a new world, and master mod Off/On. Master-off must restore the native map. Both reset presets return Show minimap to On after Save; cancelling settings must not affect visibility.
- Fresh config: slots are Empty / Biome / Wind / Empty; no coordinate switch; target indicator on with no selected target.
- StoreAndCraft: change each slot, Save, reopen. Close/Back without Save must discard changes. Confirm Russian and English names, enum choices and descriptions.
- Set all slots to Empty; then to Biome, Wind and Coordinates in turn. Repeated contents must work; no stale outlines or native duplicate biome/wind visuals.
- Selecting Coordinates in any slot immediately shows X/Y/Z; selecting Empty hides them. Verify the 9-unit font and visible 3-pixel outline, all three values including negative coordinates/zero crossings, and Y changing when jumping/climbing. Long coordinate strings must fit without losing Z. Biome and player-name outlines must remain unchanged at 2 pixels.
- Upgrade configs: Above+Bottom stays biome 2/wind 3; Below+Bottom becomes biome 3/wind 4; top corners migrate above, bottom corners below. Already saved slot entries win. Restart and verify persistence.
- Upgrade from test 1.1.0: coordinate slots with the old switch off become Empty; with it on, they stay Coordinates. The old switch disappears from the UI and file. Choose Coordinates after migrating and restart: it must remain visible without a switch.
- Reset presets only apply after Save and preserve the master switch. Vanilla-like preset disables coordinates/target; slots remain in default arrangement. Master-off restores the actual native layout.

## Rendering and lifecycle

- Circle/square; camera/character/north-up; rotation disabled; sizes 50/100/130/150%; different HUD scales/resolutions/window aspect ratios.
- Brown border is a single thin band, with no dark outer layer. It follows both shapes without filling the map, survives repeated shape changes, and disappears on master-off and on the large map. No circular stencil clipping of its outer edge.
- Fill both top slots; ensure the block shifts down enough to keep them visible and restores its position when cleared or disabled. Rotate wind through all directions: adjacent slots must not overlap.
- Verify compact row spacing with all four slots filled, including coordinates in 1/4. Empty inner slots must not reserve a blank row. Wind must match its original native size, not the shrunk early-prototype size; it must not touch the border or coordinates.
- Biome text must be visible in every slot, in both languages, at all supported map/HUD sizes. Remain in a biome, cross biome boundaries, wait for the native pulse animation to finish, and reopen M: the persistent slot label must not disappear. Its font size remains native, while coordinates remain small.
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

- Sky, snow, water, dark forest, day/night: coordinates have NO background rectangle. Same size-9 white font and complete black 3-pixel outline in all eight directions, not just a single offset shadow. Move across positive/negative coordinates and stand still: copies must stay current and not disappear on repeated text reads.
- Coordinates in each slot, multiple coordinate slots, HUD/map sizes 50–150%: original compact row height, no wind/biome/zoom-label overlap. Change Coordinates → Biome/Wind/Empty → Coordinates without moving: correct text and outline, no stale biome string or ghost text. Reopen M and toggle mod/minimap.
- Biome, wind, names, border and icon layering remain unchanged.
- 7. Transport → Transport icon size / «Размер иконок транспорта»: default 12 in new/upgraded profiles, bounds 5–20. Test 5, 12 and 20 on both maps. Save applies immediately; Close cancels; restart retains choice; reset restores 12. Silhouettes/outlines scale together. Icons at size 20 do not protrude at round/square map edges.
- Player icons remain the same size and render on top at every transport size, including after repeated map/shape changes. Toggling transport Off/On retains its chosen size.

## Settings regrouping (1.1.1)

- Exactly seven RU/EN sections in the README order, all 30 settings present once, no old headers or duplicate rows. General contains two switches then both reset buttons before Map appearance. Zoom shortcuts follow zoom options; ship zoom stays there, not under Transport.
- Around the map shows the slot-order diagram. Compass and navigation shows informational ping help without an extra toggle; target description still explains Shift+left/right-click.
- Upgrade copied 1.0.x/1.1.0 and local-test configs with non-default switches, shortcuts, slots, colors, zoom limits, name font size and transport size. All values carry over; old bound section keys are removed. Master Off and minimap Off must remain Off. Test fresh config separately: all previous defaults unchanged.
- Restart with a migrated config: values persist. If both old and new sections contain a key, the saved new-section value wins. Test a partially migrated config as well, including slots set to Coordinates and Empty.
- Save/Close/Back still apply or discard correctly. Reset buttons stay under General after a preset rebuild, without duplicates; presets preserve the unsaved master switch. Fractional zoom/line-width sliders still step by 0.1, integer icon/name sizes remain integers.
- Open another mod's settings page and return: no leaked headers, hints, row ordering, preset placement or localization changes. Regrouping alone must not affect in-game rendering or controls.
