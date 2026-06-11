using System.Collections;
using UnityEngine;

/// <summary>
///     Phát hiệu ứng "bounce wave" qua toàn bộ các Cell khi người chơi thắng.
///     Gắn vào cùng GameObject với GridManager hoặc GameManager.
///     Cách dùng: Gọi WinAnimator.Instance.PlayWinWave() từ GameManager.OnWin().
/// </summary>
public class WinAnimator : MonoBehaviour
{
    [Header("Wave Settings")]
    [Tooltip("Khoảng thời gian delay giữa mỗi ô (tính theo cột).")]
    [SerializeField]
    private float delayPerColumn = 0.05f;

    [Tooltip("Thêm delay nhỏ giữa các hàng.")]
    [SerializeField]
    private float delayPerRow = 0.02f;

    [Header("References")]
    [SerializeField]
    private GridManager gridManager;

    public static WinAnimator Instance { get; private set; }

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

    // ── Public API ─────────────────────────────────────────────────────────

    /// <summary>Kích hoạt hiệu ứng wave từ góc trái-trên sang phải-dưới.</summary>
    public void PlayWinWave()
    {
        StartCoroutine(WaveRoutine());
    }

    // ── Private ────────────────────────────────────────────────────────────

    private IEnumerator WaveRoutine()
    {
        var cells = gridManager.Cells;
        if (cells == null)
        {
            yield break;
        }

        var rows = cells.GetLength(0);
        var cols = cells.GetLength(1);

        for (var r = 0; r < rows; r++)
        for (var c = 0; c < cols; c++)
        {
            var cell = cells[r, c];
            if (cell == null)
            {
                continue;
            }

            var visuals = cell.GetComponent<CellVisuals>();
            if (visuals == null)
            {
                continue;
            }

            var delay = c * delayPerColumn + r * delayPerRow;
            visuals.PlayWinBounce(delay);
        }

        // Đợi animation xong rồi mới cho UIManager hiện panel
        var maxDelay = (cols - 1) * delayPerColumn + (rows - 1) * delayPerRow + 0.5f;
        yield return new WaitForSeconds(maxDelay);
    }
}
