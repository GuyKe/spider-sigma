using System.Collections.Generic;
using UnityEngine;

// Procedurally builds the player-spawn bedroom out of primitives so the
// layout can be iterated on without any imported art. Re-runs automatically
// in the editor (ExecuteAlways) and on Play; right-click the component
// header and choose "Rebuild Room" to force a rebuild after tweaking the
// public fields.
[ExecuteAlways]
public class RoomBuilder : MonoBehaviour
{
    [Header("Room Dimensions (meters)")]
    public float width = 4.5f;
    public float depth = 5.5f;
    public float height = 2.7f;

    public Transform playerSpawn;

    static Shader _shader;
    Dictionary<Color, Material> _materialCache = new Dictionary<Color, Material>();

    void OnEnable()
    {
        Build();
    }

    [ContextMenu("Rebuild Room")]
    public void Build()
    {
        Clear();
        BuildShell();
        BuildWindow();
        BuildDoor();
        BuildBookshelf();
        BuildDesk();
        BuildChair();
        BuildBed();
        BuildCorkboardAndPosters();
        BuildLamp();
        BuildClutter();
        BuildSpawnPoint();
        PositionMainCamera();
    }

    void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            var child = transform.GetChild(i).gameObject;
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
        _materialCache.Clear();
    }

    static Shader GetShader()
    {
        if (_shader != null) return _shader;
        string[] names =
        {
            "Universal Render Pipeline/Lit",
            "Standard",
            "HDRP/Lit",
            "Diffuse",
            "Legacy Shaders/Diffuse",
            "Unlit/Color",
        };
        foreach (var n in names)
        {
            var s = Shader.Find(n);
            if (s != null) { _shader = s; break; }
        }
        return _shader;
    }

    Material GetMat(Color c)
    {
        if (_materialCache.TryGetValue(c, out var m)) return m;
        m = new Material(GetShader());
        m.color = c;
        _materialCache[c] = m;
        return m;
    }

    GameObject CreatePart(string name, Transform parent, PrimitiveType type, Vector3 localPos, Vector3 localEuler, Vector3 localScale, Color color)
    {
        var go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.localPosition = localPos;
        go.transform.localEulerAngles = localEuler;
        go.transform.localScale = localScale;
        var rend = go.GetComponent<Renderer>();
        if (rend != null) rend.sharedMaterial = GetMat(color);
        return go;
    }

    void BuildShell()
    {
        var shell = new GameObject("Shell").transform;
        shell.SetParent(transform, false);

        Color floorColor = new Color(0.45f, 0.30f, 0.18f);
        Color ceilingColor = new Color(0.92f, 0.90f, 0.85f);
        Color wallColor = new Color(0.85f, 0.82f, 0.74f);

        CreatePart("Floor", shell, PrimitiveType.Cube, new Vector3(0, -0.05f, 0), Vector3.zero, new Vector3(width, 0.1f, depth), floorColor);
        CreatePart("Ceiling", shell, PrimitiveType.Cube, new Vector3(0, height + 0.05f, 0), Vector3.zero, new Vector3(width, 0.1f, depth), ceilingColor);
        CreatePart("Wall_Left", shell, PrimitiveType.Cube, new Vector3(-width / 2f, height / 2f, 0), Vector3.zero, new Vector3(0.1f, height, depth), wallColor);
        CreatePart("Wall_Right", shell, PrimitiveType.Cube, new Vector3(width / 2f, height / 2f, 0), Vector3.zero, new Vector3(0.1f, height, depth), wallColor);
        CreatePart("Wall_Back", shell, PrimitiveType.Cube, new Vector3(0, height / 2f, depth / 2f), Vector3.zero, new Vector3(width, height, 0.1f), wallColor);
        CreatePart("Wall_Front", shell, PrimitiveType.Cube, new Vector3(0, height / 2f, -depth / 2f), Vector3.zero, new Vector3(width, height, 0.1f), wallColor);
    }

    void BuildWindow()
    {
        var win = new GameObject("Window").transform;
        win.SetParent(transform, false);

        float x = -width / 2f + 0.06f;
        float centerZ = -0.6f;
        float winWidth = 2.4f, winHeight = 1.5f, sill = 0.9f;

        CreatePart("WindowLight", win, PrimitiveType.Cube, new Vector3(x, sill + winHeight / 2f, centerZ), new Vector3(0, 90, 0), new Vector3(0.02f, winHeight, winWidth), new Color(0.95f, 0.97f, 1f));

        Color frameColor = new Color(0.9f, 0.88f, 0.8f);
        float t = 0.06f;
        CreatePart("Frame_Top", win, PrimitiveType.Cube, new Vector3(x, sill + winHeight + t / 2f, centerZ), new Vector3(0, 90, 0), new Vector3(0.04f, t, winWidth + t), frameColor);
        CreatePart("Frame_Bottom", win, PrimitiveType.Cube, new Vector3(x, sill - t / 2f, centerZ), new Vector3(0, 90, 0), new Vector3(0.04f, t, winWidth + t), frameColor);
        CreatePart("Frame_Left", win, PrimitiveType.Cube, new Vector3(x, sill + winHeight / 2f, centerZ - winWidth / 2f), new Vector3(0, 90, 0), new Vector3(0.04f, winHeight + t, t), frameColor);
        CreatePart("Frame_Right", win, PrimitiveType.Cube, new Vector3(x, sill + winHeight / 2f, centerZ + winWidth / 2f), new Vector3(0, 90, 0), new Vector3(0.04f, winHeight + t, t), frameColor);
        CreatePart("Frame_Mullion", win, PrimitiveType.Cube, new Vector3(x, sill + winHeight / 2f, centerZ), new Vector3(0, 90, 0), new Vector3(0.04f, winHeight, t), frameColor);

        Color blindColor = new Color(0.82f, 0.78f, 0.68f);
        int slats = 10;
        float blindTop = sill + winHeight;
        float blindBottom = sill + winHeight * 0.15f;
        for (int i = 0; i < slats; i++)
        {
            float tlerp = i / (float)(slats - 1);
            float yPos = Mathf.Lerp(blindTop, blindBottom, tlerp);
            CreatePart($"Slat_{i}", win, PrimitiveType.Cube, new Vector3(x + 0.05f, yPos, centerZ), new Vector3(15, 90, 0), new Vector3(0.01f, 0.06f, winWidth - 0.05f), blindColor);
        }
    }

    void BuildDoor()
    {
        var doorRoot = new GameObject("Door").transform;
        doorRoot.SetParent(transform, false);

        float doorWidth = 0.9f, doorHeight = 2.05f;
        float dx = 0.6f;
        float z = depth / 2f - 0.06f;
        Color frameColor = new Color(0.55f, 0.42f, 0.28f);

        CreatePart("DoorFrame_Top", doorRoot, PrimitiveType.Cube, new Vector3(dx, doorHeight + 0.05f, z), Vector3.zero, new Vector3(doorWidth + 0.1f, 0.1f, 0.06f), frameColor);
        CreatePart("DoorFrame_Left", doorRoot, PrimitiveType.Cube, new Vector3(dx - doorWidth / 2f - 0.05f, doorHeight / 2f, z), Vector3.zero, new Vector3(0.1f, doorHeight, 0.06f), frameColor);
        CreatePart("DoorFrame_Right", doorRoot, PrimitiveType.Cube, new Vector3(dx + doorWidth / 2f + 0.05f, doorHeight / 2f, z), Vector3.zero, new Vector3(0.1f, doorHeight, 0.06f), frameColor);
        CreatePart("DoorwayLight", doorRoot, PrimitiveType.Cube, new Vector3(dx, doorHeight / 2f, z + 0.15f), Vector3.zero, new Vector3(doorWidth, doorHeight, 0.02f), new Color(0.98f, 0.95f, 0.85f));

        var hinge = new GameObject("DoorHinge").transform;
        hinge.SetParent(doorRoot, false);
        hinge.localPosition = new Vector3(dx - doorWidth / 2f, 0, z);
        hinge.localEulerAngles = new Vector3(0, 35, 0);
        CreatePart("DoorSlab", hinge, PrimitiveType.Cube, new Vector3(doorWidth / 2f, doorHeight / 2f, 0), Vector3.zero, new Vector3(doorWidth, doorHeight, 0.05f), new Color(0.62f, 0.47f, 0.3f));
    }

    void BuildBookshelf()
    {
        var shelf = new GameObject("Bookshelf").transform;
        shelf.SetParent(transform, false);
        float x = -width / 2f + 0.35f;
        float zCenter = -1.6f;
        float shelfWidth = 1.5f, shelfHeight = 2.0f, shelfDepth = 0.35f;
        shelf.localPosition = new Vector3(x, 0, zCenter);

        Color caseColor = new Color(0.3f, 0.2f, 0.12f);
        CreatePart("Case_Back", shelf, PrimitiveType.Cube, new Vector3(0, shelfHeight / 2f, -shelfDepth / 2f), Vector3.zero, new Vector3(shelfWidth, shelfHeight, 0.04f), caseColor);
        CreatePart("Case_Side_A", shelf, PrimitiveType.Cube, new Vector3(-shelfWidth / 2f, shelfHeight / 2f, 0), Vector3.zero, new Vector3(0.04f, shelfHeight, shelfDepth), caseColor);
        CreatePart("Case_Side_B", shelf, PrimitiveType.Cube, new Vector3(shelfWidth / 2f, shelfHeight / 2f, 0), Vector3.zero, new Vector3(0.04f, shelfHeight, shelfDepth), caseColor);
        CreatePart("Case_Top", shelf, PrimitiveType.Cube, new Vector3(0, shelfHeight, 0), Vector3.zero, new Vector3(shelfWidth, 0.04f, shelfDepth), caseColor);

        int rows = 4;
        var rng = new System.Random(7);
        for (int r = 0; r < rows; r++)
        {
            float shelfY = (r + 1) * shelfHeight / (rows + 1f);
            CreatePart($"Shelf_{r}", shelf, PrimitiveType.Cube, new Vector3(0, shelfY, 0), Vector3.zero, new Vector3(shelfWidth, 0.03f, shelfDepth), caseColor);
            float cursor = -shelfWidth / 2f + 0.05f;
            while (cursor < shelfWidth / 2f - 0.05f)
            {
                float bw = 0.03f + (float)rng.NextDouble() * 0.04f;
                float bh = 0.18f + (float)rng.NextDouble() * 0.08f;
                if (cursor + bw > shelfWidth / 2f - 0.05f) break;
                Color bc = Color.HSVToRGB((float)rng.NextDouble(), 0.5f, 0.6f);
                CreatePart("Book", shelf, PrimitiveType.Cube, new Vector3(cursor + bw / 2f, shelfY + bh / 2f + 0.015f, 0), new Vector3(0, (float)(rng.NextDouble() * 6 - 3), 0), new Vector3(bw, bh, shelfDepth - 0.06f), bc);
                cursor += bw + 0.005f;
            }
        }
    }

    void BuildDesk()
    {
        var desk = new GameObject("Desk").transform;
        desk.SetParent(transform, false);
        float x = width / 2f - 0.45f;
        float z = 1.0f;
        desk.localPosition = new Vector3(x, 0, z);

        Color woodColor = new Color(0.55f, 0.4f, 0.25f);
        CreatePart("DeskTop", desk, PrimitiveType.Cube, new Vector3(0, 0.75f, 0), Vector3.zero, new Vector3(0.7f, 0.04f, 1.3f), woodColor);
        float legY = 0.375f;
        CreatePart("Leg1", desk, PrimitiveType.Cube, new Vector3(-0.3f, legY, -0.6f), Vector3.zero, new Vector3(0.05f, 0.75f, 0.05f), woodColor);
        CreatePart("Leg2", desk, PrimitiveType.Cube, new Vector3(0.3f, legY, -0.6f), Vector3.zero, new Vector3(0.05f, 0.75f, 0.05f), woodColor);
        CreatePart("Leg3", desk, PrimitiveType.Cube, new Vector3(-0.3f, legY, 0.6f), Vector3.zero, new Vector3(0.05f, 0.75f, 0.05f), woodColor);
        CreatePart("Leg4", desk, PrimitiveType.Cube, new Vector3(0.3f, legY, 0.6f), Vector3.zero, new Vector3(0.05f, 0.75f, 0.05f), woodColor);

        CreatePart("MonitorStand", desk, PrimitiveType.Cube, new Vector3(0, 0.83f, 0.2f), Vector3.zero, new Vector3(0.06f, 0.15f, 0.06f), Color.black);
        CreatePart("MonitorScreen", desk, PrimitiveType.Cube, new Vector3(0, 1.02f, 0.2f), Vector3.zero, new Vector3(0.55f, 0.32f, 0.03f), new Color(0.05f, 0.05f, 0.06f));
        CreatePart("Keyboard", desk, PrimitiveType.Cube, new Vector3(0, 0.775f, 0.45f), Vector3.zero, new Vector3(0.35f, 0.02f, 0.13f), new Color(0.1f, 0.1f, 0.1f));
        CreatePart("DeskLamp_Base", desk, PrimitiveType.Cylinder, new Vector3(-0.25f, 0.78f, 0.55f), Vector3.zero, new Vector3(0.08f, 0.02f, 0.08f), Color.black);
    }

    void BuildChair()
    {
        var chair = new GameObject("Chair").transform;
        chair.SetParent(transform, false);
        chair.localPosition = new Vector3(width / 2f - 0.9f, 0, 0.9f);
        chair.localEulerAngles = new Vector3(0, 160, 0);

        Color red = new Color(0.65f, 0.1f, 0.1f);
        Color black = new Color(0.08f, 0.08f, 0.08f);
        CreatePart("SeatCushion", chair, PrimitiveType.Cube, new Vector3(0, 0.48f, 0), Vector3.zero, new Vector3(0.45f, 0.08f, 0.45f), red);
        CreatePart("Backrest", chair, PrimitiveType.Cube, new Vector3(0, 0.85f, -0.2f), new Vector3(-10, 0, 0), new Vector3(0.42f, 0.6f, 0.08f), red);
        CreatePart("ChairPost", chair, PrimitiveType.Cylinder, new Vector3(0, 0.24f, 0), Vector3.zero, new Vector3(0.05f, 0.24f, 0.05f), black);
        CreatePart("ChairBase", chair, PrimitiveType.Cylinder, new Vector3(0, 0.03f, 0), Vector3.zero, new Vector3(0.3f, 0.02f, 0.3f), black);
        CreatePart("DraggedJacket", chair, PrimitiveType.Cube, new Vector3(0, 0.7f, -0.28f), new Vector3(20, 10, 0), new Vector3(0.4f, 0.35f, 0.15f), new Color(0.2f, 0.25f, 0.35f));
    }

    void BuildBed()
    {
        var bed = new GameObject("Bed").transform;
        bed.SetParent(transform, false);
        float x = -width / 2f + 1.1f;
        float z = -depth / 2f + 1.1f;
        bed.localPosition = new Vector3(x, 0, z);

        Color frameColor = new Color(0.4f, 0.28f, 0.18f);
        Color sheetColor = new Color(0.75f, 0.78f, 0.8f);
        Color blanketColor = new Color(0.25f, 0.35f, 0.5f);
        CreatePart("BedFrame", bed, PrimitiveType.Cube, new Vector3(0, 0.22f, 0), Vector3.zero, new Vector3(1.5f, 0.3f, 2.0f), frameColor);
        CreatePart("Mattress", bed, PrimitiveType.Cube, new Vector3(0, 0.42f, 0), Vector3.zero, new Vector3(1.45f, 0.18f, 1.95f), sheetColor);
        CreatePart("Blanket", bed, PrimitiveType.Cube, new Vector3(0.05f, 0.53f, 0.3f), new Vector3(2, 0, 3), new Vector3(1.3f, 0.12f, 1.3f), blanketColor);
        CreatePart("Pillow", bed, PrimitiveType.Cube, new Vector3(-0.4f, 0.56f, -0.75f), new Vector3(0, 8, 4), new Vector3(0.5f, 0.18f, 0.35f), Color.white);
    }

    void BuildCorkboardAndPosters()
    {
        // Plain colored placeholders standing in for the framed photos/posters
        // in the reference image. Swap these for your own art later - the
        // reference still is copyrighted, so we don't reproduce its poster
        // artwork here.
        var deco = new GameObject("WallDecor").transform;
        deco.SetParent(transform, false);
        Color boardColor = new Color(0.55f, 0.42f, 0.3f);

        CreatePart("Corkboard", deco, PrimitiveType.Cube, new Vector3(width / 2f - 0.05f, 1.8f, 1.9f), new Vector3(0, 90, 0), new Vector3(0.03f, 0.9f, 0.7f), boardColor);

        var rng = new System.Random(3);
        for (int i = 0; i < 8; i++)
        {
            float py = 1.8f + ((float)rng.NextDouble() - 0.5f) * 0.7f;
            float pz = 1.9f + ((float)rng.NextDouble() - 0.5f) * 0.55f;
            Color pc = Color.HSVToRGB((float)rng.NextDouble(), 0.4f, 0.9f);
            CreatePart("PinnedPhoto", deco, PrimitiveType.Cube, new Vector3(width / 2f - 0.07f, py, pz), new Vector3(0, 90, (float)(rng.NextDouble() * 20 - 10)), new Vector3(0.01f, 0.1f, 0.08f), pc);
        }

        CreatePart("MapPoster", deco, PrimitiveType.Cube, new Vector3(width / 2f - 0.05f, 2.0f, 0.6f), new Vector3(0, 90, 0), new Vector3(0.02f, 1.1f, 0.75f), new Color(0.85f, 0.8f, 0.65f));
        CreatePart("FramedPicture", deco, PrimitiveType.Cube, new Vector3(-0.5f, 1.7f, depth / 2f - 0.05f), Vector3.zero, new Vector3(0.4f, 0.5f, 0.02f), new Color(0.2f, 0.2f, 0.22f));
        CreatePart("Poster_A", deco, PrimitiveType.Cube, new Vector3(1.5f, 1.7f, depth / 2f - 0.05f), Vector3.zero, new Vector3(0.5f, 0.7f, 0.02f), new Color(0.75f, 0.15f, 0.15f));
        CreatePart("Poster_B", deco, PrimitiveType.Cube, new Vector3(-width / 2f + 0.06f, 2.1f, -1.8f), new Vector3(0, 90, 0), new Vector3(0.02f, 0.6f, 0.45f), new Color(0.15f, 0.15f, 0.18f));
    }

    void BuildLamp()
    {
        var lamp = new GameObject("PendantLamp").transform;
        lamp.SetParent(transform, false);
        Vector3 pos = new Vector3(width / 2f - 1.1f, 0, 1.3f);

        CreatePart("Cord", lamp, PrimitiveType.Cylinder, new Vector3(pos.x, height - 0.5f, pos.z), Vector3.zero, new Vector3(0.01f, 0.5f, 0.01f), Color.black);
        CreatePart("ShadeOuter", lamp, PrimitiveType.Cylinder, new Vector3(pos.x, height - 1.05f, pos.z), Vector3.zero, new Vector3(0.35f, 0.1f, 0.35f), new Color(0.75f, 0.1f, 0.1f));

        var lightGO = new GameObject("LampLight");
        lightGO.transform.SetParent(lamp, false);
        lightGO.transform.localPosition = new Vector3(pos.x, height - 1.15f, pos.z);
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = new Color(1f, 0.75f, 0.55f);
        light.intensity = 3f;
        light.range = 5f;
    }

    void BuildClutter()
    {
        var clutter = new GameObject("FloorClutter").transform;
        clutter.SetParent(transform, false);
        var rng = new System.Random(11);
        Vector2[] zones = { new Vector2(-1.6f, -0.6f), new Vector2(-0.3f, 0.8f), new Vector2(1.0f, -1.0f) };

        for (int i = 0; i < 14; i++)
        {
            Vector2 zone = zones[i % zones.Length];
            float x = zone.x + ((float)rng.NextDouble() - 0.5f) * 0.8f;
            float z = zone.y + ((float)rng.NextDouble() - 0.5f) * 0.8f;
            x = Mathf.Clamp(x, -width / 2f + 0.3f, width / 2f - 0.3f);
            z = Mathf.Clamp(z, -depth / 2f + 0.3f, depth / 2f - 0.3f);
            float s = 0.08f + (float)rng.NextDouble() * 0.1f;
            Color c = Color.HSVToRGB((float)rng.NextDouble(), 0.35f, 0.55f + (float)rng.NextDouble() * 0.3f);
            CreatePart("ClutterItem", clutter, PrimitiveType.Cube, new Vector3(x, s / 2f, z), new Vector3(0, (float)(rng.NextDouble() * 360), 0), new Vector3(0.25f + (float)rng.NextDouble() * 0.15f, s, 0.18f + (float)rng.NextDouble() * 0.12f), c);
        }
    }

    void BuildSpawnPoint()
    {
        var spawn = new GameObject("PlayerSpawn").transform;
        spawn.SetParent(transform, false);
        spawn.localPosition = new Vector3(0, 0, -0.5f);
        spawn.localEulerAngles = Vector3.zero; // facing toward the door
        playerSpawn = spawn;
    }

    void PositionMainCamera()
    {
        var cam = Camera.main;
        if (cam != null && playerSpawn != null)
        {
            cam.transform.position = playerSpawn.position + Vector3.up * 1.6f;
            cam.transform.rotation = playerSpawn.rotation;
        }
    }
}
