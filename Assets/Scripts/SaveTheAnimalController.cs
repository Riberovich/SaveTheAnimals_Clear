using UnityEngine;
using UnityEngine.UI;

public class SaveTheAnimalController : MonoBehaviour
{
    [Header("Background Auto-Start")]
    public bool autoPlaceBackgroundOnStart = true;
    public bool smoothStartBackground = true;
    public float smoothStartTime = 0.4f;
    public float bgStepPerBalloon = 120f; // скільки піднімаємо фон за 1 pop (крім останнього)

    [Header("Input Lock (keep balloons visible)")]
    public bool lockInputDuringSmoothStart = true;
    public GameObject balloonsRoot;                 // перетягни сюди об’єкт "Balloons"
    public CanvasGroup balloonsCanvasGroup;         // або перетягни CanvasGroup з "Balloons"
    public float unlockDelay = 0f;                  // опціонально: 0.1-0.2с якщо хочеш паузу після старту

    [Header("Landing FX")]
    public RectTransform floatGroup;   // наш FloatGroup (щоб зсунути всю зв’язку на землю)
    public float landFallTime = 0.25f;
    public float bounceUp = 50f;
    public float bounceTime = 0.18f;
    public float squishX = 1.15f;
    public float squishY = 0.85f;

    [Header("References")]
    public FloatBobbingUI floatBobbing;
    public RectTransform animal;
    public Image animalImage;          // UI Image тваринки (для зміни спрайта)
    public BalloonTap[] balloons;

    [Header("Animal Sprites")]
    public Sprite flyingSprite;        // спрайт коли летить
    public Sprite sittingSprite;       // спрайт коли сидить на землі

    [Header("Background (moves up)")]
    public RectTransform background;   // твій Background (UI Image)
    public float bgMoveSpeed = 8f;     // плавність руху фону

    [Header("Landing")]
    public float groundY = -350f;      // Y позиція "землі" для тваринки (підкрутиш під свій фон)

    private int remaining;
    private int total;

    private Vector2 bgTargetPos;
    private float bgMaxY;              // позиція Y, коли низ фону = низу канвасу
    private float bgStep;              // скільки піднімати фон за 1 кульку
    private bool isSmoothingStart = false;

    private void Start()
    {
        // --- Валідація посилань ---
        if (animalImage == null && animal != null)
            animalImage = animal.GetComponent<Image>();

        // --- Порахувати кульки ---
        total = 0;
        remaining = 0;

        foreach (var b in balloons)
        {
            if (b == null) continue;

            if (b.gameObject.activeSelf)
            {
                total++;
                remaining++;
            }

            b.onPopped += OnBalloonPopped;
        }

        // --- Встановити початковий спрайт ---
        if (animalImage != null && flyingSprite != null)
            animalImage.sprite = flyingSprite;

        // --- Підняти/знайти CanvasGroup для блокування кліків ---
        TryResolveBalloonsCanvasGroup();

        // --- Background init ---
        if (background != null)
        {
            var canvas = background.GetComponentInParent<Canvas>();
            RectTransform canvasRt = canvas.GetComponent<RectTransform>();

            float canvasH = canvasRt.rect.height;
            float bgH = background.rect.height;

            // maxY = позиція, коли низ фону = низу канвасу
            bgMaxY = Mathf.Max(0f, (bgH - canvasH) * 0.5f);

            // базова позиція (X лишається як у сцені)
            bgTargetPos = background.anchoredPosition;

            // крок підняття фону — керований параметр
            bgStep = bgStepPerBalloon;

            // Автостарт позиції фону
            if (autoPlaceBackgroundOnStart)
            {
                float desiredStartY = bgMaxY - bgStep * Mathf.Max(0, total - 1);
                desiredStartY = Mathf.Max(0f, desiredStartY);

                bgTargetPos = new Vector2(bgTargetPos.x, desiredStartY);

                if (!smoothStartBackground)
                {
                    background.anchoredPosition = bgTargetPos; // миттєво
                }
                else
                {
                    StartCoroutine(SmoothBackgroundTo(bgTargetPos, smoothStartTime));
                }
            }
        }
    }

    private void Update()
    {
        if (background == null) return;
        if (isSmoothingStart) return;

        background.anchoredPosition = Vector2.Lerp(
            background.anchoredPosition,
            bgTargetPos,
            Time.deltaTime * bgMoveSpeed
        );
    }

    private void OnBalloonPopped()
    {
        if (remaining <= 0) return;

        if (remaining == 1)
        {
            // остання кулька: фон не рухаємо, приземляємо
            LandAnimal();
            remaining = 0;
            return;
        }

        // піднімаємо фон на крок
        if (background != null)
        {
            bgTargetPos.y += bgStep;
            bgTargetPos.y = Mathf.Min(bgTargetPos.y, bgMaxY);
        }

        remaining--;
    }

    private void LandAnimal()
    {
        // зупинити паріння
        if (floatBobbing != null)
            floatBobbing.enabled = false;

        // змінити спрайт
        if (animalImage != null && sittingSprite != null)
            animalImage.sprite = sittingSprite;

        StopAllCoroutines();
        StartCoroutine(LandingRoutine());
    }

    private System.Collections.IEnumerator LandingRoutine()
    {
        RectTransform target = floatGroup != null ? floatGroup : animal;

        Vector2 startPos = target.anchoredPosition;
        Vector2 endPos = new Vector2(startPos.x, groundY);

        Vector3 startScale = target.localScale;

        // 1) падіння вниз
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, landFallTime);
            float k = EaseOutCubic(t);
            target.anchoredPosition = Vector2.Lerp(startPos, endPos, k);
            yield return null;
        }
        target.anchoredPosition = endPos;

        // 2) squish
        target.localScale = new Vector3(startScale.x * squishX, startScale.y * squishY, startScale.z);
        yield return new WaitForSeconds(0.06f);

        // 3) bounce up
        Vector2 bouncePos = endPos + new Vector2(0f, bounceUp);

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, bounceTime);
            float k = EaseOutCubic(t);
            target.anchoredPosition = Vector2.Lerp(endPos, bouncePos, k);
            target.localScale = Vector3.Lerp(target.localScale, startScale, Time.deltaTime * 18f);
            yield return null;
        }

        // 4) повернення вниз
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, bounceTime);
            float k = EaseInCubic(t);
            target.anchoredPosition = Vector2.Lerp(bouncePos, endPos, k);
            target.localScale = Vector3.Lerp(target.localScale, startScale, Time.deltaTime * 18f);
            yield return null;
        }

        target.anchoredPosition = endPos;
        target.localScale = startScale;
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

    private System.Collections.IEnumerator SmoothBackgroundTo(Vector2 targetPos, float duration)
    {
        if (background == null) yield break;

        isSmoothingStart = true;
        SetBalloonsInputLocked(true); // <-- блокуємо кліки, але НЕ ховаємо

        Vector2 startPos = background.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, duration);
            float k = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f); // EaseOutCubic
            background.anchoredPosition = Vector2.Lerp(startPos, targetPos, k);
            yield return null;
        }

        background.anchoredPosition = targetPos;

        if (unlockDelay > 0f)
            yield return new WaitForSeconds(unlockDelay);

        SetBalloonsInputLocked(false);

        isSmoothingStart = false;
    }

    private void TryResolveBalloonsCanvasGroup()
    {
        if (balloonsCanvasGroup != null) return;

        if (balloonsRoot != null)
        {
            balloonsCanvasGroup = balloonsRoot.GetComponent<CanvasGroup>();
        }
    }

    private void SetBalloonsInputLocked(bool locked)
    {
        if (!lockInputDuringSmoothStart) return;
        if (balloonsCanvasGroup == null) return;

        // ВИДИМОСТЬ НЕ ЧІПАЄМО (alpha не міняємо)
        balloonsCanvasGroup.interactable = !locked;
        balloonsCanvasGroup.blocksRaycasts = !locked; // ключ: блокує тап/клік
    }
}
