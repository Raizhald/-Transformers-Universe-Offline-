# Transformers Universe — Unity v13

Unity 6000.6.7f1, Built-in Render Pipeline. Game UI is English.

Open this folder in Unity Hub, open Assets/Scenes/Hangar.unity and press Play. Follow the local commander log-in and select Decepticons. Near CENTRAL CITY press E to load the recovered Decepticon Exterior. Outside, E returns to the hangar and R resets to the exterior entry. The historical identity of the exterior as Central City is still unconfirmed.

## City restoration in v13

The previous missing built-in mesh references were resolved using the archived client's original unity default resources file and a matching mesh schema. This restores 700 physical road/pavement surfaces and five atmosphere/glow planes. The two helper-only meshes, CubemapController and MinimapBackground, are deliberately excluded from world rendering. The latter's original texture is used by the HUD instead.

Recovered roads and pavements now have their original mesh collision: 1,796 additional triangles. These are separate from the existing 168,533 exterior mesh-collision triangles and native terrain collision. Across 5,388 recovered collision vertices, the maximum difference from the visible recovered surfaces is approximately 0.000129 metres. This checks geometry alignment, not native physics behaviour.

The city now uses the original con_base_001_alien_SKY cube, original scene fog/ambient settings, and original directional-light orientation, colour and intensity. The previous generic sky/background and synthetic daylight are replaced. Original point-light intensities are used. Opaque city surfaces can cast and receive shadows; actual performance still needs profiling in Unity.

A material catalog restores 163 source material records. Seventeen additional normal/illumination texture assets are included, tangents are generated for meshes that need normal maps, alpha-tested materials use their original cutoffs, and additive/multiply/alpha materials use corresponding rendering modes. Legacy shininess is adapted to Unity 6 smoothness. These are modern shader adaptations, not the original compiled shader programs. Original baked lighting, full material cubemap reflection behaviour and full particle simulation are not restored.

The HUD uses the recovered HUD_MiniMap_DecepticonExterior texture. Its alignment comes from the original minimap quad's world corners and UVs, rather than collision bounds. Its cyan contrast is adjusted for the circular radar. The original unmodified image is retained in CityRestoration/OriginalCityMinimap.png.

Six original exterior ambience clips are recovered. Only HighGrand_Wind_02 is enabled: the original nonspatial looping wind, at the archived volume 0.4786301. It stops when the exterior is deactivated and restarts on entry. Fire/explosion/one-shot clips are retained as source data but are not triggered without their world-event implementation. M continues to mute all audio.

## Preserved terrain and earlier fixes

The original 1025 × 1025 terrain and TerrainCollider, eight ground surface layers, restored Energon Lift material inputs, and 19 building sections from v12 remain. The core hangar/exterior packs and original terrain heights are unchanged. The corrected ground uses source geometry and height data rather than a broad flat cover plane. Building UV repair provenance remains in building-uv-repairs.json.

## Controls

WASD / arrows: move. Shift: run. Space: jump. Mouse: camera. Wheel: camera distance. Left Ctrl: mouse mode. Esc: release mouse. Enter: local chat. E: travel. R: reset in current area. M: mute. F1: controls. P/H: hangar review viewpoints; from outside the first press returns to the hangar. V: hangar decorative effects.

## Verification and remaining limitations

Runtime and Editor C# compiles against installed Unity 6000.6.7f1 without warnings. The startup state machine passes 24 executable checks. Packed source meshes, indices, source transforms, road-collision alignment, terrain preservation, minimap alignment, sky dimensions and all recovered Ogg page CRCs pass offline checks.

Native rendering, shader compilation, walking, shadows, performance and audible playback remain UNVERIFIED. Computer Use initialization was attempted and failed before attaching to Unity: failed to write kernel assets, system cannot find path (os error 3). No screenshot of the v13 runtime or successful native play test is claimed.

Transformers Universe → Verify Startup, Movement and UI includes checks for the 11,204 exterior mesh-renderer instances, eleven mesh-collision chunks, restored material catalog, original sky shader, recovered road collider, wind clip, eight terrain layers, terrain collision and walking at four locations, and return to the hangar. These native checks are included but have not run here. Outputs go to RuntimeChecks. Build Windows Game produces Build/TransformersUniverse-v13.exe. No prebuilt executable is supplied.

The prototype is local exploration. Original NPCs, missions, scripted world events, original server behaviour and complete historical rendering are not implemented. The absence of unresolved built-in mesh references applies to the selected active static-renderer export, not every original game system or every LOD.

## Records

- v13-city-checks.json: current city checks, measurements and limitations.
- v13-recovered-resources.json and v13-resource-pack.json: recovered roads/effect geometry.
- v13-original-map-alignment.json: original map corners, UV mapping and display adjustment.
- v13-city-audio-sources.json: recovered ambience clips and source IDs.
- Assets/StreamingAssets/CityRestoration/sources.json: material inputs, texture provenance, original environment and sun/sky data.
- README-v12.md: preceding terrain/import repair details.
- Earlier version reports are historical; their missing-resource counts are superseded by v13-city-checks.json.

Original archived executables and assemblies are never executed.
