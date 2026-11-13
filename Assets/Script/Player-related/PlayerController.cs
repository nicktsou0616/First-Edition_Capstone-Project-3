using UnityEngine;

// 防呆機制：確保掛載此腳本的 GameObject 上一定有 SpriteRenderer 元件
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("移動設定 (Movement Settings)")]

    [Tooltip("玩家 Scale = 1 時的基準移動速度")]
    [SerializeField] private float baseMoveSpeed = 5f;

    [Tooltip("用哪一個 Scale 當作速度計算基準")]
    [SerializeField] private float baseScale = 1f;

    // 最終實際使用的速度
    private float moveSpeed;


    [Header("角色圖片 (Sprites)")]

    [Tooltip("玩家第一次進入場景時使用的初始圖片")]
    [SerializeField] private Sprite defaultSprite;

    [Tooltip("玩家停止移動且最後朝左時顯示的圖片")]
    [SerializeField] private Sprite leftSprite;

    [Tooltip("玩家停止移動且最後朝右時顯示的圖片")]
    [SerializeField] private Sprite rightSprite;


    [Header("左走路動畫 (Left Walk Animation)")]

    [Tooltip("請依序放入左走 1、左走 2、左走 3")]
    [SerializeField] private Sprite[] leftWalkSprites;


    [Header("右走路動畫 (Right Walk Animation)")]

    [Tooltip("請依序放入右走 1、右走 2、右走 3")]
    [SerializeField] private Sprite[] rightWalkSprites;


    [Header("走路動畫設定 (Walk Animation Settings)")]

    [Tooltip("每一張走路圖片持續顯示的時間（秒）")]
    [SerializeField] private float frameDuration = 0.12f;


    // SpriteRenderer
    private SpriteRenderer spriteRenderer;

    // 水平輸入
    private float horizontalInput;

    // 最後面向方向
    // -1 = 左
    //  1 = 右
    private int facingDirection = 1;

    // 目前走路動畫幀數
    private int currentWalkFrame = 0;

    // 動畫換幀計時器
    private float animationTimer = 0f;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("PlayerController: 找不到 SpriteRenderer！");
            return;
        }

        // 第一次進入場景時使用初始圖片
        spriteRenderer.sprite = defaultSprite;

        // 初始朝向右
        facingDirection = 1;

        // 初始化動畫
        currentWalkFrame = 0;
        animationTimer = 0f;

        // 計算一次初始移動速度
        CalculateMoveSpeed();
    }


    private void CalculateMoveSpeed()
    {
        // 取得目前 Player 的 Scale
        float currentScale = Mathf.Abs(transform.localScale.x);

        // 避免 baseScale 為 0
        if (baseScale <= 0f)
        {
            Debug.LogError("PlayerController: baseScale 必須大於 0！");
            moveSpeed = 0f;
            return;
        }

        // ============================================================
        // 移動速度計算
        //
        // Scale = baseScale
        // → moveSpeed = baseMoveSpeed
        //
        // Scale = 2 倍
        // → moveSpeed = baseMoveSpeed × 2
        //
        // Scale = 0.5 倍
        // → moveSpeed = baseMoveSpeed × 0.5
        //
        // 注意：
        // 動畫速度完全不參與這裡
        // ============================================================

        moveSpeed = baseMoveSpeed * (currentScale / baseScale);
    }


    private void Update()
    {
        HandleInput();

        // 每幀重新計算移動速度
        // 確保如果 Scale 或 baseMoveSpeed 改變，
        // 移動速度會立即跟著更新
        CalculateMoveSpeed();

        HandleMovement();
        HandleSprite();
        HandleWalkAnimation();
    }


    private void HandleInput()
    {
        bool isLeftPressed = Input.GetKey(KeyCode.LeftArrow);
        bool isRightPressed = Input.GetKey(KeyCode.RightArrow);

        // 同時按下左右 → 抵消
        if (isLeftPressed && isRightPressed)
        {
            horizontalInput = 0f;
        }
        else if (isLeftPressed)
        {
            horizontalInput = -1f;
        }
        else if (isRightPressed)
        {
            horizontalInput = 1f;
        }
        else
        {
            horizontalInput = 0f;
        }
    }


    private void HandleMovement()
    {
        // 沒有輸入就不移動
        if (horizontalInput == 0f)
            return;

        Vector3 movement = new Vector3(
            horizontalInput,
            0f,
            0f
        );

        // ============================================================
        // 移動只使用 moveSpeed
        //
        // frameDuration 完全不參與移動速度計算
        // ============================================================

        transform.Translate(
            movement * moveSpeed * Time.deltaTime
        );
    }


    private void HandleSprite()
    {
        if (spriteRenderer == null)
            return;


        // ============================================================
        // 向左
        // ============================================================

        if (horizontalInput < 0f)
        {
            // 如果原本不是面向左
            if (facingDirection != -1)
            {
                facingDirection = -1;

                // 轉向時從左走路動畫第一幀開始
                ResetWalkAnimation();
            }
        }


        // ============================================================
        // 向右
        // ============================================================

        else if (horizontalInput > 0f)
        {
            // 如果原本不是面向右
            if (facingDirection != 1)
            {
                facingDirection = 1;

                // 轉向時從右走路動畫第一幀開始
                ResetWalkAnimation();
            }
        }


        // ============================================================
        // 沒有移動
        // ============================================================

        else
        {
            // 不回 defaultSprite
            //
            // 保持最後一次移動的方向

            if (facingDirection < 0)
            {
                spriteRenderer.sprite = leftSprite;
            }
            else
            {
                spriteRenderer.sprite = rightSprite;
            }
        }
    }


    private void HandleWalkAnimation()
    {
        // 沒有移動 → 不播放走路動畫
        if (horizontalInput == 0f)
            return;


        // ============================================================
        // 取得目前方向的動畫
        // ============================================================

        Sprite[] currentWalkSprites;

        if (facingDirection < 0)
        {
            currentWalkSprites = leftWalkSprites;
        }
        else
        {
            currentWalkSprites = rightWalkSprites;
        }


        // 沒有動畫圖片就不播放
        if (currentWalkSprites == null ||
            currentWalkSprites.Length == 0)
        {
            return;
        }


        // ============================================================
        // 防呆
        //
        // frameDuration 是「每一幀持續幾秒」
        //
        // 例如：
        // 0.10 = 每 0.10 秒換一張
        // 0.12 = 每 0.12 秒換一張
        // 0.20 = 每 0.20 秒換一張
        // ============================================================

        if (frameDuration <= 0f)
            return;


        // ============================================================
        // 累積時間
        // ============================================================

        animationTimer += Time.deltaTime;


        // ============================================================
        // 到達換幀時間
        // ============================================================

        if (animationTimer >= frameDuration)
        {
            animationTimer -= frameDuration;

            currentWalkFrame++;

            // 超過最後一張 → 從第一張重新開始
            if (currentWalkFrame >= currentWalkSprites.Length)
            {
                currentWalkFrame = 0;
            }

            spriteRenderer.sprite =
                currentWalkSprites[currentWalkFrame];
        }
    }


    private void ResetWalkAnimation()
    {
        // 回到第一幀
        currentWalkFrame = 0;

        // 重置換幀時間
        animationTimer = 0f;


        // 轉向後立即顯示第一張走路圖
        if (facingDirection < 0)
        {
            if (leftWalkSprites != null &&
                leftWalkSprites.Length > 0)
            {
                spriteRenderer.sprite = leftWalkSprites[0];
            }
        }
        else
        {
            if (rightWalkSprites != null &&
                rightWalkSprites.Length > 0)
            {
                spriteRenderer.sprite = rightWalkSprites[0];
            }
        }
    }
}