using UnityEngine;

public class CameraFollowPlayer : MonoBehaviour
{
    [SerializeField] Transform target;              
    [SerializeField] bool useCurrentOffset = true;  
    [SerializeField] Vector3 fallbackOffset = new Vector3(30f, 50f, -30f);
    [SerializeField] float smoothTime = 0.25f;

    Vector3 _offset, _vel;
    bool _offsetSet;

    void OnEnable() { TryAttach(); }
    void Update()
    {
       
        if (!target) TryAttach();
    }

    void LateUpdate()
    {
        if (!target || !_offsetSet) return;
        Vector3 desired = target.position + _offset;
        transform.position = Vector3.SmoothDamp(transform.position, desired, ref _vel, smoothTime);
        
    }

    void TryAttach()
    {
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (!p) return;
            target = p.transform;
        }

        if (!_offsetSet)
        {
            _offset = useCurrentOffset ? (transform.position - target.position) : fallbackOffset;
            if (Mathf.Abs(_offset.z) < 0.01f) _offset.z = -10f; // safety for ortho
            _offsetSet = true;
        }
    }

    
    public void SetTarget(Transform t, bool recalcOffset = true)
    {
        target = t;
        if (recalcOffset)
        {
            _offset = transform.position - t.position;
            if (Mathf.Abs(_offset.z) < 0.01f) _offset.z = -10f;
            _offsetSet = true;
        }
    }
}
