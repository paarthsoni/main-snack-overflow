// using UnityEngine;
// using UnityEngine.EventSystems;

// public class ClickToSmite : MonoBehaviour
// {
//     public LayerMask npcLayer; // set to NPC layer in Inspector

//     void Update()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//                 return;

//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit, 200f, npcLayer))
//             {
//                 var death = hit.collider.GetComponentInParent<NPCDeath>();
//                 if (death != null)
//                     SunbeamManager.Instance.Smite(death);
//             }
//         }
//     }
// }


// using UnityEngine;
// using UnityEngine.EventSystems;

// public class ClickToSmite : MonoBehaviour
// {
//     public LayerMask npcLayer; // set to NPC layer in Inspector

//     void Update()
//     {
//         if (Input.GetMouseButtonDown(0))
//         {
//             if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
//                 return;

//             Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
//             if (Physics.Raycast(ray, out RaycastHit hit, 200f, npcLayer))
//             {
//                 var death = hit.collider.GetComponentInParent<NPCDeath>();
//                 if (death == null) return;

//                 var id = hit.collider.GetComponentInParent<NPCIdentity>();
//                 if (id == null) { WrongHit(); return; }

//                 // Compare against the allowed (shape, colorId) pairs from the memory bar
//                 var grs = GameRoundState.Instance;
//                 bool correct = (grs != null) && grs.MatchesAllowed(id.shapeType, id.colorId);

//                 if (correct)
// {
//     // Correct → lethal beam + kill, lives unchanged
//     SunbeamManager.Instance.Smite(death);
// }
// else
// {
//     // Wrong → lethal beam + kill, AND lose a life
//     SunbeamManager.Instance.Smite(death);
//     if (LivesManager.Instance != null)
//         LivesManager.Instance.LoseLife();
// }
//             }
//         }
//     }

//     void WrongHit()
//     {
//         if (LivesManager.Instance != null)
//             LivesManager.Instance.LoseLife();
//         // (Optional) play a fail SFX or small UI flash here
//     }
// }

using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToSmite : MonoBehaviour
{
    public LayerMask npcLayer; // set to NPC layer in Inspector

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // Ignore UI clicks
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, npcLayer))
            {
                var death = hit.collider.GetComponentInParent<NPCDeath>();
                if (death == null) return;

                var id = hit.collider.GetComponentInParent<NPCIdentity>();
                var grs = GameRoundState.Instance;

                // If no ID, treat as wrong civilian
                if (id == null)
                {
                    HandleWrong(death);
                    return;
                }

                bool correct = (grs != null) && grs.MatchesAllowed(id.shapeType, id.colorId);

                if (correct)
                {
                    HandleCorrect(death, id);
                }
                else
                {
                    HandleWrong(death);
                }
            }
        }
    }

    void HandleCorrect(NPCDeath death, NPCIdentity id)
    {
        // Disable colliders immediately so we don't double count
        PreventDoubleHit(death);

        // Notify that an impostor was killed
        if (id != null && id.isImpostor)
            ImpostorTracker.Instance?.OnImpostorKilled();

        // Beam + delete NPC
        SunbeamManager.Instance.Smite(death);
    }

    void HandleWrong(NPCDeath death)
    {
        PreventDoubleHit(death);

        // Beam + delete NPC
        SunbeamManager.Instance.Smite(death);

        // Lose a life on wrong hit
        if (LivesManager.Instance != null)
            LivesManager.Instance.LoseLife();
    }

    void PreventDoubleHit(NPCDeath death)
    {
        var cols = death.GetComponentsInChildren<Collider>(false);
        for (int i = 0; i < cols.Length; i++)
            cols[i].enabled = false;
    }
}
