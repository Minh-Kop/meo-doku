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
    private Image[] _heartIcons;

    [SerializeField]
    private Sprite _heartFull;

    [SerializeField]
    private Sprite _heartEmpty;

    [SerializeField]
    private TextMeshProUGUI _bunnyCounter;

    // ── Win Screen ─────────────────────────────────────────────────────────
    [Header("Win Screen")]
    [SerializeField]
    private GameObject _winScreenPanel;

    [SerializeField]
    private Button _nextLevelButton;

    [SerializeField]
    private TextMeshProUGUI _nextLevelButtonText;

    // ── Game Over Screen ───────────────────────────────────────────────────
    [Header("Game Over Screen")]
    [SerializeField]
    private GameObject _gameOverPanel;

    [SerializeField]
    private Button _retryButton;

    // ── Level Info ─────────────────────────────────────────────────────────
    [Header("Level Info")]
    [SerializeField]
    private TextMeshProUGUI _levelLabel; // "Level 12"

    // ── Toast ──────────────────────────────────────────────────────────────
    [Header("Toast")]
    [SerializeField]
    private GameObject _toastPanel;

    [SerializeField]
    private TextMeshProUGUI _toastText;

    [SerializeField]
    private float _toastDuration = 2f;

    private Coroutine _toastCoroutine;
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
        _nextLevelButton.onClick.AddListener(GameManager.Instance.NextLevel);
        _retryButton.onClick.AddListener(GameManager.Instance.RestartLevel);

        HideWinScreen();
        HideGameOverScreen();
        if (_toastPanel)
        {
            _toastPanel.SetActive(false);
        }
    }

    // ── Public API ─────────────────────────────────────────────────────────
    /// <summary>Cập nhật toàn bộ UI khi load level mới.</summary>
    public void RefreshAll(int mistakesLeft, int maxMistakes)
    {
        UpdateMistakes(mistakesLeft, maxMistakes);

        _levelLabel.text = $"Level {LevelManager.Instance.CurrentLevelIndex + 1}";
    }

    /// <summary>Cập nhật hình trái tim theo số lần sai còn lại.</summary>
    public void UpdateMistakes(int mistakesLeft, int maxMistakes)
    {
        for (var i = 0; i < _heartIcons.Length; i++)
        {
            if (_heartIcons[i] == null)
            {
                continue;
            }

            _heartIcons[i].sprite = i < mistakesLeft ? _heartFull : _heartEmpty;
        }
    }

    public void UpdateBunnyCounter(int bunnyCount)
    {
        _bunnyCounter.text =
            $"<color=#00C517>{bunnyCount}</color>/{GameManager.Instance.CurrentPuzzle.gridSize}";
    }

    // ── Win Screen ─────────────────────────────────────────────────────────
    public void ShowWinScreen()
    {
        _winScreenPanel.SetActive(true);

        // Ẩn Next Level nếu đây là level cuối
        _nextLevelButton.gameObject.SetActive(
            LevelManager.Instance.CurrentLevelIndex < LevelManager.Instance.TotalLevels - 1
        );
        _nextLevelButtonText.text = $"Level {LevelManager.Instance.CurrentLevelIndex + 2}";
    }

    public void HideWinScreen()
    {
        _winScreenPanel.SetActive(false);
    }

    // ── Game Over Screen ───────────────────────────────────────────────────
    public void ShowGameOverScreen()
    {
        _gameOverPanel.SetActive(true);
    }

    public void HideGameOverScreen()
    {
        _gameOverPanel.SetActive(false);
    }

    // ── Toast ──────────────────────────────────────────────────────────────
    public void ShowToast(string message)
    {
        if (!_toastPanel || !_toastText)
        {
            return;
        }

        if (_toastCoroutine != null)
        {
            StopCoroutine(_toastCoroutine);
        }

        _toastCoroutine = StartCoroutine(ToastRoutine(message));
    }

    private IEnumerator ToastRoutine(string message)
    {
        _toastText.text = message;
        _toastPanel.SetActive(true);
        yield return new WaitForSeconds(_toastDuration);
        _toastPanel.SetActive(false);
    }
}
