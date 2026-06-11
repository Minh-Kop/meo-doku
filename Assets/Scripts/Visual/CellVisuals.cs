using System.Collections;
using UnityEngine;

/// <summary>
///     Xử lý tất cả visual feedback của một Cell:
///     • Pulse khi đặt thỏ
///     • Flash đỏ khi có lỗi
///     • Shake khi bị highlight lỗi
///     • Bounce khi win
///     Gắn cùng GameObject với Cell.cs.
/// </summary>
public class CellVisuals : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private SpriteRenderer _backgroundRenderer;

    [SerializeField]
    private Transform _bunnyTransform; // Transform của sprite con thỏ

    [Header("Colors")]
    [SerializeField]
    private Color _errorFlashColor = new(1f, 0.3f, 0.3f, 1f);

    [SerializeField]
    private Color _hintFlashColor = new(0.3f, 1f, 0.6f, 1f);

    [SerializeField]
    private float _flashDuration = 0.35f;

    [Header("Animations")]
    [SerializeField]
    private float _pulseDuration = 0.15f; // Thời gian scale lên khi đặt thỏ

    [SerializeField]
    private float _pulseScale = 1.25f;

    [SerializeField]
    private float _shakeMagnitude = 0.06f;

    [SerializeField]
    private float _shakeDuration = 0.3f;

    [SerializeField]
    private float _winBounceHeight = 0.3f;

    [SerializeField]
    private float _winBounceDuration = 0.4f;

    // Màu gốc của background — được set bởi Cell.Init()
    private Color _baseColor;

    private Coroutine _flashCoroutine;
    private Coroutine _shakeCoroutine;

    // ──────────────────────────────────────────────────────────────────────

    /// <summary>Gọi từ Cell.Init() để lưu lại màu nền gốc.</summary>
    public void SetBaseColor(Color color)
    {
        _baseColor = color;
        if (_backgroundRenderer)
        {
            _backgroundRenderer.color = color;
        }
    }

    // ── Placement Feedback ─────────────────────────────────────────────────

    /// <summary>Pulse nhỏ khi đặt thỏ vào ô.</summary>
    public void PlayPlacePulse()
    {
        if (_bunnyTransform == null)
        {
            return;
        }

        StopAllAnimOnBunny();
        StartCoroutine(PulseRoutine(_bunnyTransform, _pulseScale, _pulseDuration));
    }

    /// <summary>Shrink nhanh khi gỡ thỏ ra.</summary>
    public void PlayRemovePulse()
    {
        if (_bunnyTransform == null)
        {
            return;
        }

        StopAllAnimOnBunny();
        StartCoroutine(PulseRoutine(_bunnyTransform, 0f, _pulseDuration * 0.5f));
    }

    // ── Error Feedback ─────────────────────────────────────────────────────

    /// <summary>Flash đỏ nền ô khi vi phạm luật.</summary>
    public void FlashError()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(FlashColorRoutine(_errorFlashColor));
        if (_shakeCoroutine != null)
        {
            StopCoroutine(_shakeCoroutine);
        }

        _shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    /// <summary>Flash xanh lá khi là ô được hint.</summary>
    public void FlashHint()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
        }

        _flashCoroutine = StartCoroutine(FlashColorRoutine(_hintFlashColor));
    }

    /// <summary>Trở về màu gốc (reset error state).</summary>
    public void ResetColor()
    {
        if (_flashCoroutine != null)
        {
            StopCoroutine(_flashCoroutine);
            _flashCoroutine = null;
        }

        if (_backgroundRenderer)
        {
            _backgroundRenderer.color = _baseColor;
        }
    }

    // ── Win Animation ──────────────────────────────────────────────────────

    /// <summary>Bounce lên theo delay để tạo hiệu ứng wave khi win.</summary>
    public void PlayWinBounce(float delay = 0f)
    {
        StartCoroutine(WinBounceRoutine(delay));
    }

    // ── Coroutines ─────────────────────────────────────────────────────────

    private IEnumerator PulseRoutine(Transform target, float targetScale, float duration)
    {
        var origin = Vector3.one;
        var goal = Vector3.one * targetScale;
        var t = 0f;

        // Scale lên
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 0.5f);
            target.localScale = Vector3.Lerp(origin, goal, t);
            yield return null;
        }

        // Scale về lại
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 0.5f);
            target.localScale = Vector3.Lerp(goal, origin, t);
            yield return null;
        }

        target.localScale = Vector3.one;
    }

    private IEnumerator FlashColorRoutine(Color flashColor)
    {
        if (!_backgroundRenderer)
        {
            yield break;
        }

        var t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (_flashDuration * 0.5f);
            _backgroundRenderer.color = Color.Lerp(_baseColor, flashColor, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (_flashDuration * 0.5f);
            _backgroundRenderer.color = Color.Lerp(flashColor, _baseColor, t);
            yield return null;
        }

        _backgroundRenderer.color = _baseColor;
    }

    private IEnumerator ShakeRoutine()
    {
        var origin = transform.localPosition;
        var elapsed = 0f;

        while (elapsed < _shakeDuration)
        {
            elapsed += Time.deltaTime;
            var x = origin.x + Random.Range(-_shakeMagnitude, _shakeMagnitude);
            var y = origin.y + Random.Range(-_shakeMagnitude, _shakeMagnitude);
            transform.localPosition = new Vector3(x, y, origin.z);
            yield return null;
        }

        transform.localPosition = origin;
    }

    private IEnumerator WinBounceRoutine(float delay)
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        var origin = transform.localPosition;
        var top = origin + Vector3.up * _winBounceHeight;
        var t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / (_winBounceDuration * 0.4f);
            transform.localPosition = Vector3.Lerp(origin, top, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (_winBounceDuration * 0.6f);
            transform.localPosition = Vector3.Lerp(top, origin, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        transform.localPosition = origin;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private void StopAllAnimOnBunny()
    {
        // Dừng tất cả coroutine liên quan đến bunnyTransform bằng cách reset scale
        if (_bunnyTransform)
        {
            _bunnyTransform.localScale = Vector3.one;
        }
    }
}
