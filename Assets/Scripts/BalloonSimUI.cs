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

    [Header("Pop Shockwave")]
    [Tooltip("Push nearby balloons when one pops (cheap imitation of pressure wave).")]
    public bool enablePopShock = true;
    [Tooltip("How long the shock pushes (seconds). 0.12–0.2 feels good.")]
    public float popShockDuration = 0.16f;


    [Tooltip("Immediate position nudge (pixels) so shock is visible even with strong rope/damping.")]
    public float popShockPositionKick = 18f;


    [Tooltip("Shock radius in multiples of popped balloon radius (1.0 = its radius, 2.0 = twice).")]
    public float popShockRadiusMul = 2.2f;

    [Tooltip("How strong the shock impulse is (adds to balloon velocity).")]
    public float popShockStrength = 180f;

    [Tooltip("Extra upward bias for shock, makes balloons 'jump' a bit.")]
    public float popShockUpBias = 0.25f;


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

    [Header("Animal Jelly")]
    public bool animalJelly = true;

    [Tooltip("How much animal is pushed by pop. 10–40")]
    public float animalPopImpulse = 22f;

    [Tooltip("Spring strength returning to base. 40–120")]
    public float animalSpring = 70f;

    [Tooltip("Damping for spring. 8–20")]
    public float animalSpringDamping = 14f;

    [Tooltip("Max offset from base (pixels), prevents crazy jumps")]
    public float animalMaxOffset = 60f;

    private struct BalloonState
    {
        public RectTransform rt;
        public Vector2 vel;
        public float ropeLen;
        public float radius;
    }

    private readonly List<BalloonState> _states = new();
    private struct PopShock
    {
        public Vector2 pos;
        public float radius;
        public float strength;
        public float timeLeft;
    }

    private readonly List<PopShock> _shocks = new();

    private Vector2 _animalBasePos;
    private Vector2 _animalVel;


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

            // Hook pop event (optional): lets us add a small shockwave to nearby balloons.
            var tap = b.GetComponent<BalloonTap>();
            if (tap != null)
            {
                RectTransform captured = b; // avoid closure issues
                tap.onPopped += () => OnBalloonPopped(captured);
            }

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
        _animalBasePos = animal.anchoredPosition;
        _animalVel = Vector2.zero;

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

    private void OnBalloonPopped(RectTransform popped)
    {
        if (!enablePopShock) return;
        if (popped == null) return;

        float poppedR = 0.5f * Mathf.Min(popped.rect.width, popped.rect.height) * collisionScale;
        float shockRadius = Mathf.Max(1f, poppedR * popShockRadiusMul);

        _shocks.Add(new PopShock
        {
            pos = popped.anchoredPosition,
            radius = shockRadius,
            strength = popShockStrength,
            timeLeft = popShockDuration
        });
        if (animalJelly && animal != null)
        {
            // push animal slightly DOWN + tiny sideways randomness (nice feel)
            float side = Random.Range(-0.4f, 0.4f);
            Vector2 impulseDir = (Vector2.down + Vector2.right * side).normalized;

            _animalVel += impulseDir * animalPopImpulse;
        }

        // tiny counter-move of animal (very subtle)
        if (animal != null)
        {
            animal.anchoredPosition += Vector2.down * 6f;
        }

        Debug.Log("BalloonSimUI got POP from: " + popped.name); // прибери потім
    }
    private void LateUpdate()
    {
        if (!simulate) return;
        if (_shocks.Count == 0) return;

        float dt = Time.unscaledDeltaTime;

        for (int sIdx = _shocks.Count - 1; sIdx >= 0; sIdx--)
        {
            var sh = _shocks[sIdx];
            sh.timeLeft -= dt;

            float life01 = Mathf.Clamp01(sh.timeLeft / popShockDuration);   // 1 -> 0
            float decay = life01;                                           // лінійний спад

            for (int i = 0; i < _states.Count; i++)
            {
                var st = _states[i];
                if (st.rt == null || !st.rt.gameObject.activeInHierarchy) continue;

                Vector2 p = st.rt.anchoredPosition;
                Vector2 d = p - sh.pos;
                float dist = d.magnitude;
                if (dist < 0.001f) dist = 0.001f;
                if (dist > sh.radius) continue;

                float falloff = 1f - Mathf.Clamp01(dist / sh.radius);
                Vector2 n = d / dist;

                // напрям поштовху + легкий “пух” вверх
                Vector2 dir = (n + Vector2.up * popShockUpBias).normalized;

                // IMPORTANT: штовхаємо і позицію, і швидкість
                // позиція — щоб було видно навіть при жорсткій мотузці
                st.rt.anchoredPosition += dir * (popShockPositionKick * falloff * decay);


                // швидкість — щоб потім красиво “розгойдувалось”
                st.vel += dir * (sh.strength * falloff * decay * dt);

                _states[i] = st;

            }

            if (sh.timeLeft <= 0f) _shocks.RemoveAt(sIdx);
            else _shocks[sIdx] = sh;

            
        }
        // --- Animal Jelly Spring ---
        if (animalJelly && animal != null)
        {

            // IMPORTANT: if some other script moves animal (descend), keep base synced
            // Smoothly follow external movement so jelly returns to the "new" base.

            if (_animalVel.sqrMagnitude < 0.01f)
                _animalBasePos = animal.anchoredPosition; // підхоплюємо зовнішній рух (descend)

            Vector2 pos = animal.anchoredPosition;

            // spring force towards base
            Vector2 toBase = _animalBasePos - pos;
            Vector2 acc = toBase * animalSpring;

            _animalVel += acc * dt;

            // damping
            _animalVel *= Mathf.Exp(-animalSpringDamping * dt);


            // integrate
            pos += _animalVel * dt;

            // clamp so it doesn't fly away
            Vector2 offset = pos - _animalBasePos;
            if (offset.magnitude > animalMaxOffset)
                pos = _animalBasePos + offset.normalized * animalMaxOffset;

            animal.anchoredPosition = pos;

            float v = Mathf.Clamp01(_animalVel.magnitude / 200f);
            float sx = 1f + v * 0.08f;
            float sy = 1f - v * 0.10f;
            animal.localScale = Vector3.Lerp(animal.localScale, new Vector3(sx, sy, 1f), 0.35f);

        }
    }


    private void ApplyPopShock(Vector2 popPos, RectTransform popped)
    {
        // Determine a "radius" based on popped balloon size (same as collision radius base).
        float poppedR = 0.5f * Mathf.Min(popped.rect.width, popped.rect.height) * collisionScale;
        float shockRadius = Mathf.Max(1f, poppedR * popShockRadiusMul);

        for (int i = 0; i < _states.Count; i++)
        {
            var s = _states[i];
            if (s.rt == null || !s.rt.gameObject.activeInHierarchy) continue;
            if (s.rt == popped) continue;

            Vector2 p = s.rt.anchoredPosition;
            Vector2 d = p - popPos;
            float dist = d.magnitude;
            if (dist < 0.001f) dist = 0.001f;

            if (dist > shockRadius) continue;

            // 0..1 falloff (strong near pop, weak at edge)
            float falloff = 1f - Mathf.Clamp01(dist / shockRadius);

            Vector2 n = d / dist;

            // Small upward bias makes it feel "puffy"
            Vector2 impulseDir = (n + Vector2.up * popShockUpBias).normalized;

            // Add to velocity (impulse-like)
            // Add to velocity (impulse-like)
            s.vel += impulseDir * (popShockStrength * falloff);

            // ALSO nudge position a bit (so it’s visible even if rope/damping cancels velocity)
            s.rt.anchoredPosition += impulseDir * (popShockPositionKick * falloff);

            _states[i] = s;

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
