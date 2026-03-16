using UnityEngine;

/// <summary>
/// Reads a LevelDefSO and configures the scene for that level.
/// - Applies animal sprites to SaveTheAnimalController.
/// - Pushes visual set + pattern presets into BalloonSpawner, then triggers spawn.
///
/// If no levelDef is assigned, BalloonSpawner runs in its own manual test mode
/// and this script does nothing. Safe to leave in scene without a level assigned.
/// </summary>
public class LevelBuilder : MonoBehaviour
{
    [Header("Level Data")]
    [SerializeField] private LevelDefSO levelDef;

    [Header("Scene References")]
    [SerializeField] private BalloonSpawner spawner;
    [SerializeField] private SaveTheAnimalController controller;

    private void Awake()
    {
        // Awake runs before any Start(), so we can safely disable spawner's auto-start
        // before BalloonSpawner.Start() fires.
        if (levelDef == null) return;
        if (spawner == null) return;

        spawner.SetAutoSpawn(false);
        spawner.ConfigureFromLevel(levelDef);
    }

    private void Start()
    {
        if (levelDef == null) return;

        ApplyAnimalToController();

        if (spawner != null)
            spawner.Spawn(levelDef.balloonCount);
    }

    private void ApplyAnimalToController()
    {
        if (controller == null) return;
        if (levelDef.animal == null) return;

        AnimalDefSO a = levelDef.animal;

        // Only overwrite if the level provides a value — keep scene defaults otherwise
        if (a.flyingSprite != null)  controller.flyingSprite  = a.flyingSprite;
        if (a.sittingSprite != null) controller.sittingSprite = a.sittingSprite;
        if (a.scaredSprite != null)  controller.scaredSprite  = a.scaredSprite;
    }
}
