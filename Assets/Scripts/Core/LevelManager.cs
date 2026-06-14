using UnityEngine;

/// <summary>
///     Quản lý danh sách PuzzleData và lưu tiến độ người chơi vào PlayerPrefs.
///     Singleton — tồn tại xuyên suốt các scene (DontDestroyOnLoad).
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Bank")]
    [Tooltip("Kéo tất cả PuzzleData ScriptableObject vào đây theo thứ tự độ khó.")]
    [SerializeField]
    private PuzzleData[] _allLevels;

    public static LevelManager Instance { get; private set; }

    // ── State ──────────────────────────────────────────────────────────────
    public int CurrentLevelIndex { get; private set; }
    public int TotalLevels => _allLevels.Length;

    // ──────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Trả về PuzzleData của level hiện tại.</summary>
    public PuzzleData GetCurrentPuzzle()
    {
        if (_allLevels == null || _allLevels.Length == 0)
        {
            Debug.LogError("LevelManager: allLevels is empty! Hãy gán PuzzleData vào Inspector.");
            return null;
        }

        return _allLevels[CurrentLevelIndex];
    }

    /// <summary>Chuyển sang level tiếp theo (không lưu — MarkCurrentLevelComplete đã làm).</summary>
    public void AdvanceLevel()
    {
        CurrentLevelIndex = Mathf.Min(CurrentLevelIndex + 1, _allLevels.Length - 1);
    }
}
