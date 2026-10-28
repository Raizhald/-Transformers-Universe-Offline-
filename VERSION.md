# Unity v23 (current)

Visible concrete lower Autobot hangar finish using original texture assets, recovered tunnel grille, and removal of 44 black lower fade overlays. Source/data validation and C# compilation passed; the v23 appearance has not been visually verified in Unity. See README.md and v23-verification.json.

# Unity v20 (historical)

Both factions use the existing shared city. Safe capsule/footprint arrivals replace the Autobot service-trench return marker. Sixteen source-collision-aligned visible floor underlays use the original asphalt material.

# Unity v19

Autobot hangar ↔ High Grand exterior travel with E; original terrain, map, sky and source vehicle blockers. Original Autobot portal, signage, screen textures, normal maps, two-tone parameters and vertex colors restored.

# Unity v18

2026-10-28: Both factions are selectable. AUTOBOTS loads Autobot_Interior and Catapult/A001 with original geometry, collisions, RigA locomotion, minimap and ambience. F2 returns to the startup menu. Decepticon data remains unchanged from v17.

# Versions

- v1: Decepticon hangar, A004, original animation clips, movement, camera and collisions.
- v2: portal, hologram, mist, sparks, original point lights and emissive surfaces.
- v3: smoother camera and locomotion, animation transitions, smaller collision steps, improved portal and glow. User confirmed working gameplay.
- v4: reference-inspired HUD, moving minimap marker, local chat, local mode selector and mouse mode.
- **v5 — 25 October 2026:** English launcher, logo/loading, local commander log-in, faction selection with only Decepticons available, staged hangar loading and commander name in the English HUD. Original archived front-end artwork and user-supplied reference backgrounds. Startup flow passed 24 state checks; actual Unity startup rendering still requires runtime verification.

- **v6 — 25 October 2026:** recovered HUD atlas variants and original Decepticon minimap, centred visible player marker, compact chat, original gloss/emission channel maps and source UV/shininess restoration. All C# compiled without warnings; runtime/shader appearance unverified.

- **v7 — 25 October 2026:** original Hangar Uplink / Central City sign frame and squareblok font, solid restricted-door shields, brighter original crystal illumination, recovered native launcher and Firebreaker loading artwork, corrected wide-screen log-in composition. Native rendering and new shield interaction remain unverified.

- **v8:** fit shields to exact archived doorway inner rims; restore original additive blending, depth state, textures and tint for crystal glow meshes; prevent opaque glow planes returning when V disables effects. Native v8 rendering unverified.

- **v9:** original GroundBridge meshes/vertex colours/textures/UV animation; original diffuse/illumination restoration; original ceiling audit and unlit backdrop; collision-derived visual floor backing; fixed minimap with zoom removed; 23 archived robot/ambient clips with locomotion, jump, landing and spatial loops. Native rendering and audible playback remain unverified.

- **v10:** removed overlapping v9 floor backing; corrected source UI texture imports and sliced borders; recovered original social UI sprites and PLAY fonts; enlarged fixed minimap with actual player position and centred arrow; maintained mouse capture; three original loading-ring layers with centred rotation; moved added Central City barrier to the original Hangar Uplink frame. Native appearance remains unverified.

- **v11:** imported recovered Decepticon Exterior with original static geometry, textures and collisions; added E travel at Central City, source-floor entry validation, archived loading art, area-specific overview, current-area reset, hangar ambient isolation and return travel. Added native exterior/return verification, not executed here. Original Central City identity and native appearance remain unverified.

- **v12:** restored omitted original 1025x1025 exterior terrain and native terrain collision, eight archived ground layers and normalized weights; included terrain/physics modules; recovered lift emissive/normal material inputs and adapted refraction/Fresnel rendering; restored 19 building sections by repairing six nonfinite UV components from coincident original vertices. Added native multi-location terrain walking/collision checks, not executed here.

- **v13:** resolved archived built-in mesh resources; recovered 700 road/pavement surfaces with original collision and five atmosphere planes; restored original city minimap, sky cube, scene fog, ambient light and sun; adapted 163 source material records and 17 normal/illumination textures, cutout/alpha/additive/multiply rendering; recovered six exterior ambience clips and enabled the original nonspatial wind loop. Offline geometry/CRC/compile/startup checks pass; native rendering, walking and performance remain unverified due to Computer Use initialization failure.

- **v14:** corrected per-face sky upload row orientation; added 599 source-aligned parked vehicle blockers; recovered A004 car rig and baked original transformation sequences; added F mode switching and newly authored four-wheel Rigidbody driving, braking, reverse, handbrake and recovery; original transformation sounds and eleven spatial city ambience loops. Offline rig/box/compile/startup checks pass; native sky, blocking and driving remain unverified.

- **v15:** return to robot always available from a moving, inverted, airborne or transforming vehicle; upright placement with nearby clearance search and area-entry fallback; responsive 34-to-10 degree steering, lower centre of mass and axle anti-roll forces; replaced per-face cube upload with a continuous panorama projected from original sky data. Compile/startup/numeric tests pass; native recovery, driving and sky appearance remain unverified.

- **v16:** restored original two-stage alpha-to-white multiply blending and white fog for graffiti, grime and tarmac overlays; recovered exact source vertex-colour arrays for 491 main exterior mesh variants. Three materials affect 1,037 render instances. Compile/data/startup checks pass; native decal appearance remains unverified. Full city effects and historical lighting/reflection behaviour remain incomplete.

- **v17:** identified the apparent sky seams as five large additive atmosphere/glow quads contaminated by scene-coloured fog. Restored archived Particles/Additive black fog, source blend state, vertex tint and factor-two modulation for seven materials. Sky panorama/shader, geometry, collisions and driving remain unchanged. Added a native black-edge/background pixel regression, not executed here; compile/source/numeric/startup checks pass.
