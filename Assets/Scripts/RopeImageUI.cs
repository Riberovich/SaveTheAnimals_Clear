using UnityEngine;

public class RopeImageUI : MonoBehaviour
{
    [Header("Attach points (RectTransforms inside Animal/Balloon)")]
    public RectTransform startPoint; // RopeStartPoint (дитина Animal)
    public RectTransform endPoint;   // RopeEndPoint (дитина Balloon)

    [Header("Rope look")]
    public float ropeThickness = 8f;

    private RectTransform ropeRt;
    private RectTransform commonSpace; // у якому просторі рахуємо (зазвичай FloatGroup або Canvas)

    void Awake()
    {
        ropeRt = (RectTransform)transform;
        ropeRt.pivot = new Vector2(0.5f, 0.5f);

        // найчастіше мотузка лежить у FloatGroup або Canvas
        commonSpace = ropeRt.parent as RectTransform;
    }

    void LateUpdate()
    {
        if (startPoint == null || endPoint == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // якщо кулька зникла — мотузку теж сховати
        if (!endPoint.gameObject.activeInHierarchy || !endPoint.parent.gameObject.activeInHierarchy)
        {
            gameObject.SetActive(false);
            return;
        }

        if (!gameObject.activeSelf) gameObject.SetActive(true);

        // Переводимо позиції точок у простір батька мотузки (commonSpace)
        Vector2 a = WorldToLocal(commonSpace, startPoint.position);
        Vector2 b = WorldToLocal(commonSpace, endPoint.position);

        Vector2 mid = (a + b) * 0.5f;
        ropeRt.anchoredPosition = mid;

        float len = Vector2.Distance(a, b);
        ropeRt.sizeDelta = new Vector2(ropeThickness, len);

        Vector2 dir = (b - a);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        ropeRt.localRotation = Quaternion.Euler(0f, 0f, angle - 90f);
    }

    private Vector2 WorldToLocal(RectTransform space, Vector3 worldPos)
    {
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(space, screenPoint, null, out Vector2 localPoint);
        return localPoint;
    }
}
