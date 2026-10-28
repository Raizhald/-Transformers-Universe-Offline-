# Transformers Universe — Unity v16

Unity 6000.6.7f1, Built-in Render Pipeline. Game UI is English.

Open this project in Unity Hub, open Assets/Scenes/Hangar.unity and press Play. Existing local start-up, E travel, F robot/vehicle switching, driving and recovery controls remain unchanged from v15.

## Black graffiti and ground rectangles

The affected original materials are:

- Libraries_Props_Civilian_Graffifi_Graffiti
- Libraries_Grounds_Decals_Impact_GrimeEdge
- Libraries_[GenericTextures]_Decals_TarmacGrime01_DiffA

They use the archived Mobile/Particles/Multiply shader. The earlier adaptation multiplied raw texture RGB into the scene and ignored its second fixed-function stage. Black RGB in transparent parts therefore produced black rectangles.

The recovered shader explicitly performs texture × primary vertex colour, then blends that result with constant white using its alpha. It uses Blend Zero SrcColor and white fog. v16 restores these operations in a dedicated modern shader. Transparent pixels now produce neutral white before multiplying the scene; they should leave the underlying surface unchanged. Fog also approaches white rather than darkening entire decal rectangles. Old saved material colour fields are ignored for this shader because the recovered shader does not use them.

This changes the shader rather than hiding graffiti or deleting dirty surfaces. The three materials are used by 1,037 original render instances.

## Original vertex colours

Original mesh colour arrays are matched by exact source-position hashes for all 491 main exterior mesh variants. There are 22 non-white arrays and 1,076 vertices with source alpha below one. These recover decal/mesh opacity information that the earlier pack did not carry. Meshes without source colours receive explicit white defaults. Core geometry, transforms, textures, collisions, vehicles and sky data are unchanged.

## Is the city complete?

No. Most selected active static geometry and source textures are restored, together with the ground, parked blockers, original minimap, sky data and part of the ambience. Complete historical lighting/reflection behaviour and full particle/world-event simulation are not restored. Native visual comparison and performance verification are also incomplete. Original servers, NPC/mission systems and full original gameplay are not implemented. This remains a reconstruction for local exploration, not a complete original client.

## Verification

Runtime and Editor C# compiles against Unity 6000.6.7f1 without warnings. Startup state tests pass 24 checks. Data checks verify source colour counts and alignment, original texture alpha, and 36 neutral-background regression cases. The native verification menu checks that the dedicated multiply shader is assigned to these materials and is supported, along with the previous movement/driving/recovery checks.

Native rendering of the repaired graffiti and decals has NOT been run here. The screenshots supplied by the user establish the previous error; no successful v16 screenshot or native visual test is claimed. Previous native UI initialization failed before attaching to Unity.

Use Transformers Universe → Verify Startup, Movement and UI to run supplied native checks, writing reports/screenshots to RuntimeChecks. Build Windows Game produces Build/TransformersUniverse-v16.exe; no prebuilt executable is supplied.

## Records

- v16-decal-checks.json: current findings, counts and verification limits.
- original-multiply-shader.txt: recovered fixed-function shader source.
- v16-vertex-color-sources.json: per-mesh source object IDs and matching colour-array provenance.
- Assets/StreamingAssets/exterior-vertex-colors.bin: source colour arrays in packed mesh order.
- README-v15.md: preceding driving/recovery/sky behaviour; earlier version reports are historical.

Original archived executables and assemblies are never executed.
