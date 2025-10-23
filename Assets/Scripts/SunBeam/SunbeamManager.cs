using UnityEngine;

public class SunbeamManager : MonoBehaviour
{
    public static SunbeamManager Instance { get; private set; }
    public GameObject sunbeamPrefab; // assign the Sunbeam prefab in Inspector

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void Smite(NPCDeath npcDeath)
    {
        if (npcDeath == null) return;

        // Get impostor flag from NPCIdentity
        var identity = npcDeath.GetComponent<NPCIdentity>();
        bool isImpostor = identity != null && identity.isImpostor;

        var go = Instantiate(sunbeamPrefab);
        var beam = go.GetComponent<Sunbeam>();

        beam.Init(npcDeath.transform, () => npcDeath.DieCute(), isImpostor);
    }
}
