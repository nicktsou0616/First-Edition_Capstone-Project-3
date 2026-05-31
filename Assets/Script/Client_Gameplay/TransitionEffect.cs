// 檔案: TransitionEffect.cs
using UnityEngine;

namespace RPGFramework.Client
{
    /// <summary>
    /// 控制轉場效果物件 (如黑畫面、Loading 畫面) 的顯示與隱藏
    /// 僅操作 GameObject 的 Active 狀態，不干涉邏輯
    /// </summary>
    public class TransitionEffect : MonoBehaviour
    {
        public void ActivateEffect()
        {
            gameObject.SetActive(true);
        }

        public void DeactivateEffect()
        {
            gameObject.SetActive(false);
        }
    }
}