using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class RopeLineUI : MonoBehaviour
{
    public Canvas canvas;
    public RectTransform animal;     // тваринка
    public RectTransform balloon;    // кулька

    [Header("Attach points (UI local offsets)")]
    public Vector2 animalOffset = new Vector2(0f, 0f); // Уза спинуФ Ч п≥дкрутиш
    public float balloonBottomOffsetMultiplier = 0.5f; // вниз в≥д центру кульки (0.5*height)

    private LineRenderer lr;
    private Camera uiCam;
    private RectTransform canvasRt;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
    }

    void Start()
    {
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("RopeLineUI: Canvas not found.");
            enabled = false;
            return;
        }

        canvasRt = canvas.GetComponent<RectTransform>();
        uiCam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    void LateUpdate()
    {
        if (animal == null || balloon == null)
        {
            lr.enabled = false;
            return;
        }

        if (!balloon.gameObject.activeInHierarchy)
        {
            lr.enabled = false;
            return;
        }

        lr.enabled = true;

        Vector3 p0 = UIToWorld(animal, animalOffset);
        Vector3 p1 = UIToWorld(balloon, new Vector2(0f, -balloon.rect.height * balloonBottomOffsetMultiplier));

        lr.SetPosition(0, p1);
        lr.SetPosition(1, p0);
        Debug.Log($"Rope: {p0} -> {p1}");
    }

    Vector3 UIToWorld(RectTransform rt, Vector2 localOffset)
    {
        // TransformPoint(localOffset) переводить локальний offset у world
        Vector3 world = rt.TransformPoint(localOffset);
        Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCam, world);

        RectTransformUtility.ScreenPointToWorldPointInRectangle(canvasRt, screen, uiCam, out Vector3 outWorld);
        return outWorld;
    }
}
