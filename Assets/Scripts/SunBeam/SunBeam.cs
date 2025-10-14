using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class Sunbeam : MonoBehaviour
{
    public float skyHeight = 20f;
    public float chargeTime = 0.25f; // pre-impact tracking
    public float impactTime = 0.35f; // visible “zap” time
    public float fadeTime = 0.2f;

    private Transform target;
    private LineRenderer lr;
    private Action onImpact;
    private bool hasImpacted;

    public void Init(Transform target, Action onImpact)
    {
        this.target = target;
        this.onImpact = onImpact;
    }

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.enabled = false;
    }

    void OnEnable() { StartCoroutine(RunBeam()); }

    IEnumerator RunBeam()
    {
        lr.enabled = true;

        // charge/lock-on
        float t = 0f;
        while (t < chargeTime)
        {
            UpdateBeamPositions();
            t += Time.deltaTime; yield return null;
        }

        // impact window
        t = 0f;
        while (t < impactTime)
        {
            UpdateBeamPositions();
            t += Time.deltaTime; yield return null;
        }

        if (!hasImpacted)
        {
            hasImpacted = true;
            onImpact?.Invoke(); // trigger NPCDeath.DieCute()
        }

        // fade out
        t = 0f;
        float startWidth = lr.widthMultiplier;
        while (t < fadeTime)
        {
            lr.widthMultiplier = Mathf.Lerp(startWidth, 0f, t / fadeTime);
            t += Time.deltaTime; yield return null;
        }

        Destroy(gameObject);
    }

    void Update()
    {
        if (target == null && !hasImpacted)
            Destroy(gameObject);
    }

    void UpdateBeamPositions()
    {
        if (target == null) return;
        Vector3 bottom = target.position;
        Vector3 top = bottom + Vector3.up * skyHeight;
        lr.SetPosition(0, top);
        lr.SetPosition(1, bottom);
    }
}
