using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CreateAssetMenu(menuName = "Wheel Contents/Content", fileName ="Content Item")]
public class WheelContent : ScriptableObject
{
    public string Name;
    public Sprite ContentIcon;
    public int RewardCount;
    [Range(0f, 100f)]
    public float DropRate = 50f;
    public Classes RewardClass = new Classes();
    public bool Active = true;

    [HideInInspector] public int Index;
    [HideInInspector] public double weight = 0f;

    public enum Classes
    {
        Common,
        Normal,
        Rare,
        Bomb
    };
}
