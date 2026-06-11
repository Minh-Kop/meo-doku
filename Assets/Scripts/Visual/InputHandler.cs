using UnityEngine;

/// <summary>
///     Nhận input tap / double-tap từ chuột hoặc màn hình cảm ứng.
///     Single tap  → Cell.CycleState()  (đánh dấu X / bỏ dấu)
///     Double tap  → Cell.PlaceBunny()  (đặt / gỡ thỏ)
///     Hỗ trợ cả Editor (mouse) và mobile (touch).
/// </summary>
public class InputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GridManager gridManager;

    [SerializeField]
    private SolverValidator solverValidator;

    [Header("Settings")]
    [SerializeField]
    private float doubleTapThreshold = 0.3f; // giây

    private Cell lastTappedCell;

    private float lastTapTime;

    // ──────────────────────────────────────────────────────────────────────

    private void Update()
    {
        if (GameManager.Instance && GameManager.Instance.IsGameOver)
        {
            return;
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    // ── Mouse (Editor / Desktop) ───────────────────────────────────────────

    private void HandleMouseInput()
    {
        if (!Input.GetMouseButtonDown(0))
        {
            return;
        }

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        ProcessTap(worldPos);
    }

    // ── Touch (Mobile) ────────────────────────────────────────────────────

    private void HandleTouchInput()
    {
        if (Input.touchCount == 0)
        {
            return;
        }

        var touch = Input.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
        {
            return;
        }

        Vector2 worldPos = Camera.main.ScreenToWorldPoint(touch.position);
        ProcessTap(worldPos);
    }

    // ── Core Logic ─────────────────────────────────────────────────────────

    private void ProcessTap(Vector2 worldPos)
    {
        var cell = GetCellAt(worldPos);
        if (!cell)
        {
            return;
        }

        var timeSinceLast = Time.time - lastTapTime;
        var isDoubleTap = cell == lastTappedCell && timeSinceLast < doubleTapThreshold;

        if (isDoubleTap)
        {
            cell.PlaceBunny();
        }
        else
        {
            cell.CycleState();
        }

        lastTapTime = Time.time;
        lastTappedCell = cell;

        solverValidator.Validate(gridManager.Cells);
    }

    private Cell GetCellAt(Vector2 worldPos)
    {
        var hit = Physics2D.OverlapPoint(worldPos);
        return hit ? hit.GetComponent<Cell>() : null;
    }
}
