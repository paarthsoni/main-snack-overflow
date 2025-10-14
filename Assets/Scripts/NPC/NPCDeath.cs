using UnityEngine;
using System.Collections;

public class NPCDeath : MonoBehaviour
{
    public bool scaleDownOnDeath = true;
    public float scaleDownTime = 0.2f;

    public void DieCute()
    {
        if (scaleDownOnDeath && gameObject.activeInHierarchy)
            StartCoroutine(ScaleDownThenDestroy());
        else
            Destroy(gameObject);
    }

    private IEnumerator ScaleDownThenDestroy()
    {
        Vector3 start = transform.localScale;
        float t = 0f;
        while (t < scaleDownTime)
        {
            transform.localScale = Vector3.Lerp(start, Vector3.zero, t / scaleDownTime);
            t += Time.deltaTime;
            yield return null;
        }
        Destroy(gameObject);
    }
}
