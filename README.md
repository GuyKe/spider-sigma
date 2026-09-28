# spider-sigma

A native Unity VR game, targeting Meta Quest (standalone/Android) via OpenXR.

## Status

First milestone: the player-spawn room. `Assets/Scenes/Bedroom.unity` procedurally
builds a cluttered teenage-bedroom whitebox (bed, desk + monitor, bookshelf,
gaming chair, corkboard, window with blinds, ajar door, hanging pendant lamp,
floor clutter) via `Assets/Scripts/RoomBuilder.cs` — everything is built from
primitives and flat colors, no imported art yet. The corkboard/poster/map decor
are plain colored placeholders, not reproductions of any copyrighted reference
image; swap in your own art/photos there whenever you're ready.

Gameplay ("actual Spider-Man stuff") is intentionally not started yet — this is
just the room to spawn into.

## Opening the project

Requires the Unity Editor (`ProjectSettings/ProjectVersion.txt` pins
2022.3.21f1 LTS — Unity Hub will offer to install a matching version, or you
can just open with whatever recent 2022.3 LTS you already have).

1. Open the project folder in Unity Hub / the Editor.
2. Open `Assets/Scenes/Bedroom.unity`.
3. Press Play. The scene builds itself on load (via `RoomBuilder`'s
   `ExecuteAlways`/`OnEnable`), and you can fly around with WASD + mouse look
   (Q/E for up/down, Shift to move faster) using the desktop preview camera —
   this is a stand-in until the real XR rig is wired in.
4. To tweak the room, select the `Bedroom` GameObject and edit `RoomBuilder`'s
   public fields, or edit the script directly, then right-click the component
   header → "Rebuild Room" (or just re-enter Play mode).

**I could not open/run the Unity Editor myself in this environment** (no GUI,
no Unity install here) — everything above was authored as text (C# + a
hand-written `.unity` scene file), not verified by actually running the
Editor. If `Bedroom.unity` fails to load for any reason, the safe fallback is:
delete it, create a new empty scene in the Editor, add an empty GameObject,
and drag `RoomBuilder.cs` onto it — that reconstructs the exact same room
since all the geometry lives in the script, not the scene file.

## Next steps toward Quest VR

The project isn't XR-enabled yet. To get it running on a Quest headset:

1. **Package Manager**: install `XR Interaction Toolkit` (it will offer to
   install `XR Plugin Management` and `OpenXR Plugin` as dependencies — accept
   those). Optionally install the Universal Render Pipeline for better mobile
   GPU performance on Quest.
2. **Project Settings → XR Plug-in Management**: switch to the Android tab,
   check "OpenXR", and enable the Meta Quest / Oculus Touch controller profile
   under OpenXR's interaction profiles.
3. **Build Settings**: switch platform to Android, set minimum API level to
   Quest's minimum (API 29+), scripting backend IL2CPP, target architecture
   ARM64, and enable Vulkan or OpenGLES3 graphics API.
4. **Replace the desktop preview camera**: swap the `Main Camera` +
   `DesktopFlyCamera` in `Bedroom.unity` for the XR Interaction Toolkit's
   "XR Origin (VR)" prefab (from its Starter Assets sample), positioned at the
   `PlayerSpawn` transform under the `Bedroom` GameObject.
5. Then: the actual Spider-Man mechanics (web-swinging/traversal, etc.) on top
   of this foundation.
