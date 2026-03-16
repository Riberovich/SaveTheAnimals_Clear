using UnityEngine;

/// <summary>
/// Defines a food item that pops out of the last balloon.
/// One FoodDefSO per balloon color variant.
/// </summary>
[CreateAssetMenu(menuName = "SaveTheAnimals/Content/Food Definition")]
public class FoodDefSO : ScriptableObject
{
    [Header("Identity")]
    public string id;           // "carrot", "banana", "grapes" etc.
    public string colorTag;     // "orange", "yellow", "violet" — informational only

    [Header("Visual")]
    public Sprite sprite;
}
