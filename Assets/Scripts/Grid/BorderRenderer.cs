using UnityEngine;

// Sau khi BuildGrid xong, gọi BorderRenderer

/// <summary>
///     Vẽ đường border đậm giữa các region khác nhau trên board.
///     Gọi DrawBorders() sau khi GridManager.BuildGrid() xong.
///     Cách hoạt động:
///     Duyệt từng Cell, kiểm tra 4 cạnh (trên/dưới/trái/phải).
///     Nếu Cell kế bên thuộc region khác (hoặc nằm ngoài lưới) → spawn một LineRenderer edge.
/// </summary>
public class BorderRenderer : MonoBehaviour
{
    [Header("Border Visual")]
    [SerializeField]
    private Color _borderColor = new(0.2f, 0.2f, 0.2f, 1f);

    [SerializeField]
    private float _borderWidth = 0.08f;

    [SerializeField]
    private float _borderZOffset = -0.1f; // Đặt phía trước Cell

    [Header("Inner Grid Line (tùy chọn)")]
    [SerializeField]
    private bool _drawInnerLines = true;

    [SerializeField]
    private Color _innerLineColor = new(0.6f, 0.6f, 0.6f, 0.4f);

    [SerializeField]
    private float _innerLineWidth = 0.02f;

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Vẽ toàn bộ border. Gọi từ GameManager sau BuildGrid().</summary>
    public void DrawBorders(Cell[,] cells, float cellSize)
    {
        // Xóa border cũ
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        if (cells == null)
        {
            return;
        }

        var n = cells.GetLength(0);

        for (var r = 0; r < n; r++)
        for (var c = 0; c < n; c++)
        {
            var cell = cells[r, c];
            var center = cell.transform.position;
            var half = cellSize * 0.5f;

            // ── Cạnh trên ──────────────────────────────────────────────────
            if (NeedsBorder(cells, r, c, r - 1, c))
            {
                DrawEdge(
                    center + new Vector3(-half, +half, _borderZOffset),
                    center + new Vector3(+half, +half, _borderZOffset),
                    _borderColor,
                    _borderWidth
                );
            }
            else if (_drawInnerLines)
            {
                DrawEdge(
                    center + new Vector3(-half, +half, _borderZOffset),
                    center + new Vector3(+half, +half, _borderZOffset),
                    _innerLineColor,
                    _innerLineWidth
                );
            }

            // ── Cạnh dưới ─────────────────────────────────────────────────
            if (NeedsBorder(cells, r, c, r + 1, c))
            {
                DrawEdge(
                    center + new Vector3(-half, -half, _borderZOffset),
                    center + new Vector3(+half, -half, _borderZOffset),
                    _borderColor,
                    _borderWidth
                );
            }

            // ── Cạnh trái ─────────────────────────────────────────────────
            if (NeedsBorder(cells, r, c, r, c - 1))
            {
                DrawEdge(
                    center + new Vector3(-half, +half, _borderZOffset),
                    center + new Vector3(-half, -half, _borderZOffset),
                    _borderColor,
                    _borderWidth
                );
            }
            else if (_drawInnerLines)
            {
                DrawEdge(
                    center + new Vector3(-half, +half, _borderZOffset),
                    center + new Vector3(-half, -half, _borderZOffset),
                    _innerLineColor,
                    _innerLineWidth
                );
            }

            // ── Cạnh phải ─────────────────────────────────────────────────
            if (NeedsBorder(cells, r, c, r, c + 1))
            {
                DrawEdge(
                    center + new Vector3(+half, +half, _borderZOffset),
                    center + new Vector3(+half, -half, _borderZOffset),
                    _borderColor,
                    _borderWidth
                );
            }
        }
    }

    // ── Private ────────────────────────────────────────────────────────────

    /// <summary>Trả về true nếu cần vẽ border đậm giữa [r,c] và [nr,nc].</summary>
    private bool NeedsBorder(Cell[,] cells, int r, int c, int nr, int nc)
    {
        var n = cells.GetLength(0);
        // Ngoài lưới → luôn vẽ border
        if (nr < 0 || nr >= n || nc < 0 || nc >= n)
        {
            return true;
        }

        // Khác region → vẽ border
        return cells[r, c].RegionId != cells[nr, nc].RegionId;
    }

    private void DrawEdge(Vector3 start, Vector3 end, Color color, float width)
    {
        var go = new GameObject("Edge");
        go.transform.SetParent(transform, true);

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = width;
        lr.endWidth = width;
        lr.startColor = color;
        lr.endColor = color;
        lr.useWorldSpace = true;

        // Dùng Sprites-Default để không cần assign material thủ công
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 5;
    }
}
