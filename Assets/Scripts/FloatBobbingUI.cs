using UnityEngine;

public class FloatBobbingUI : MonoBehaviour
{
    public RectTransform target;   // кого рухаємо (FloatGroup)
    public float amplitude = 15f;  // висота паріння в пікселях
    public float speed = 1.2f;     // швидкість

    private Vector2 startPos;

    private void Awake()
    {
        if (target == null) target = (RectTransform)transform;
        startPos = target.anchoredPosition;
    }

    private void Update()
    {
        float y = Mathf.Sin(Time.time * speed) * amplitude;
        target.anchoredPosition = startPos + new Vector2(0f, y);
    }
}
