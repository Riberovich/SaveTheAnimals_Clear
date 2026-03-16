using UnityEngine;

/// <summary>
/// Defines a single rescuable animal: sprites, optional audio, rarity, biome tags.
/// </summary>
[CreateAssetMenu(menuName = "SaveTheAnimals/Content/Animal Definition")]
public class AnimalDefSO : ScriptableObject
{
    [Header("Identity")]
    public string id;

    [Header("Sprites")]
    public Sprite flyingSprite;    // shown while floating
    public Sprite sittingSprite;   // shown after landing
    public Sprite scaredSprite;    // shown briefly on each balloon pop

    [Header("Audio (optional)")]
    [Tooltip("Short voice blips played on landing or pop. Leave empty if unused.")]
    public AudioClip[] voiceBlips;

    [Header("Meta")]
    [Range(0f, 1f)]
    [Tooltip("0 = common, 1 = very rare. Used by future unlock/reward logic.")]
    public float rarity = 0.5f;

    [Tooltip("Which biomes this animal can appear in. e.g. 'forest', 'beach'.")]
    public string[] biomeTags;
}
