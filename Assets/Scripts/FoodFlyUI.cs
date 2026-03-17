using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Placed on a spawned food prefab (Image + RectTransform).
/// Drives: burst upward from balloon pop → arc down into animal's mouth → disappear.
/// Works entirely in the local space of its parent container RectTransform.
/// </summary>
[RequireComponent(typeof(Image))]
public class FoodFlyUI : MonoBehaviour
{
    [Header("Phase 1 – Burst Up")]
    public float burstHeight = 110f;        // pixels upward
    public float burstSideRange = 35f;      // random horizontal scatter
    public float burstTime = 0.22f;

    [Header("Phase 2 – Fall to Mouth")]
    public float fallTime = 0.52f;

    [Header("Spin")]
    public float spinSpeed = 480f;          // degrees per second

    [Header("Arrive Pop")]
    public float arrivePopTime = 0.1f;      // quick scale-to-zero on arrival

    [Header("Sound")]
    public AudioSource sfxSource;
    public AudioClip flySound;              // one-shot played when food launches

    [Header("Mouth VFX")]
    public GameObject mouthVfxPrefab;       // particle prefab spawned when food arrives

    // --- runtime ---
    private Image _img;
    private RectTransform _rt;
    private RectTransform _container;  // parent RT — all positions are relative to this
    private RectTransform _mouthTarget;
    private Action _onArrived;

    private void Awake()
    {
        _img = GetComponent<Image>();
        _rt = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Call immediately after instantiation (as child of container).
    /// startPos is in container's local anchoredPosition space.
    /// mouthTarget is any RectTransform in the scene — its world position is tracked each frame.
    /// </summary>
    public void StartFlight(Sprite foodSprite, Vector2 startPos, RectTransform mouthTarget, RectTransform container, Action onArrived)
    {
        if (foodSprite != null) _img.sprite = foodSprite;

        _container  = container;
        _mouthTarget = mouthTarget;
        _onArrived  = onArrived;

        _rt.anchoredPosition = startPos;
        _rt.localScale = Vector3.one;

        if (sfxSource != null && flySound != null)
            sfxSource.PlayOneShot(flySound);

        StartCoroutine(FlightRoutine(startPos));
    }

    private IEnumerator FlightRoutine(Vector2 startPos)
    {
        float spin = 0f;

        // ── Phase 1: burst upward from balloon pop force ──────────────────
        float burstX = UnityEngine.Random.Range(-burstSideRange, burstSideRange);
        Vector2 peak = startPos + new Vector2(burstX, burstHeight);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, burstTime);
            float k = EaseOut(t);
            _rt.anchoredPosition = Vector2.Lerp(startPos, peak, k);
            spin += spinSpeed * Time.deltaTime;
            _rt.localEulerAngles = new Vector3(0f, 0f, spin);
            yield return null;
        }

        // ── Phase 2: fall straight toward mouth (ease-in = accelerate down) ─
        Vector2 fallStart = _rt.anchoredPosition;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, fallTime);
            float k = EaseIn(t);
            Vector2 mouthPos = GetMouthInContainerSpace();
            _rt.anchoredPosition = Vector2.Lerp(fallStart, mouthPos, k);
            spin += spinSpeed * Time.deltaTime;
            _rt.localEulerAngles = new Vector3(0f, 0f, spin);
            yield return null;
        }

        // ── Arrive: spawn mouth VFX then scale to zero ───────────────────
        SpawnMouthVfx();

        Vector3 s0 = _rt.localScale;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, arrivePopTime);
            _rt.localScale = Vector3.Lerp(s0, Vector3.zero, EaseOut(t));
            yield return null;
        }

        _onArrived?.Invoke();
        Destroy(gameObject);
    }

    private void SpawnMouthVfx()
    {
        if (mouthVfxPrefab == null || _container == null) return;

        // Spawn in the same container as the food, at the food's current position (= mouth)
        GameObject vfx = Instantiate(mouthVfxPrefab, _container);
        vfx.GetComponent<RectTransform>().anchoredPosition = _rt.anchoredPosition;
        vfx.transform.SetAsLastSibling();

        ParticleSystem ps = vfx.GetComponentInChildren<ParticleSystem>();
        float lifetime = ps != null ? ps.main.duration + ps.main.startLifetime.constantMax + 0.2f : 1.5f;
        Destroy(vfx, lifetime);
    }

    // Returns mouth position in container's local anchoredPosition space.
    private Vector2 GetMouthInContainerSpace()
    {
        if (_mouthTarget == null || _container == null)
            return _rt.anchoredPosition; // hover in place if refs missing

        // world → container local
        return (Vector2)_container.InverseTransformPoint(_mouthTarget.position);
    }

    private static float EaseOut(float t) { t = Mathf.Clamp01(t); return 1f - Mathf.Pow(1f - t, 2f); }
    private static float EaseIn(float t)  { t = Mathf.Clamp01(t); return t * t; }
}
