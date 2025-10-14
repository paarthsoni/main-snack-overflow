using UnityEngine;

public class SpawnManagerFlat : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Transform playerSpawn;

    [Header("NPCs")]
    public GameObject npcPrefab;
    public int civilianCount = 10;
    public int impostorCount = 2;

    public Transform[] npcSpawns;
    public BoxCollider movementBounds;     // Bounds3D
    public PathShape[] impostorPaths;

    [Header("Parents")]
    public Transform npcsParent;
    public Transform playerParent;

    void Start(){ SpawnPlayer(); SpawnCivilians(); SpawnImpostors(); }

    void SpawnPlayer() {
        Vector3 p = playerSpawn?playerSpawn.position:Vector3.zero; p.y=0f;
        Instantiate(playerPrefab, p, Quaternion.identity, playerParent);
    }

    void SpawnCivilians() {
        for (int i=0;i<civilianCount;i++) {
            Transform sp = npcSpawns[Random.Range(0, npcSpawns.Length)];
            Vector3 pos = new Vector3(sp.position.x, 0f, sp.position.z);
            var npc = Instantiate(npcPrefab, pos, Quaternion.identity, npcsParent);
            var w = npc.AddComponent<NPCWander>();
            w.movementBounds = movementBounds;
        }
    }

    void SpawnImpostors() {
        for (int i=0;i<impostorCount;i++) {
            Transform sp = npcSpawns[Random.Range(0, npcSpawns.Length)];
            Vector3 pos = new Vector3(sp.position.x, 0f, sp.position.z);
            var npc = Instantiate(npcPrefab, pos, Quaternion.identity, npcsParent);
            var f = npc.AddComponent<PathFollower>();
            f.pathShape = impostorPaths[i % impostorPaths.Length];
        }
    }
}
