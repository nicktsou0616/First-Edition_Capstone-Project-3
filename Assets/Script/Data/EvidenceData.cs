using UnityEngine;

[CreateAssetMenu(fileName = "NewEvidence", menuName = "StorySystem/EvidenceData")]
public class EvidenceData : ScriptableObject
{
    [Header("證物基本資料")]
    public string evidenceID;
    public string evidenceName;
    public Sprite icon;
    [TextArea(3, 5)]
    public string description;
}