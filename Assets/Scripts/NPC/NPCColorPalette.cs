using UnityEngine;

[CreateAssetMenu(menuName = "Game/NPC Color Palette")]
public class NPCColorPalette : ScriptableObject
{
    [System.Serializable]
    public struct Entry { public string name; public Color color; }
    [Tooltip("Index = colorId. Keep these distinct and readable.")]
    public Entry[] entries;

    public int Count => entries != null ? entries.Length : 0;
    public Color Get(int id) => (entries != null && id >= 0 && id < entries.Length) ? entries[id].color : Color.white;
    public string GetName(int id) => (entries != null && id >= 0 && id < entries.Length) ? entries[id].name : "?";
}
