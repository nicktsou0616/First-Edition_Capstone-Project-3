using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TowerCard : MonoBehaviour
{
    [SerializeField] private Image towerImage;
    [SerializeField] private TMP_Text costText;

    public TowerData Data { get; private set; }

    public void Initialize(TowerData data)
    {
        Data = data;
        
        // 確保你的 TowerData 裡有這些欄位
        if (towerImage != null) towerImage.sprite = data.sprite;
        if (costText != null) costText.text = data.cost.ToString();
    }
}
