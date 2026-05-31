using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("數值設定")]
    public float speed = 8f;
    public bool canMove = true;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 如果被鎖死 (例如正在傳送)
        if (!canMove)
        {
            // 鎖定時將速度完全歸零，確保傳送瞬間與位移時不會有任何物理干擾
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);
    }
}