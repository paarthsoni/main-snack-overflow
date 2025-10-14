// PathFollowerFlat.cs
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PathFollower : MonoBehaviour
{
    public PathShape pathShape;
    public float speed = 2.4f;
    public float waypointTolerance = 0.08f;
    public Vector2 pauseRange = new Vector2(0.2f, 0.8f);
    public bool loop = true;

    Rigidbody rb;
    Vector3[] points;
    int idx;
    float pauseTimer;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Start() {
        points = pathShape ? pathShape.GetPoints() : null;
        if (points==null || points.Length==0) { enabled=false; return; }
        SnapToNearestPoint();
    }

    void Update()
    {
        if (pauseTimer>0f) { pauseTimer-=Time.deltaTime; rb.velocity=Vector3.zero; return; }
        Vector3 pos = transform.position, tgt = points[idx];
        Vector3 dir = tgt - pos; dir.y=0f;

        if (dir.sqrMagnitude <= waypointTolerance*waypointTolerance) {
            pauseTimer = Random.Range(pauseRange.x, pauseRange.y);
            idx = (idx+1) % points.Length;
            if (!loop && idx==0) System.Array.Reverse(points);
            return;
        }
        rb.velocity = dir.normalized * speed;
    }

    void SnapToNearestPoint()
    {
        float best=float.MaxValue; int bestIdx=0;
        for (int i=0;i<points.Length;i++) {
            float d = new Vector2(points[i].x-transform.position.x, points[i].z-transform.position.z).sqrMagnitude;
            if (d<best){best=d; bestIdx=i;}
        }
        idx = bestIdx;
    }
}
