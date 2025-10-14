using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NPCWander : MonoBehaviour
{
    public float minSpeed = 1.2f;
    public float maxSpeed = 2.2f;
    public Vector2 idleTimeRange = new Vector2(0.4f, 1.2f);
    public BoxCollider movementBounds;   // assign Bounds3D

    Rigidbody rb;
    float speed;
    Vector3 target;
    float idleTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        speed = Random.Range(minSpeed, maxSpeed);
    }

    void Start() => PickNewTarget();

    void Update()
    {
        if (idleTimer > 0f) { idleTimer -= Time.deltaTime; rb.velocity = Vector3.zero; return; }

        Vector3 pos = transform.position;
        Vector3 dir = target - pos; dir.y = 0f;

        if (dir.sqrMagnitude < 0.05f) { idleTimer = Random.Range(idleTimeRange.x, idleTimeRange.y); PickNewTarget(); return; }

        rb.velocity = dir.normalized * speed;
    }

    void PickNewTarget()
    {
        if (!movementBounds)
        {
            target = transform.position + Random.insideUnitSphere * 6f;
            target.y = 0f; return;
        }
        Bounds b = movementBounds.bounds;
        float x = Random.Range(b.min.x, b.max.x);
        float z = Random.Range(b.min.z, b.max.z);
        target = new Vector3(x, 0f, z);
    }
}
