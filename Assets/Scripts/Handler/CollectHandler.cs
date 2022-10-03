using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Manager;
using Model;

namespace Handler
{

    public class CollectHandler : MonoBehaviour
    {
        #region Variables
        private List<Reward> gainedRewards = new List<Reward>();

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
        private bool firstLoad = true;
        private int rewardIndex = 0;
        private float rewardTime = 0f;
        private float rewardRate = 2.4f;
        private bool animationOn = true;
        #endregion

        // Start is called before the first frame update
        void Start()
        {
            if (AudioListener.volume == 0)
            {
                soundButton.gameObject.GetComponentInChildren<Text>().text = "Sound Off";
            }
            else
            {
                soundButton.gameObject.GetComponentInChildren<Text>().text = "Sound On";
            }

            gainedRewards = GainedRewardsHandler.Instance.GainedRewards;

            ButtonListeners();
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
                if (AudioListener.volume == 0)
                {
                    AudioListener.volume = 1;
                    soundButton.gameObject.GetComponentInChildren<Text>().text = "Sound On";
                }
                else
                {
                    AudioListener.volume = 0;
                    soundButton.gameObject.GetComponentInChildren<Text>().text = "Sound Off";
                }
            });
            #endregion
        }

        // Update is called once per frame
        async void Update()
        {
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
                if (rewardObject != null)
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
            AddressablesManager.Instance.ObjectRewardPrefabAssetReference.InstantiateAsync(rewardContainer.transform, false).Completed += (op) =>
            {
                GameObject newReward = op.Result;
            //newReward = Instantiate(rewardPrefab, rewardContainer.transform, false);
            newReward.transform.SetAsFirstSibling();
                newReward.GetComponentInChildren<Image>().sprite = rewardSprite;
                newReward.GetComponentInChildren<Text>().text = rewardCount.ToString();
                transform.GetComponent<AudioSource>().Play();
            };
        }

        
    }
}
