# v21 hotfix in this project

The gatehouse opening has two additional walkable floor caps. Restart Play and check the persistent BUILD v21 label. See README-v21.md. The project folder may still be named v20 because the current project was patched in place.

# Transformers Universe — Unity v20

Unity 6000.6.7f1, Built-in Render Pipeline. Game UI is English.

Open this folder in Unity Hub, open Assets/Scenes/Hangar.unity, press Play and choose AUTOBOTS or DECEPTICONS.

## Correct shared city route

Both factions now use exactly the same exterior.tupack, recovered buildings/roads, original city collisions, native Terrain, sky, ambience, parked-vehicle blockers and ExteriorMinimap.png. The Autobot_Exterior_Tutorial route from v19 is no longer instantiated by the menu/travel flow. Its tutorial barriers are therefore not part of this route. The existing city data used by Decepticons is unchanged.

For Autobots, walk toward the original HIGH GRAND sign/exit and press E when ENTER CITY appears. Press E outside to return to the Autobot hangar. The exterior is labelled SHARED CITY for the Autobot route. Catapult remains the playable robot. Autobot vehicle mode is not enabled.

## Safe hangar return

The old return marker was inside the service gap between the road lanes. A broad source collision surface underneath it passed the previous point-only floor check, even though this was not a suitable arrival location.

Autobots now return to the verified initial hangar room near (-79.46286, 14.36509, -3.245). The shared arrival helper checks the actual CharacterController height, center and radius with extra clearance, checks eight supporting footprint points and searches nearby alternatives if occupied. It synchronizes transforms after environment activation and refuses to teleport into an occupied capsule. City entry uses the same helper. A failed transition also uses safe recovery instead of blindly teleporting to the old marker. R resets the Autobot hangar through this helper; F2 returns to the startup flow.

## Visible floors in the black gaps

Sixteen visible asphalt underlays follow existing source FloorCOL top footprints at the lower hangar level. They use the original Libraries_Transformers_HangarBOT_MainBASE_Tarmac01_Diff material. They sit 3.5 cm below the existing physical floor, so the original road geometry remains above them. This is an explicit reconstruction of missing visible floor coverage, not a claim that a complete original service-pit mesh was recovered. No new invisible blocker or broad world-floor collider was added.

Original v19 Autobot portal, signage, screens, material details and source colors remain.

## Verification

All runtime/Editor C# compile against installed Unity assemblies without warnings. All 36 startup state checks pass. The shared-city spawn/map configuration matches the Decepticon configuration, and all 240 established shared-city data files remain byte-identical. Independent checks confirm support at 33 footprint samples around the return point. A conservative closest-distance calculation over 39 capsule-axis samples bounds the entire return capsule's mesh clearance above the required controller radius plus margin. Floor-underlay source vertices/material references are validated.

A native v20 batch-editor attempt was made. It failed to establish the LicenseClient-sesta IPC connection and reported the headless licensing feature unavailable. Native gameplay, new shader rendering and the city round trip have not been verified by the agent. This does not imply that the user's interactive Unity license is inactive.

In Unity, Transformers Universe > Verify Autobot Hangar provides the opt-in native regression check: occupied-arrival rejection and an alternative, floor underlays, entry into the shared city (11,204 renderers and 599 parked-vehicle blockers), and return to the clear Autobot start room. It writes a report/screenshots under RuntimeChecks/Autobot. The test does not run automatically.

Details: v20-verification.json. Historical README-v19.md describes the superseded tutorial route. The reconstruction remains offline; original server, NPCs and mission logic are not restored.
