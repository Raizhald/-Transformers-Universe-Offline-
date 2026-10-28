# Transformers Universe — Unity v14

Unity 6000.6.7f1, Built-in Render Pipeline. Game UI is English.

Open this folder as a project in Unity Hub, open Assets/Scenes/Hangar.unity and press Play. Follow the local commander log-in, choose Decepticons, and use E near CENTRAL CITY to enter the exterior. Vehicle transformation is available outside.

## Controls

- F: transform between robot and vehicle. Land first; stop the vehicle before returning to robot form. Clearance checks can refuse transformation near obstacles or low ceilings.
- In vehicle form, W accelerates, S brakes and then reverses, A/D steer, Space applies the rear handbrake, and Left Shift raises the forward speed target. Mouse controls the follow camera.
- R: recover/reset to the current area's entry point, returning to robot form. E: return to the hangar from outside. Travel also returns to robot form.
- Left Ctrl / Esc: release the mouse; the car brakes while driving input is released. Enter: chat. M: mute all audio, including in vehicle mode. F1: controls.
- Robot movement remains WASD/arrows, Shift to run, Space to jump. P/H are hangar review viewpoints; V toggles hangar decorative effects.

## Parked vehicles

599 sedan, people-carrier, truck, bus and other parked vehicle sections now have solid oriented box colliders. Bounds come from each selected original renderer's indexed geometry and source transform, not the bounds of a complete static batch. A 0.03 metre size margin reduces visual penetration. These are static blockers; parked cars do not become drivable or destructible. Standing on top of a car is allowed; passing inside its body should be blocked. Open doors and crushed shapes use a bounding-box approximation.

## A004 transformation and driving

The original A004 vehicle model is recovered: five meshes and 9,890 vertices. Both original rigs are used during transformation, with the original sampled animation curves and rig switches. Robot-to-vehicle lasts approximately 1.66 seconds; the reverse sequence lasts approximately 1.69 seconds. Exact switch boundaries are included in the baked sample times. The vehicle panel mask is reconstructed from changing original tracks, as documented in the earlier sequence export; exact original layered Unity evaluation is not claimed.

Driving is newly authored for Unity 6 using a Rigidbody, four WheelColliders, suspension, wheel visual rotation, speed-dependent steering, acceleration, braking, reverse and handbrake. Wheel centres match the original posed wheel joints. Generic archived VehicleData/GroundVehicle source was inspected; the generic 2,000 kg mass is used. A004-specific suspension, gearing, traction and handling settings were not recovered, so the tuning is a new approximation. This is not the original networked vehicle controller.

Current tuning uses 0.37 metre wheel radius, 0.22 metre suspension travel, 40,000 spring stiffness, 6,000 damper, all-wheel motor torque up to 1,100 Nm per wheel, a forward target of 26 m/s (34 with boost), reverse target 8 m/s, and steering capped at 30 degrees and reduced with wheelbase/speed to an 8 m/s² target lateral-acceleration limit (approximately 1.5 degrees at 26 m/s). These are target/tuning values, not measured native driving results. The car has two chassis box colliders and continuous collision detection. The Unity vehicles module is explicitly included.

Original A004 transformation sounds play during rig changes. An A004 engine loop was not recovered; no unrelated base engine sound is substituted. Robot footsteps pause while transforming or driving.

## Sky and city sound

The sky upload now reverses the rows of each raw cubemap face when filling the bottom-to-top Cubemap.SetPixels array. The original cubemap bytes, six-face order and sky material remain. This addresses the face/band orientation issue shown in the supplied v13 screenshot, but the new native appearance has not been visually verified.

Alongside the existing original nonspatial wind, eleven spatial ambience loops use archived city trigger positions: burning fires, fire rumble and the hollow loop near the entry. Volumes/ranges are adapted for the local prototype. Warning, explosion and other one-shot events are not triggered without their original event logic. All sound stops with the exterior when returning to the hangar; M mutes the listener.

## Verification and limits

C# runtime and Editor scripts compile against installed Unity 6000.6.7f1 without warnings. Startup state tests pass 24 executable checks. Offline checks validate both packed rigs, skinning, animation tracks, explicit rig-switch samples, wheel-joint centres, final vehicle bounds, and all 599 parked bounding boxes against their source render geometry. City ambience references resolve to included clips.

Native sky rendering, parked-car blocking, transformation, suspension, driving stability, braking and sound playback are NOT verified here. The native UI helper has previously failed before attaching to Unity; no successful v14 play test or screenshot is claimed. Use R if the new driving model becomes stuck or unstable.

Transformers Universe → Verify Startup, Movement and UI additionally checks parked-car blockers, four wheels, transformation in both directions, wheel grounding, acceleration and braking, then return to the hangar. These native checks are included but have not run in this environment. They write reports/screenshots to RuntimeChecks. Build Windows Game produces Build/TransformersUniverse-v14.exe; no prebuilt executable is supplied.

## Records

- v14-vehicle-checks.json: current data checks and limitations.
- v14-rig-checks.json: packed rig counts and hashes.
- Assets/StreamingAssets/a004-car-config.json: source switch timings and wheel-joint positions.
- Assets/StreamingAssets/parked-vehicle-collisions.json: per-object source IDs and oriented blocker bounds.
- Assets/StreamingAssets/city-ambient-loops.json: original trigger positions and selected loop clips.
- v14-transform-audio-sources.json: transformation sound provenance.
- README-v13.md: preceding city restoration details.

The project remains local exploration without original servers, missions or complete original gameplay. Original archived executables and assemblies are never executed.
