using UnityEngine;
public class PathShape : MonoBehaviour
{
    public enum ShapeType { Triangle, Square, Pentagon, Circle }
    public ShapeType shape = ShapeType.Triangle;
    [Min(0.5f)] public float radius = 6f;
    [Range(3,64)] public int circlePoints = 20;
    public float rotationDegY = 0f;
    public bool closed = true;

    public Vector3[] GetPoints() {
        int count = shape==ShapeType.Circle ? circlePoints :
                   (shape==ShapeType.Square ? 4 : (shape==ShapeType.Pentagon?5:3));
        Vector3[] pts = new Vector3[count];
        float rot = rotationDegY * Mathf.Deg2Rad;
        for (int i=0;i<count;i++) {
            float a = (Mathf.PI*2f)*i/count + rot;
            pts[i] = transform.position + new Vector3(Mathf.Cos(a)*radius, 0f, Mathf.Sin(a)*radius);
        }
        return pts;
    }

    void OnDrawGizmos() {
        var pts = GetPoints(); if (pts==null || pts.Length==0) return;
        Gizmos.color = Color.yellow;
        for (int i=0;i<pts.Length;i++) {
            Gizmos.DrawSphere(pts[i] + Vector3.up*0.02f, 0.12f);
            int j=(i+1)%pts.Length;
            if (!closed && i==pts.Length-1) break;
            Gizmos.DrawLine(pts[i], pts[j]);
        }
    }
}
