# Transformers Universe — Unity v17

Unity 6000.6.7f1, Built-in Render Pipeline. Game UI is English.

Open this folder in Unity Hub, open Assets/Scenes/Hangar.unity and press Play. Existing local start-up, E travel, F transformation, driving/recovery and parked-vehicle blockers remain.

## Actual cause of the apparent sky seams

The city contains five very large atmosphere/glow meshes: two Quadfog objects, two QuadSun objects and QuadshipHighlight. Their original PointGlowEffect_Diff texture has black RGB but fully opaque alpha at its corners: (0,0,0,255).

The earlier generic effect shader blended scene-coloured fog into these black corners. With additive blending, those corners then added coloured light across the entire rectangular or triangular mesh. The result looked like broken skybox faces even after changing the sky texture or its upload.

v17 restores the original Particles/Additive behaviour in a dedicated shader: black fog, Blend SrcAlpha One, RGB-only output, alpha threshold and 2 × texture × vertex colour × original tint. Under fog, the black corners remain zero added light and should not reveal the outline of the atmosphere meshes. The effects are retained, not deleted.

The source sky panorama and sky shader are byte-identical to v16. Geometry, collisions, driving and vehicle recovery are also unchanged. The fix targets the atmosphere overlays instead of applying another sky-image conversion.

## Scope

Seven source additive material records use the corrected shader, including the three atmosphere materials, point glows, fire, barrier effects and military markings. Original source shader text is included in original-additive-shader.txt. The preceding multiply-decal fix remains in place.

## Verification

Runtime and Editor C# compiles against installed Unity 6000.6.7f1 without warnings. Startup flow passes 24 executable checks. Source audit confirms the five large overlays, opaque black texture corners, original black-fog/blend state, and 90 neutral-edge numeric cases. Preservation checks confirm no changes to sky artwork/shader, core geometry, road collision or driving code.

The native verification menu now includes a GPU pixel regression: it renders a known background, adds a black fully opaque additive quad under full coloured fog, and checks that the background pixel does not change. It also checks assignment/support of all seven corrected additive materials. This would detect the previous coloured-rectangle bug.

Native GPU regression and full-city visual verification have NOT been run here. Prior UI automation initialization failed before attaching to Unity. No successful v17 Unity screenshot or native visual pass is claimed. The user's screenshots establish the previous overlay error; source data and shader states establish the correction being made.

Use Transformers Universe → Verify Startup, Movement and UI to run supplied native checks and write RuntimeChecks reports/screenshots. Build Windows Game produces Build/TransformersUniverse-v17.exe; no prebuilt executable is supplied.

## Records

- v17-sky-overlay-checks.json: current cause, material/overlay lists and verification scope.
- original-additive-shader.txt: recovered archived shader source and compiled subprogram records.
- README-v16.md: preceding decal restoration and city-completeness limitations.
- Earlier sky conversion reports are historical; v17 identifies the overlay fog path as the cause addressed here.

The city is still a local reconstruction; full historical lighting/reflections, particle/world-event simulation and original server gameplay are not complete. Original archived executables and assemblies are never executed.
