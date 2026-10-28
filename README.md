# Transformers Universe offline reconstruction — v23

Updated directly in the existing TransformersUniverse-v20-Unity directory. The game displays BUILD v23.

## Download and open

Install Git LFS, then clone the repository with Git so that binary assets are downloaded:

```sh
git lfs install
git clone https://github.com/Raizhald/-Transformers-Universe-Offline-.git
cd -- -Transformers-Universe-Offline-
git lfs pull
```

Use Unity Hub to add the cloned directory as a project. The project uses Unity 6000.6.7f1 (see ProjectSettings/ProjectVersion.txt). Open Assets/Scenes/Hangar.unity and press Play. Unity generates Library and other local caches on first import. Game UI is English; the current in-game badge is BUILD v23.

Large binary assets are stored in Git LFS. GitHub's Download ZIP may contain LFS pointers depending on repository settings; cloning and running git lfs pull avoids that issue. Library, logs, runtime diagnostics and editor user settings are intentionally excluded.

## Visible lower Autobot hangar finish

The user's result confirmed that restoring the original black Plane in v22 did not address the visible unfinished pits. The compiled user project was checked for BUILD v22 and the lower pack reference: this was not an old-project issue.

The older 4.2 Autobot_Interior variants were also inspected. Their lower layout is substantially the same; they do not supply a separate finished textured basement that can simply be enabled.

v23 deliberately reconstructs a visible lower floor using the recovered Plane mesh and Libraries_Transformers_HangarBOT_MainBASE_ConcreteTile01Dark_Diff, including its original concrete texture. The floor retains Y -24.34690094 and uses four-metre texture repeats. Its north edge is extended to cover the last lower wall, four metres past that footprint. The plane covers all 84 source underfloor wall/support footprints. This is an offline reconstruction, not a recovered original textured basement.

44 original lower-wall/support BlackGradient overlays are disabled so the black-void transition no longer obscures the finished lower area. Opaque walls and structural supports remain. Their geometry below the floor is occluded by the floor. The original tunnel grille and geometry-based mesh colliders remain.

Runtime loads Assets/StreamingAssets/autobot-lower-finished.tupack. The exact original recovered v22 pack is preserved separately as autobot-lower-original.tupack. The old guessed road-level asphalt patches are still unused.

## Validation and limitations

Compilation against the installed Unity assemblies passed with compiler exit 0. Independent data checks confirm pack triangle indices/finite values, floor bounds, all 84 footprint coverage, exact matching of the 44 disabled source overlays, source concrete material/texture references and startup wiring. See v23-verification.json and v23-lower-provenance.json.

Unity was not launched and Play mode was not controlled. The finished floor has not been visually checked in the running editor. These source/data checks are not a claim of a verified final appearance.

Shared city travel, safe arrival/return, robot controls, startup menu and English game UI are unchanged. There is no automatic editor test or Play-mode trigger.
