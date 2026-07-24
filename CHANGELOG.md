# Changelog

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
