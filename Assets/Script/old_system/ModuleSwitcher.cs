using UnityEngine;
using System.Collections.Generic; // 👇 新增這行：這是使用 List (清單) 必須引入的命名空間

public class ModuleSwitcher : MonoBehaviour
{
    // 👇 將單一 GameObject 改成 List<GameObject>，Inspector 就會變成可擴增的清單陣列
    [Header("⚙️ 要關閉的舊系統 (可拖曳多個)")]
    public List<GameObject> OldSystems = new List<GameObject>();

    [Header("🚀 要開啟的新模組 (可拖曳多個)")]
    public List<GameObject> NewSystems = new List<GameObject>();

    public void SwitchNow()
    {
        // 👇 使用 foreach 迴圈，把清單裡所有的舊系統一次關掉
        foreach (GameObject oldSys in OldSystems)
        {
            if (oldSys != null) // 防呆：確保你沒有留空的欄位
            {
                oldSys.SetActive(false);
                Debug.Log($"🟥 [ModuleSwitcher] 已安全卸載 (SetActive=false): {oldSys.name}");
            }
        }

        // 👇 使用 foreach 迴圈，把清單裡所有的新模組一次打開
        foreach (GameObject newSys in NewSystems)
        {
            if (newSys != null) // 防呆：確保你沒有留空的欄位
            {
                newSys.SetActive(true);
                Debug.Log($"🟩 [ModuleSwitcher] 已成功掛載 (SetActive=true): {newSys.name}");
            }
        }
    }
}