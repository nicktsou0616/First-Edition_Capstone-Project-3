using UnityEngine;
using System.Collections.Generic;
using System.IO;

[CreateAssetMenu(fileName = "GameProgress", menuName = "StorySystem/GameProgress")]
public class GameProgress : ScriptableObject
{
    [Header("玩家目前持有的 Flags")]
    public List<string> flags = new List<string>();

    [Header("玩家目前持有的證物")]
    public List<EvidenceData> evidences = new List<EvidenceData>();

    // 專門給 JSON 轉換用的輕量結構 (不能存物件，只能存 ID)
    [System.Serializable]
    private class SaveData
    {
        public List<string> flags;
        public List<string> evidenceIDs;
    }

    public void AddFlag(string flag)
    {
        if (!flags.Contains(flag))
        {
            flags.Add(flag);
            Debug.Log($"[GameProgress] 獲得 Flag: {flag}");
        }
    }

    public bool HasFlag(string flag)
    {
        return flags.Contains(flag);
    }

    public void AddEvidence(EvidenceData evidence)
    {
        if (evidence != null && !evidences.Contains(evidence))
        {
            evidences.Add(evidence);
            Debug.Log($"[GameProgress] 獲得證物: {evidence.evidenceName}");
        }
    }

    public void ResetProgress()
    {
        flags.Clear();
        evidences.Clear();
        Debug.Log("[GameProgress] 進度已重置 (清空)！");
    }

    // --- 存檔系統 ---
    public void SaveProgress()
    {
        SaveData data = new SaveData();
        data.flags = new List<string>(flags);
        data.evidenceIDs = new List<string>();

        // 只把證物的 ID 存進 JSON
        foreach (var ev in evidences)
        {
            if (ev != null && !string.IsNullOrEmpty(ev.evidenceID))
            {
                data.evidenceIDs.Add(ev.evidenceID);
            }
        }

        string json = JsonUtility.ToJson(data, true);
        string path = Path.Combine(Application.persistentDataPath, "SaveGame.json");
        File.WriteAllText(path, json);

        Debug.Log($"[GameProgress] 💾 存檔成功！路徑: {path}");
    }

    // --- 讀檔系統 ---
    public void LoadProgress()
    {
        string path = Path.Combine(Application.persistentDataPath, "SaveGame.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            flags = data.flags != null ? data.flags : new List<string>();
            evidences.Clear();

            // 從 Unity 的 Resources 資料夾還原證物
            if (data.evidenceIDs != null)
            {
                foreach (string id in data.evidenceIDs)
                {
                    EvidenceData ev = Resources.Load<EvidenceData>($"Evidences/{id}");
                    if (ev != null)
                    {
                        evidences.Add(ev);
                    }
                    else
                    {
                        Debug.LogWarning($"[GameProgress] 讀檔警告：找不到證物 '{id}'。請確保檔案放在 Resources/Evidences 資料夾，且檔名與 ID 一致。");
                    }
                }
            }

            Debug.Log("[GameProgress] 📂 讀檔成功！");
        }
        else
        {
            Debug.LogWarning("[GameProgress] ⚠️ 找不到存檔檔案！已為您準備全新空白進度。");
            ResetProgress();
        }
    }
}