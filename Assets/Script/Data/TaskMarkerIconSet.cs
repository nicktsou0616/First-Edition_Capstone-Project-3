using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TaskMarkerIconEntry
{
    public string iconId;
    public Sprite sprite;
    public Color color = Color.white;
    public Vector2 miniSize = new Vector2(12f, 12f);
    public Vector2 fullSize = new Vector2(20f, 20f);
    public float miniYOffset;
    public float fullYOffset;
}

[CreateAssetMenu(fileName = "NewTaskMarkerIconSet", menuName = "RPG/Task Marker Icon Set")]
public class TaskMarkerIconSet : ScriptableObject
{
    public List<TaskMarkerIconEntry> icons = new List<TaskMarkerIconEntry>();

    public bool TryGetIcon(string iconId, out TaskMarkerIconEntry icon)
    {
        icon = null;
        if (string.IsNullOrEmpty(iconId)) return false;

        foreach (TaskMarkerIconEntry entry in icons)
        {
            if (entry == null || entry.iconId != iconId) continue;

            icon = entry;
            return true;
        }

        return false;
    }
}
