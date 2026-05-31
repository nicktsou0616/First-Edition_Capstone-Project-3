using UnityEngine;

public class Projectile : MonoBehaviour
{
    private TowerData _data;
    private Vector3 _shootDirection;
    private float _projectileDuration;
    private Enemy _targetEnemy;

    

    void Start()
    {
        if (_data != null)
            transform.localScale = Vector3.one * _data.projectileSize;
    }

    void Update()
    {
        if (_data == null) return;

        if (_projectileDuration <= 0)
        {
            gameObject.SetActive(false);
            return;
        }

        _projectileDuration -= Time.deltaTime;

        // --- 核心修改：移動邏輯 ---
        
        // 如果不是爆炸子彈（一般子彈），且目標還活著並在場景中
        if (!_data.isExplosive && _targetEnemy != null && _targetEnemy.gameObject.activeSelf)
        {
            // 更新方向：不斷指向敵人的當前位置
            _shootDirection = (_targetEnemy.transform.position - transform.position).normalized;
            
            // 讓子彈的角度轉向飛行方向 (選擇性視覺優化)
            float angle = Mathf.Atan2(_shootDirection.y, _shootDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // 根據目前的方向移動（一般子彈會不斷修正，爆炸子彈則維持初始方向）
        transform.position += _shootDirection * _data.projectileSpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 安全檢查：如果 _data 是空的，絕對不執行傷害邏輯，直接回收
        if (_data == null) 
        {
            Debug.LogWarning("子彈碰撞時發現 _data 為空，取消傷害計算並回收。");
            gameObject.SetActive(false);
            return;
        }

        if (collision.CompareTag("Enemy"))
        {
            if (_data.isExplosive)
            {
                Explode(); 
            }
            else
            {
                Enemy enemy = collision.GetComponent<Enemy>();
                if (enemy != null) 
                {
                    // 這裡會正確使用從 Soldier 傳過來的 _stats 數據
                    enemy.TakeDamage(_data.damage, _data.damageType); 
                }
            }
            
            gameObject.SetActive(false); 
            Debug.Log($"子彈擊中 {collision.name}，造成 {_data.damage} 傷害");
        }
    }

    public void Shoot(TowerData data, Vector3 shootDirection, Enemy targetEnemy)
    {
        _data = data; // 關鍵：確保這行執行了
        _shootDirection = shootDirection;
        _projectileDuration = _data.projectileDuration;
        _targetEnemy = targetEnemy;

        transform.localScale = Vector3.one * _data.projectileSize;
        
        // 診斷：發射瞬間檢查 _data 是否真的有特效
        if(_data.isExplosive && _data.explosionEffectPrefab == null)
        {
            Debug.LogError($"{_data.name} 勾選了爆炸，但特效欄位是空的！");
        }
    }

    private void Explode()
{
    if (_data == null) return;

    // 1. 產生視覺特效並修正縮放
    if (_data.isExplosive && _data.explosionEffectPrefab != null)
    {
        GameObject effect = Instantiate(_data.explosionEffectPrefab, transform.position, Quaternion.identity);
        float visualAdjustment = 1.0f;
        effect.transform.localScale = Vector3.one * (_data.explosionRadius * 2f) * visualAdjustment;
        Destroy(effect, 0.5f);
    }

    // 2. 傷害計算邏輯：偵測範圍內的敵人
    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, _data.explosionRadius);
    foreach (Collider2D hit in hitColliders)
    {
        // 檢查碰撞到的物件是否帶有 Enemy 組件
        if (hit.CompareTag("Enemy"))
        {
            Enemy enemy = hit.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(_data.damage, _data.damageType);
                Debug.Log($"爆炸波擊中了：{hit.name}，造成 {_data.damage} 點傷害");
            }
        }
    }
}

    private void ShowExplosionEffect()
{
    // 直接從發射這顆子彈的 _data 中抓取特效
    if (_data.explosionEffectPrefab != null)
    {
        GameObject effect = Instantiate(_data.explosionEffectPrefab, transform.position, Quaternion.identity);
        effect.transform.localScale = Vector3.one * _data.explosionRadius * 2f;
        Destroy(effect, 0.5f);
    }
}
}
