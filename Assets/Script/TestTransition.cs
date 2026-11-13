using UnityEngine;

public class TestTransition : MonoBehaviour
{
    [Header("測試用轉場")]
    [SerializeField] private GameObject blackTransitionPrefab;
    [SerializeField] private GameObject whiteTransitionPrefab;

    // 測試黑色轉場
    public void TestBlackTransition()
    {
        TransitionManager.LoadScene(
            "SampleScene",
            blackTransitionPrefab
        );
    }

    // 測試白色轉場
    public void TestWhiteTransition()
    {
        TransitionManager.LoadScene(
            "SampleScene",
            whiteTransitionPrefab
        );
    }
}
