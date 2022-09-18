using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainedRewardsHandle : MonoBehaviour
{
    #region Variables
    public static GainedRewardsHandle instance;
    [HideInInspector] public List<Reward> gainedRewards = new List<Reward>();
    #endregion

    private void Awake()
    {
        if (instance==null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
}
