using UnityEngine;

public class ParallaxUIController : MonoBehaviour
{
    [Header("Reference (100%)")]
    public RectTransform groundLayer;

    [Header("Layers")]
    public RectTransform skyLayer;
    public RectTransform farLayer;
    public RectTransform midLayer;
    public RectTransform foregroundLayer;

    [Header("Multipliers")]
    [Range(0f, 2f)] public float farMultiplier = 0.25f;
    [Range(0f, 2f)] public float midMultiplier = 0.6f;
    [Range(0f, 2f)] public float foregroundMultiplier = 1.15f;

    private Vector2 groundStart;
    private Vector2 skyStart;
    private Vector2 farStart;
    private Vector2 midStart;
    private Vector2 fgStart;

    private void Awake()
    {
        CacheAuthoredPositions();
        ResetToAuthoredPositions();
    }

    private void LateUpdate()
    {
        if (groundLayer == null) return;

        Vector2 delta = groundLayer.anchoredPosition - groundStart;

        if (skyLayer != null)
            skyLayer.anchoredPosition = skyStart;

        if (farLayer != null)
            farLayer.anchoredPosition = farStart + delta * farMultiplier;

        if (midLayer != null)
            midLayer.anchoredPosition = midStart + delta * midMultiplier;

        if (foregroundLayer != null)
            foregroundLayer.anchoredPosition = fgStart + delta * foregroundMultiplier;
    }

    private void CacheAuthoredPositions()
    {
        if (groundLayer != null) groundStart = groundLayer.anchoredPosition;
        if (skyLayer != null) skyStart = skyLayer.anchoredPosition;
        if (farLayer != null) farStart = farLayer.anchoredPosition;
        if (midLayer != null) midStart = midLayer.anchoredPosition;
        if (foregroundLayer != null) fgStart = foregroundLayer.anchoredPosition;
    }

    public void ResetToAuthoredPositions()
    {
        if (groundLayer != null) groundLayer.anchoredPosition = groundStart;
        if (skyLayer != null) skyLayer.anchoredPosition = skyStart;
        if (farLayer != null) farLayer.anchoredPosition = farStart;
        if (midLayer != null) midLayer.anchoredPosition = midStart;
        if (foregroundLayer != null) foregroundLayer.anchoredPosition = fgStart;
    }
}