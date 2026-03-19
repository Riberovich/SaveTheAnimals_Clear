using UnityEngine;
using UnityEngine.UI;

public class SaveTheAnimalController : MonoBehaviour
{
    [Header("Background Auto-Start (LEGACY - now disabled by default)")]
    public bool autoPlaceBackgroundOnStart = true;
    public bool smoothStartBackground = true;
    public float smoothStartTime = 0.4f;
    public float bgStepPerBalloon = 120f; // legacy (background step)

    [Header("Input Lock (keep balloons visible)")]
    public bool lockInputDuringSmoothStart = true;
    public GameObject balloonsRoot;
    public CanvasGroup balloonsCanvasGroup;
    public float unlockDelay = 0f;

    [Header("Landing FX")]
    public RectTransform floatGroup;
    public float landFallTime = 0.25f;
    public float bounceUp = 50f;
    public float bounceTime = 0.18f;
    public float squishX = 1.15f;
    public float squishY = 0.85f;
    [SerializeField] private AnimalHangingIdle hangingIdle;

    [Header("A4.3 - Landing Dust VFX")]
    public GameObject landingDustVfxPrefab;
    public Canvas landingVfxCanvas;
    public float landingDustYOffset = 0f;
    public float landingDustAutoDestroy = 2f;

    [Header("Landing Shadow")]
    public RectTransform animalShadow;   // assign AnimalShadow RectTransform
    public CanvasGroup shadowCanvasGroup; // optional, auto-resolve if null
    public float shadowFadeInTime = 0.12f;
    public float shadowScaleInTime = 0.12f;
    public Vector2 shadowOffset = new Vector2(0f, 20f); // tweak in inspector
    public float shadowScaleMultiplier = 1.0f; // tweak (0.9-1.2)


    [Header("Ground System (Phase 2 - moves UP)")]
    public RectTransform groundLayer;              // MOVES UP each pop
    public RectTransform groundAnchor;             // landing Y comes from here (inside GroundLayer)
    public float groundStepPerBalloon = 120f;      // how much ground moves per pop
    public float groundMoveSpeed = 8f;             // smoothing speed

    [Header("A4.X – Food Fly System")]
    [Tooltip("Prefab: RectTransform + Image + FoodFlyUI. No content — sprite is set at runtime.")]
    public GameObject foodItemPrefab;
    [Tooltip("Parent container where food is spawned. Must share a canvas with the balloons.")]
    public RectTransform foodContainer;
    [Tooltip("Empty child RectTransform placed at the animal's mouth position.")]
    public RectTransform mouthPoint;

    [Header("Food Audio")]
    [Tooltip("AudioSource in scene used for food fly + chew sounds.")]
    public AudioSource foodSfxSource;
    [Tooltip("Played once when food launches from balloon.")]
    public AudioClip foodFlySound;
    [Tooltip("Played on each chew squish cycle.")]
    public AudioClip chewSound;

    [Header("Mouth VFX")]
    [Tooltip("Particle prefab spawned when food arrives at mouth.")]
    public GameObject mouthVfxPrefab;

    [Header("Slow-Mo Burst")]
    [Tooltip("Time scale during the dramatic burst moment (0.1–0.4).")]
    [Range(0.05f, 0.5f)] public float slowMoScale = 0.25f;
    [Tooltip("How long the slow-mo lasts (real seconds).")]
    public float slowMoDuration = 0.18f;

    [Header("Chew Animation")]
    public int chewCount = 3;
    public float chewSquishTime = 0.08f;
    public float chewSquishX = 1.12f;
    public float chewSquishY = 0.88f;

    [Header("Idle VFX")]
    public GameObject shakingVFXGroup;

    [Header("References")]
    public FloatBobbingUI floatBobbing;
    public RectTransform animal;
    public Image animalImage;
    public BalloonTap[] balloons;

    [Header("Animal Sprites")]
    public Sprite flyingSprite;
    public Sprite sittingSprite;

    [Header("M1 A4.2 - Pop Reaction (Scared Sprite)")]
    public Sprite scaredSprite;
    public float popReactionTime = 0.12f;

    private Coroutine popReactionRoutine;
    private Sprite spriteBeforePop;
    private bool isLanded = false;

    [Header("Background (LEGACY - stays still now)")]
    public RectTransform background;   // can stay assigned but will not move
    public float bgMoveSpeed = 8f;     // legacy

    [Header("Landing (LEGACY fallback)")]
    public float groundY = -350f;      // used only if groundAnchor not assigned

    private int remaining;
    private int total;

    private Canvas rootCanvas;
    private Camera uiCamera;

    // ground movement target
    private Vector2 groundTargetPos;
    private float groundFinalY;

    private float authoredGroundFinalY;
    private bool groundFinalYCached = false;


    private bool isSmoothingStart = false;

    private Vector3 baseAnimalScale;
    private Vector3 baseFloatGroupScale;

    private Vector3 baseShadowScale;

    public void BindBalloonsFromRoot(RectTransform balloonsRootRt)
    {
        if (balloonsRootRt == null) return;

        balloonsRoot = balloonsRootRt.gameObject;
        TryResolveBalloonsCanvasGroup();

        // IMPORTANT: only active spawned balloons
        var taps = balloonsRootRt.GetComponentsInChildren<BalloonTap>(false);
        BindBalloons(taps);
    }

    public void BindBalloons(BalloonTap[] newBalloons)
    {
        // 1) unsubscribe old
        if (balloons != null)
        {
            foreach (var b in balloons)
            {
                if (b != null) b.onPoppedSource -= OnBalloonPopped;
            }
        }

        // 2) assign new
        balloons = newBalloons;

        if (balloons == null || balloons.Length == 0)
        {
            total = 0;
            remaining = 0;
            return;
        }
        // 3) recount
        total = 0;
        remaining = 0;

        if (balloons != null)
        {
            foreach (var b in balloons)
            {
                if (b == null) continue;

                if (b.gameObject.activeSelf)
                {
                    total++;
                    remaining++;
                }

                b.onPoppedSource += OnBalloonPopped;
            }
        }

        // 4) re-init ground start position based on balloon count
        if (groundLayer != null)
        {
            if (!groundFinalYCached)
            {
                authoredGroundFinalY = groundLayer.anchoredPosition.y;
                groundFinalYCached = true;
            }

            groundFinalY = authoredGroundFinalY;

            float startY = groundFinalY - GetGroundStep() * Mathf.Max(0, total - 1);
            groundTargetPos = new Vector2(groundLayer.anchoredPosition.x, startY);
            groundLayer.anchoredPosition = groundTargetPos;
        }
    }

    private void Awake()
    {
        if (groundLayer != null)
        {
            authoredGroundFinalY = groundLayer.anchoredPosition.y;
            groundFinalYCached = true;
        }
    }
    private void Start()
    {

        rootCanvas = GetComponentInParent<Canvas>();
        if (rootCanvas != null)
        {
            // For Screen Space - Camera this should be set. For Overlay it can be null.
            uiCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceCamera ? rootCanvas.worldCamera : null;
        }

        // --- Validate references ---
        if (animalImage == null && animal != null)
            animalImage = animal.GetComponent<Image>();

        // Resolve idle component
        if (hangingIdle == null)
        {
            if (animal != null) hangingIdle = animal.GetComponentInChildren<AnimalHangingIdle>(true);
            if (hangingIdle == null && floatGroup != null) hangingIdle = floatGroup.GetComponentInChildren<AnimalHangingIdle>(true);
        }

        // --- Count balloons ---
        total = 0;
        remaining = 0;

        // If balloons were assigned manually in inspector, bind them.
        // If balloons are spawned later, BalloonSpawner will call BindBalloonsFromRoot().
        if (balloons != null && balloons.Length > 0)
        {
            BindBalloons(balloons);
        }

        // --- Set initial sprite ---
        if (animalImage != null && flyingSprite != null)
            animalImage.sprite = flyingSprite;

        // --- Input lock support ---
        TryResolveBalloonsCanvasGroup();

        // --- Ground init (Phase 2) ---

        if (groundLayer != null)
        {
            if (!groundFinalYCached)
            {
                authoredGroundFinalY = groundLayer.anchoredPosition.y;
                groundFinalYCached = true;
            }

            groundFinalY = authoredGroundFinalY;

            float startY = groundFinalY - GetGroundStep() * Mathf.Max(0, total - 1);

            groundTargetPos = new Vector2(groundLayer.anchoredPosition.x, startY);
            groundLayer.anchoredPosition = groundTargetPos;
        }


        // We keep legacy SmoothStart lock logic available (optional)
        // but we DO NOT move background anymore.
        // If you still want to lock input for a moment on start, enable smoothStartBackground and use smoothStartTime.
        if (smoothStartBackground && smoothStartTime > 0f && lockInputDuringSmoothStart)
        {
            // just do a short lock/unlock without moving anything
            StartCoroutine(SmoothStartInputLockOnly(smoothStartTime));
        }

        if (animal != null) baseAnimalScale = animal.localScale;
        if (floatGroup != null) baseFloatGroupScale = floatGroup.localScale;

        // Shadow setup
        if (animalShadow != null)
        {
            baseShadowScale = animalShadow.localScale;

            // ensure it has CanvasGroup for fade
            if (shadowCanvasGroup == null)
                shadowCanvasGroup = animalShadow.GetComponent<CanvasGroup>();

            if (shadowCanvasGroup == null)
                shadowCanvasGroup = animalShadow.gameObject.AddComponent<CanvasGroup>();

            // start hidden
            shadowCanvasGroup.alpha = 0f;
            animalShadow.gameObject.SetActive(false);

            // optional: apply offset
            animalShadow.anchoredPosition += shadowOffset;
        }
    }

    private void Update()
    {
        // Move ONLY ground layer towards target
        if (!isSmoothingStart && groundLayer != null)
        {
            groundLayer.anchoredPosition = Vector2.Lerp(
                groundLayer.anchoredPosition,
                groundTargetPos,
                Time.deltaTime * groundMoveSpeed
            );
        }
    }

    private void OnBalloonPopped(BalloonTap source)
    {
        if (remaining <= 0) return;

        // A4.2: scared sprite briefly on every pop
        TriggerPopReaction();

        if (remaining == 1)
        {
            // Snap ground to its target so GroundAnchor is correct when landing starts
            if (groundLayer != null)
                groundLayer.anchoredPosition = groundTargetPos;

            TrySpawnFood(source);
            LandAnimal();
            remaining = 0;
            return;
        }

        // Move ground UP by step (Phase 2)
        if (groundLayer != null)
        {
            groundTargetPos.y += GetGroundStep();
            groundTargetPos.y = Mathf.Min(groundTargetPos.y, groundFinalY); // clamp
        }

        remaining--;
    }

    private void TrySpawnFood(BalloonTap source)
    {
        if (foodItemPrefab == null) return;
        if (source == null || source.food == null) return;
        if (foodContainer == null) return;

        // Balloon is already SetActive(false) but its transform position is still valid
        var balloonRT = source.GetComponent<RectTransform>();
        if (balloonRT == null) return;

        // Convert balloon world position to foodContainer local space
        Vector2 startPos = (Vector2)foodContainer.InverseTransformPoint(balloonRT.position);

        GameObject foodGO = Instantiate(foodItemPrefab, foodContainer);
        foodGO.transform.SetAsLastSibling();
        var fly = foodGO.GetComponent<FoodFlyUI>();
        if (fly == null) fly = foodGO.AddComponent<FoodFlyUI>();

        fly.sfxSource      = foodSfxSource;
        fly.flySound       = foodFlySound;
        fly.mouthVfxPrefab = mouthVfxPrefab;
        fly.mouthVfxColor  = source.food.vfxColor;

        RectTransform target = mouthPoint != null ? mouthPoint : animal;
        fly.StartFlight(source.food.sprite, startPos, target, foodContainer, OnFoodArrived);
    }

    private void OnFoodArrived()
    {
        StartCoroutine(ChewRoutine());
    }

    private System.Collections.IEnumerator ChewRoutine()
    {
        if (animal == null) yield break;

        Vector3 baseScale = baseAnimalScale;

        for (int i = 0; i < chewCount; i++)
        {
            if (foodSfxSource != null && chewSound != null)
                foodSfxSource.PlayOneShot(chewSound);

            // squish
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, chewSquishTime);
                float k = Mathf.Clamp01(t);
                animal.localScale = Vector3.Lerp(baseScale, new Vector3(baseScale.x * chewSquishX, baseScale.y * chewSquishY, baseScale.z), k);
                yield return null;
            }
            // restore
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / Mathf.Max(0.01f, chewSquishTime);
                animal.localScale = Vector3.Lerp(new Vector3(baseScale.x * chewSquishX, baseScale.y * chewSquishY, baseScale.z), baseScale, Mathf.Clamp01(t));
                yield return null;
            }
        }

        animal.localScale = baseScale;
    }

    private void ShowLandingShadow()
    {
        if (animalShadow == null) return;

        animalShadow.gameObject.SetActive(true);

        // reset before animation
        animalShadow.localScale = baseShadowScale * 0.6f;
        if (shadowCanvasGroup != null) shadowCanvasGroup.alpha = 0f;

        StartCoroutine(ShadowInRoutine());
    }

    private System.Collections.IEnumerator ShadowInRoutine()
    {
        float t = 0f;

        Vector3 startScale = baseShadowScale * 0.6f;
        Vector3 endScale = baseShadowScale * shadowScaleMultiplier;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, shadowScaleInTime);
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // ease out

            if (animalShadow != null)
                animalShadow.localScale = Vector3.Lerp(startScale, endScale, k);

            if (shadowCanvasGroup != null)
            {
                float a = Mathf.Clamp01(t / Mathf.Max(0.01f, shadowFadeInTime));
                shadowCanvasGroup.alpha = a;
            }

            yield return null;
        }

        if (animalShadow != null) animalShadow.localScale = endScale;
        if (shadowCanvasGroup != null) shadowCanvasGroup.alpha = 1f;
    }


    private void LandAnimal()
    {
        isLanded = true;

        // stop bobbing
        if (floatBobbing != null)
            floatBobbing.enabled = false;

        // stop idle (blink+tremble)
        if (hangingIdle != null)
        {
            hangingIdle.StopAndReset();
            hangingIdle.enabled = false;
        }

        if (shakingVFXGroup != null)
            shakingVFXGroup.SetActive(false);

        // change sprite
        if (animalImage != null && sittingSprite != null)
            animalImage.sprite = sittingSprite;

        StopAllCoroutines();
        StartCoroutine(SlowMoRoutine(slowMoScale, slowMoDuration)); // dramatic burst moment
        StartCoroutine(LandingRoutine());
        ShowLandingShadow();
    }

    private System.Collections.IEnumerator LandingRoutine()
    {
        RectTransform target = floatGroup != null ? floatGroup : animal;

        Vector2 startPos = target.anchoredPosition;
        float groundYLocal = GetGroundY();
        Vector2 endPos = new Vector2(startPos.x, groundYLocal);

        // Debug (enable only when troubleshooting)
        // Debug.Log($"GroundY computed = {groundYLocal} | anchor={groundAnchor?.name}");


        Vector3 startScaleCurrent = target.localScale;
        Vector3 baseTargetScale = (target == floatGroup) ? baseFloatGroupScale : baseAnimalScale;
        Vector3 animalStartScaleCurrent = (animal != null) ? animal.localScale : Vector3.one;

        // 1) fall down
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, landFallTime);
            float k = EaseOutCubic(t);
            target.anchoredPosition = Vector2.Lerp(startPos, endPos, k);

            // smooth scale back to base (no snap)
            target.localScale = Vector3.Lerp(startScaleCurrent, baseTargetScale, k);
            if (animal != null) animal.localScale = Vector3.Lerp(animalStartScaleCurrent, baseAnimalScale, k);

            yield return null;
        }
        target.anchoredPosition = endPos;

        SpawnLandingDustAtContact(target, endPos);

        // 2) squish
        target.localScale = new Vector3(baseTargetScale.x * squishX, baseTargetScale.y * squishY, baseTargetScale.z);
        yield return new WaitForSeconds(0.06f);

        // 3) bounce up
        Vector2 bouncePos = endPos + new Vector2(0f, bounceUp);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, bounceTime);
            float k = EaseOutCubic(t);
            target.anchoredPosition = Vector2.Lerp(endPos, bouncePos, k);
            target.localScale = Vector3.Lerp(target.localScale, baseTargetScale, Time.deltaTime * 18f);
            yield return null;
        }

        // 4) return down
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, bounceTime);
            float k = EaseInCubic(t);
            target.anchoredPosition = Vector2.Lerp(bouncePos, endPos, k);
            target.localScale = Vector3.Lerp(target.localScale, baseTargetScale, Time.deltaTime * 18f);
            yield return null;
        }

        target.anchoredPosition = endPos;
        target.localScale = baseTargetScale;

        if (floatGroup != null) floatGroup.localScale = baseFloatGroupScale;
        if (animal != null) animal.localScale = baseAnimalScale;
    }

    private void SpawnLandingDustAtContact(RectTransform target, Vector2 endPos)
    {
        if (landingDustVfxPrefab == null) return;
        if (target == null) return;

        RectTransform targetParent = target.parent as RectTransform;
        if (targetParent == null) return;

        Vector3 contactWorld = targetParent.TransformPoint(new Vector3(endPos.x, endPos.y + landingDustYOffset, 0f));

        if (landingVfxCanvas != null)
        {
            RectTransform canvasRect = landingVfxCanvas.transform as RectTransform;
            if (canvasRect != null)
            {
                Camera vfxCam = landingVfxCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : landingVfxCanvas.worldCamera;
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(uiCamera, contactWorld);

                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, vfxCam, out Vector2 localPoint))
                {
                    GameObject vfxUi = Instantiate(landingDustVfxPrefab, canvasRect);
                    vfxUi.transform.localPosition = new Vector3(localPoint.x, localPoint.y, 0f);
                    Destroy(vfxUi, GetVfxLifetime(vfxUi));
                    return;
                }
            }
        }

        GameObject vfxWorld = Instantiate(landingDustVfxPrefab, contactWorld, Quaternion.identity);
        Destroy(vfxWorld, GetVfxLifetime(vfxWorld));
    }

    private float GetVfxLifetime(GameObject vfxInstance)
    {
        if (vfxInstance == null) return Mathf.Max(0.2f, landingDustAutoDestroy);

        ParticleSystem ps = vfxInstance.GetComponentInChildren<ParticleSystem>();
        if (ps == null) return Mathf.Max(0.2f, landingDustAutoDestroy);

        return Mathf.Max(0.2f, ps.main.duration + ps.main.startLifetime.constantMax + 0.2f);
    }

    private float GetGroundY()
    {
        if (groundAnchor == null) return groundY;

        RectTransform target = (floatGroup != null ? floatGroup : animal);
        if (target == null) return groundY;

        RectTransform targetParent = target.parent as RectTransform;
        if (targetParent == null) return groundY;

        // Convert groundAnchor WORLD position to LOCAL point in targetParent space
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(uiCamera, groundAnchor.position);

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetParent,
                screenPoint,
                uiCamera,
                out localPoint))
        {
            return localPoint.y;
        }

        return groundY;
    }

    private float GetGroundStep()
    {
        return groundStepPerBalloon > 0f ? groundStepPerBalloon : bgStepPerBalloon;
    }


    private float EaseOutCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return 1f - Mathf.Pow(1f - x, 3f);
    }

    private float EaseInCubic(float x)
    {
        x = Mathf.Clamp01(x);
        return x * x * x;
    }

    private void OnDisable()
    {
        // Safety: if scene reloads or object is destroyed mid slow-mo, restore timescale
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.fixedUnscaledDeltaTime;
    }

    private System.Collections.IEnumerator SlowMoRoutine(float scale, float realDuration)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = Time.fixedUnscaledDeltaTime * scale;

        float elapsed = 0f;
        while (elapsed < realDuration)
        {
            elapsed += Time.unscaledDeltaTime; // count real time, not slowed time
            yield return null;
        }

        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.fixedUnscaledDeltaTime;
    }

    private System.Collections.IEnumerator SmoothStartInputLockOnly(float duration)
    {
        isSmoothingStart = true;
        SetBalloonsInputLocked(true);

        yield return new WaitForSeconds(Mathf.Max(0.01f, duration));

        if (unlockDelay > 0f)
            yield return new WaitForSeconds(unlockDelay);

        SetBalloonsInputLocked(false);
        isSmoothingStart = false;
    }

    private void TryResolveBalloonsCanvasGroup()
    {
        if (balloonsCanvasGroup != null) return;

        if (balloonsRoot != null)
            balloonsCanvasGroup = balloonsRoot.GetComponent<CanvasGroup>();
    }

    private void SetBalloonsInputLocked(bool locked)
    {
        if (!lockInputDuringSmoothStart) return;
        if (balloonsCanvasGroup == null) return;

        balloonsCanvasGroup.interactable = !locked;
        balloonsCanvasGroup.blocksRaycasts = !locked;
    }

    private void TriggerPopReaction()
    {
        if (animalImage == null) return;
        if (scaredSprite == null) return;
        if (isLanded) return;

        spriteBeforePop = animalImage.sprite;

        if (popReactionRoutine != null)
            StopCoroutine(popReactionRoutine);

        popReactionRoutine = StartCoroutine(PopReactionRoutine());
    }

    private System.Collections.IEnumerator PopReactionRoutine()
    {
        animalImage.sprite = scaredSprite;

        yield return new WaitForSeconds(popReactionTime);

        if (isLanded) yield break;

        if (animalImage != null && animalImage.sprite == scaredSprite)
            animalImage.sprite = spriteBeforePop != null ? spriteBeforePop : flyingSprite;

        popReactionRoutine = null;
    }
}
