using UnityEngine;

[CreateAssetMenu(menuName = "SaveTheAnimals/Balloons/Balloon Pattern Preset")]
public class BalloonPatternPresetSO : ScriptableObject
{
    [Header("Pattern Info")]
    [Min(1)] public int balloonCount = 5;

    [Tooltip("Local positions relative to bouquet center. Size should match balloonCount.")]
    public Vector2[] localPositions;

    [Header("Optional Variation")]
    [Tooltip("Small random offset added on top of preset positions.")]
    [Min(0f)] public float jitterX = 6f;

    [Min(0f)] public float jitterY = 6f;

    [Tooltip("Optional slight scale variation per balloon.")]
    public bool useScaleVariation = false;

    public Vector2 scaleRange = new Vector2(0.96f, 1.04f);

    public bool IsValid()
    {
        return localPositions != null && localPositions.Length == balloonCount;
    }
}