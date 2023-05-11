using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewFoodData",menuName = "自作データ/FoodData")]
public class FoodData : ScriptableObject
{
    public new string name;

    public string eatSound;

    public bool dring;
    
}
