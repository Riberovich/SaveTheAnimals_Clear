using UnityEngine;

/// <summary>
/// Defines a biome: its default balloon visuals, background prefab (future), and music.
/// </summary>
[CreateAssetMenu(menuName = "SaveTheAnimals/Content/Biome Definition")]
public class BiomeDefSO : ScriptableObject
{
    [Header("Identity")]
    public string id;

    [Header("Balloon Visuals")]
    [Tooltip("Default balloon visual set for this biome. LevelDefSO can override per-level.")]
    public BalloonVisualSetSO defaultBalloonVisuals;

    [Header("Visuals (future biome swap)")]
    [Tooltip("Background rig prefab to instantiate for this biome. Not used yet — placeholder.")]
    public GameObject backgroundPrefab;

    [Header("Audio")]
    [Tooltip("Ambient music loop for this biome. Not wired yet — placeholder.")]
    public AudioClip ambientMusic;
}
