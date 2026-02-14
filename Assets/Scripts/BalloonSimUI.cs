using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Псевдо-реалістична поведінка кульок для UI (без Rigidbody).
/// Працює в anchoredPosition (локально в UI).
/// - Up pull (прагнуть вгору)
/// - Center pull (прагнуть до центру біля тваринки)
/// - Separation (колізія: радіус * collisionScale)
/// - Rope constraint (довжина мотузки фіксується на старті)
/// </summary>
public class BalloonSimUI : MonoBehaviour
{
    [Header("Balloon Rotation")]
    public bool rotateBalloons = true;
    public float rotateSpeed = 10f;   // як швидко “довертається” (6–18)
    public float maxTilt = 25f;       // максимальний нахил в градусах (15–35)
    public float tiltDeadZone = 8f;   // зона, де не нахиляємо (щоб не трусилось)

    [Header("Rotation Target")]
    public RectTransform ropeStartPoint; // перетягни сюди Animal/RopeStartPoint

    [Header("References")]
    public RectTransform animal;                 // перетягни Animal (RectTransform)
    public RectTransform[] balloons;             // можна не заповнювати: авто-знайде дітей

    [Header("Forces")]
    public float upForce = 220f;                 // “тягне вгору”
    public float centerForce = 35f;              // “тягне до центру”
    public float damping = 8f;                   // гасіння швидкості (більше = стабільніше)

    [Header("Collision")]
    [Range(0.3f, 1.0f)] public float collisionScale = 0.75f; // 0.75 = “на 25% менше”
    public int relaxIterations = 3;              // скільки разів за кадр розпихати (2–5)

    [Header("Rope")]
    public Vector2 animalAttachOffset = Vector2.zero; // “за спину” - пізніше підкрутиш
    public float balloonBottomOffsetMultiplier = 0.5f; // точка кріплення на кульці: вниз (0.5 * height)

    [Header("Debug")]
    public bool simulate = true;

    private struct BalloonState
    {
        public RectTransform rt;
        public Vector2 vel;
        public float ropeLen;
        public float radius;
    }

    private readonly List<BalloonState> _states = new();

    void Start()
    {
        if (animal == null)
        {
            Debug.LogError("BalloonSimUI: Animal is not assigned.");
            enabled = false;
            return;
        }

        // Автозбір кульок, якщо масив не заданий
        if (balloons == null || balloons.Length == 0)
        {
            var list = new List<RectTransform>();
            for (int i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i) as RectTransform;
                if (child != null) list.Add(child);
            }
            balloons = list.ToArray();
        }

        _states.Clear();

        Vector2 aPos = animal.anchoredPosition + animalAttachOffset;

        foreach (var b in balloons)
        {
            if (b == null) continue;

            // радіус колізії: 0.5 * min(w,h) * collisionScale
            float r = 0.5f * Mathf.Min(b.rect.width, b.rect.height) * collisionScale;

            // довжина мотузки = поточна відстань від тварини до точки кріплення на кульці (нижня середина)
            Vector2 bAttach = GetBalloonAttachPoint(b);
            float ropeLen = Vector2.Distance(aPos, bAttach);

            _states.Add(new BalloonState
            {
                rt = b,
                vel = Vector2.zero,
                ropeLen = ropeLen,
                radius = r
            });
        }
    }

    void Update()
    {
        if (!simulate) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        Vector2 aPos = animal.anchoredPosition + animalAttachOffset;

        // 1) Інтеграція сил (up + center) у швидкість/позицію
        for (int i = 0; i < _states.Count; i++)
        {
            var s = _states[i];
            if (s.rt == null || !s.rt.gameObject.activeInHierarchy) continue;

            Vector2 pos = s.rt.anchoredPosition;

            // “вгору”
            Vector2 force = Vector2.up * upForce;

            // “до центру” (але не дуже агресивно)
            force += (aPos - pos) * centerForce;

            // інтеграція
            s.vel += force * dt;

            // демпфування
            s.vel = Vector2.Lerp(s.vel, Vector2.zero, dt * damping);

            // рух
            pos += s.vel * dt;
            s.rt.anchoredPosition = pos;

            _states[i] = s;
        }

        // 2) Розслаблення: колізія + мотузка (кілька ітерацій для стабільності)
        for (int iter = 0; iter < relaxIterations; iter++)

        {
            // 2a) separation
            for (int i = 0; i < _states.Count; i++)
            {
                var si = _states[i];
                if (si.rt == null || !si.rt.gameObject.activeInHierarchy) continue;

                for (int j = i + 1; j < _states.Count; j++)
                {
                    var sj = _states[j];
                    if (sj.rt == null || !sj.rt.gameObject.activeInHierarchy) continue;

                    Vector2 pi = si.rt.anchoredPosition;
                    Vector2 pj = sj.rt.anchoredPosition;

                    Vector2 d = pj - pi;
                    float dist = d.magnitude;
                    float minDist = si.radius + sj.radius;

                    if (dist < 0.001f) dist = 0.001f;

                    if (dist < minDist)
                    {
                        Vector2 n = d / dist;
                        float push = (minDist - dist) * 0.5f;

                        si.rt.anchoredPosition = pi - n * push;
                        sj.rt.anchoredPosition = pj + n * push;
                    }
                }
            }

            // 3) Поворот кульок: “низ” кульки спрямований до RopeStartPoint
            if (rotateBalloons)
            {
                for (int i = 0; i < _states.Count; i++)
                {
                    var s = _states[i];
                    if (s.rt == null || !s.rt.gameObject.activeInHierarchy) continue;

                    Vector2 bPos = s.rt.anchoredPosition;

                    Vector2 targetPos =
                        ropeStartPoint != null
                            ? ropeStartPoint.anchoredPosition
                            : (animal.anchoredPosition + animalAttachOffset);

                    Vector2 toTarget = targetPos - bPos;

                    float dist = toTarget.magnitude;
                    if (dist < tiltDeadZone)
                    {
                        float z0 = Mathf.LerpAngle(s.rt.localEulerAngles.z, 0f, Time.deltaTime * rotateSpeed);
                        s.rt.localEulerAngles = new Vector3(0f, 0f, z0);
                        continue;
                    }

                    float angleToTarget = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;

                    // “низ” кульки (-Y) дивиться в ціль => +90
                    float targetZ = angleToTarget + 90f;

                    float clamped = Mathf.Clamp(Mathf.DeltaAngle(0f, targetZ), -maxTilt, maxTilt);

                    float z = Mathf.LerpAngle(s.rt.localEulerAngles.z, clamped, Time.deltaTime * rotateSpeed);
                    s.rt.localEulerAngles = new Vector3(0f, 0f, z);
                }
            }

            // 2b) rope constraint (кулька не може піти далі/інакше від довжини мотузки)
            for (int i = 0; i < _states.Count; i++)
            {
                var s = _states[i];
                if (s.rt == null || !s.rt.gameObject.activeInHierarchy) continue;

                Vector2 attach = GetBalloonAttachPoint(s.rt);
                Vector2 dir = attach - aPos;
                float dist = dir.magnitude;
                if (dist < 0.001f) dist = 0.001f;

                // ми хочемо, щоб dist == ropeLen
                // якщо dist != ropeLen, коригуємо позицію кульки так, щоб її attach став на потрібній відстані
                float target = s.ropeLen;
                Vector2 dirN = dir / dist;

                Vector2 desiredAttach = aPos + dirN * target;

                // Різниця між бажаною точкою кріплення і поточною
                Vector2 delta = desiredAttach - attach;

                // зсуваємо кульку на цю дельту
                s.rt.anchoredPosition += delta;
            }
        }

    }

    // Точка кріплення мотузки: “нижня середина” кульки
    private Vector2 GetBalloonAttachPoint(RectTransform b)
    {
        // anchoredPosition — центр. Нижня точка = центр + (0, -height*0.5*mult)
        float down = b.rect.height * balloonBottomOffsetMultiplier;
        return b.anchoredPosition + new Vector2(0f, -down);
    }
}
