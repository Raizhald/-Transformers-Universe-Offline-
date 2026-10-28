# Transformers Universe — Unity v18

Unity 6000.6.7f1, Built-in Render Pipeline. All game UI is English.

Open this folder in Unity Hub, open `Assets/Scenes/Hangar.unity` and press Play.
The sequence is PLAY → logo/loading → local commander login → faction selection → loading → the selected hangar.
Both AUTOBOTS and DECEPTICONS are selectable by their picture or ENTER HANGAR button.
The local profile is offline; it does not connect to the former game servers.

## New: Autobot hangar and Catapult

AUTOBOTS loads the archived Autobot_Interior scene and Catapult / A001, identified by m_FriendlyName=Catapult and m_BotFaction=0 in BotData_Splittable_A001. The original audio entries also identify A001 as AB.

- 639 original static render instances, 287,249 triangles, original placement and diffuse textures.
- 227 original BoxCollider and 45 MeshCollider instances, converted into 5,700 double-sided collision triangles.
- Catapult's original skinned model: 3 meshes, 11,520 vertices, 7,825 triangles; 2 source materials, diffuse and Mat2 channel maps.
- Four original RigA animations: Idle, Walk, Run and Fall, each with 195 bound tracks. The prototype controls movement, blending, gravity and camera.
- Original Autobot minimap, aligned from the archived plane's transform and UVs. A live player marker and portal marker use Autobot coordinates.
- 29 original point lights, recovered material settings and original cyan hologram geometry. Custom legacy shaders and lighting are reconstructed, with a neutral supplemental fill.
- Original Autobot loading art recovered from the loading atlas: Game Loading Screen_Test and Game Loading Screen_Warrior_Macro. This is promotional artwork; the controllable character is Catapult.
- Original generator and hologram ambience at source SFX trigger positions. The archive yielded 17 Autobot audio clips. A001's event table references missing _Clips0 bundles, so its movement temporarily reuses the previously recovered A004 metallic footsteps, jump and landing sounds.

The Autobot exterior, vehicle mode, NPCs, missions and original server logic are not enabled. Decepticon hangar, city, transformation, driving and the v17 additive-fog fix remain available through DECEPTICONS. All 240 existing Decepticon data files remain byte-identical to v17.

## Controls

Click the game view to capture the mouse. WASD/arrows move, Shift runs, Space jumps, mouse controls the camera and the wheel changes camera distance. LCTRL/Esc releases the mouse. Enter opens local chat. M mutes audio. R resets to the selected hangar's safe spawn. P reviews the portal area, H reviews the hologram, V toggles effects. F2 returns to the startup menu so another faction can be selected.

Decepticon exterior retains E travel, F transformation, WASD driving, Space handbrake and Shift boost.

## Verification and limits

All runtime and Editor C# scripts compile against the installed Unity 6 assemblies without warnings. 36 startup state checks pass, covering both factions, invalid choices and duplicate loads. Data checks validate 20 skinned animation samples, 125 supporting-floor samples and 121 original minimap UV samples. Pack indices, skin weights, joints, textures and source files are checked.

Native Unity rendering, shader compilation, live movement and audio playback have NOT been verified by the agent. Previous native attempts were blocked by Unity licensing IPC from the agent environment, despite the user's active license. Data and C# checks do not establish the final appearance or gameplay physics.

In Unity, use Transformers Universe → Verify Autobot Hangar for the opt-in native check. It checks menu selection, geometry, floor contact, walking/running, jump/landing, safe reset and player marker, and writes a screenshot/report to RuntimeChecks/Autobot. The existing Verify Startup, Movement and UI command checks the Decepticon route. Neither check runs automatically.

Details: v18-autobot-checks.json, autobot-data-verification.json, autobot-source-scene.json and autobot-audio-sources.json. Historical README-v17.md describes the previous sky repair.
