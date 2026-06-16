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

    [SerializeField]
    private float _gridPadding = 0.3f;

    [SerializeField]
    private float _gridSize = 5.5f;

    private SpriteRenderer _gridRenderer;

    public float GridCellSize { get; private set; }

    public float GridSize => _gridSize;

    public float GridGutter => _gutterSize;

    public float GridPadding => _gridPadding;

    public Vector2 GridOriginInWorldSpace { get; private set; }

    public int[] BunnyCellResult { get; set; }

    // ── Public ─────────────────────────────────────────────────────────────
    public Cell[,] Cells { get; private set; }

    private void Awake()
    {
        _gridRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        _gridRenderer.size = new Vector2(_gridSize, _gridSize);
    }

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Xây dựng lưới từ PuzzleData.</summary>
    public void BuildGrid(PuzzleData puzzle)
    {
        ScaleToFitScreen();

        var n = puzzle.gridSize;
        Cells = new Cell[n, n];

        GridCellSize = (_gridSize - _gridPadding * 2 - _gutterSize * (n - 1)) / n;
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
            cell.transform.localScale *= GridCellSize;
            cell.transform.localPosition = pos;

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
            transform.position.x - _gridSize * 0.5f + _gridPadding,
            transform.position.y + _gridSize * 0.5f - _gridPadding
        );
    }

    private void ScaleToFitScreen()
    {
        var cam = Camera.main;
        var worldWidth = cam.orthographicSize * 2f * cam.aspect;
        var worldHeight = cam.orthographicSize * 2f;

        print($"Grid size: {_gridSize}");
        print($"World size: {worldWidth} x {worldHeight}");

        var scaleX = worldWidth / _gridSize;
        var scaleY = worldHeight / _gridSize;

        print($"ScaleX: {scaleX}, ScaleY: {scaleY}");

        // Chọn cái nhỏ hơn để object không bị crop
        var scale = Mathf.Min(scaleX, scaleY);
        if (scale < 1f)
        {
            // transform.localScale *= scale * 0.98f;
            scale *= 0.98f;
            _gridRenderer.transform.localScale *= scale;
            _gridSize *= scale;
            _gutterSize *= scale;
            _gridPadding *= scale;
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
