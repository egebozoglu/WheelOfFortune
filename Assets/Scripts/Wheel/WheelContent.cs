using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Wheel
{
    [CreateAssetMenu(menuName = "Wheel Contents/Content", fileName = "Content Item")]
    public class WheelContent : ScriptableObject
    {
        public string Name;
        public Sprite ContentIcon;
        public int RewardCount;
        [Range(0f, 100f)]
        public float DropRate = 50f;
        // Prize Rarity
        public Classes RewardClass = new Classes();
        // Prize available or not
        public bool Active = true;

        [HideInInspector] public int Index;
        [HideInInspector] public double Weight = 0f;

        public enum Classes
        {
            Common,
            Normal,
            Rare,
            Bomb
        };
    }
}
