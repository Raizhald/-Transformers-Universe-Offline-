# v21 gatehouse floor hotfix

Applied directly to the project folder TransformersUniverse-v20-Unity that the interactive Unity log identifies. Restart Play after script import. A persistent BUILD v21 label identifies this revision.

The earlier 16 underlays did not cover the entire gatehouse opening. Two additional walkable floor caps now follow convex ground-level footprints derived from the original WallsGatehouseV201/V202 mesh vertices. They use the original asphalt material at Y=14.25, beneath existing road surfaces. This closes the uncovered area rather than changing a texture elsewhere. Original roads and other building geometry stay above the caps.

A regression point at X=-100, Z=-20 was uncovered by all previous 16 polygons and is covered by the new gatehouse cap. Polygon orientation, geometry, collision flags and C# compilation are checked. Native visual validation was not completed: the desktop app inventory did not expose an interactive Unity window. No automatic restart request remains queued.

The optional editor menu Transformers Universe > Verify v21 Floor Fix In This Editor can restart Play, capture the gatehouse floor, check shared-city entry and verify safe hangar return in the interactive editor. It does not run automatically.

The v20 shared-city routing and safe return changes remain.
