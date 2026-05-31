// 檔案: FlagData.cs
using UnityEngine;

namespace RPGFramework.Data
{
    [CreateAssetMenu(fileName = "NewFlag", menuName = "RPG/FlagData")]
    public class FlagData : ScriptableObject
    {
        [Tooltip("用於內部辨識的唯一 Flag 名稱")]
        public string flagName;

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(flagName))
            {
                flagName = this.name; // 預設使用檔案名稱
            }
        }
    }
}