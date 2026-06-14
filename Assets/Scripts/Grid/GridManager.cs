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
        new(1.00f, 0.45f, 0.45f), // hồng
        new(0.35f, 0.90f, 0.35f), // xanh lá
        new(0.40f, 0.60f, 1.00f), // xanh dương
        new(1.00f, 0.65f, 0.20f), // cam
        new(0.80f, 0.40f, 1.00f), // tím
        new(0.20f, 0.95f, 0.85f), // cyan
        new(0.25f, 1.00f, 0.60f), // mint
        new(1.00f, 0.30f, 0.65f), // hồng đậm
        new(0.65f, 1.00f, 0.20f), // xanh lá nhạt
        new(0.50f, 0.55f, 1.00f), // lavender
        new(1.00f, 0.95f, 0.20f), // vàng
        new(1.00f, 0.88f, 0.30f), // kem/vàng nhạt
    };

    [Header("Prefab")]
    [SerializeField]
    private Cell _cellPrefab;

    [Header("Layout")]
    [SerializeField]
    private float _gutterSize = 0.1f;

    public float GridCellSize { get; private set; }

    public float GridSize { get; private set; }

    public float GridGutter => _gutterSize;

    public Vector2 GridOriginInWorldSpace { get; private set; }

    public int[] BunnyCellResult { get; set; }

    // ── Public ─────────────────────────────────────────────────────────────
    public Cell[,] Cells { get; private set; }

    private void Awake()
    {
        GridSize = transform.localScale.x;
    }

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Xây dựng lưới từ PuzzleData.</summary>
    public void BuildGrid(PuzzleData puzzle)
    {
        var n = puzzle.gridSize;
        Cells = new Cell[n, n];

        GridCellSize = (GridSize - _gutterSize * (n - 1 + 2)) / n;
        var offset = (n - 1) * 0.5f * (GridCellSize + _gutterSize);

        for (var r = 0; r < n; r++)
        for (var c = 0; c < n; c++)
        {
            var pos = new Vector3(
                c * (GridCellSize + _gutterSize) - offset,
                -r * (GridCellSize + _gutterSize) + offset,
                0f
            );

            var cell = Instantiate(_cellPrefab, transform);
            cell.transform.localScale *= GridCellSize / GridSize;
            cell.transform.localPosition = pos / GridSize;

            var regionId = puzzle.regionMap[r * n + c];
            var color = RegionColors[regionId % RegionColors.Length];
            cell.Init(r, c, regionId, color);

            Cells[r, c] = cell;
        }

        foreach (var c in puzzle.revealCells)
        {
            var row = Mathf.FloorToInt(c / n);
            var col = c - row * n;

            Cells[row, col].ChangeState(Cell.State.Bunny);
        }

        BunnyCellResult = puzzle.solutionCells;

        GridOriginInWorldSpace = new Vector2(
            transform.position.x - GridSize * 0.5f,
            transform.position.y + GridSize * 0.5f
        );
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
