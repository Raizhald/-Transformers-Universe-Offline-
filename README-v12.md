# Transformers Universe — Unity v12

Unity 6000.6.7f1, Built-in Render Pipeline. Game UI is English.

Open this project in Unity Hub, open Assets/Scenes/Hangar.unity and press Play. Follow the local log-in and select Decepticons. Near CENTRAL CITY press E to load the exterior. Outside, E returns to the hangar; R resets to the entry point.

## Terrain and falling-through fix

v11 omitted the original TerrainData while importing buildings and their mesh/box collisions. Most ground therefore had neither its original heightfield surface nor its TerrainCollider. This was an import omission, not an absent source asset.

v12 restores the original 1025 × 1025 heightfield over a 1024 × 1024 metre area. Height and surface-weight axes are converted into the packed world's (-sourceX, sourceY, -sourceZ) coordinates. Eight original diffuse surface layers and two source weight maps are included. Ground is rendered and collided through native Unity Terrain/TerrainCollider using the same height data. Terrain and terrain-physics modules are now explicitly included in the package manifest. This does not add a flat cover plane over the city.

The original terrain elevations range from approximately 28.384 to 95.729 metres. Outside the recovered ground, falling below its minimum elevation minus 15 metres returns the player to the last grounded position. This boundary recovery is separate from terrain collision.

## Additional recovered assets

The huge white cylinder in the supplied screenshot was the Energon Lift effect, whose archived material uses _Emissive and _Normal rather than _MainTex. Both original textures, their source tiling, Fresnel colours/power and distortion setting are recovered. A Unity 6 refraction/Fresnel shader adapts those inputs; it is not the exact historical compiled shader. Its native appearance still needs verification.

Nineteen previously skipped building sections are restored. Their positions and normals were valid; six UV components were nonfinite. Only those invalid UV components are replaced using finite original UVs at coincident vertices, prioritizing matching normals. The repair is recorded in building-uv-repairs.json. The exact original texture seam choice is not confirmed.

The original v11 exterior pack, collision pack and hangar pack remain unchanged. The restored building sections are supplied in a separate small pack. Some built-in Quad/Plane resources and full particle behaviours remain unresolved; v12 is not a claim of complete original-client restoration. No original missions, server logic or exterior soundscape is supplied. The recovered exterior's historical identity as Central City remains unconfirmed.

## Controls

WASD / arrows: move. Shift: run. Space: jump. Mouse: camera. Wheel: camera distance. Left Ctrl: mouse mode. Esc: release mouse. Enter: local chat. E: travel. R: reset in current area. M: mute. F1: controls. P/H: hangar review viewpoints; from outside the first press returns to the hangar. V: hangar decorative effects.

## Verification

Runtime and Editor C# compiles against installed Unity 6000.6.7f1 without warnings. Startup state tests pass 24 checks. All source heights were compared with the converted heightfile, with maximum float conversion error below 0.000004 metres. Axis/world correspondence passes 1,089 grid samples. The eight surface-weight channels are normalized, all eight source diffuse copies match their documented axis conversion, and the supplementary building pack passes geometry/index checks.

Native Unity rendering, shader compilation and physical walking over the restored terrain have NOT run here. Earlier batch testing could not connect to LicenseClient-sesta, and the native UI helper failed before opening an app. The user's screenshot establishes the v11 problem; it does not establish the result of v12.

Transformers Universe → Verify Startup, Movement and UI now additionally loads the terrain, checks all eight layers, checks the TerrainCollider at four city locations, settles the robot and walks while checking for terrain penetration, then verifies return to the hangar. These native checks are provided but not yet executed. Reports/screenshots go to RuntimeChecks. Build Windows Game produces Build/TransformersUniverse-v12.exe. No prebuilt executable is supplied.

## Records

- v12-terrain-checks.json: current terrain, UV and source checks, including limitations.
- terrain-source-record.json: original TerrainData metadata and coordinate conversion.
- Assets/StreamingAssets/ExteriorTerrain/: heights, weights and eight recovered diffuse layers.
- Assets/StreamingAssets/ExteriorLift/material.json: original lift inputs and provenance.
- building-uv-repairs.json, v12-building-sources.json, v12-building-pack.json: supplementary building records.
- README-v11.md: previous travel/environment reconstruction notes.
- Earlier v8/v9/v10/v11 reports are historical; v11 missing-terrain/UV counts are superseded by the v12 report.

Original archived executables and assemblies are never executed.
