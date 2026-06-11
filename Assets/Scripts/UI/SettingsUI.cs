using UnityEngine;
using UnityEngine.UI;

/// <summary>
///     Settings Panel — reset tiến độ.
///     Setup trong Inspector:
///     • resetButton  → Button
///     • closeButton  → Button (ẩn panel)
///     • settingsPanel → GameObject chứa toàn bộ UI này
/// </summary>
public class SettingsUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField]
    private GameObject settingsPanel;

    [Header("Buttons")]
    [SerializeField]
    private Button resetProgressButton;

    [SerializeField]
    private Button closeButton;

    // ──────────────────────────────────────────────────────────────────────

    private void Start()
    {
        if (resetProgressButton)
        {
            resetProgressButton.onClick.AddListener(OnResetProgress);
        }

        if (closeButton)
        {
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    // ── Public API ─────────────────────────────────────────────────────────

    public void Show()
    {
        if (settingsPanel)
        {
            settingsPanel.SetActive(true);
        }
    }

    public void Hide()
    {
        if (settingsPanel)
        {
            settingsPanel.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (!settingsPanel)
        {
            return;
        }

        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }

    // ── Callbacks ──────────────────────────────────────────────────────────

    private void OnResetProgress()
    {
        LevelManager.Instance.ResetProgress();
        UIManager.Instance?.ShowToast("Đã reset tiến độ!");
    }
}
