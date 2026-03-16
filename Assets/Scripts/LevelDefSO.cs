using UnityEngine;

/// <summary>
/// Defines everything needed to play one level: which animal, which biome,
/// how many balloons, which balloon visuals and bouquet layouts to use.
/// LevelBuilder reads this and configures the scene accordingly.
/// </summary>
[CreateAssetMenu(menuName = "SaveTheAnimals/Content/Level Definition")]
public class LevelDefSO : ScriptableObject
{
    [Header("Content")]
    public AnimalDefSO animal;
    public BiomeDefSO biome;

    [Header("Balloons")]
    [Min(1)]
    public int balloonCount = 6;

    [Tooltip("Overrides biome.defaultBalloonVisuals for this level. Leave empty to use biome default.")]
    public BalloonVisualSetSO overrideVisualSet;

    [Tooltip("Bouquet layout presets valid for this level. Should contain presets that match balloonCount.")]
    public BalloonPatternPresetSO[] patternPresets;

    // --- Future systems (not wired yet) ---
    // public FoodDefSO[] foods;

    /// <summary>
    /// Returns the visual set to use: level override → biome default → null.
    /// </summary>
    public BalloonVisualSetSO GetVisualSet()
    {
        if (overrideVisualSet != null) return overrideVisualSet;
        if (biome != null) return biome.defaultBalloonVisuals;
        return null;
    }
}
