using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Visual
{
    /// <summary>
    ///     Nhận input tap / double-tap từ chuột hoặc màn hình cảm ứng.
    ///     Single tap  → Cell.CycleState()  (đánh dấu X / bỏ dấu)
    ///     Double tap  → Cell.PlaceBunny()  (đặt / gỡ thỏ)
    ///     Hỗ trợ cả Editor (mouse) và mobile (touch).
    /// </summary>
    // public class Test : MonoBehaviour, InputSystem_Actions.IPlayerActions
    public class InputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField]
        private GridManager _gridManager;

        [SerializeField]
        private SolverValidator _solverValidator;

        [Header("Settings")]
        [SerializeField]
        private float _doubleTapThreshold = 0.3f;

        [SerializeField]
        private float dragThreshold = 5f; // pixel — di chuyển quá ngưỡng này mới tính là drag

        private HashSet<Cell> _draggedCells; // tránh mark lại Cell đã xử lý
        private Cell.State? _draggedCellState; // nullable to represent "not initialized"
        private Cell _firstDraggedCell;

        private InputSystem_Actions _inputActions;

        // ── Drag state ─────────────────────────────────────────────────────────
        private bool _isDragging;
        private Cell _lastTappedCell;

        // ── Tap state ──────────────────────────────────────────────────────────
        private float _lastTapTime;

        private Camera _mainCamera;
        private InputAction _positionAction;

        private InputAction _pressAction;
        private Vector2 _pressStartPosition; // vị trí pixel lúc nhấn xuống

        private void Awake()
        {
            _inputActions = new InputSystem_Actions();
            // _inputActions.Player.AddCallbacks(this);
            _pressAction = _inputActions.Player.Press;
            _positionAction = _inputActions.Player.Position;

            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (GameManager.Instance && GameManager.Instance.IsGameOver)
            {
                return;
            }

            // Chỉ chạy khi đang nhấn giữ
            if (!_pressAction.IsPressed())
            {
                return;
            }

            var currentScreenPos = _positionAction.ReadValue<Vector2>();

            // Kiểm tra ngưỡng drag (tránh nhầm tap rất nhỏ thành drag)
            if (!_isDragging)
            {
                var distance = Vector2.Distance(currentScreenPos, _pressStartPosition);
                if (distance < dragThreshold)
                {
                    return; // chưa đủ để tính là drag
                }

                _isDragging = true;
            }

            // Đang drag → mark Cell ở vị trí con trỏ
            HandleDragMove(currentScreenPos);
        }

        private void OnEnable()
        {
            _pressAction.performed += OnPressPerformed;
            _pressAction.canceled += OnPressCanceled;
            _inputActions.Player.Enable();
        }

        private void OnDisable()
        {
            _pressAction.performed -= OnPressPerformed;
            _pressAction.canceled -= OnPressCanceled;
            _inputActions.Player.Disable();
        }

        private void OnPressPerformed(InputAction.CallbackContext context)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            _pressStartPosition = _positionAction.ReadValue<Vector2>();
            _isDragging = false;
            _draggedCells = new HashSet<Cell>();
        }

        private void OnPressCanceled(InputAction.CallbackContext ctx)
        {
            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            if (!_isDragging)
            {
                // Không drag → xử lý như tap / double-tap
                HandleTap(_pressStartPosition);
            }

            // Kết thúc drag session
            _draggedCells?.Clear();
            _draggedCellState = null;
        }

        private void HandleDragMove(Vector2 screenPos)
        {
            var cell = GetCellAtScreenPos(screenPos);

            // Bỏ qua nếu không trúng ô nào, hoặc cùng ô vừa xử lý
            if (!cell || !_draggedCells.Add(cell))
            {
                return;
            }

            if (cell.CurrentState is Cell.State.Wrong or Cell.State.Bunny)
            {
                return;
            }

            if (_draggedCellState == null) // nullable check
            {
                _draggedCellState =
                    cell.CurrentState == Cell.State.Empty ? Cell.State.Marked : Cell.State.Empty;
            }

            cell.ChangeState(_draggedCellState.Value); // use .Value to unwrap the nullable
        }

        private void HandleTap(Vector2 screenPos)
        {
            var cell = GetCellAtScreenPos(screenPos);
            if (
                !cell
                || cell.CurrentState == Cell.State.Wrong
                || cell.CurrentState == Cell.State.Bunny
            )
            {
                return;
            }

            var timeSinceLast = Time.time - _lastTapTime;
            var isDoubleTap = cell == _lastTappedCell && timeSinceLast < _doubleTapThreshold;

            if (isDoubleTap)
            {
                _solverValidator.Validate(_gridManager.Cells, _gridManager.BunnyCellResult, cell);
            }
            else
            {
                cell.CycleState();
            }

            _lastTapTime = Time.time;
            _lastTappedCell = cell;
        }

        private Cell GetCellAtScreenPos(Vector2 screenPos)
        {
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);

            if (
                worldPos.x < _gridManager.GridOriginInWorldSpace.x
                || worldPos.y > _gridManager.GridOriginInWorldSpace.y
                || worldPos.x
                    > _gridManager.GridOriginInWorldSpace.x
                        + _gridManager.GridSize
                        - _gridManager.GridGutter
                || worldPos.y
                    < _gridManager.GridOriginInWorldSpace.y
                        - _gridManager.GridSize
                        + _gridManager.GridGutter
            )
            {
                return null;
            }

            var gridPos = new Vector2(
                worldPos.x - _gridManager.GridOriginInWorldSpace.x,
                worldPos.y - _gridManager.GridOriginInWorldSpace.y
            );

            var row = Mathf.FloorToInt(
                -gridPos.y / (_gridManager.GridCellSize + _gridManager.GridGutter)
            );
            var col = Mathf.FloorToInt(
                gridPos.x / (_gridManager.GridCellSize + _gridManager.GridGutter)
            );
            if (
                gridPos.x
                    < col * (_gridManager.GridCellSize + _gridManager.GridGutter)
                        + _gridManager.GridGutter
                || gridPos.y
                    > -row * (_gridManager.GridCellSize + _gridManager.GridGutter)
                        - _gridManager.GridGutter
            )
            {
                return null;
            }

            return _gridManager.Cells[row, col];
        }
    }
}
