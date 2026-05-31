using UnityEngine;
using UnityEngine.UI;

public class SpeedButtonController : MonoBehaviour
{
    [Header("Speed Buttons")]
    [SerializeField] private Button speed1Button;
    [SerializeField] private Button speed2Button;
    [SerializeField] private Button speed3Button;

    private int _currentSpeed = 1;

    private void Awake()
    {
        CacheButtonsIfNeeded();
        AddClickListener(speed1Button);
        AddClickListener(speed2Button);
        AddClickListener(speed3Button);
    }

    private void Start()
    {
        SetSpeed(1);
    }

    private void OnDestroy()
    {
        RemoveClickListener(speed1Button);
        RemoveClickListener(speed2Button);
        RemoveClickListener(speed3Button);
    }

    public void CycleSpeed()
    {
        SetSpeed(_currentSpeed >= 3 ? 1 : _currentSpeed + 1);
    }

    private void SetSpeed(int speed)
    {
        _currentSpeed = Mathf.Clamp(speed, 1, 3);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameSpeed(_currentSpeed);
        }
        else
        {
            Time.timeScale = _currentSpeed;
        }

        UpdateButtonVisibility();
    }

    private void UpdateButtonVisibility()
    {
        SetButtonActive(speed1Button, _currentSpeed == 1);
        SetButtonActive(speed2Button, _currentSpeed == 2);
        SetButtonActive(speed3Button, _currentSpeed == 3);
    }

    private void CacheButtonsIfNeeded()
    {
        if (speed1Button == null)
        {
            speed1Button = transform.Find("Speed1Button")?.GetComponent<Button>();
        }

        if (speed2Button == null)
        {
            speed2Button = transform.Find("Speed2Button")?.GetComponent<Button>();
        }

        if (speed3Button == null)
        {
            speed3Button = transform.Find("Speed3Button")?.GetComponent<Button>();
        }
    }

    private void AddClickListener(Button button)
    {
        if (button != null)
        {
            button.onClick.AddListener(CycleSpeed);
        }
    }

    private void RemoveClickListener(Button button)
    {
        if (button != null)
        {
            button.onClick.RemoveListener(CycleSpeed);
        }
    }

    private static void SetButtonActive(Button button, bool isActive)
    {
        if (button != null)
        {
            button.gameObject.SetActive(isActive);
        }
    }
}
