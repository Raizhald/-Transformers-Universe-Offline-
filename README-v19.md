# Transformers Universe — Unity v19

Unity 6000.6.7f1, Built-in Render Pipeline. All game UI is English. The user confirmed v18 works.

Open this folder in Unity Hub, open `Assets/Scenes/Hangar.unity` and press Play. Choose AUTOBOTS after local commander login. Catapult / A001 starts in the original Autobot hangar.

## Travel to High Grand

Walk to the exit marked HIGH GRAND. The [ E ] ENTER HIGH GRAND hint appears near the original Autobot exterior spawn marker. Press E to load the archived Autobot_Exterior_Tutorial environment. Press E outside to return to the Autobot hangar. R resets to a safe point in the current environment. F2 returns to the startup menu.

The city contains 2,233 original render instances, 49,592 source collision triangles and a native 1025 × 1025 Terrain with all eight source ground layers. The original map is aligned from 121 source plane UV samples and shows the live player marker. The original six sky textures, source sun, fog and ambient values are restored. Wind ambience uses HighGrand_Wind_02 from Audio_Ambience_Autobot_Exterior. Twenty original exterior audio clips are retained for future sound placement.

Twenty-three source-derived vehicle blockers stop the robot entering parked vehicles. Static geometry is compacted to 310,031 stored vertices while preserving the original referenced triangles and all transforms, including 123 sheared transforms. This avoids retaining millions of unused static-batch vertices.

## New Autobot hangar details

- Original HANGAR UPLINK, BATTLE SIMULATOR and HIGH GRAND signage, at archived positions, using the recovered sign frame and font.
- The original Autobot portal VFX prefab: two mesh parts with blue electricity, smoke and a multiply layer, using original textures and vertex colors. Its placement comes from the source VFX spawn object, not an estimated doorway position.
- Recovered holographic screen textures, including the wanted-character screen. Untextured white panels are replaced by their source emissive inputs.
- Source normal maps and two-tone material parameters, original source vertex colors and alpha fades. Separate Autobot shaders leave the confirmed Decepticon shaders unchanged.
- Existing original hologram, 29 point lights, generator ambience and Catapult locomotion remain.

## Controls

Click the game view to capture the mouse. WASD/arrows move; Shift runs; Space jumps; mouse controls the camera; wheel changes camera distance. LCTRL/Esc releases the mouse. Enter opens local chat. M mutes sound. P/H review portal/hologram areas; from the exterior these controls return to the hangar first. V toggles hangar effects. E travels; R resets; F2 opens the startup flow again.

Decepticon hangar/city and A004 vehicle transformation remain available through DECEPTICONS. Catapult's Autobot route currently supports walking rather than vehicle mode.

## Verification

All runtime and Editor C# compile against the installed Unity assemblies without warnings. All 36 startup state checks pass. Data checks validate 6,699 original/reconstructed world-coordinate samples (maximum difference below 0.00004 world units), 121 source height samples, terrain blend normalization, 49 entry-floor neighborhood samples, five hangar return-floor samples, source map UVs, material files, portal meshes, colors and parked-vehicle bounds. All 240 existing Decepticon data files are unchanged.

Native Unity v19 rendering, shader compilation and the live city round trip have NOT been tested by the agent. The data and C# checks do not establish final in-game appearance or physics. In Unity, Transformers Universe → Verify Autobot Hangar runs the opt-in native test: startup, character movement, signs/portal, city entry, terrain contact, walking, blockers and return. It writes screenshots and a report to RuntimeChecks/Autobot. This check does not run automatically.

The reconstruction is offline. Original server, NPCs, tutorial missions and faction-war logic are not restored. Sixteen source MeshCollider records have null mesh references; these are listed in autobot-city-source.json, and no invented collision geometry was substituted for them. A001 movement audio still uses the documented shared A004 archive fallback from v18.

Evidence: v19-verification.json, v19-data-pack.json, autobot-city-source.json, autobot-city-audio-sources.json and v19-hangar-detail-sources.json. README-v18.md documents the previous version.
