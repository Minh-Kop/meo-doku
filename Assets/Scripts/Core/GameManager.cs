using UnityEngine;

/// <summary>
///     Điều phối toàn bộ game flow: load level, win/lose, hint, restart.
///     Singleton — truy cập qua GameManager.Instance từ bất kỳ đâu.
/// </summary>
public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GridManager _gridManager;

    [SerializeField]
    private SolverValidator _solverValidator;

    [SerializeField]
    private UIManager _uiManager;

    [Header("Settings")]
    [SerializeField]
    private int _maxMistakes = 3;

    private int _hintsUsed;

    public static GameManager Instance { get; private set; }

    // ── State ──────────────────────────────────────────────────────────────
    public PuzzleData CurrentPuzzle { get; private set; }
    public int MistakesLeft { get; private set; }
    public int BunnyCount { get; private set; }
    public bool IsGameOver { get; private set; }

    // ──────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        LoadLevel(LevelManager.Instance.GetCurrentPuzzle());
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Load và render một puzzle mới.</summary>
    public void LoadLevel(PuzzleData puzzle)
    {
        if (puzzle == null)
        {
            Debug.LogWarning("GameManager: puzzle is null!");
            return;
        }

        CurrentPuzzle = puzzle;
        MistakesLeft = _maxMistakes;
        _hintsUsed = 0;
        IsGameOver = false;

        _gridManager.ClearGrid();
        _gridManager.BuildGrid(puzzle);

        BunnyCount = puzzle.revealCells.Length;

        // _gridManager.Result = PuzzleSolver.FindSolution(puzzle);

        _uiManager.RefreshAll(MistakesLeft, _maxMistakes);
        _uiManager.UpdateBunnyCounter(BunnyCount);
        _uiManager.HideWinScreen();
        _uiManager.HideGameOverScreen();
    }

    /// <summary>Gọi từ SolverValidator mỗi khi người chơi đặt thỏ sai.</summary>
    public void OnMistake()
    {
        if (IsGameOver)
        {
            return;
        }

        MistakesLeft--;
        _uiManager.UpdateMistakes(MistakesLeft, _maxMistakes);

        if (MistakesLeft <= 0)
        {
            TriggerGameOver();
        }
    }

    /// <summary>Gọi từ SolverValidator khi board được giải hoàn toàn.</summary>
    public void OnWin()
    {
        if (IsGameOver)
        {
            return;
        }

        IsGameOver = true;
        // WinAnimator.Instance.PlayWinWave();
        _uiManager.ShowWinScreen();
    }

    public void UpdateBunnyCount()
    {
        BunnyCount++;
        _uiManager.UpdateBunnyCounter(BunnyCount);
    }

    /// <summary>Nút Hint trên UI gọi hàm này.</summary>
    public void UseHint()
    {
        if (IsGameOver)
        {
            return;
        }

        var hint = _solverValidator.FindHintCell(_gridManager.Cells);
        if (hint == null)
        {
            _uiManager.ShowToast("Không tìm được gợi ý!");
            return;
        }

        hint.PlaceBunny();
        _hintsUsed++;
        // _solverValidator.Validate(_gridManager.Cells);
    }

    /// <summary>Nút Restart trên UI gọi hàm này.</summary>
    public void RestartLevel()
    {
        LoadLevel(CurrentPuzzle);
    }

    /// <summary>Nút Next Level trên Win Screen gọi hàm này.</summary>
    public void NextLevel()
    {
        LevelManager.Instance.AdvanceLevel();
        LoadLevel(LevelManager.Instance.GetCurrentPuzzle());
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void TriggerGameOver()
    {
        IsGameOver = true;
        _uiManager.ShowGameOverScreen();
    }
}
