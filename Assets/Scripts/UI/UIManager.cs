using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Quản lý toàn bộ UI in-game:
///     • Mistake counter (trái tim)
///     • Win screen
///     • Game Over screen
///     • Toast notification
///     Gán tất cả references trong Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    // ── Mistake Counter ────────────────────────────────────────────────────
    [Header("Mistake Counter")]
    [Tooltip("Các Image trái tim — đủ số lượng bằng maxMistakes.")]
    [SerializeField]
    private Image[] heartIcons;

    [SerializeField]
    private Sprite heartFull;

    [SerializeField]
    private Sprite heartEmpty;

    // ── Win Screen ─────────────────────────────────────────────────────────
    [Header("Win Screen")]
    [SerializeField]
    private GameObject winScreenPanel;

    [SerializeField]
    private TextMeshProUGUI winTitleText;

    [SerializeField]
    private TextMeshProUGUI winStatsText; // "0 hints · 3 hearts left"

    [SerializeField]
    private Button nextLevelButton;

    [SerializeField]
    private Button restartFromWinButton;

    // ── Game Over Screen ───────────────────────────────────────────────────
    [Header("Game Over Screen")]
    [SerializeField]
    private GameObject gameOverPanel;

    [SerializeField]
    private Button retryButton;

    // ── Level Info ─────────────────────────────────────────────────────────
    [Header("Level Info")]
    [SerializeField]
    private TextMeshProUGUI levelLabel; // "Level 12"

    // ── Toast ──────────────────────────────────────────────────────────────
    [Header("Toast")]
    [SerializeField]
    private GameObject toastPanel;

    [SerializeField]
    private TextMeshProUGUI toastText;

    [SerializeField]
    private float toastDuration = 2f;

    private Coroutine toastCoroutine;
    public static UIManager Instance { get; private set; }

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
        // Nối nút với GameManager
        if (nextLevelButton)
        {
            nextLevelButton.onClick.AddListener(GameManager.Instance.NextLevel);
        }

        if (retryButton)
        {
            retryButton.onClick.AddListener(GameManager.Instance.RestartLevel);
        }

        if (restartFromWinButton)
        {
            restartFromWinButton.onClick.AddListener(GameManager.Instance.RestartLevel);
        }

        HideWinScreen();
        HideGameOverScreen();
        if (toastPanel)
        {
            toastPanel.SetActive(false);
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Cập nhật toàn bộ UI khi load level mới.</summary>
    public void RefreshAll(int mistakesLeft, int maxMistakes)
    {
        UpdateMistakes(mistakesLeft, maxMistakes);

        if (levelLabel)
        {
            levelLabel.text = $"Level {LevelManager.Instance.CurrentLevelIndex + 1}";
        }
    }

    /// <summary>Cập nhật hình trái tim theo số lần sai còn lại.</summary>
    public void UpdateMistakes(int mistakesLeft, int maxMistakes)
    {
        for (var i = 0; i < heartIcons.Length; i++)
        {
            if (heartIcons[i] == null)
            {
                continue;
            }

            heartIcons[i].sprite = i < mistakesLeft ? heartFull : heartEmpty;
        }
    }

    // ── Win Screen ─────────────────────────────────────────────────────────

    public void ShowWinScreen(int hintsUsed, int mistakesLeft)
    {
        if (!winScreenPanel)
        {
            return;
        }

        winScreenPanel.SetActive(true);

        if (winTitleText)
        {
            winTitleText.text = "🐰 Solved!";
        }

        if (winStatsText)
        {
            var hintStr =
                hintsUsed == 0 ? "No hints" : $"{hintsUsed} hint{(hintsUsed > 1 ? "s" : "")}";
            var heartStr = mistakesLeft == 0 ? "No hearts" : $"{mistakesLeft} ❤ left";
            winStatsText.text = $"{hintStr}  ·  {heartStr}";
        }

        // Ẩn Next Level nếu đây là level cuối
        if (nextLevelButton)
        {
            nextLevelButton.gameObject.SetActive(
                LevelManager.Instance.CurrentLevelIndex < LevelManager.Instance.TotalLevels - 1
            );
        }
    }

    public void HideWinScreen()
    {
        if (winScreenPanel)
        {
            winScreenPanel.SetActive(false);
        }
    }

    // ── Game Over Screen ───────────────────────────────────────────────────

    public void ShowGameOverScreen()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(true);
        }
    }

    public void HideGameOverScreen()
    {
        if (gameOverPanel)
        {
            gameOverPanel.SetActive(false);
        }
    }

    // ── Toast ──────────────────────────────────────────────────────────────

    public void ShowToast(string message)
    {
        if (!toastPanel || !toastText)
        {
            return;
        }

        if (toastCoroutine != null)
        {
            StopCoroutine(toastCoroutine);
        }

        toastCoroutine = StartCoroutine(ToastRoutine(message));
    }

    private IEnumerator ToastRoutine(string message)
    {
        toastText.text = message;
        toastPanel.SetActive(true);
        yield return new WaitForSeconds(toastDuration);
        toastPanel.SetActive(false);
    }
}
