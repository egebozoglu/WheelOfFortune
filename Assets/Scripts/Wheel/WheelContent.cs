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
        // Reward available or not
        public bool Active = true;
        // Reward Rarity
        [SerializeField] private Rarity rarity;

        public string RarityProperty { get { return rarity.RarityProperty; } }

        [HideInInspector] public int Index;
        [HideInInspector] public double Weight = 0f;
    }
}
