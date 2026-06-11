using UnityEngine;

/// <summary>
///     Đại diện cho một ô trên lưới.
///     Mỗi ô có 3 trạng thái: Empty → Marked (X) → Bunny
///     Yêu cầu trên cùng GameObject:
///     • SpriteRenderer  (bg)
///     • BoxCollider2D   (để InputHandler detect click)
///     • CellVisuals     (visual feedback)
///     • GameObject con tên "XMark"    (sprite dấu X)
///     • GameObject con tên "Bunny"    (sprite con thỏ)
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(CellVisuals))]
public class Cell : MonoBehaviour
{
    public enum State
    {
        Empty,
        Marked,
        Bunny,
    }

    // ── References ─────────────────────────────────────────────────────────
    [SerializeField]
    private SpriteRenderer _backgroundRenderer;

    [SerializeField]
    private GameObject _xMark;

    [SerializeField]
    private GameObject _bunnyObject;

    private CellVisuals _visuals;

    // ── Data ───────────────────────────────────────────────────────────────
    public int Row { get; private set; }
    public int Col { get; private set; }
    public int RegionId { get; private set; }

    public State CurrentState { get; private set; }
    public bool IsError { get; private set; }

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Gọi từ GridManager sau khi Instantiate.</summary>
    public void Init(int row, int col, int regionId, Color color)
    {
        Row = row;
        Col = col;
        RegionId = regionId;

        _visuals = GetComponent<CellVisuals>();
        _visuals.SetBaseColor(color);

        SetState(State.Empty);
    }

    // ── Input Actions ──────────────────────────────────────────────────────

    /// <summary>Single tap — toggle Marked / Empty.</summary>
    public void CycleState()
    {
        if (CurrentState == State.Bunny)
        {
            return; // tap không ảnh hưởng ô đã có thỏ
        }

        SetState(CurrentState == State.Marked ? State.Empty : State.Marked);
    }

    /// <summary>Double tap — toggle Bunny / Empty.</summary>
    public void PlaceBunny()
    {
        if (CurrentState == State.Bunny)
        {
            _visuals.PlayRemovePulse();
            SetState(State.Empty);
        }
        else
        {
            SetState(State.Bunny);
            _visuals.PlayPlacePulse();
        }
    }

    // ── Error Highlight ────────────────────────────────────────────────────

    /// <summary>Gọi từ SolverValidator để highlight lỗi.</summary>
    public void SetError(bool error)
    {
        IsError = error;
        if (error)
        {
            _visuals.FlashError();
        }
        else
        {
            _visuals.ResetColor();
        }
    }

    // ── Private ────────────────────────────────────────────────────────────

    private void SetState(State newState)
    {
        CurrentState = newState;
        if (_xMark)
        {
            _xMark.SetActive(newState == State.Marked);
        }

        if (_bunnyObject)
        {
            _bunnyObject.SetActive(newState == State.Bunny);
        }
    }
}
