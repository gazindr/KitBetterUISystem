# Changelog

## 0.1.12 - 2026-08-18

- Fix CS0117: add missing `UIPresetOverrideDrawer.DrawTogglePresetsTab`.
- Full preset + per-field overrides for `UIToggle` (Preset *, Save, `+`, orange overrides, RMB Apply/Revert), matching `UIButton`.

## 0.1.11 - 2026-08-16

- `UIBackground` default attach mode is now `Behind Container`: spawn as a sibling immediately before the container so Scale/Move on the panel does not scale the dimmer. `Inside Container` keeps the old child behavior.

## 0.1.10 - 2026-07-29

- Fix button / selectable position snap-back: return-to-start only when the previous state actually animated that property (manual RectTransform edits no longer get snapped to a cached start pose).
- Recapture start values on enable; use `GetInstanceID()` for the start-value cache.
- Fix `ShowIsolated` restore: after Hide, Show could animate scale/fade to `CurrentValue` (already 0) so UI stayed invisible. `ShowRoutine` now reapplies captured start values first.
- Isolation: suppress with `InstantHide`, restore with `InstantShow`; switching isolation restores the previous set; `Remove(isolated)` restores instead of dropping the list; `ending` guarded with try/finally.
- Style presets (`UIButton` / Toggle / Tab / Slider) **never overwrite instance behaviours**. Use `UIBehaviourPreset` to copy behaviours intentionally.
- `UIButton.SaveAllToPreset` no longer pushes behaviours into the button preset asset.
- Unity 6: `Reset` / `OnValidate` are editor-only (no longer override player-only missing base methods).
- `UISFX` uses an explicit `SetHandler` bridge (SFXManager registers in Awake) instead of reflection.

## 0.1.9 - 2026-07-29

- Inspector: quick-click buttons (LMB / MMB / RMB) above the behaviour trigger dropdown for faster Pointer Left/Middle/Right Click setup; those triggers are removed from the dropdown.

## 0.1.8 - 2026-07-25

- Inspector RMB on UIContainer/UIButton fields: **Apply to Preset** / **Revert to Preset** via Unity `contextualPropertyMenu` (Unity's own PropertyField menu was swallowing the old ContextClick handler).
- Clearer orange override wash + left bar on overridden fields.

## 0.1.7 - 2026-07-25

- Full preset + per-field overrides for `UIContainer` / `UIButton`: `overriddenPaths`, orange tint in inspector, RMB Apply to Preset / Revert to Preset.
- Presets tab: Preset + dirty `*`, Save, `+` (no Apply Mask). Animation-only preset bar removed from Animations tab.
- Assigning a preset does a full apply and clears overrides; editing a field marks an override; changing the preset asset syncs non-overridden fields to instances.
- `UISystemDefaults` + Resources defaults (`Default-UIContainerPreset`, `Default-UIButtonPreset`) applied in `Reset()`.
- UnityEvents are not auto-copied from presets (scene bindings stay intact).

## 0.1.6 - 2026-07-25

- Inspector: wider labels (no clipping), Values From/To stacked to prevent overflow.
- Move `Direction` mode with arrow grid: Show = where it comes from, Hide = where it goes to.
- Default UIContainer animations: Scale on (0↔current); Move/Fade templates ready when enabled (Top).
- Default UIButton animations: Highlight scale 1.1, Pressed 0.9, Disabled fade 0.8, Normal/Selected fade 1.
- Animation preset bar for UIContainer/UIButton: select preset, dirty `*`, Save, and `+` to create.

## 0.1.5 - 2026-07-25

- `UIContainer` backgrounds spawn as the first child of the container (`SetAsFirstSibling`) so they render behind content instead of covering it as a sibling.

## 0.1.4 - 2026-07-25

- Soft UI SFX via `SFXManager` (`UIClickSFX`, `UIContainerShowSFX`, `UIContainerHideSFX`) on a dedicated UI AudioSource; missing manager never throws.
- `UIButton` / `UIContainer`: `muteUISound` and optional custom override clips.
- `Show(bool showCursor)` / `ShowIsolated(bool showCursor)` (+ id overloads): unlock/lock cursor through soft `CursorLocker` bridge; plain `Show()` does not touch the cursor.

## 0.1.3 - 2026-07-25

- `UIContainer.ShowIsolated()` / `ShowIsolated(id)`: shows one container, hides all other open containers, blocks their Show until the isolated container hides, then restores the suppressed containers.
- Debug inspector: `Show Isolated` button.

## 0.1.2 - 2026-07-24

- `UIContainer.Show()`: ignore repeat Show while Visible/Showing (no re-animation) unless `useInQueue` is on.
- `Use In Queue`: same container can be enqueued multiple times; each Show becomes a separate queue entry until the queue empties.
- Inspector Animations: color indicators on state tabs and Move/Rotate/Scale/Fade (lime / orange / crimson / purple) when enabled.
- Inspector spacing increased to match reference layouts.

## 0.1.1 - 2026-07-24

- `UIBehaviourBlock`: optional `keyboardKey` — behaviour also fires on `Input.GetKeyDown` in Play Mode.
- Inspector shows Keyboard Key under each Behaviour Block; tab label includes the key when set.
- `UIContainer`: `isVisible` / `isShowing` / `isHiding` / `isHidden`, `deactivateOnHidden`.
- `UIToggle`: `OnValueChangedCallback` alias for Doozy migration.

## 0.1.0 - 2026-07-01

- Initial UPM package export of UI System.
- Includes runtime components, editor inspectors, animation icons, presets, behaviours, queue managers, and a test scene sample.
- Requires Odin Inspector to be installed in the consuming Unity project.
