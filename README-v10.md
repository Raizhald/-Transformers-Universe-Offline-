# Transformers Universe — Unity v10

Unity 6000.6.7f1, Built-in Render Pipeline. Game text is English.

Open this folder in Unity Hub, open Assets/Scenes/Hangar.unity and press Play. Launcher → logo loading → local commander log-in → faction selection → Decepticon loading → hangar. Only Decepticons are available. Log-in uses a local profile; no original server or remote authentication is connected.

## Changes in v10

- Removed the broad floor backing introduced in v9. It caused the overlapping floor surfaces in the supplied screenshot. Original packed hangar render geometry is unchanged. Removing this overlay does not recover any original missing floor render surfaces; those still require source geometry rather than another broad cover plane.
- Corrected import settings for all 105 UI images: original pixel dimensions, no power-of-two resizing, no mipmaps, no compression, clamped edges. Recovered slice borders are constrained to valid source dimensions. This addresses distorted and repeated panel parts.
- Chat now uses recovered social panel, chat frame, text box, enter button and close-button artwork. The original PLAY regular/bold font files are recovered from the archive and used by the HUD and front end. Layout remains a reconstruction and is not claimed to be pixel-identical to the original client.
- Minimap now displays a larger fixed overview of the original base plan on an opaque background. Player position is mapped from world coordinates; the original player arrow rotates around its own centre. The YOU label and map zoom are removed. Mouse capture is maintained during movement while the app is focused.
- Logo and hangar loading screens use all three archived Loadingwheel images. Explicit canvas transforms keep the rotating rings centred when the game view is scaled. Hangar loading displays its actual staged progress percentage. The original loading backgrounds are retained.
- Removed the added Central City barrier and fitted the violet barrier to the original Hangar Uplink doorway rim. Training's restricted-door barrier remains. Original base collision data is unchanged; this does not enable travel to a playable Central City map.

## Preserved restoration

The original GroundBridge portal meshes, vertex colours, textures and UV animation from v9 remain, together with 42 restored diffuse textures, original ceiling geometry and illumination masks. Twenty-three archived robot/ambient clips remain: footsteps, mechanical foley, jump, landing and spatial base/portal loops. Some particle behaviour and lighting are reconstructed. The original InnerBase surface-footstep bundle is missing; available A004 robot step recordings are used.

## Controls

WASD / arrows: move. Shift: run. Space: jump. Mouse: camera. Wheel: camera distance. Left Ctrl: mouse mode. Esc: release mouse. Enter: local chat. M: mute/unmute audio. F1: controls. P: portal viewpoint. H: hologram viewpoint. V: decorative effects. R: return to spawn.

## Verification and limits

All runtime and Editor C# scripts compile against installed Unity 6000.6.7f1 without errors or warnings. Startup flow passes 24 executable checks. Data checks confirm original packed hangar bytes are unchanged, both barriers match their original frame corners within 0.001 metres, UI import settings preserve source dimensions, and scaled rotation keeps UI pivots fixed.

Native Unity rendering, shader compilation, audio playback and the resulting visual appearance have not been verified in this environment. Earlier editor batch testing could not connect to LicenseClient-sesta; the Windows UI helper also failed before opening an app. No new Windows executable is supplied.

In Unity, use Transformers Universe → Verify Startup, Movement and UI to run native checks and capture RuntimeChecks reports/screenshots. It additionally checks imported UI dimensions against the source files. Use Transformers Universe → Build Windows Game to create Build/TransformersUniverse-v10.exe.

## Source records

- v10-restoration-checks.json: current data checks and limitations.
- Assets/Resources/Hud/v10-sources.json: recovered social UI source records.
- Assets/Resources/Hud/v10-font-sources.json: original archived font records.
- Assets/Resources/ui-source-dimensions.json: expected native UI texture dimensions.
- Assets/StreamingAssets/doorways.json: current source-frame barrier geometry.
- audio-sources.json and ceiling-source-audit.json: retained source records.
- v8/v9 check reports: historical verification for those versions; v9 floor-backing results do not describe v10.

Original archived executables and assemblies are never executed.
