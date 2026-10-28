# Transformers Universe offline reconstruction — v22

This update is applied directly to the existing TransformersUniverse-v20-Unity project. The game displays BUILD v22. There is no need to switch project directories.

## Original lower hangar geometry

An audit of all 1,035 MeshRenderers in Autobot_Interior identified an omitted original Plane (GameObject 2498). The previous exporter could not resolve its mesh in library/unity default resources. The plane is about 403 by 403 metres at source world Y -24.34690094. It closes the bottom of the hangar beneath the original lower walls and fade surfaces. Its source material is Libraries_[GenericTextures]_Black_Diff, explicitly black: the source does not provide an asphalt basement at this height.

The recovered mesh and its original transform are in Assets/StreamingAssets/autobot-lower-original.tupack. All 84 existing underfloor wall/support instances remain. The audit confirms that the only other omitted active renderers are the cubemap test sphere and the DESIGN/HUD minimap background; these are not missing hangar walls or floors.

The same pack contains the original IteriorTunnelGrillV2_01 geometry and its original cutout fence material. That source object was inactive in the archive; it is deliberately enabled here to cover the tunnel service slot. Mesh colliders use the recovered geometry.

The 18 guessed asphalt underlays/caps from v20/v21 are no longer instantiated. Their historical source files are retained but unused. The file-triggered v21 editor automation has been removed. This update does not start Unity or switch Play mode.

## Existing behavior

Both factions retain the same shared city route. Autobot return still uses the initial hangar room and full CharacterController clearance/support checks. City data, robot controls and travel logic are unchanged by v22. All game UI remains English.

## Validation

C# compilation against installed Unity assemblies passed with compiler exit 0. Independent pack checks passed for triangle indices, finite positions, source transforms/bounds, original material/texture references, shader selection and removal of the guessed-floor startup call. See v22-verification.json.

Unity was not launched. The new lower geometry has not been visually verified in the running editor; data and compilation checks do not establish the final appearance. Previous runtime reports describe earlier versions and do not verify this change.

The original game server, missions and NPC logic remain outside this offline reconstruction. README-v20-historical.md describes superseded floor patches.
