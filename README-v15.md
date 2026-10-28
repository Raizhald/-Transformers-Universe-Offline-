# Transformers Universe — Unity v15

Unity 6000.6.7f1, Built-in Render Pipeline. Game UI is English.

Open this folder as a new Unity Hub project, open Assets/Scenes/Hangar.unity and press Play. E near CENTRAL CITY enters the exterior. F outside enters vehicle mode; F in vehicle mode always requests return to robot form.

## Unconditional return to robot

The v14 restrictions requiring low speed and grounded wheels are removed from the return path. F is handled before vehicle-entry restrictions. An overturned, moving or airborne car returns immediately to upright robot form. F during a transformation cancels it and restores robot form. When the car is upright, nearly stopped and has clear room, the original return animation is still used.

Recovery searches nearby source collision floors and tests standing capsule clearance. If no nearby clear position is found, it uses the current area's entry point. The vehicle is hidden/deactivated, its physics stopped, and the robot CharacterController restored. The robot does not inherit the car's roll or pitch. R remains a separate area-entry reset. Gameplay hotkeys remain suppressed while typing chat or during area loading.

## More responsive steering

The overly restrictive 8 m/s² steering envelope from v14 is replaced by an arcade-oriented range: 34 degrees at low speed, decreasing to 10 degrees at 25 m/s and above. Forward and reverse use the same limit. At 15 m/s the limit is 19.6 degrees. Numeric checks prevent regression to tiny high-speed steering angles.

The centre of mass is lowered from Y=0.55 to Y=0.25. Anti-roll forces are applied across each axle, capped through normalized suspension-travel difference and a 4,500 force factor. These changes are intended to improve turning response and reduce rollover; native driving stability still needs testing. Four-wheel suspension, braking, reverse and handbrake remain newly authored approximations, not recovered exact A004 handling.

## Sky

The v14 cubemap row-flip change is removed along with direct per-face Cubemap.SetPixels upload. The original six source faces are bilinearly projected to one 2048 × 1024 equirectangular panorama. A 2D sky shader samples it by view direction, with horizontal wrapping and vertical clamping. The original source sky bytes, colours and world rotation are retained; no replacement sky artwork is generated. This avoids the per-face upload/orientation path that produced the visible bands. The resulting source panorama was visually inspected, but its appearance in native Unity has not been verified.

## Controls

F: vehicle/robot. WASD: drive. S: brake/reverse. Space: handbrake. Left Shift: vehicle boost. R: reset to area entry. E: return to hangar. Left Ctrl/Esc: release mouse and brake the car. M: mute. Robot movement and existing hangar controls remain.

## Verification

All runtime/Editor C# compiles against installed Unity 6000.6.7f1 without warnings. Startup flow passes 24 checks; steering limits pass 153 numeric checks. The native verification menu additionally tests returning from a moving overturned car and a moving airborne car, with standing-ground checks and bounded waits.

Native sky rendering, car turning, rollover stability and recovery physics have NOT been run here. Previous native UI initialization failed before attaching to Unity. No successful v15 native play test or screenshot is claimed. Transformers Universe → Verify Startup, Movement and UI runs the supplied checks and writes RuntimeChecks reports/screenshots when executed in Unity.

The original city geometry, 599 parked blockers, source terrain, original A004 rigs/animations, city ambience and transformation sounds from v14 are retained. Engine audio and exact original vehicle tuning remain unavailable. This is local exploration without original servers/missions.

Build Windows Game produces Build/TransformersUniverse-v15.exe; no prebuilt executable is supplied. v15-fix-checks.json records current checks; README-v14.md and older reports are historical and their return/steering/sky descriptions are superseded here.
