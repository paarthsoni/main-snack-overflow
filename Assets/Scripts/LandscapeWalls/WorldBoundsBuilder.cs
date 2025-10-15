using UnityEngine;

[ExecuteAlways]
public class WorldBoundsBuilder : MonoBehaviour
{
    [Tooltip("Drag the parent that contains ALL green ground meshes (e.g., your 'Ground' root).")]
    public Transform sourceRoot;

    [Header("Wall placement")]
    public float yCenter = 2f;     
    public float height  = 8f;     
    public float thickness = 0.5f; 
    public float padding   = 0.0f; 

    void OnEnable()   { Build(); }
    void OnValidate() { Build(); }

    void Build()
    {
        if (!sourceRoot) return;

        
        var rs = sourceRoot.GetComponentsInChildren<Renderer>(true);
        if (rs.Length == 0) return;

        Bounds b = rs[0].bounds;
        for (int i = 1; i < rs.Length; i++) b.Encapsulate(rs[i].bounds);

        
        float minX = b.min.x + 0f;
        float maxX = b.max.x - 0f;
        float minZ = b.min.z + 0f;
        float maxZ = b.max.z - 0f;

        
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

        t.position  = worldPos;                 
        t.rotation  = Quaternion.identity;      
        t.GetComponent<BoxCollider>().size = size;

       
        var mr = t.GetComponent<MeshRenderer>();
        if (mr) mr.enabled = false;
    }
}
