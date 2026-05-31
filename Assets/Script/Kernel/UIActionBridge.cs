//// 檔案: UIActionBridge.cs
//using UnityEngine;
//using System.Collections.Generic;

//namespace RPGFramework.Kernel
//{
//    [System.Serializable]
//    public struct DialogueMapping
//    {
//        public string dialogueID;
//        public GameObject dialogueObject;
//    }

//    public class UIActionBridge : MonoBehaviour
//    {
//        [Header("Dialogue 映射表 (ID -> Scene Object)")]
//        [SerializeField] private List<DialogueMapping> dialogueMappings = new List<DialogueMapping>();

//        public void ShowDialogue(string dialogueID)
//        {
//            if (string.IsNullOrEmpty(dialogueID)) return;

//            // 透過查表找到對應的 Dialogue，避免 FindObjectOfType
//            foreach (var mapping in dialogueMappings)
//            {
//                if (mapping.dialogueID == dialogueID && mapping.dialogueObject != null)
//                {
//                    mapping.dialogueObject.SetActive(true);
//                    var dialogueManager = mapping.dialogueObject.GetComponent<DialogueManager>();
//                    if (dialogueManager != null)
//                    {
//                        dialogueManager.PlayLine(0);
//                    }
//                    return;
//                }
//            }
//            Debug.LogWarning($"[UIActionBridge] 找不到對應的 Dialogue ID: {dialogueID}");
//        }

//        public void ShowPopup(string text)
//        {
//            // 呼叫既有的 Singleton 不違反規則
//            if (UIPopupManager.Instance != null && !string.IsNullOrEmpty(text))
//            {
//                UIPopupManager.Instance.ShowPopup(text);
//            }
//        }
//    }
//}