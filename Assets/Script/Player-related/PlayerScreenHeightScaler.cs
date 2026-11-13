using UnityEngine;

[DefaultExecutionOrder(-100)]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerScreenHeightScaler : MonoBehaviour
{
    [Header("Camera Settings")]
    [Tooltip("自動尋找目前場景的 Main Camera")]
    public bool autoFindMainCamera = true;

    [Header("Scale Settings")]
    [Tooltip("Player 視覺高度佔 Camera 可視高度的比例")]
    [Range(0.1f, 1f)]
    public float heightRatio = 1f / 3f;

    [Header("Ground Position")]
    [Tooltip("Player 腳底位於 Camera 畫面高度的比例")]
    [Range(0f, 1f)]
    public float groundHeightRatio = 0.8f;

    private Camera targetCamera;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (autoFindMainCamera)
        {
            targetCamera = Camera.main;
        }

        UpdateScale();
        UpdatePosition();
    }

    public void UpdateScale()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
        {
            Debug.LogError(
                "[PlayerScreenHeightScaler] 找不到 Main Camera。"
            );
            return;
        }

        if (!targetCamera.orthographic)
        {
            Debug.LogError(
                "[PlayerScreenHeightScaler] Camera 必須使用 Orthographic。"
            );
            return;
        }

        // Camera 可視高度
        float cameraHeight = targetCamera.orthographicSize * 2f;

        // Player 目標高度
        float targetHeight = cameraHeight * heightRatio;

        // Sprite 原始高度
        float spriteHeight = spriteRenderer.sprite.bounds.size.y;

        if (spriteHeight <= 0f)
            return;

        // 算出需要的縮放
        float requiredScale = targetHeight / spriteHeight;

        // 保留 X / Y 翻轉方向
        float signX = Mathf.Sign(transform.localScale.x);
        float signY = Mathf.Sign(transform.localScale.y);

        if (signX == 0f) signX = 1f;
        if (signY == 0f) signY = 1f;

        transform.localScale = new Vector3(
            requiredScale * signX,
            requiredScale * signY,
            transform.localScale.z
        );
    }

    public void UpdatePosition()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null)
            return;

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        if (!targetCamera.orthographic)
            return;

        // Camera 世界座標的上下邊界
        float cameraBottom =
            targetCamera.transform.position.y -
            targetCamera.orthographicSize;

        float cameraHeight =
            targetCamera.orthographicSize * 2f;

        // 計算地面基準線
        float groundY =
            cameraBottom +
            cameraHeight * groundHeightRatio;

        // Sprite 底部距離 Player Pivot 的世界座標距離
        float spriteBottomOffset =
            spriteRenderer.bounds.min.y -
            transform.position.y;

        // 讓 Sprite 的底部剛好位於 groundY
        transform.position = new Vector3(
            transform.position.x,
            groundY - spriteBottomOffset,
            transform.position.z
        );
    }
}
