using UnityEngine;

public static class GameResultState
{
    // 0: perfect clear, 1: clear with damage, 2: game over
    public static int EndingResult { get; private set; } = -1;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState()
    {
        EndingResult = -1;
        Time.timeScale = 1f;
    }

    public static void SetResultByLives(int lives)
    {
        if (lives >= 5)
        {
            SetResult(0);
        }
        else if (lives <= 0)
        {
            SetResult(2);
        }
        else
        {
            SetResult(1);
        }
    }

    public static int GetResult()
    {
        return EndingResult;
    }

    public static void ClearResult()
    {
        EndingResult = -1;
    }

    private static void SetResult(int result)
    {
        EndingResult = result;
    }
}
