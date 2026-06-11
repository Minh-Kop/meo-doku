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
    public class Test : MonoBehaviour
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

        private InputSystem_Actions _inputActions;

        // ── Drag state ─────────────────────────────────────────────────────────
        private bool _isDragging;
        private Cell _lastDraggedCell; // Cell cuối cùng đã được mark trong drag
        private Cell _lastTappedCell;

        // ── Tap state ──────────────────────────────────────────────────────────
        private float _lastTapTime;

        private Camera _mainCamera;

        private HashSet<Cell> _markedThisDrag; // tránh mark lại Cell đã xử lý
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

        // ── Tap / Double-tap ──────────────────────────────────────────────────

        private void HandleTap(Vector2 screenPos)
        {
            var cell = GetCellAtScreenPos(screenPos);
            if (cell == null)
            {
                return;
            }

            var timeSinceLast = Time.time - _lastTapTime;
            var isDoubleTap = cell == _lastTappedCell && timeSinceLast < _doubleTapThreshold;

            if (isDoubleTap)
            {
                cell.PlaceBunny();
            }
            else
            {
                cell.CycleState();
            }

            _lastTapTime = Time.time;
            _lastTappedCell = cell;

            // _solverValidator.Validate(_gridManager.Cells);
        }

        // ── Drag move ─────────────────────────────────────────────────────────

        private void HandleDragMove(Vector2 screenPos)
        {
            var cell = GetCellAtScreenPos(screenPos);

            // Bỏ qua nếu không trúng ô nào, hoặc cùng ô vừa xử lý
            if (!cell)
            {
                return;
            }

            if (!_markedThisDrag.Add(cell))
            {
                return;
            }

            // Chỉ mark ô trống — không gỡ thỏ đã đặt khi kéo qua
            if (cell.CurrentState == Cell.State.Empty)
            {
                cell.CycleState(); // Empty → Marked
            }

            _lastDraggedCell = cell;
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
                var releasePos = _positionAction.ReadValue<Vector2>();
                HandleTap(releasePos);
            }

            // Kết thúc drag session
            _isDragging = false;
            _lastDraggedCell = null;
            _markedThisDrag?.Clear();
        }

        // public void OnPress(InputAction.CallbackContext context)
        private void OnPressPerformed(InputAction.CallbackContext context)
        {
            // var screenPos =
            //     Mouse.current != null ? Mouse.current.position.ReadValue()
            //     : Touchscreen.current != null
            //         ? Touchscreen.current.primaryTouch.position.ReadValue()
            //     : Vector2.zero;
            // Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
            // ProcessTap(worldPos);

            if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
            {
                return;
            }

            _pressStartPosition = _positionAction.ReadValue<Vector2>();
            _isDragging = false;
            _markedThisDrag = new HashSet<Cell>();
            _lastDraggedCell = null;
        }

        private void ProcessTap(Vector2 worldPos)
        {
            var cell = GetCellAt(worldPos);
            if (!cell)
            {
                return;
            }

            var timeSinceLast = Time.time - _lastTapTime;
            var isDoubleTap = cell == _lastTappedCell && timeSinceLast < _doubleTapThreshold;

            if (isDoubleTap)
            {
                cell.PlaceBunny();
            }
            else
            {
                cell.CycleState();
            }

            _lastTapTime = Time.time;
            _lastTappedCell = cell;

            // solverValidator.Validate(gridManager.Cells);
        }

        private Cell GetCellAt(Vector2 worldPos)
        {
            var hit = Physics2D.OverlapPoint(worldPos);
            return hit ? hit.GetComponent<Cell>() : null;
        }

        private Cell GetCellAtScreenPos(Vector2 screenPos)
        {
            Vector2 worldPos = _mainCamera.ScreenToWorldPoint(screenPos);
            var hit = Physics2D.OverlapPoint(worldPos);
            return hit ? hit.GetComponent<Cell>() : null;
        }
    }
}
