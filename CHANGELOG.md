# Changelog

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
