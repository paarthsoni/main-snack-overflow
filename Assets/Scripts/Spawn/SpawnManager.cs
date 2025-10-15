using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Player")]
    public GameObject playerPrefab;
    public Transform playerSpawn;

    [Header("NPCs")]
    public GameObject npcPrefab;
    public int civilianCount = 30;
    public int impostorCount = 4;

    public Transform[] npcSpawns;
    public BoxCollider movementBounds;     // Bounds3D
    public PathShape[] impostorPaths;

    [Header("Parents")]
    public Transform npcsParent;
    public Transform playerParent;
    readonly List<Vector3> usedSpawnPositions = new List<Vector3>();

    const float minimumSpawnSpacing = 1.1f;
    const float spacingRelaxFactor = 0.7f;
    const int spacingRelaxIterations = 4;
    const int maxAttemptsPerIteration = 24;
    const float defaultPathScale = 0.55f;

    void Start(){ SpawnPlayer(); SpawnCivilians(); SpawnImpostors(); }

    void SpawnPlayer() {
        Vector3 p = playerSpawn?playerSpawn.position:Vector3.zero; p.y=0f;
        Instantiate(playerPrefab, p, Quaternion.identity, playerParent);
        usedSpawnPositions.Add(p);
    }

    void SpawnCivilians() {
        int spawnCount = npcSpawns != null ? npcSpawns.Length : 0;
        for (int i=0;i<civilianCount;i++) {
            Transform sp = spawnCount > 0 ? npcSpawns[Random.Range(0, spawnCount)] : null;
            Vector3 pos = SampleSpawnPosition(sp);
            var npc = Instantiate(npcPrefab, pos, Quaternion.identity, npcsParent);
            ApplyNPCColor(npc);
            var w = npc.AddComponent<NPCWander>();
            w.movementBounds = movementBounds;
        }
    }

    void SpawnImpostors() {
        if (impostorPaths == null || impostorPaths.Length == 0) return;
        int spawnCount = npcSpawns != null ? npcSpawns.Length : 0;
        for (int i = 0; i < impostorCount; i++) {
            Transform sp = spawnCount > 0 ? npcSpawns[Random.Range(0, spawnCount)] : null;
            PathShape shape = impostorPaths[i % impostorPaths.Length];
            Vector3 pos = SampleImpostorSpawnPosition(sp, shape);
            var npc = Instantiate(npcPrefab, pos, Quaternion.identity, npcsParent);
            ApplyNPCColor(npc);
            var f = npc.AddComponent<PathFollower>();
            f.pathShape = shape;
        }
    }
    
    void ApplyNPCColor(GameObject npc)
    {
        // Create cohesive color variations (e.g. shades of blue)
        Color baseColor = new Color(0.3f, 0.55f, 0.95f);
        float shade = Random.Range(-0.18f, 0.18f);
        Color varied = new Color(
            Mathf.Clamp01(baseColor.r + shade * 0.35f),
            Mathf.Clamp01(baseColor.g + shade * 0.55f),
            Mathf.Clamp01(baseColor.b + shade)
        );

        var renderers = npc.GetComponentsInChildren<Renderer>();
        foreach (var r in renderers)
        {
            if (r == null) continue;
            if (r.material != null)
            {
                r.material = new Material(r.material);
                r.material.color = varied;
            }
        }
    }

    Vector3 SampleSpawnPosition(Transform fallback)
    {
        if (TrySampleWithinBounds(out Vector3 position))
        {
            return position;
        }

        Vector3 basePos = fallback ? new Vector3(fallback.position.x, 0f, fallback.position.z) : Vector3.zero;
        if (TrySampleAround(basePos, 0.9f, out position))
        {
            return position;
        }

        position = basePos + RandomHorizontalOffset(0.45f);
        usedSpawnPositions.Add(position);
        return position;
    }

    Vector3 SampleImpostorSpawnPosition(Transform fallback, PathShape path)
    {
        if (path)
        {
            var points = path.GetPoints();
            if (points != null && points.Length > 0)
            {
                Vector3 center = path.transform.position;
                float scale = Mathf.Clamp(defaultPathScale, 0.15f, 1f);
                float spacing = minimumSpawnSpacing;
                for (int relax = 0; relax < spacingRelaxIterations; relax++)
                {
                    for (int attempt = 0; attempt < maxAttemptsPerIteration; attempt++)
                    {
                        int pointIndex = Random.Range(0, points.Length);
                        Vector3 start = Vector3.Lerp(center, points[pointIndex], scale);
                        if (IsFarEnough(start, spacing))
                        {
                            usedSpawnPositions.Add(start);
                            return start;
                        }
                    }
                    spacing *= spacingRelaxFactor;
                }
            }
        }

        return SampleSpawnPosition(fallback);
    }

    bool TrySampleWithinBounds(out Vector3 position)
    {
        position = default;
        if (!movementBounds) return false;

        Bounds b = movementBounds.bounds;
        float spacing = minimumSpawnSpacing;
        for (int relax = 0; relax < spacingRelaxIterations; relax++)
        {
            for (int attempt = 0; attempt < maxAttemptsPerIteration; attempt++)
            {
                float x = Random.Range(b.min.x, b.max.x);
                float z = Random.Range(b.min.z, b.max.z);
                Vector3 candidate = new Vector3(x, 0f, z);
                if (IsFarEnough(candidate, spacing))
                {
                    usedSpawnPositions.Add(candidate);
                    position = candidate;
                    return true;
                }
            }
            spacing *= spacingRelaxFactor;
        }

        return false;
    }

    bool TrySampleAround(Vector3 center, float radius, out Vector3 position)
    {
        position = default;
        float spacing = minimumSpawnSpacing;
        for (int relax = 0; relax < spacingRelaxIterations; relax++)
        {
            for (int attempt = 0; attempt < maxAttemptsPerIteration; attempt++)
            {
                Vector3 candidate = center + RandomHorizontalOffset(radius);
                if (IsFarEnough(candidate, spacing))
                {
                    usedSpawnPositions.Add(candidate);
                    position = candidate;
                    return true;
                }
            }
            spacing *= spacingRelaxFactor;
        }

        return false;
    }

    bool IsFarEnough(Vector3 candidate, float spacing)
    {
        foreach (var pos in usedSpawnPositions)
        {
            if ((pos - candidate).sqrMagnitude < spacing * spacing)
                return false;
        }
        return true;
    }

    static Vector3 RandomHorizontalOffset(float radius)
    {
        Vector2 offset2D = Random.insideUnitCircle * radius;
        return new Vector3(offset2D.x, 0f, offset2D.y);
    }
}
