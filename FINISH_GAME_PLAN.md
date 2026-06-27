# Finish Game Plan: Vampire Survivors-Like Vertical Slice

## Goal

Turn the current Unity project into a small but complete Vampire Survivors-like playable vertical slice.

The target is not a full commercial game. The target is one stable playable path where the player can start a run, move, fight enemies automatically, collect XP, level up, choose upgrades, survive for a short session, die, and restart.

## MVP Definition of Done

The MVP is considered finished when:

* `Assets/Scenes/Game.unity` enters Play Mode without C# compile errors.
* The player can move using keyboard input.
* Enemies spawn around the play area and chase the player.
* The starting weapon attacks automatically.
* Enemies can take damage and die.
* Dead enemies drop XP pickups.
* The player can collect XP pickups.
* XP fills the XP bar correctly.
* Level-up opens the existing upgrade choice panel.
* At least one selected upgrade visibly changes gameplay.
* The HUD shows health, XP, timer, and level.
* Player death shows game over.
* Restart reloads a playable scene.
* A 1–3 minute smoke test produces no repeating runtime exceptions or console spam.

## Important Scope Rules

* Do not rewrite the entire project from scratch.
* Reuse existing scripts, prefabs, scenes, UI, audio, weapons, tilemap, and character work where possible.
* Keep `Assets/Scenes/Game.unity` as the main MVP scene.
* Player A is the required MVP playable character.
* Other characters, menus, extra weapons, health pickups, damage numbers, and existing bonus features can remain, but they are not required to be fully polished for MVP acceptance.
* Do not add paid assets or unnecessary external packages.
* Do not touch or revert `Assets/Thaleah_PixelFont/Materials/ThaleahFat_TTF SDF.asset` unless specifically needed.
* Prefer small, safe fixes over large architecture changes.

---

# Goal 1: Make Project Compile and Main Scene Playable

## Objective

Ensure the Unity project can enter Play Mode from `Assets/Scenes/Game.unity` without compile errors or missing critical references.

## Tasks

* Check for C# compile errors.
* Check `Assets/Scenes/Game.unity` for missing required references.
* Check current player, enemy, weapon, UI, audio, and tilemap objects.
* Confirm Unity is not stuck compiling.
* Confirm existing scripts are not broken by previous changes.
* Preserve current project structure and asset usage.

## Known Notes

* No missing script references were found in `Assets/Scripts`, `Assets/Scenes`, or `Assets/Prefabs`.
* Unity is currently idle and not compiling.
* Console history shows old `ArgumentOutOfRangeException` errors around XP UI updates after enemy death. This must be verified and fixed during implementation.

## Acceptance Criteria

* Unity has zero C# compile errors.
* `Assets/Scenes/Game.unity` opens normally.
* The scene can enter Play Mode.
* No critical scene object needed for the MVP is missing.

---

# Goal 2: Implement Core Gameplay Loop

## Objective

Make the basic Vampire Survivors-like loop work:

move → enemies spawn → enemies chase → player attacks automatically → enemies die.

## Tasks

* Confirm Player A can move using keyboard input.
* Confirm enemies spawn around the play area.
* Fix `EnemySpawner.RandomSpawnPoint()` if the off-screen spawn bounds math has swapped X/Y logic.
* Guard `EnemySpawner` against empty waves.
* Confirm enemies chase the player.
* Confirm the starting weapon attacks automatically.
* Confirm enemies receive damage.
* Confirm enemies die correctly.
* Make sure enemy death does not break UI, XP, or game state.

## Acceptance Criteria

* Player A moves correctly.
* Enemies spawn outside or around the visible play area.
* Enemies chase the player.
* The starting weapon attacks without manual input.
* Enemies can be killed.
* A short combat test produces no repeating runtime exceptions.

---

# Goal 3: Add Progression System

## Objective

Make XP and leveling feel like a proper Vampire Survivors-like progression loop.

## Tasks

* Add a minimal `ExperiencePickup` script.
* XP pickup should use a small generated visible XP gem if no asset exists.
* XP pickup should have a trigger collider.
* XP pickup should be collectible by the player.
* On collision, XP pickup should call `PlayerController.GetExperience(value)`.
* XP pickup should destroy itself after being collected.
* Change enemy death flow so enemies drop XP pickups instead of directly granting XP.
* Keep `PlayerController.EnsurePlayerLevels()` as the source of truth for XP requirements.
* Verify the old XP UI `ArgumentOutOfRangeException` is gone.
* Confirm level-up opens the existing upgrade choice panel.
* Confirm selecting an upgrade resumes gameplay.
* At least one upgrade must visibly affect gameplay, such as weapon damage, attack speed, movement speed, max health, or projectile count.

## Acceptance Criteria

* Dead enemies drop XP pickups.
* XP pickups are visible.
* XP pickups are collectible.
* Collecting XP fills the XP bar.
* Level-up happens at the expected threshold.
* Upgrade choice panel opens.
* Selecting an upgrade changes gameplay.
* Gameplay resumes after selecting an upgrade.
* XP and level-up flow does not produce console exceptions.

---

# Goal 4: Add HUD, Game Over, and Restart Flow

## Objective

Make the run readable and restartable.

## Tasks

* Extend `UIController` with a level text update path.
* Show `Level N` in the existing HUD.
* Update level text at game start.
* Update level text after level-up.
* Confirm health UI updates correctly.
* Confirm XP UI updates correctly.
* Confirm timer UI updates correctly.
* Confirm game over UI appears when the player dies.
* Ensure `GameManager.Restart()` resets `Time.timeScale` before loading `Game`.
* Confirm restart reloads a playable scene.

## Acceptance Criteria

* HUD shows health.
* HUD shows XP.
* HUD shows timer.
* HUD shows current level.
* Player death triggers game over.
* Restart works after game over.
* Restart does not leave the game paused because of `Time.timeScale`.

---

# Goal 5: Polish, Balance, and Final Smoke Test

## Objective

Make the MVP stable enough for demo or submission.

## Tasks

* Run a 1–3 minute gameplay smoke test.
* Confirm enemy spawn rate is playable and not instantly overwhelming.
* Confirm XP gain and level-up pacing are reasonable.
* Confirm player can survive long enough to experience the core loop.
* Confirm the game does not softlock after level-up.
* Confirm the game does not softlock after game over.
* Confirm no repeating runtime exceptions appear in the console.
* Quick smoke test menu-to-game path if the project already has a menu scene.
* Do not block MVP acceptance on full polish for all characters.

## Menu Scope

MVP acceptance requires `Game.unity` to work directly.

If the project already has a menu scene, the main menu should still be able to start Player A’s game path, but full character selection polish is not required.

## Optional Build Check

If this is for class submission, also verify:

* The MVP scene is included in Build Settings.
* The project can build for the target platform.
* No editor-only code blocks the build.

## Final Acceptance Checklist

* [ ] Project compiles with zero C# errors.
* [ ] `Game.unity` enters Play Mode.
* [ ] Player A can move.
* [ ] Enemies spawn.
* [ ] Enemies chase the player.
* [ ] Starting weapon attacks automatically.
* [ ] Enemies take damage.
* [ ] Enemies die.
* [ ] Enemies drop XP pickups.
* [ ] Player can collect XP pickups.
* [ ] XP bar updates.
* [ ] Level text updates.
* [ ] Level-up panel appears.
* [ ] Selecting an upgrade resumes gameplay.
* [ ] At least one upgrade visibly changes gameplay.
* [ ] Health UI updates.
* [ ] Timer UI updates.
* [ ] Player can die.
* [ ] Game over screen appears.
* [ ] Restart reloads a playable scene.
* [ ] A 1–3 minute smoke test has no repeating runtime errors.
* [ ] Optional: menu can start Player A’s game path.
* [ ] Optional: project builds for the target platform.
