# AGENTS.md

This is a Unity 2D Vampire Survivors-like game.

## Main Goal

Finish a small playable vertical slice, not a full commercial game.

## Priority

1. Keep the game simple and playable.
2. Do not rewrite the whole project.
3. Reuse existing scripts, prefabs, scenes, UI, audio, weapons, tilemap, and character work where possible.
4. Keep `Assets/Scenes/Game.unity` as the main MVP scene.
5. Player A is the required MVP playable character.
6. Prefer small safe fixes over large architecture changes.
7. Avoid adding paid assets or unnecessary external packages.
8. Every change should keep the Unity project compiling.

## MVP Definition

The MVP is finished when:

- `Assets/Scenes/Game.unity` enters Play Mode without C# compile errors.
- Player A can move.
- Enemies spawn and chase the player.
- The starting weapon attacks automatically.
- Enemies take damage and die.
- Enemies drop XP pickups.
- Player can collect XP pickups.
- XP fills the XP bar.
- Level-up opens the upgrade choice panel.
- At least one selected upgrade visibly changes gameplay.
- HUD shows health, XP, timer, and level.
- Player death shows game over.
- Restart reloads a playable scene.
- A 1–3 minute smoke test has no repeating runtime exceptions.

## Scope Limits

Do not prioritize:
- extra characters
- extra weapons
- perfect menu flow
- new art
- new audio
- new maps
- boss enemies
- complex balancing

These can stay if they already exist, but they are not required for the MVP.