using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMover : MonoBehaviour
{
    public float moveSpeed = 6f;
    public float accel = 20f;
    public float groundY = 0.1f;

    Rigidbody rb;
    Camera cam;
    Vector3 desiredVel;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        // Camera-aligned XZ axes
        Vector3 fwd = cam.transform.forward; fwd.y = 0f; fwd.Normalize();
        Vector3 right = cam.transform.right; right.y = 0f; right.Normalize();

        // ---- Direct key input (WASD + Arrow keys), no analog drift ----
        float h = 0f, v = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  h -= 1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) h += 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    v += 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  v -= 1f;

        Vector3 input = new Vector3(h, 0f, v);
        if (input.sqrMagnitude > 0f) input.Normalize();

        desiredVel = input * moveSpeed;

        // Pin to ground height
        var p = transform.position;
        if (Mathf.Abs(p.y - groundY) > 0.0001f)
            transform.position = new Vector3(p.x, groundY, p.z);
    }

    void FixedUpdate()
    {
        // Smooth toward desired velocity; snap to 0 when no input to avoid drift
        Vector3 vel = rb.velocity; vel.y = 0f;
        Vector3 target = desiredVel;

        // if no input, decelerate hard to a full stop
        if (target == Vector3.zero && vel.sqrMagnitude < 0.0001f)
            rb.velocity = Vector3.zero;
        else
            rb.velocity = Vector3.MoveTowards(vel, target, accel * Time.fixedDeltaTime);
    }
}
