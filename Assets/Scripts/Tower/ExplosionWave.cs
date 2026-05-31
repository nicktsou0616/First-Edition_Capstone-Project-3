using UnityEngine;

public class ExplosionWave : MonoBehaviour
{
    public float expandSpeed = 5f;
    private SpriteRenderer _sr;
    private Color _color;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _color = _sr.color;
    }

    void Update()
    {
        // 讓圓圈持續變大
        transform.localScale += Vector3.one * expandSpeed * Time.deltaTime;
        
        // 讓圓圈逐漸變透明
        _color.a = Mathf.Lerp(_color.a, 0, Time.deltaTime * 10f);
        _sr.color = _color;
    }
}