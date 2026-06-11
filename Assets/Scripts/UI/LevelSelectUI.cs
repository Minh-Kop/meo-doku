using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
///     Level Select Screen — hiển thị tất cả level dưới dạng nút bấm.
///     Các level chưa mở khóa sẽ bị dim và không thể bấm.
///     Setup:
///     1. Tạo một ScrollView với Content grid.
///     2. Gán LevelButtonPrefab (Button + TextMeshPro).
///     3. Kéo Content transform vào levelButtonContainer.
///     4. Set gameSceneName đúng với tên scene gameplay.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private Transform levelButtonContainer;

    [SerializeField]
    private GameObject levelButtonPrefab;

    [Header("Scene")]
    [SerializeField]
    private string gameSceneName = "GameScene";

    [Header("Visuals")]
    [SerializeField]
    private Color unlockedColor = Color.white;

    [SerializeField]
    private Color lockedColor = new(0.5f, 0.5f, 0.5f, 0.6f);

    // ──────────────────────────────────────────────────────────────────────

    private void Start()
    {
        BuildButtons();
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void BuildButtons()
    {
        // Xóa các nút cũ (nếu có từ lần trước)
        foreach (Transform child in levelButtonContainer)
        {
            Destroy(child.gameObject);
        }

        var total = LevelManager.Instance.TotalLevels;

        for (var i = 0; i < total; i++)
        {
            var index = i; // Capture cho lambda
            var unlocked = LevelManager.Instance.IsLevelUnlocked(index);

            var btnObj = Instantiate(levelButtonPrefab, levelButtonContainer);
            var btn = btnObj.GetComponent<Button>();

            // Label
            var label = btnObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label)
            {
                label.text = unlocked ? (index + 1).ToString() : "🔒";
            }

            // Color
            var img = btnObj.GetComponent<Image>();
            if (img)
            {
                img.color = unlocked ? unlockedColor : lockedColor;
            }

            // Interactable
            btn.interactable = unlocked;
            if (unlocked)
            {
                btn.onClick.AddListener(() => SelectLevel(index));
            }
        }
    }

    private void SelectLevel(int index)
    {
        LevelManager.Instance.GoToLevel(index);
        SceneManager.LoadScene(gameSceneName);
    }
}
