using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CollectHandler : MonoBehaviour
{
    #region Variables
    List<Reward> gainedRewards = new List<Reward>();

    [Header("Reward Values")]
    [SerializeField] private GameObject rewardObject;
    [SerializeField] private Text rewardName;
    [SerializeField] private Text rewardCount;
    [SerializeField] private Image rewardImage;

    [Header("Reward Panel")]
    [SerializeField] private RectTransform rewardGardient;
    [SerializeField] private GameObject rewardContainer;
    private Reward reward;

    [Header("Others")]
    // Reward Animation
    [SerializeField] private RectTransform rewardBG;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button soundButton;
    bool firstLoad = true;
    int rewardIndex = 0;
    float rewardTime = 0f;
    float rewardRate = 2.4f;
    bool animationOn = true;
    #endregion
    
    // Start is called before the first frame update
    void Start()
    {
        gainedRewards = GainedRewardsHandle.instance.gainedRewards;

        ButtonListeners();
    }

    // Update is called once per frame
    async void Update()
    {
        // Set Sound Button Text
        soundButton.GetComponentInChildren<Text>().text = "Sound: " + PlayerPrefs.GetString("Sound");

        // Star Animation
        rewardBG.Rotate(new Vector3(0f, 0f, 100f) * Time.deltaTime);

        await RewardAnimation();
    }

    async Task RewardAnimation()
    {
        // Wait a little bit on load
        if (firstLoad)
        {
            await Task.Delay(500);
            firstLoad = false;
            if(rewardObject!=null)
                rewardObject.SetActive(true);
        }

        if (animationOn && rewardName != null && rewardCount != null && rewardImage != null)
        {
            // Set Reward Values
            reward = gainedRewards[rewardIndex];
            rewardName.text = reward.Name;
            rewardCount.text = reward.Count.ToString();
            rewardImage.sprite = reward.Sprite;
            ShowRewardsInOrder();
        }
        else
        {
            rewardObject.SetActive(false);
        }
    }

    void ShowRewardsInOrder()
    {
        rewardTime += Time.deltaTime;
        if (rewardTime > rewardRate)
        {
            rewardTime = 0f;

            // Avoid from out of range error, check index value
            if (rewardIndex + 1 >= gainedRewards.Count)
            {
                animationOn = false;
                rewardGardient.DOAnchorPosX(0f, 2f);
            }
            else
            {
                rewardIndex++;
            }

            AddReward(reward.Sprite, reward.Count);
        }
    }

    void AddReward(Sprite rewardSprite, int rewardCount)
    {
        // Add Reward to panel after show on the center
        AddressablesManager.instance.objectRewardPrefabAssetReference.InstantiateAsync(rewardContainer.transform, false).Completed += (op) =>
        {
            GameObject newReward = op.Result;
            //newReward = Instantiate(rewardPrefab, rewardContainer.transform, false);
            newReward.transform.SetAsFirstSibling();
            newReward.GetComponentInChildren<Image>().sprite = rewardSprite;
            newReward.GetComponentInChildren<Text>().text = rewardCount.ToString();
            transform.GetComponent<AudioSource>().Play();
        };
    }

    void ButtonListeners()
    {
        #region Restart Button
        restartButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("GameScene");
        });
        #endregion

        #region Sound Button
        soundButton.onClick.AddListener(() =>
        {
            if (PlayerPrefs.GetString("Sound") == "On")
            {
                PlayerPrefs.SetString("Sound", "Off");
            }
            else
            {
                PlayerPrefs.SetString("Sound", "On");
            }
        });
        #endregion
    }
}
