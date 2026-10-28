# Transformers Universe — Unity v11

Unity 6000.6.7f1, Built-in Render Pipeline. All game text is English.

Open this project folder in Unity Hub, open Assets/Scenes/Hangar.unity and press Play. Follow the launcher and local commander log-in, choose Decepticons, then enter the hangar. Walk toward the CENTRAL CITY doorway and press E when the prompt appears. The recovered Decepticon Exterior loads as a separate local area. Press E anywhere outside to return to the hangar. R resets within the current area. The exterior is cached for subsequent visits during the same session.

## New exterior area

The source is generated_bundles/3354185567/Decepticon_Exterior.unity3d, scene BuildPlayer-Decepticon_Exterior. Its correspondence to the original game's Central City destination is not confirmed; this prototype connects that recovered exterior to the existing Central City exit. Outside, the HUD identifies the area as DECEPTICON EXTERIOR.

The import includes 10,480 render instances, 143 archived textures, 155 materials and 168,533 original collision triangles. Shared mesh/material variants are retained, and opaque materials enable Unity GPU instancing. Collision geometry is split into ten chunks. Fifteen sheared source transforms are baked into separate geometry variants, preserving their shapes rather than forcing them into rotation/scale transforms.

The entrance uses the source GroundBridge location. Offline geometry checks find its collision floor at Y=80.826576. At runtime, the loader checks nearby source floors and capsule clearance before placing the robot. If loading or the floor check fails, it restores the hangar and displays an error. Original hangar geometry remains unchanged. The new area uses the existing A004 robot, movement, camera, footstep and mechanical audio. Hangar ambient loops are silenced while outside; original exterior ambience has not been restored.

The transition uses archived loading art and all three loading-wheel layers. Percentages represent loading stages, not estimated time remaining. The initial model/texture decode is synchronous and can briefly pause the spinner, especially on a slower computer. Geometry/collider setup yields between batches.

Outside, the minimap switches to an overview reconstructed from original walkable collision triangles, with the player's world position and entry marker. This is not a recovered original exterior minimap.

## Incomplete source data and appearance

The source export skipped 19 renderers with nonfinite geometry data and 707 renderers referencing missing Unity default resources. Another 705 texture/material/collision resolution issues are retained in exterior-source-report.json. Some may overlap renderer failures. Building textures and available static meshes are imported; full original particle effects, cutscenes, NPCs, missions, server logic and exterior soundscape are not restored. Lighting, fog and legacy material adaptation are approximate. This is a local exploration prototype.

## Controls

WASD / arrows: move. Shift: run. Space: jump. Mouse: camera. Wheel: camera distance. Left Ctrl: mouse mode. Esc: release mouse. Enter: local chat. E: enter the exterior near Central City / return from the exterior. R: reset within the current area. M: mute/unmute. F1: controls. P/H: hangar review viewpoints; from outside the first press returns to the hangar. V: hangar decorative effects.

## Verification

C# runtime and Editor scripts compile against installed Unity 6000.6.7f1 without warnings. The startup state machine passes 24 executable checks. Packed geometry, indices, transforms, source floor and collision counts pass offline data checks. Sample world-space vertex comparisons across every imported instance have a maximum error below 0.0001 metres. The original hangar pack is byte-identical to v10.

Native scene rendering, shader compilation, travel, movement in the exterior and audio playback have NOT been verified here. Earlier Unity batch testing could not connect to LicenseClient-sesta, and the native UI helper failed before opening an app. No new Windows executable is supplied.

In Unity, Transformers Universe → Verify Startup, Movement and UI now additionally loads the exterior, checks its renderer/collision counts, shader support and grounded player, captures the exterior, then checks return to the hangar. These native tests are provided but have not run in this environment. Outputs go to RuntimeChecks. Use Transformers Universe → Build Windows Game to create Build/TransformersUniverse-v11.exe.

## Source records

- v11-exterior-checks.json: current exterior checks and limitations.
- exterior-source-report.json: source bundle hash, instances, markers, lights and all export issues.
- exterior-pack-checks.json: generated pack counts and hash.
- Assets/StreamingAssets/exterior-area.json: entry, exit and overview bounds.
- Assets/StreamingAssets/exterior-materials.json: original material names, shader names, UV transforms and source lights.
- v10-restoration-checks.json: inherited HUD/door restoration checks; historical v8/v9 reports describe their respective versions.
- README-v10.md: retained restoration details for the hangar, portal and UI.

Original archived executables and assemblies are never executed.
