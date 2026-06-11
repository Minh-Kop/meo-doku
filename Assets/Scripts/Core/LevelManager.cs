using UnityEngine;

/// <summary>
///     Quản lý danh sách PuzzleData và lưu tiến độ người chơi vào PlayerPrefs.
///     Singleton — tồn tại xuyên suốt các scene (DontDestroyOnLoad).
/// </summary>
public class LevelManager : MonoBehaviour
{
    private const string SaveKey = "Bunnydoku_CurrentLevel";

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

        CurrentLevelIndex = PlayerPrefs.GetInt(SaveKey, 0);
        CurrentLevelIndex = Mathf.Clamp(CurrentLevelIndex, 0, _allLevels.Length - 1);
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

    /// <summary>Trả về PuzzleData theo index cụ thể (dùng cho Level Select screen).</summary>
    public PuzzleData GetPuzzleAt(int index)
    {
        if (index < 0 || index >= _allLevels.Length)
        {
            return null;
        }

        return _allLevels[index];
    }

    /// <summary>Đánh dấu level hiện tại đã hoàn thành, lưu xuống PlayerPrefs.</summary>
    public void MarkCurrentLevelComplete()
    {
        // Chỉ lưu nếu đây là level cao nhất đã đạt được
        var savedIndex = PlayerPrefs.GetInt(SaveKey, 0);
        if (CurrentLevelIndex >= savedIndex)
        {
            var next = Mathf.Min(CurrentLevelIndex + 1, _allLevels.Length - 1);
            PlayerPrefs.SetInt(SaveKey, next);
            PlayerPrefs.Save();
        }
    }

    /// <summary>Chuyển sang level tiếp theo (không lưu — MarkCurrentLevelComplete đã làm).</summary>
    public void AdvanceLevel()
    {
        CurrentLevelIndex = Mathf.Min(CurrentLevelIndex + 1, _allLevels.Length - 1);
    }

    /// <summary>Chuyển tới level theo index cụ thể (dùng cho Level Select).</summary>
    public void GoToLevel(int index)
    {
        CurrentLevelIndex = Mathf.Clamp(index, 0, _allLevels.Length - 1);
    }

    /// <summary>Level nào đã mở khóa chưa (dùng để vẽ Level Select screen).</summary>
    public bool IsLevelUnlocked(int index)
    {
        var reached = PlayerPrefs.GetInt(SaveKey, 0);
        return index <= reached;
    }

    /// <summary>Xóa toàn bộ tiến độ (dùng cho nút Reset trong Settings).</summary>
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        PlayerPrefs.Save();
        CurrentLevelIndex = 0;
    }
}
