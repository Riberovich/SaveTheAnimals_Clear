using UnityEngine;

[CreateAssetMenu(menuName = "SaveTheAnimals/Balloons/Balloon Visual Set")]
public class BalloonVisualSetSO : ScriptableObject
{
    [Header("Visual Variants")]
    public Sprite[] balloonSprites;

    [Header("Optional")]
    [Tooltip("If true, same sprite can repeat in one bouquet.")]
    public bool allowRepeats = true;
}