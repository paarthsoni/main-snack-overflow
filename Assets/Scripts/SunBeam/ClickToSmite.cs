using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToSmite : MonoBehaviour
{
    public LayerMask npcLayer; // set to NPC layer in Inspector

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, npcLayer))
            {
                var death = hit.collider.GetComponentInParent<NPCDeath>();
                if (death != null)
                    SunbeamManager.Instance.Smite(death);
            }
        }
    }
}
