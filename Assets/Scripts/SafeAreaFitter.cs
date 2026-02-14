using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    RectTransform rt;
    Rect lastSafeArea = new Rect(0, 0, 0, 0);
    ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != lastSafeArea || Screen.orientation != lastOrientation)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect sa = Screen.safeArea;
        lastSafeArea = sa;
        lastOrientation = Screen.orientation;

        Vector2 anchorMin = sa.position;
        Vector2 anchorMax = sa.position + sa.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
