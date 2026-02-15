using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class BalloonTap : MonoBehaviour, IPointerDownHandler
{
    public System.Action onPopped;

    [Header("Pop Feedback")]
    [Tooltip("4 pop clips. One will be chosen randomly.")]
    public AudioClip[] popClips; // size 4
    [Tooltip("AudioSource in scene used to play pop sounds (2D).")]
    public AudioSource sfxSource;

    [Tooltip("Optional VFX prefab (particle). Leave empty if none.")]
    public GameObject popVfxPrefab;

    [Header("VFX in Canvas (works for UI + Sprite)")]
    public Canvas vfxCanvas;              // assign VFXCanvas here
    public Camera worldCameraForSprites;  // usually Main Camera


    [Header("Pop Animation (No Animator)")]
    public float popAnimTime = 0.12f;
    public float popScaleUp = 1.15f;   // quick squash-up before disappear

    // Safety: block double taps
    private bool _popped;
    private Collider2D _col2D;
    private CanvasGroup _canvasGroup; // for UI balloons (optional)

    private void Awake()
    {
        _col2D = GetComponent<Collider2D>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_popped) return;
        _popped = true;

        // 1) Prevent further clicks instantly (works for Sprite + UI)
        if (_col2D != null) _col2D.enabled = false;
        if (_canvasGroup != null) _canvasGroup.blocksRaycasts = false;

        // 2) Play SFX (random of 4)
        PlayRandomPopSfx();

        // 3) Spawn VFX (optional)
        SpawnVfx();

        // 4) Animate + then deactivate and notify
        StartCoroutine(PopRoutine());
    }

    private IEnumerator PopRoutine()
    {
        Vector3 startScale = transform.localScale;
        Vector3 peakScale = startScale * popScaleUp;

        float t = 0f;

        // Quick scale up (0 -> 1)
        while (t < popAnimTime)
        {
            t += Time.unscaledDeltaTime; // UI-friendly; doesn’t care about timescale
            float k = Mathf.Clamp01(t / popAnimTime);
            // simple ease-out
            float eased = 1f - Mathf.Pow(1f - k, 3f);
            transform.localScale = Vector3.LerpUnclamped(startScale, peakScale, eased);
            yield return null;
        }

        // Optional tiny extra frame, makes it feel snappy
        yield return null;

        // Important: now we "remove" balloon exactly like before
        gameObject.SetActive(false);

        // Notify controller (same as your original behavior, just delayed by ~0.12s)
        onPopped?.Invoke();
        

    }

    private void PlayRandomPopSfx()
    {
        if (sfxSource == null) return;
        if (popClips == null || popClips.Length == 0) return;

        int idx = Random.Range(0, popClips.Length);
        AudioClip clip = popClips[idx];
        if (clip == null) return;

        sfxSource.PlayOneShot(clip);
    }

    private void SpawnVfx()
    {
        if (popVfxPrefab == null) return;
        if (vfxCanvas == null) return;

        // 1) Get a screen position for BOTH cases
        Vector2 screenPos;

        RectTransform rt = transform as RectTransform;
        if (rt != null)
        {
            // UI balloon: convert its world position to screen point
            Camera uiCam = null;
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
                uiCam = parentCanvas.worldCamera;

            screenPos = RectTransformUtility.WorldToScreenPoint(uiCam, rt.position);
        }
        else
        {
            // Sprite balloon: convert world to screen using the camera
            Camera cam = worldCameraForSprites != null ? worldCameraForSprites : Camera.main;
            if (cam == null) return;
            Vector3 sp = cam.WorldToScreenPoint(transform.position);
            screenPos = new Vector2(sp.x, sp.y);
        }

        // 2) Convert screen point into VFXCanvas local position
        RectTransform canvasRect = vfxCanvas.transform as RectTransform;
        Camera vfxCam = (vfxCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : vfxCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, vfxCam, out Vector2 localPoint))
            return;

        // 3) Spawn as child of the canvas and place it
        GameObject vfx = Instantiate(popVfxPrefab, canvasRect);
        vfx.transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);

        // 4) Optional: auto-destroy after particles finish
        ParticleSystem ps = vfx.GetComponentInChildren<ParticleSystem>();
        if (ps != null)
        {
            float lifetime = ps.main.duration + ps.main.startLifetime.constantMax;
            Destroy(vfx, lifetime + 0.2f);
            ps.Play(true);
        }
        else
        {
            Destroy(vfx, 1f);
        }
    }

}
