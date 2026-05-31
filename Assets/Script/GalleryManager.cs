using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class GalleryItem
{
    public string itemName;       // 怪物/塔的名字
    [TextArea(10, 30)]
    public string itemDescription; // 詳細介紹數值
    // 未來如果美術給圖了，這裡可以加一行 public Sprite itemIcon; 來換圖！
}

[System.Serializable]
public class GalleryCategory
{
    [Tooltip("拖入左上角的分類按鈕(A/B/C)，大腦會去抓字！")]
    public GameObject categoryButton;
    public List<GalleryItem> items = new List<GalleryItem>(); // 這個分類下的所有怪/塔
}

public class GalleryManager : MonoBehaviour
{
    [Header("🗂️ 圖鑑資料庫")]
    public List<GalleryCategory> database = new List<GalleryCategory>();

    [Header("🖥️ UI 綁定 - 左側網格")]
    public GameObject gridItemPrefab;          // 剛剛做好的九宮格按鈕 Prefab
    public Transform gridContentContainer;     // 左邊 ScrollView 的 Content

    [Header("🖥️ UI 綁定 - 右側詳細介紹")]
    public TextMeshProUGUI detailNameText;     // 右邊顯示名字的 Text
    public TextMeshProUGUI detailStatsText;    // 右邊顯示詳細介紹的 Text

    private GalleryCategory currentCategory;

    void Start()
    {
        // 一開始預設打開第一個分類 (怪)
        if (database.Count > 0) OpenCategory(0);
    }

    // ★ 點擊上方 A, B, C 分類標籤時呼叫
    public void OpenCategory(int categoryIndex)
    {
        if (categoryIndex < 0 || categoryIndex >= database.Count) return;
        currentCategory = database[categoryIndex];

        // 清空舊的網格按鈕
        foreach (Transform child in gridContentContainer) Destroy(child.gameObject);

        // 生成新的網格按鈕
        for (int i = 0; i < currentCategory.items.Count; i++)
        {
            int index = i;
            GameObject newItem = Instantiate(gridItemPrefab, gridContentContainer);

            // 把按鈕上的字改成怪物的名字
            newItem.GetComponentInChildren<TextMeshProUGUI>().text = currentCategory.items[i].itemName;

            // 綁定點擊事件：點下去就顯示這隻怪的詳細資料
            newItem.GetComponent<Button>().onClick.AddListener(() => OpenItemDetails(index));
        }

        // 預設顯示這個分類裡第一隻怪的資料
        if (currentCategory.items.Count > 0) OpenItemDetails(0);
        else
        {
            detailNameText.text = "無資料";
            detailStatsText.text = "尚無內容...";
        }
    }

    // ★ 點擊左邊九宮格縮圖時呼叫
    public void OpenItemDetails(int itemIndex)
    {
        GalleryItem item = currentCategory.items[itemIndex];

        // 更新右邊面板的文字
        detailNameText.text = item.itemName;
        detailStatsText.text = item.itemDescription;
    }
}