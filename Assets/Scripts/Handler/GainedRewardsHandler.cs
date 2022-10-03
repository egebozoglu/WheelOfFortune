using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Model;

namespace Handler
{
    public class GainedRewardsHandler : MonoBehaviour
    {
        #region Variables
        public static GainedRewardsHandler Instance;
        [HideInInspector] public List<Reward> GainedRewards = new List<Reward>();
        #endregion

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);
        }
    }
}
