using UnityEngine;

public class FastSpriteSwitch : MonoBehaviour
{
    public SpriteRenderer sr;
    public Sprite left;
    public Sprite right;

    private int dir = 1; // 1 = right, -1 = left

    void Start()
    {
        dir = 1;              // 一開始朝右
        sr.sprite = right;    // 一開始顯示 right
    }

    void Update()
    {
        float input = Input.GetAxisRaw("Horizontal");

        if (input > 0)
            dir = 1;
        else if (input < 0)
            dir = -1;

        UpdateSprite();
    }

    void UpdateSprite()
    {
        if (dir == 1)
            sr.sprite = right;
        else
            sr.sprite = left;
    }
}