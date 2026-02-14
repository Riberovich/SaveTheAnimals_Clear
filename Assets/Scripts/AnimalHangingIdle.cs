using UnityEngine;
using UnityEngine.UI;

public class AnimalHangingIdle : MonoBehaviour
{
    [Header("Target (optional)")]
    [SerializeField] private RectTransform uiTarget;     // if UI
    [SerializeField] private Transform worldTarget;      // if Sprite/World

    [Header("Sway (slow)")]
    [SerializeField] private float swayDegrees = 2.5f;
    [SerializeField] private float swaySpeed = 0.7f;

    [Header("Tremble (tiny)")]
    [SerializeField] private float tremblePos = 2.0f;    // pixels for UI, units for world (keep tiny)
    [SerializeField] private float trembleRot = 0.6f;    // degrees
    [SerializeField] private float trembleSpeed = 2.5f;

    [Header("Breathing (scale)")]
    [SerializeField] private float breatheScale = 0.02f; // 2%
    [SerializeField] private float breatheSpeed = 1.1f;

    [Header("Blink (optional)")]
    [SerializeField] private bool enableBlink = true;
    [SerializeField] private Image uiImage;              // UI animal image
    [SerializeField] private SpriteRenderer spriteRenderer; // world animal sprite
    [SerializeField] private Sprite blinkSprite;         // closed eyes sprite (or blink variant)
    [SerializeField] private float blinkDuration = 0.10f;
    [SerializeField] private Vector2 blinkIntervalRange = new Vector2(2.0f, 6.0f);

    private bool isUI;
    private bool isEnabled = true;

    private Vector2 uiStartPos;
    private Quaternion uiStartRot;
    private Vector3 uiStartScale;

    private Vector3 wStartPos;
    private Quaternion wStartRot;
    private Vector3 wStartScale;

    private Sprite originalSprite;
    private float nextBlinkTime = -1f;
    private float blinkEndTime = -1f;

    private float noiseSeed;

    private void Awake()
    {
        // Auto-detect if target not assigned
        if (uiTarget == null && worldTarget == null)
        {
            uiTarget = GetComponent<RectTransform>();
            if (uiTarget != null) isUI = true;
            else worldTarget = transform;
        }

        if (uiTarget != null)
        {
            isUI = true;
            uiStartPos = uiTarget.anchoredPosition;
            uiStartRot = uiTarget.localRotation;
            uiStartScale = uiTarget.localScale;
        }
        else
        {
            isUI = false;
            wStartPos = worldTarget.localPosition;
            wStartRot = worldTarget.localRotation;
            wStartScale = worldTarget.localScale;
        }

        // Cache original sprite for blink swap
        if (uiImage == null && isUI) uiImage = GetComponent<Image>();
        if (spriteRenderer == null && !isUI) spriteRenderer = GetComponent<SpriteRenderer>();

        if (uiImage != null) originalSprite = uiImage.sprite;
        if (spriteRenderer != null) originalSprite = spriteRenderer.sprite;

        noiseSeed = Random.Range(0f, 999f);
        ScheduleNextBlink();
    }

    private void Update()
    {
        if (!isEnabled) return;

        float t = Time.time;

        // Slow sway
        float sway = Mathf.Sin(t * swaySpeed) * swayDegrees;

        // Tiny tremble via Perlin
        float n1 = Mathf.PerlinNoise(noiseSeed, t * trembleSpeed) - 0.5f;
        float n2 = Mathf.PerlinNoise(noiseSeed + 10f, t * trembleSpeed) - 0.5f;

        float trembleX = n1 * tremblePos;
        float trembleY = n2 * tremblePos;
        float trembleR = (n1 + n2) * 0.5f * trembleRot;

        // Breathing scale
        float b = 1f + Mathf.Sin(t * breatheSpeed) * breatheScale;

        if (isUI)
        {
            uiTarget.anchoredPosition = uiStartPos + new Vector2(trembleX, trembleY);
            uiTarget.localRotation = uiStartRot * Quaternion.Euler(0f, 0f, sway + trembleR);
            uiTarget.localScale = new Vector3(uiStartScale.x * b, uiStartScale.y * (2f - b), uiStartScale.z);
        }
        else
        {
            worldTarget.localPosition = wStartPos + new Vector3(trembleX, trembleY, 0f);
            worldTarget.localRotation = wStartRot * Quaternion.Euler(0f, 0f, sway + trembleR);
            worldTarget.localScale = new Vector3(wStartScale.x * b, wStartScale.y * (2f - b), wStartScale.z);
        }

        HandleBlink(t);
    }

    private void HandleBlink(float t)
    {
        if (!enableBlink) return;
        if (blinkSprite == null) return;
        if (originalSprite == null) return;

        // start blink
        if (blinkEndTime < 0f && t >= nextBlinkTime)
        {
            SetSprite(blinkSprite);
            blinkEndTime = t + Mathf.Max(0.02f, blinkDuration);
        }

        // end blink
        if (blinkEndTime > 0f && t >= blinkEndTime)
        {
            SetSprite(originalSprite);
            blinkEndTime = -1f;
            ScheduleNextBlink();
        }
    }

    private void SetSprite(Sprite s)
    {
        if (uiImage != null) uiImage.sprite = s;
        if (spriteRenderer != null) spriteRenderer.sprite = s;
    }

    private void ScheduleNextBlink()
    {
        float min = Mathf.Max(0.2f, blinkIntervalRange.x);
        float max = Mathf.Max(min, blinkIntervalRange.y);
        nextBlinkTime = Time.time + Random.Range(min, max);
    }

    // Public API for your other controllers:
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;

        // Reset to clean pose when disabled (important for landing)
        if (!enabled)
        {
            if (isUI)
            {
                uiTarget.anchoredPosition = uiStartPos;
                uiTarget.localRotation = uiStartRot;
                uiTarget.localScale = uiStartScale;
            }
            else
            {
                worldTarget.localPosition = wStartPos;
                worldTarget.localRotation = wStartRot;
                worldTarget.localScale = wStartScale;
            }

            // restore sprite if mid-blink
            if (originalSprite != null) SetSprite(originalSprite);
            blinkEndTime = -1f;
            ScheduleNextBlink();
        }
    }
    public void StopAndReset()
    {
        // зупин€Їмо лог≥ку
        isEnabled = false;

        // скидаЇмо позу
        if (isUI)
        {
            if (uiTarget != null)
            {
                uiTarget.anchoredPosition = uiStartPos;
                uiTarget.localRotation = uiStartRot;
                uiTarget.localScale = uiStartScale;
            }
        }
        else
        {
            if (worldTarget != null)
            {
                worldTarget.localPosition = wStartPos;
                worldTarget.localRotation = wStartRot;
                worldTarget.localScale = wStartScale;
            }
        }

        // повертаЇмо нормальний спрайт (щоб не УзависФ blinkSprite)
        if (originalSprite != null)
            SetSprite(originalSprite);

        // скидаЇмо морганн€
        blinkEndTime = -1f;
        ScheduleNextBlink();
    }

    private void OnDisable()
    {
        // €кщо компонент вимкнули п≥д час посадки Ч все одно очистити стан
        StopAndReset();
    }

}
