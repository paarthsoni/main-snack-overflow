// // PathFollowerFlat.cs
// using UnityEngine;

// [RequireComponent(typeof(Rigidbody))]
// public class PathFollower : MonoBehaviour
// {
//     public PathShape pathShape;
//     public float speed = 2.4f;
//     public float waypointTolerance = 0.08f;
//     public Vector2 pauseRange = new Vector2(0.2f, 0.8f);
//     public bool loop = true;
//     [Range(0.15f, 1f)] public float pathScale = 0.55f;

//     Rigidbody rb;
//     Vector3[] points;
//     int idx;
//     float pauseTimer;
//     bool movingForward = true;
//     Vector3 center;

//     void Awake() => rb = GetComponent<Rigidbody>();

//     void Start() {
//         points = pathShape ? pathShape.GetPoints() : null;
//         if (points == null || points.Length == 0) { enabled = false; return; }
//         center = pathShape.transform.position;
//         float scale = Mathf.Clamp(pathScale, 0.15f, 1f);
//         for (int i=0;i<points.Length;i++)
//         {
//             points[i] = Vector3.Lerp(center, points[i], scale);
//         }
//         SnapToNearestPoint();
//     }

//     void Update()
//     {
//         if (pauseTimer>0f) { pauseTimer-=Time.deltaTime; rb.velocity=Vector3.zero; return; }
//         Vector3 pos = transform.position, tgt = points[idx];
//         Vector3 dir = tgt - pos; dir.y=0f;

//         if (dir.sqrMagnitude <= waypointTolerance*waypointTolerance) {
//             pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
//             idx = NextIndex(idx);
//             return;
//         }
//         rb.velocity = dir.normalized * speed;
//     }

//     void SnapToNearestPoint()
//     {
//         float best = float.MaxValue; int bestIdx = 0;
//         for (int i = 0; i < points.Length; i++)
//         {
//             float d = new Vector2(points[i].x - transform.position.x, points[i].z - transform.position.z).sqrMagnitude;
//             if (d < best) { best = d; bestIdx = i; }
//         }
//         idx = bestIdx;
//     }
    
//     int NextIndex(int current)
//     {
//         if (movingForward)
//         {
//             int next = current + 1;
//             if (next >= points.Length)
//             {
//                 if (!loop)
//                 {
//                     movingForward = false;
//                     System.Array.Reverse(points);
//                     return points.Length > 1 ? 1 : 0;
//                 }
//                 return 0;
//             }
//             return next;
//         }

//         int prev = current - 1;
//         if (prev < 0)
//         {
//             if (!loop)
//             {
//                 movingForward = true;
//                 System.Array.Reverse(points);
//                 return points.Length > 1 ? 1 : 0;
//             }
//             return points.Length-1;
//         }
//         return prev;
//     }

//     void OnCollisionEnter(Collision collision)
//     {
//         if (collision.collider.isTrigger || points == null || points.Length == 0) return;

//         bool hitGround = false;
//         foreach (var contact in collision.contacts)
//         {
//             if (contact.normal.y > 0.5f)
//             {
//                 hitGround = true;
//                 break;
//             }
//         }

//         if (hitGround) return;

//         rb.velocity = Vector3.zero;
//         movingForward = !movingForward;
//         idx = movingForward ? 0 : points.Length - 1;
//         transform.position = points[idx];
//         pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
//     }
// }


using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PathFollower : MonoBehaviour
{
    public PathShape pathShape;
    public float speed = 2.4f;
    public float waypointTolerance = 0.08f;
    public Vector2 pauseRange = new Vector2(0.2f, 0.8f);
    public bool loop = true;
    [Range(0.15f, 1f)] public float pathScale = 0.55f;

    [Header("Grounding")]
    public LayerMask groundMask = ~0;
    public float groundProbeHeight = 4f;
    public float groundProbeDistance = 10f;

    Rigidbody rb;
    Vector3[] points;
    int idx;
    float pauseTimer;
    bool movingForward = true;
    Vector3 center;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Start() {
        points = pathShape ? pathShape.GetPoints() : null;
        if (points == null || points.Length == 0) { enabled = false; return; }
        center = pathShape.transform.position;
        float scale = Mathf.Clamp(pathScale, 0.15f, 1f);
        for (int i=0;i<points.Length;i++)
        {
            Vector3 projected = Vector3.Lerp(center, points[i], scale);
            points[i] = ProjectToGround(projected);
        }
        SnapToNearestPoint();
        idx = Random.Range(0, points.Length);
        pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
    }

    void Update()
    {
        if (pauseTimer>0f) { pauseTimer-=Time.deltaTime; rb.velocity=Vector3.zero; return; }
        Vector3 pos = transform.position, tgt = points[idx];
        Vector3 dir = tgt - pos; dir.y=0f;

        if (dir.sqrMagnitude <= waypointTolerance*waypointTolerance) {
            pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
            idx = NextIndex(idx);
            return;
        }
        rb.velocity = dir.normalized * speed;
    }

    void SnapToNearestPoint()
    {
        float best = float.MaxValue; int bestIdx = 0;
        for (int i = 0; i < points.Length; i++)
        {
            float d = new Vector2(points[i].x - transform.position.x, points[i].z - transform.position.z).sqrMagnitude;
            if (d < best) { best = d; bestIdx = i; }
        }
        idx = bestIdx;
    }
    
    int NextIndex(int current)
    {
        if (movingForward)
        {
            int next = current + 1;
            if (next >= points.Length)
            {
                if (!loop)
                {
                    movingForward = false;
                    System.Array.Reverse(points);
                    return points.Length > 1 ? 1 : 0;
                }
                return 0;
            }
            return next;
        }

        int prev = current - 1;
        if (prev < 0)
        {
            if (!loop)
            {
                movingForward = true;
                System.Array.Reverse(points);
                return points.Length > 1 ? 1 : 0;
            }
            return points.Length-1;
        }
        return prev;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.isTrigger || points == null || points.Length == 0) return;

        bool hitGround = false;
        foreach (var contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                hitGround = true;
                break;
            }
        }

        if (hitGround) return;

        rb.velocity = Vector3.zero;
        movingForward = !movingForward;
        idx = movingForward ? 0 : points.Length - 1;
        transform.position = points[idx];
        pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
    }

    Vector3 ProjectToGround(Vector3 position)
    {
        Vector3 origin = position + Vector3.up * groundProbeHeight;
        float maxDistance = groundProbeHeight + groundProbeDistance;
        if (Physics.Raycast(origin, Vector3.down, out var hit, maxDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            position.y = hit.point.y;
        }
        else
        {
            position.y = 0f;
        }
        return position;
    }
}
