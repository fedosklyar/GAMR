# GAMR — Spatial Mesh Extension

This is a fork of [parisasrg/GAMR](https://github.com/parisasrg/GAMR), a game analytics tool for mixed reality (HoloLens 2) games built on MRTK. This fork adds a module for recording, persisting, and reconstructing the player's real-world spatial mesh, so that a recorded play session can be reviewed later alongside an accurate model of the room it happened in.

Developed during my internship, where I was the primary developer of this extension.

This work (together with an object-detection component built by my project partner, who is the primary author) is described in our paper, accepted and presented at the [IEEE Conference on Games (CoG) 2026](https://arxiv.org/abs/2610.08619), where I am secondary author. More details on the original GAMR tool can be found in its own [paper](https://arxiv.org/abs/2408.01573).

**Scope of this repository:** this repo contains only the spatial mesh recording/management/reconstruction work described above — my contribution to the paper. The object-detection component also described in the paper was developed separately by my project partner and isn't part of this codebase.

## What this extension adds

**Dedicated scanning scene.** Spatial mesh construction is decoupled from actual gameplay: a separate scene is responsible solely for scanning the player's environment, using MRTK's Spatial Awareness API. Once the mesh meets a detail threshold (`Scanning.cs` tracks mesh-update and mesh-addition counts against configurable thresholds), the observer is suspended and the game transitions to the actual level. Spatial meshes are then static for the rest of the session — keeping scanning concentrated in one scene, and skipping continuous mesh updates during gameplay, which keeps rendering and physics overhead predictable during play rather than scanning-driven.

**Mesh persistence across scenes.** MRTK's spatial meshes don't inherently survive a scene transition the way other game objects can be made to with `DontDestroyOnLoad` — they're tied to the active mesh observer, which gets reinitialized per scene. `SpatialMeshManager` works around this with a custom serialization step: it reads each spatial mesh's geometry (vertices, triangles, normals) and world transform, and on request rebuilds it as an ordinary Unity `Mesh` with its own `MeshFilter`, `MeshRenderer`, and `MeshCollider` — reusing the original material, so the reconstruction is visually and physically indistinguishable from the real spatial mesh. The same serialized format can be written to and read back from a file, which is what allows a recorded session to be reconstructed from disk during analytics review, not just within the same play session.

**Session repositioning (`SessionMover.cs`).** A menu element with three sliders (one per axis) lets a reconstructed session — including its persisted mesh — be moved as a whole, decoupling review from needing to stand in the exact physical spot the recording was made in.

## Engineering conveniences

**Custom data logger (`DataLogger.cs`).** A singleton, scene-persistent logger that writes to a file on disk (so it's inspectable from the HoloLens's file storage after a session), re-initializing its output file on every scene load. Includes a couple of purpose-built logging methods (spatial observer state, mesh data) on top of general string logging, and is meant to be extended with more as needed.

**Spatial Awareness helpers (`SpatialAwarenessExtensions.cs`).** Extension methods on top of MRTK's Spatial Awareness system for two things the SDK doesn't expose directly: finding the currently active mesh observer among all registered providers, and suspending every running mesh observer at once.

**Reorganized replay hierarchy.** Recreated sessions are now nested under a parent object per session, itself under a single top-level container for all sessions — rather than recreated objects sitting loose in the scene. This mainly exists to make whole-session operations (like the slider-based repositioning above) simple: transform the one parent, and everything under it — including its mesh — moves together.

## Known limitations

A few things worth knowing before building on this, drawn from development notes kept during the internship:

- **Mesh coloring across multiple simultaneous sessions is unreliable.** When reconstructing more than one recorded session at once, all of their meshes currently end up rendered in whichever session's color was applied most recently. This stems from `persistentMeshes` being a single shared list rather than partitioned per session — a call to `RecreateMeshes` recolors everything currently in that list, not just the session it was invoked for. A real fix would scope mesh storage per session rather than using one shared list.
- Picking a specific recorded session to review currently isn't supported directly — only stepping forward/backward through sessions in sequence. Naming sessions and listing them for direct selection was identified as a improvement but not implemented.
- Script execution order matters for `DataLogger`: it needs to initialize before other components that log through it, or calls can hit a null `StreamWriter`.

## Requirements

Same as the upstream project: Unity 2020.3.18, MRTK 2.8, built and tested for HoloLens 2.

## Acknowledgements

Built on top of [parisasrg/GAMR](https://github.com/parisasrg/GAMR). Developed with guidance from my academic supervisor during an internship.