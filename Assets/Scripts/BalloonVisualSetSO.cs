using UnityEngine;

[CreateAssetMenu(menuName = "SaveTheAnimals/Balloons/Balloon Visual Set")]
public class BalloonVisualSetSO : ScriptableObject
{
    [Header("Visual Variants")]
    public Sprite[] balloonSprites;

    [Header("Optional")]
    [Tooltip("If true, same sprite can repeat in one bouquet.")]
    public bool allowRepeats = true;

    [Header("Food Mapping (optional)")]
    [Tooltip("One FoodDefSO per entry in balloonSprites (matched by index). Leave a slot null for no food on that variant.")]
    public FoodDefSO[] foodPerSprite;
}