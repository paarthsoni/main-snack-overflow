using UnityEngine;

[ExecuteAlways]
public class WorldBoundsBuilder : MonoBehaviour
{
    [Tooltip("Drag the parent that contains ALL green ground meshes (e.g., your 'Ground' root).")]
    public Transform sourceRoot;

    [Header("Wall placement")]
    public float yCenter = 2f;     // player capsule center Y
    public float height  = 8f;     // how tall the wall is
    public float thickness = 0.5f; // wall thickness
    public float padding   = 0.0f; // 0 = sit exactly on ground border

    void OnEnable()   { Build(); }
    void OnValidate() { Build(); }

    void Build()
    {
        if (!sourceRoot) return;

        // Union of all renderers under Ground
        var rs = sourceRoot.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return;

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);

        // Exact inside faces: inner face sits on min/max of ground bounds.
        float minX = b.min.x + 0f;
        float maxX = b.max.x - 0f;
        float minZ = b.min.z + 0f;
        float maxZ = b.max.z - 0f;

        // Push the wall center just outside by half thickness, add optional padding
        Setup("North", new Vector3(b.center.x, yCenter, maxZ + (thickness * 0.5f) + padding),
              new Vector3((maxX - minX) + 2f * padding, height, thickness));

        Setup("South", new Vector3(b.center.x, yCenter, minZ - (thickness * 0.5f) - padding),
              new Vector3((maxX - minX) + 2f * padding, height, thickness));

        Setup("East",  new Vector3(maxX + (thickness * 0.5f) + padding, yCenter, b.center.z),
              new Vector3(thickness, height, (maxZ - minZ) + 2f * padding));

        Setup("West",  new Vector3(minX - (thickness * 0.5f) - padding, yCenter, b.center.z),
              new Vector3(thickness, height, (maxZ - minZ) + 2f * padding));
    }

    void Setup(string name, Vector3 worldPos, Vector3 size)
    {
        var t = transform.Find(name);
        if (!t)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform, false);
            t = go.transform;
            go.AddComponent<BoxCollider>().isTrigger = false;
        }

        t.position  = worldPos;                 // world-space placement
        t.rotation  = Quaternion.identity;      // axis-aligned with ground
        t.GetComponent<BoxCollider>().size = size;

        // make sure they're invisible
        var mr = t.GetComponent<MeshRenderer>();
        if (mr) mr.enabled = false;
    }
}
