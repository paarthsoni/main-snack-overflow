using UnityEngine;

public class SunbeamManager : MonoBehaviour
{
    public static SunbeamManager Instance { get; private set; }
    public GameObject sunbeamPrefab; // assign the Sunbeam prefab in Inspector

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Smite(NPCDeath npcDeath)
    {
        if (npcDeath == null) return;
        var go = Instantiate(sunbeamPrefab);
        var beam = go.GetComponent<Sunbeam>();
        beam.Init(npcDeath.transform, () => npcDeath.DieCute());
    }
}
