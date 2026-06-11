using UnityEngine;

/// <summary>
///     Sinh và quản lý toàn bộ lưới Cell.
///     Gắn vào một GameObject rỗng trong scene (đặt tên "GridManager").
/// </summary>
public class GridManager : MonoBehaviour
{
    // Màu cho từng region (tối đa 12 region cho lưới 12×12)
    private static readonly Color[] RegionColors =
    {
        new(1.00f, 0.82f, 0.82f), // hồng
        new(0.82f, 1.00f, 0.82f), // xanh lá
        new(0.82f, 0.88f, 1.00f), // xanh dương
        new(1.00f, 1.00f, 0.78f), // vàng
        new(0.94f, 0.82f, 1.00f), // tím nhạt
        new(0.82f, 1.00f, 0.98f), // cyan nhạt
        new(1.00f, 0.90f, 0.78f), // cam nhạt
        new(0.78f, 1.00f, 0.88f), // mint
        new(1.00f, 0.78f, 0.90f), // hồng đậm
        new(0.88f, 0.96f, 0.78f), // xanh lá nhạt
        new(0.78f, 0.90f, 1.00f), // lavender
        new(1.00f, 0.96f, 0.82f), // kem
    };

    [Header("Prefab")]
    [SerializeField]
    private Cell _cellPrefab;

    [Header("Layout")]
    [SerializeField]
    private float _gutterSize = 0.1f;

    private float _gridWidth;

    // ── Public ─────────────────────────────────────────────────────────────
    public Cell[,] Cells { get; private set; }

    private void Awake()
    {
        _gridWidth = transform.localScale.x;
    }

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Xây dựng lưới từ PuzzleData.</summary>
    public void BuildGrid(PuzzleData puzzle)
    {
        var n = puzzle.gridSize;
        Cells = new Cell[n, n];

        var cellSize = (_gridWidth - _gutterSize * (n - 1 + 2)) / n;
        var offset = (n - 1) * 0.5f * (cellSize + _gutterSize);

        for (var r = 0; r < n; r++)
        for (var c = 0; c < n; c++)
        {
            var pos = new Vector3(
                c * (cellSize + _gutterSize) - offset,
                -r * (cellSize + _gutterSize) + offset,
                0f
            );
            print(pos);

            var cell = Instantiate(_cellPrefab, pos, Quaternion.identity, transform);
            cell.transform.localScale *= cellSize / _gridWidth;

            var regionId = puzzle.regionMap[r * n + c];
            var color = RegionColors[regionId % RegionColors.Length];
            cell.Init(r, c, regionId, color);

            Cells[r, c] = cell;
        }
    }

    /// <summary>Xóa toàn bộ Cell con — gọi trước khi load level mới.</summary>
    public void ClearGrid()
    {
        if (Cells == null)
        {
            return;
        }

        foreach (var c in Cells)
        {
            if (c != null)
            {
                Destroy(c.gameObject);
            }
        }

        Cells = null;
    }
}
